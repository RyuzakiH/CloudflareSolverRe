using CloudflareSolverRe.Extensions;
using CloudflareSolverRe.Types;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Solvers
{
    public class ChallengeSolver
    {
        protected const string LayerJavaScript = "JavaScript";
        protected const string LayerCaptcha = "Captcha";

        protected const int DefaultMaxRetries = 1;

        public HttpClient HttpClient { get; }
        public HttpClientHandler HttpClientHandler { get; }
        public DetectResult DetectResult { get; private set; }
        public Uri SiteUrl { get; }

        public int MaxRetries { get; set; } = DefaultMaxRetries;


        public ChallengeSolver(HttpClient client, HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, [Optional]int maxRetries)
        {
            HttpClient = client;
            HttpClientHandler = handler;
            SiteUrl = siteUrl;
            DetectResult = detectResult;

            if (maxRetries != default(int))
                MaxRetries = maxRetries;
        }

        public ChallengeSolver(HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, [Optional]int maxRetries)
        {
            HttpClient = new HttpClient(handler);
            HttpClientHandler = handler;
            DetectResult = detectResult;

            if (maxRetries != default(int))
                MaxRetries = maxRetries;
        }


        protected void PrepareHttpHandler()
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

        protected void PrepareHttpHeaders()
        {
            if (HttpClient.DefaultRequestHeaders.Host == null)
                HttpClient.DefaultRequestHeaders.Host = SiteUrl.Host;

            if (!HttpClient.DefaultRequestHeaders.UserAgent.Any())
                HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

            if (!HttpClient.DefaultRequestHeaders.Accept.Any())
                HttpClient.DefaultRequestHeaders.AddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            if (!HttpClient.DefaultRequestHeaders.AcceptLanguage.Any())
                HttpClient.DefaultRequestHeaders.AddWithoutValidation("Accept-Language", "en-US,en;q=0.5");

            //if (!HttpClient.DefaultRequestHeaders.AcceptEncoding.Any())
            //    HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");

            if (HttpClient.DefaultRequestHeaders.Referrer == null)
                HttpClient.DefaultRequestHeaders.Referrer = SiteUrl;

            if (!HttpClient.DefaultRequestHeaders.Connection.Any())
                HttpClient.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");

            //if (!headers.Contains("DNT"))
            //    headers.Add("DNT", "1");

            if (!HttpClient.DefaultRequestHeaders.Contains("Upgrade-Insecure-Requests"))
                HttpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        }


        public virtual Task<SolveResult> Solve()
        {
            throw new NotImplementedException();
        }


    }
}
