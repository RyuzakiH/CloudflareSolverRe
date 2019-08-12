using CloudflareSolverRe.Extensions;
using CloudflareSolverRe.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CloudflareSolverRe
{
    public class CloudflareDetector
    {
        private static readonly IEnumerable<string> CloudflareServerNames = new[] { "cloudflare", "cloudflare-nginx" };

        /// <summary>
        /// Checks if the response declaring that the website is protected by cloudflare.
        /// </summary>
        /// <param name="response">The HttpResponseMessage to check.</param>
        public static bool IsCloudflareProtected(HttpResponseMessage response) => 
            response.Headers.Server
                .Any(i => i.Product != null
                    && CloudflareServerNames.Any(s => string.Compare(s, i.Product.Name, StringComparison.OrdinalIgnoreCase).Equals(0)));

        /// <summary>
        /// Checks if the response status code is a cloudflare error status code.
        /// </summary>
        /// <param name="response">The HttpResponseMessage to check.</param>
        public static bool IsErrorStatusCode(HttpResponseMessage response) =>
            response.StatusCode.Equals(HttpStatusCode.ServiceUnavailable) || response.StatusCode.Equals(HttpStatusCode.Forbidden);

        /// <summary>
        /// Checks if cloudflare clearance required.
        /// </summary>
        /// <param name="response">The HttpResponseMessage to check.</param>
        public static bool IsClearanceRequired(HttpResponseMessage response) =>
            IsErrorStatusCode(response) && IsCloudflareProtected(response);


        /// <summary>
        /// Detects cloudflare protection type for a specific website.
        /// </summary>
        /// <param name="httpClient">The HttpClient to use in detection process.</param>
        /// <param name="httpClientHandler">The HttpClientHandler of the HttpClient.</param>
        /// <param name="targetUri">The uri of the website.</param>
        /// <param name="requireHttps">Https is required.</param>
        public static async Task<DetectResult> Detect(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri targetUri, bool requireHttps = false)
        {
            var cloudflareHandler = new CloudflareHandler(httpClientHandler);

            var result = await Detect(httpClient, cloudflareHandler, targetUri, requireHttps);

            cloudflareHandler.Dispose();

            return result;
        }

        /// <summary>
        /// Detects cloudflare protection type for a specific website.
        /// </summary>
        /// <param name="httpClient">The HttpClient to use in detection process (if its handler is not the cloudflareHandler).</param>
        /// <param name="cloudflareHandler">The CloudflareHandler of the HttpClient.</param>
        /// <param name="targetUri">The uri of the website.</param>
        /// <param name="requireHttps">Https is required.</param>
        internal static async Task<DetectResult> Detect(HttpClient httpClient, CloudflareHandler cloudflareHandler, Uri targetUri, bool requireHttps = false)
        {
            var _httpClient = httpClient.Clone(cloudflareHandler, false);

            if (!requireHttps)
                targetUri = targetUri.ForceHttp();

            var detectResult = await Detect(_httpClient, targetUri);

            if (detectResult.Protection.Equals(CloudflareProtection.Unknown) && !detectResult.SupportsHttp)
            {
                targetUri = targetUri.ForceHttps();
                detectResult = await Detect(_httpClient, targetUri);
            }

            _httpClient.Dispose();

            return detectResult;
        }

        /// <summary>
        /// Detects cloudflare protection type for a specific website.
        /// </summary>
        /// <param name="httpClient">The HttpClient to use in detection process (must have <see cref="CloudflareHandler"/> handler).</param>
        /// <param name="targetUri">The uri of the website.</param>
        /// <param name="requireHttps">Https is required.</param>
        internal static async Task<DetectResult> Detect(HttpClient httpClient, Uri targetUri, bool requireHttps = false)
        {
            if (!requireHttps)
                targetUri = targetUri.ForceHttp();

            var detectResult = await Detect(httpClient, targetUri);

            if (detectResult.Protection.Equals(CloudflareProtection.Unknown) && !detectResult.SupportsHttp)
            {
                targetUri = targetUri.ForceHttps();
                detectResult = await Detect(httpClient, targetUri);
            }

            return detectResult;
        }

        private static async Task<DetectResult> Detect(HttpClient httpClient, Uri targetUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, targetUri);
            var response = await httpClient.SendAsync(request);

            return await Detect(response);
        }

        /// <summary>
        /// Detects cloudflare protection type for a specific HttpResponseMessage.
        /// </summary>
        /// <param name="response">The HttpResponseMessage to check.</param>
        public static async Task<DetectResult> Detect(HttpResponseMessage response)
        {
            var html = await response.Content.ReadAsStringAsync();

            if (response.StatusCode.Equals(HttpStatusCode.ServiceUnavailable) && html.Contains("var s,t,o,p,b,r,e,a,k,i,n,g"))
            {
                return new DetectResult
                {
                    Protection = CloudflareProtection.JavaScript,
                    Html = html,
                    SupportsHttp = true
                };
            }

            if (response.StatusCode.Equals(HttpStatusCode.Forbidden))
            {
                if (html.Contains("g-recaptcha"))
                {
                    return new DetectResult
                    {
                        Protection = CloudflareProtection.Captcha,
                        Html = html,
                        SupportsHttp = true
                    };
                }

                if (html.Contains("Access denied"))
                {
                    return new DetectResult
                    {
                        Protection = CloudflareProtection.Banned,
                        Html = html,
                    };
                }
            }

            if (response.StatusCode.Equals(HttpStatusCode.MovedPermanently) && response.Headers.Location != null && response.Headers.Location.Scheme.Equals("https"))
            {
                return new DetectResult
                {
                    Protection = CloudflareProtection.Unknown,
                    SupportsHttp = false
                };
            }

            //if ((response.Headers.Contains("CF-RAY")) && (response.IsSuccessStatusCode || _statusCodeWhitelist.Contains((int)response.StatusCode)))
            if (!IsCloudflareProtected(response))
            {
                return new DetectResult
                {
                    Protection = CloudflareProtection.NoProtection,
                    SupportsHttp = true
                };
            }

            return new DetectResult
            {
                Protection = CloudflareProtection.Unknown,
            };
        }

    }
}