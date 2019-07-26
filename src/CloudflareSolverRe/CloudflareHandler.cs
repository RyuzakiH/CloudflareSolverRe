using CloudflareSolverRe.Constants;
using CloudflareSolverRe.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CloudflareSolverRe
{
    public class CloudflareHandler : DelegatingHandler
    {
        private HttpClientHandler HttpClientHandler => InnerHandler.GetMostInnerHandler() as HttpClientHandler;


        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/> class with a <see cref="System.Net.Http.HttpClientHandler"/> as inner handler.
        /// </summary>
        public CloudflareHandler() : this(new HttpClientHandler()) { }

        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/> class with a specific inner handler.
        /// </summary>
        /// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
        public CloudflareHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }


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

            // TODO: Random UserAgent
            if (!request.Headers.UserAgent.Any())
                request.Headers.UserAgent.ParseAdd(UserAgents.Firefox66_Win10);

            if (!request.Headers.Accept.Any())
                request.Headers.TryAddWithoutValidation(Constants.HttpHeaders.Accept, HttpHeaderValues.HtmlXmlAll);

            if (!request.Headers.AcceptLanguage.Any())
                request.Headers.TryAddWithoutValidation(Constants.HttpHeaders.AcceptLanguage, HttpHeaderValues.En_Us);

            if (!request.Headers.Connection.Any())
                request.Headers.Connection.ParseAdd(HttpHeaderValues.KeepAlive);

            //if (!request.Headers.Contains(Constants.HttpHeaders.DNT))
            //    request.Headers.Add(Constants.HttpHeaders.DNT, "1");

            if (!request.Headers.Contains(Constants.HttpHeaders.UpgradeInsecureRequests))
                request.Headers.Add(Constants.HttpHeaders.UpgradeInsecureRequests, "1");
        }

        private void GeneralizeCookies(Uri requestUri)
        {
            var cookies = HttpClientHandler.CookieContainer.GetCookies(requestUri);
            foreach (Cookie cookie in cookies)
                cookie.Secure = false;
        }

    }
}
