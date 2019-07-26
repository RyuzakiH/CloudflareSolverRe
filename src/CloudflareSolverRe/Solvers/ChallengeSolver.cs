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
        protected Uri SiteUrl { get; }


        internal ChallengeSolver(HttpClient client, CloudflareHandler handler, Uri siteUrl, DetectResult detectResult)
        {
            HttpClient = client;
            CloudflareHandler = handler;
            SiteUrl = siteUrl;
            DetectResult = detectResult;
        }

        internal ChallengeSolver(CloudflareHandler handler, Uri siteUrl, DetectResult detectResult)
            : this(new HttpClient(handler), handler, siteUrl, detectResult)
        {
        }


        public virtual Task<SolveResult> Solve()
        {
            throw new NotImplementedException();
        }


    }
}
