using CloudflareSolverRe.Types;
using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Solvers
{
    public class ChallengeSolver : HttpClientUtility
    {
        protected const string LayerJavaScript = "JavaScript";
        protected const string LayerCaptcha = "Captcha";

        public HttpClient HttpClient { get; }
        public HttpClientHandler HttpClientHandler { get; }
        public DetectResult DetectResult { get; private set; }
        public Uri SiteUrl { get; }


        public ChallengeSolver(HttpClient client, HttpClientHandler handler, Uri siteUrl, DetectResult detectResult)
        {
            HttpClient = client;
            HttpClientHandler = handler;
            SiteUrl = siteUrl;
            DetectResult = detectResult;            
        }

        public ChallengeSolver(HttpClientHandler handler, Uri siteUrl, DetectResult detectResult)
            : this(new HttpClient(handler), handler, siteUrl, detectResult)
        {
        }


        public virtual Task<SolveResult> Solve()
        {
            throw new NotImplementedException();
        }


    }
}
