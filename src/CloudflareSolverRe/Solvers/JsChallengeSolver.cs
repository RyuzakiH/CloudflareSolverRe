using CloudflareSolverRe.Constants;
using CloudflareSolverRe.Types;
using CloudflareSolverRe.Types.Javascript;
using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Solvers
{
    public class JsChallengeSolver : ChallengeSolver
    {
        public JsChallengeSolver(HttpClient client, HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, [Optional]int clearanceDelay)
            : base(client, handler, siteUrl, detectResult, clearanceDelay)
        {
        }

        public JsChallengeSolver(HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, [Optional]int clearanceDelay)
            : base(handler, siteUrl, detectResult, clearanceDelay)
        {
        }

        public new async Task<SolveResult> Solve()
        {
            var solution = await SolveChallenge(DetectResult.Html);

            if (!solution.Success && solution.FailReason.Contains("Captcha"))
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

            await Task.Delay(ClearanceDelay <= 0 ? challenge.Script.Delay + 100 : ClearanceDelay);

            return await SubmitJsSolution(solution);
        }

        private async Task<SolveResult> SubmitJsSolution(JsChallengeSolution solution)
        {
            PrepareHttpHandler(HttpClientHandler);

            var request = CreateRequest(new Uri(solution.ClearanceUrl), SiteUrl);
            var response = await HttpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Found)
            {
                var success = response.Headers.Contains(HttpHeaders.SetCookie);
                return new SolveResult(success, LayerJavaScript, success ? null : Errors.ClearanceCookieNotFound, DetectResult, response); // "invalid submit response"
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // Captcha
            {
                return new SolveResult(false, LayerCaptcha, Errors.CaptchaSolverRequired, DetectResult, response);
            }
            else
            {
                return new SolveResult(false, LayerJavaScript, Errors.SomethingWrongHappened, DetectResult, response);
            }
        }

    }
}
