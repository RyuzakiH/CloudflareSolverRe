using Cloudflare.Enums;
using Cloudflare.Extensions;
using Cloudflare.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Cloudflare
{
    public class Detector
    {

        private static readonly IEnumerable<string> CloudFlareServerNames = new[] { "cloudflare", "cloudflare-nginx" };

        private static readonly HashSet<int> _statusCodeWhitelist = new HashSet<int>
        {
            200,
            301, 307, 308,
            404, 410,
        };


        public static bool IsCloudflareProtected(HttpResponseMessage response)
        {
            return response.Headers.Server
                .Any(i => i.Product != null && CloudFlareServerNames.Any(s => string.Compare(s, i.Product.Name, StringComparison.OrdinalIgnoreCase).Equals(0)));
        }

        public static bool IsClearanceRequired(HttpResponseMessage response)
        {
            var isServiceUnavailable = response.StatusCode.Equals(HttpStatusCode.ServiceUnavailable);
            var isCloudflareServer = IsCloudflareProtected(response);

            return isServiceUnavailable && isCloudflareServer;
        }


        private static void PrepareHttpHandler(HttpClientHandler httpClientHandler)
        {
            httpClientHandler.AllowAutoRedirect = false;
            httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        }

        private static void PrepareHttpHeaders(HttpRequestHeaders headers, Uri targetUri)
        {
            if (headers.Host == null)
                headers.Host = targetUri.Host;

            if (!headers.UserAgent.Any())
                headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

            if (!headers.Accept.Any())
                headers.AddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            if (!headers.AcceptLanguage.Any())
                headers.AddWithoutValidation("Accept-Language", "en-US,en;q=0.5");

            if (!headers.AcceptEncoding.Any())
                headers.Add("Accept-Encoding", "gzip, deflate");

            if (!headers.Connection.Any())
                headers.Connection.ParseAdd("keep-alive");

            //if (!headers.Contains("DNT"))
            //    headers.Add("DNT", "1");

            if (!headers.Contains("Upgrade-Insecure-Requests"))
                headers.Add("Upgrade-Insecure-Requests", "1");
        }


        public static async Task<DetectResult> Detect(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri targetUri)
        {
            PrepareHttpHandler(httpClientHandler);
            PrepareHttpHeaders(httpClient.DefaultRequestHeaders, targetUri);

            var response = await httpClient.GetAsync(targetUri);
            
            return await Detect(response);
        }

        public static async Task<DetectResult> Detect(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                var html = await response.Content.ReadAsStringAsync();
                if (html.Contains("var s,t,o,p,b,r,e,a,k,i,n,g"))
                {
                    return new DetectResult
                    {
                        Protection = CloudflareProtection.JavaScript,
                        Html = html,
                    };
                }

                return new DetectResult
                {
                    Protection = CloudflareProtection.Unknown,
                    Html = html,
                };
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                var html = await response.Content.ReadAsStringAsync();
                if (html.Contains("g-recaptcha"))
                {
                    return new DetectResult
                    {
                        Protection = CloudflareProtection.Captcha,
                        Html = html,
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

                return new DetectResult
                {
                    Protection = CloudflareProtection.Unknown,
                    Html = html,
                };
            }

            //if ((response.Headers.Contains("CF-RAY")) && (response.IsSuccessStatusCode || _statusCodeWhitelist.Contains((int)response.StatusCode)))
            if (!IsCloudflareProtected(response))
            {
                return new DetectResult
                {
                    Protection = CloudflareProtection.NoProtection,
                };
            }

            return new DetectResult
            {
                Protection = CloudflareProtection.Unknown,
            };
        }



    }
}
