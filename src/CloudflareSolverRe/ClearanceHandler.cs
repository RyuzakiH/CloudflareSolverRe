using CloudflareSolverRe.CaptchaProviders;
using CloudflareSolverRe.Constants;
using CloudflareSolverRe.Exceptions;
using CloudflareSolverRe.Extensions;
using CloudflareSolverRe.Types;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    public class ClearanceHandler : DelegatingHandler, ICloudflareSolver
    {
        private readonly CookieContainer _cookies;
        private readonly HttpClient _client;
        private readonly HttpClientHandler _handler;
        private readonly CloudflareSolver _cloudflareSolver;

        /// <summary>
        /// Gets or sets the number of clearance retries, if clearance fails.
        /// </summary>
        /// <remarks>A negative value causes an infinite amount of retries.</remarks>
        public int MaxTries
        {
            get => _cloudflareSolver.MaxTries;
            set => _cloudflareSolver.MaxTries = value;
        }

        /// <summary>
        /// Gets or sets the max number of captcha clearance tries.
        /// </summary>
        public int MaxCaptchaTries
        {
            get => _cloudflareSolver.MaxCaptchaTries;
            set => _cloudflareSolver.MaxCaptchaTries = value;
        }

        /// <summary>
        /// Gets or sets the number of milliseconds to wait before sending the clearance request.
        /// </summary>
        /// <remarks>
        /// Negative value or zero means to wait the delay time required by the challenge (like a browser).
        /// </remarks>
        public int ClearanceDelay
        {
            get => _cloudflareSolver.ClearanceDelay;
            set => _cloudflareSolver.ClearanceDelay = value;
        }

        private HttpClientHandler HttpClientHandler => InnerHandler.GetMostInnerHandler() as HttpClientHandler;


        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/> class with a <see cref="System.Net.Http.HttpClientHandler"/> as inner handler.
        /// </summary>
        public ClearanceHandler() : this(new HttpClientHandler()) { }

        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/> class with a specific inner handler.
        /// </summary>
        /// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
        public ClearanceHandler(HttpMessageHandler innerHandler) : this(innerHandler, null) { }

        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/> class with a captcha provider.
        /// </summary>
        /// <param name="captchaProvider">The captcha provider which is responsible for solving captcha challenges.</param>
        public ClearanceHandler(ICaptchaProvider captchaProvider) : this(new HttpClientHandler(), captchaProvider) { }

        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/> class with a specific inner handler and a captcha provider.
        /// </summary>
        /// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
        /// <param name="captchaProvider">The captcha provider which is responsible for solving captcha challenges.</param>
        public ClearanceHandler(HttpMessageHandler innerHandler, ICaptchaProvider captchaProvider) : base(innerHandler)
        {
            _client = new HttpClient(_handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = _cookies = new CookieContainer()
            });

            _cloudflareSolver = new CloudflareSolver(captchaProvider);
        }


        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sessionCookies = SessionCookies.FromCookieContainer(HttpClientHandler.CookieContainer, request.RequestUri);

            var response = await SendRequestAsync(request, cancellationToken);

            var result = default(SolveResult);

            if (CloudflareDetector.IsClearanceRequired(response))
                result = await GetClearance(request, cancellationToken);
            
            if (result.Success)
            {
                response = await SendRequestAsync(request, cancellationToken);
                InjectSetCookieHeader(response, sessionCookies);
            }

            if (!result.Success && CloudflareDetector.IsClearanceRequired(response))
                throw new CloudflareClearanceException(MaxTries);

            return response;
        }

        private static void EnsureHeaders(HttpRequestMessage request)
        {
            if (!request.Headers.UserAgent.Any())
                request.Headers.Add(HttpHeaders.UserAgent, UserAgents.Firefox66_Win10);
        }

        private void InjectCookies(HttpRequestMessage request)
        {
            var sessionCookies = SessionCookies.FromCookieContainer(_cookies, request.RequestUri);

            if (!sessionCookies.Valid)
                return;

            if (HttpClientHandler.UseCookies)
            {
                HttpClientHandler.CookieContainer.Add(request.RequestUri, sessionCookies.Cfduid);
                HttpClientHandler.CookieContainer.Add(request.RequestUri, sessionCookies.Cf_Clearance);
            }
            else
            {
                request.Headers.Add(HttpHeaders.Cookie, sessionCookies.Cfduid.ToHeaderValue());
                request.Headers.Add(HttpHeaders.Cookie, sessionCookies.Cf_Clearance.ToHeaderValue());
            }
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            EnsureHeaders(request);
            InjectCookies(request);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private async Task<SolveResult> GetClearance(HttpRequestMessage request, CancellationToken cancellationToken) =>
            await _cloudflareSolver.Solve(_client, _handler, request.RequestUri, cancellationToken: cancellationToken);

        private void InjectSetCookieHeader(HttpResponseMessage response, SessionCookies oldSessionCookies)
        {
            var newSessionCookies = SessionCookies.FromCookieContainer(HttpClientHandler.CookieContainer, response.RequestMessage.RequestUri);

            if (oldSessionCookies.Equals(newSessionCookies))
                return;

            // inject set-cookie headers in case the cookies changed
            if (newSessionCookies.Cfduid != null && newSessionCookies.Cfduid != oldSessionCookies.Cfduid)
            {
                response.Headers.Add(HttpHeaders.SetCookie, newSessionCookies.Cfduid.ToHeaderValue());
            }
            if (newSessionCookies.Cf_Clearance != null && newSessionCookies.Cf_Clearance != oldSessionCookies.Cf_Clearance)
            {
                response.Headers.Add(HttpHeaders.SetCookie, newSessionCookies.Cf_Clearance.ToHeaderValue());
            }
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

    }
}
