using CloudflareSolverRe.Types;
using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Solvers
{
    public class ChallengeSolver : HttpClientUtility, IClearanceDelayable
    {
        protected const string LayerJavaScript = "JavaScript";
        protected const string LayerCaptcha = "Captcha";

        /// <summary>
        /// Gets or sets the number of milliseconds to wait before sending the clearance request.
        /// </summary>
        /// <remarks>
        /// Negative value or zero means to wait the delay time required by the challenge (like a browser).
        /// </remarks>
        public int ClearanceDelay { get; set; }

        public HttpClient HttpClient { get; }
        public HttpClientHandler HttpClientHandler { get; }
        public DetectResult DetectResult { get; private set; }
        public Uri SiteUrl { get; }


        public ChallengeSolver(HttpClient client, HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, [Optional]int clearanceDelay)
        {
            HttpClient = client;
            HttpClientHandler = handler;
            SiteUrl = siteUrl;
            DetectResult = detectResult;
            
            if (clearanceDelay != default(int))
                ClearanceDelay = clearanceDelay;
        }

        public ChallengeSolver(HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, [Optional]int clearanceDelay)
            : this(new HttpClient(handler), handler, siteUrl, detectResult, clearanceDelay)
        {            
        }


        public virtual Task<SolveResult> Solve()
        {
            throw new NotImplementedException();
        }


    }
}
