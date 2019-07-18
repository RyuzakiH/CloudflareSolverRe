using CloudflareSolverRe.Constants;
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
            // TODO: Random UserAgent
            if (!headers.UserAgent.Any())
                headers.UserAgent.ParseAdd(UserAgents.Firefox66_Win10);

            if (!headers.Accept.Any())
                headers.TryAddWithoutValidation(Constants.HttpHeaders.Accept, HttpHeaderValues.HtmlXmlAll);

            if (!headers.AcceptLanguage.Any())
                headers.TryAddWithoutValidation(Constants.HttpHeaders.AcceptLanguage, HttpHeaderValues.En_Us);

            if (headers.Referrer == null && referrer != null)
                headers.Referrer = referrer;

            if (!headers.Connection.Any())
                headers.Connection.ParseAdd(HttpHeaderValues.KeepAlive);

            //if (!headers.Contains(Constants.HttpHeaders.DNT))
            //    headers.Add(Constants.HttpHeaders.DNT, "1");

            if (!headers.Contains(Constants.HttpHeaders.UpgradeInsecureRequests))
                headers.Add(Constants.HttpHeaders.UpgradeInsecureRequests, "1");
        }

        protected static HttpRequestMessage CreateRequest(Uri targetUri, [Optional]Uri referrer)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, targetUri);

            PrepareHttpHeaders(request.Headers, targetUri, referrer);

            return request;
        }

    }
}
