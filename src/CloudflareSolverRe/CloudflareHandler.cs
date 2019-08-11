using CloudflareSolverRe.Constants;
using CloudflareSolverRe.Extensions;
using CloudflareSolverRe.Utilities;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CloudflareSolverRe
{
    public class CloudflareHandler : DelegatingHandler
    {
        private readonly string userAgent;

        private HttpClientHandler HttpClientHandler => InnerHandler.GetMostInnerHandler() as HttpClientHandler;


        /// <summary>
        /// Creates a new instance of the <see cref="CloudflareHandler"/> class with a <see cref="System.Net.Http.HttpClientHandler"/> as inner handler.
        /// </summary>
        /// <param name="userAgent">The user-agent which will be used accross this session.</param>
        public CloudflareHandler([Optional]string userAgent) : this(new HttpClientHandler(), userAgent) { }

        /// <summary>
        /// Creates a new instance of the <see cref="CloudflareHandler"/> class with a specific inner handler.
        /// </summary>
        /// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
        /// <param name="userAgent">The user-agent which will be used accross this session.</param>
        public CloudflareHandler(HttpMessageHandler innerHandler, [Optional]string userAgent) : base(innerHandler)
        {
            this.userAgent = userAgent ?? Utils.GetGenerateRandomUserAgent();
        }


        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            PrepareHttpHandler();
            PrepareHttpHeaders(request);

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            GeneralizeCookies(request.RequestUri);

            return response;
        }

        private void PrepareHttpHandler()
        {
            try
            {
                if (HttpClientHandler.AllowAutoRedirect)
                    HttpClientHandler.AllowAutoRedirect = false;

                if (HttpClientHandler.AutomaticDecompression != (DecompressionMethods.GZip | DecompressionMethods.Deflate))
                    HttpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            catch (Exception) { }
        }

        private void PrepareHttpHeaders(HttpRequestMessage request)
        {
            if (request.Headers.Host == null)
                request.Headers.Host = request.RequestUri.Host;

            if (!request.Headers.UserAgent.ToString().Equals(userAgent))
            {
                request.Headers.UserAgent.Clear();
                request.Headers.UserAgent.ParseAdd(userAgent);
            }

            if (!request.Headers.Accept.Any())
                request.Headers.TryAddWithoutValidation(HttpHeaders.Accept, HttpHeaderValues.HtmlXmlAll);

            if (!request.Headers.AcceptLanguage.Any())
                request.Headers.TryAddWithoutValidation(HttpHeaders.AcceptLanguage, HttpHeaderValues.En_Us);

            if (!request.Headers.Connection.Any())
                request.Headers.Connection.ParseAdd(HttpHeaderValues.KeepAlive);

            if (!request.Headers.Contains(HttpHeaders.UpgradeInsecureRequests))
                request.Headers.Add(HttpHeaders.UpgradeInsecureRequests, "1");
        }

        private void GeneralizeCookies(Uri requestUri)
        {
            var cookies = HttpClientHandler.CookieContainer.GetCookies(requestUri);
            foreach (Cookie cookie in cookies)
                cookie.Secure = false;
        }

    }
}
