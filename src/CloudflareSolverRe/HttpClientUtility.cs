using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;

namespace CloudflareSolverRe
{
    public abstract class HttpClientUtility
    {
        protected static void PrepareHttpHandler(HttpClientHandler httpClientHandler)
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

        protected static void PrepareHttpHeaders(HttpRequestHeaders headers, Uri siteUrl, [Optional]Uri referrer)
        {
            if (headers.Host == null)
                headers.Host = siteUrl.Host;

            if (!headers.UserAgent.Any())
                headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

            if (!headers.Accept.Any())
                headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            if (!headers.AcceptLanguage.Any())
                headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.5");

            if (headers.Referrer == null && referrer != null)
                headers.Referrer = referrer;

            if (!headers.Connection.Any())
                headers.Connection.ParseAdd("keep-alive");

            //if (!headers.Contains("DNT"))
            //    headers.Add("DNT", "1");

            if (!headers.Contains("Upgrade-Insecure-Requests"))
                headers.Add("Upgrade-Insecure-Requests", "1");
        }

        protected static HttpRequestMessage CreateRequest(Uri targetUri, [Optional]Uri referrer)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, targetUri);

            PrepareHttpHeaders(request.Headers, targetUri, referrer);

            return request;
        }

    }
}
