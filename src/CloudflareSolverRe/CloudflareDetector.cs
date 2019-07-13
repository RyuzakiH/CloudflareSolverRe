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
        private static readonly IEnumerable<string> CloudFlareServerNames = new[] { "cloudflare", "cloudflare-nginx" };


        public static bool IsCloudflareProtected(HttpResponseMessage response) => 
            response.Headers.Server
                .Any(i => i.Product != null
                    && CloudFlareServerNames.Any(s => string.Compare(s, i.Product.Name, StringComparison.OrdinalIgnoreCase).Equals(0)));

        public static bool IsClearanceRequired(HttpResponseMessage response) =>
            response.StatusCode.Equals(HttpStatusCode.ServiceUnavailable) && IsCloudflareProtected(response);


        private static void PrepareHttpHandler(HttpClientHandler httpClientHandler)
        {
            try
            {
                if (httpClientHandler.AllowAutoRedirect)
                    httpClientHandler.AllowAutoRedirect = false;

                if (httpClientHandler.AutomaticDecompression != (DecompressionMethods.GZip | DecompressionMethods.Deflate))
                    httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            catch (Exception) { }
        }

        private static void PrepareHttpHeaders(HttpRequestHeaders headers, Uri targetUri)
        {
            if (headers.Host == null)
                headers.Host = targetUri.Host;

            if (!headers.UserAgent.Any())
                headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

            if (!headers.Accept.Any())
                headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            if (!headers.AcceptLanguage.Any())
                headers.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
            
            if (!headers.Connection.Any())
                headers.Connection.ParseAdd("keep-alive");

            //if (!headers.Contains("DNT"))
            //    headers.Add("DNT", "1");

            if (!headers.Contains("Upgrade-Insecure-Requests"))
                headers.Add("Upgrade-Insecure-Requests", "1");
        }

        private static HttpRequestMessage CreateRequest(Uri targetUri)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, targetUri);

            PrepareHttpHeaders(request.Headers, targetUri);

            return request;
        }


        public static async Task<DetectResult> Detect(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri targetUri, bool requireHttps = false)
        {
            PrepareHttpHandler(httpClientHandler);

            if (!requireHttps)
                targetUri = targetUri.ForceHttp();

            var request = CreateRequest(targetUri);
            var response = await httpClient.SendAsync(request);

            var detectResult = await Detect(response);

            if (detectResult.Protection.Equals(CloudflareProtection.Unknown) && !detectResult.SupportsHttp)
            {
                targetUri = targetUri.ForceHttps();

                request = CreateRequest(targetUri);
                response = await httpClient.SendAsync(request);

                detectResult = await Detect(response);
            }

            return detectResult;
        }

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
