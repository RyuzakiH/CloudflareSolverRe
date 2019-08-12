using CloudflareSolverRe.Types;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Solvers
{
    internal class ChallengeSolver
    {
        protected const string LayerJavaScript = "JavaScript";
        protected const string LayerCaptcha = "Captcha";

        protected HttpClient HttpClient { get; }
        protected CloudflareHandler CloudflareHandler { get; }
        protected DetectResult DetectResult { get; private set; }
        protected string UserAgent { get; }
        protected Uri SiteUrl { get; }

        internal ChallengeSolver(HttpClient client, CloudflareHandler handler, Uri siteUrl, DetectResult detectResult, string userAgent)
        {
            HttpClient = client;
            CloudflareHandler = handler;
            SiteUrl = siteUrl;
            DetectResult = detectResult;
            UserAgent = userAgent;
        }

        internal ChallengeSolver(CloudflareHandler handler, Uri siteUrl, DetectResult detectResult, string userAgent)
            : this(new HttpClient(handler), handler, siteUrl, detectResult, userAgent)
        {
        }


        internal virtual Task<SolveResult> Solve()
        {
            throw new NotImplementedException();
        }

    }
}