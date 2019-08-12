using CloudflareSolverRe.Constants;
using CloudflareSolverRe.Types;
using CloudflareSolverRe.Types.Javascript;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Solvers
{
    internal class JsChallengeSolver : ChallengeSolver
    {
        /// <summary>
        /// Gets or sets the number of milliseconds to wait before sending the clearance request.
        /// </summary>
        /// <remarks>
        /// Negative value or zero means to wait the delay time required by the challenge (like a browser).
        /// </remarks>
        public int ClearanceDelay { get; set; }

        internal JsChallengeSolver(HttpClient client, CloudflareHandler handler, Uri siteUrl, DetectResult detectResult, string userAgent, [Optional]int? clearanceDelay)
            : base(client, handler, siteUrl, detectResult, userAgent)
        {
            if (clearanceDelay.HasValue)
                ClearanceDelay = clearanceDelay.Value;
        }

        internal JsChallengeSolver(CloudflareHandler handler, Uri siteUrl, DetectResult detectResult, string userAgent, [Optional]int? clearanceDelay)
            : base(handler, siteUrl, detectResult, userAgent)
        {
            if (clearanceDelay.HasValue)
                ClearanceDelay = clearanceDelay.Value;
        }


        internal new async Task<SolveResult> Solve()
        {
            var solution = await SolveChallenge(DetectResult.Html);

            if (!solution.Success && solution.FailReason.Contains(General.Captcha))
            {
                solution.NewDetectResult = new DetectResult
                {
                    Protection = CloudflareProtection.Captcha,
                    Html = await solution.Response.Content.ReadAsStringAsync(),
                    SupportsHttp = DetectResult.SupportsHttp
                };
            }

            return solution;
        }

        private async Task<SolveResult> SolveChallenge(string html)
        {
            var challenge = JsChallenge.Parse(html, SiteUrl);

            var jschl_answer = challenge.Solve();

            var solution = new JsChallengeSolution(SiteUrl, challenge.Form, jschl_answer);

            await Task.Delay(ClearanceDelay <= 0 ? challenge.Script.Delay : ClearanceDelay);

            return await SubmitJsSolution(solution);
        }

        private async Task<SolveResult> SubmitJsSolution(JsChallengeSolution solution)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(solution.ClearanceUrl));
            request.Headers.Referrer = SiteUrl;

            var response = await HttpClient.SendAsync(request);

            return GetSolveResult(response);
        }

        private SolveResult GetSolveResult(HttpResponseMessage submissionResponse)
        {
            var sessionCookies = SessionCookies.FromCookieContainer(CloudflareHandler.HttpClientHandler.CookieContainer, SiteUrl);

            if (submissionResponse.StatusCode == HttpStatusCode.Found)
            {
                var success = submissionResponse.Headers.Contains(HttpHeaders.SetCookie) &&
                    submissionResponse.Headers.GetValues(HttpHeaders.SetCookie)
                    .Any(cookieValue => cookieValue.Contains(SessionCookies.ClearanceCookieName));

                return new SolveResult(success, LayerJavaScript, success ? null : Errors.ClearanceCookieNotFound, DetectResult, sessionCookies, UserAgent, submissionResponse); // "invalid submit response"
            }
            else if (submissionResponse.StatusCode == HttpStatusCode.Forbidden)
            {
                return new SolveResult(false, LayerCaptcha, Errors.CaptchaSolverRequired, DetectResult, sessionCookies, UserAgent, submissionResponse);
            }
            else
            {
                return new SolveResult(false, LayerJavaScript, Errors.SomethingWrongHappened, DetectResult, sessionCookies, UserAgent, submissionResponse);
            }
        }

    }
}
