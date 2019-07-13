using CloudflareSolverRe.Exceptions;
using CloudflareSolverRe.Extensions;
using CloudflareSolverRe.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CloudflareSolverRe
{
    /// <summary>
    /// A HTTP handler that transparently manages Cloudflare's Anti-DDoS measure.
    /// </summary>
    /// <remarks>
    /// Only the JavaScript challenge can be handled. CAPTCHA and IP address blocking cannot be bypassed.
    /// </remarks>
    public class ClearanceHandler : DelegatingHandler
    {
        /// <summary>
        /// The default number of retries, if clearance fails.
        /// </summary>
        public static readonly int DefaultMaxRetries = 3;

        /// <summary>
        /// The default number of milliseconds to wait before sending the clearance request.
        /// </summary>
        public static readonly int DefaultClearanceDelay = 5000;

        private const string IdCookieName = "__cfduid";
        private const string ClearanceCookieName = "cf_clearance";

        private readonly CookieContainer _cookies = new CookieContainer();
        private readonly HttpClient _client;
        private readonly HttpClientHandler _handler;


        /// <summary>
        /// Gets or sets the number of clearance retries, if clearance fails.
        /// </summary>
        /// <remarks>A negative value causes an infinite amount of retries.</remarks>
        public int MaxRetries { get; set; } = DefaultMaxRetries;

        /// <summary>
        /// Gets or sets the number of milliseconds to wait before sending the clearance request.
        /// </summary>
        public int ClearanceDelay { get; set; } = DefaultClearanceDelay;

        private HttpClientHandler ClientHandler => InnerHandler.GetMostInnerHandler() as HttpClientHandler;


        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/> class with a <see cref="HttpClientHandler"/> as inner handler.
        /// </summary>
        public ClearanceHandler() : this(new HttpClientHandler()) { }

        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/> class with a specific inner handler.
        /// </summary>
        /// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
        public ClearanceHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
            _client = new HttpClient(_handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = _cookies
            });
        }

        private void PrepareHttpClientHandler()
        {
            ClientHandler.AllowAutoRedirect = false;
            ClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            ClientHandler.CookieContainer = _cookies;
        }

        
        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ClearanceHandler"/>, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to releases only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _client.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var idCookieBefore = GetCookie(request.RequestUri, IdCookieName);
            var clearanceCookieBefore = GetCookie(request.RequestUri, ClearanceCookieName);

            //EnsureClientHeader(request);
            InjectCookies(request);

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // (Re)try clearance if required.
            var retries = 0;
            while (CloudflareDetector.IsClearanceRequired(response) && (MaxRetries < 0 || retries <= MaxRetries))
            {
                cancellationToken.ThrowIfCancellationRequested();

                // change
                var result = await new CloudflareSolver().Solve(_client, _handler, request.RequestUri, 3);
                
                InjectCookies(request);
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                retries++;
            }

            // Clearance failed.
            if (CloudflareDetector.IsClearanceRequired(response))
                throw new CloudFlareClearanceException(retries);

            var idCookieAfter = GetCookie(request.RequestUri, IdCookieName);
            var clearanceCookieAfter = GetCookie(request.RequestUri, ClearanceCookieName);

            // inject set-cookie headers in case the cookies changed
            if (idCookieAfter != null && idCookieAfter != idCookieBefore)
            {
                response.Headers.Add(HttpHeader.SetCookie, idCookieAfter.ToHeaderValue());
            }
            if (clearanceCookieAfter != null && clearanceCookieAfter != clearanceCookieBefore)
            {
                response.Headers.Add(HttpHeader.SetCookie, clearanceCookieAfter.ToHeaderValue());
            }

            return response;
        }

        private Cookie GetCookie(Uri uri, string name) =>
            ClientHandler.CookieContainer.GetCookiesByName(uri, name).FirstOrDefault();
        
        private void InjectCookies(HttpRequestMessage request)
        {
            var cookies = _cookies.GetCookies(request.RequestUri).Cast<Cookie>();
            var idCookie = cookies.FirstOrDefault(c => c.Name.Equals(IdCookieName));
            var clearanceCookie = cookies.FirstOrDefault(c => c.Name.Equals(ClearanceCookieName));

            if (idCookie == null || clearanceCookie == null)
                return;

            if (ClientHandler.UseCookies)
            {
                ClientHandler.CookieContainer.Add(request.RequestUri, idCookie);
                ClientHandler.CookieContainer.Add(request.RequestUri, clearanceCookie);
            }
            else
            {
                request.Headers.Add(HttpHeader.Cookie, idCookie.ToHeaderValue());
                request.Headers.Add(HttpHeader.Cookie, clearanceCookie.ToHeaderValue());
            }
        }
        
    }
}
