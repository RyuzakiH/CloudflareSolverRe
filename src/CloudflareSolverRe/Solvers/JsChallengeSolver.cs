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
        public JsChallengeSolver(HttpClient client, HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, [Optional]int maxRetries)
            : base(client, handler, siteUrl, detectResult, maxRetries)
        {
        }

        public JsChallengeSolver(HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, [Optional]int maxRetries)
            : base(handler, siteUrl, detectResult, maxRetries)
        {
        }

        public new async Task<SolveResult> Solve()
        {
            var solution = default(SolveResult);

            for (int i = 0; (i < MaxRetries) && !solution.Success; i++)
                solution = await SolveChallenge(DetectResult.Html);

            if (!solution.Success && solution.FailReason.Contains("captcha"))
            {
                solution.NewDetectResult = new DetectResult
                {
                    Protection = CloudflareProtection.Captcha,
                    Html = await solution.Response.Content.ReadAsStringAsync()
                };
            }

            return solution;
        }

        private async Task<SolveResult> SolveChallenge(string html)
        {
            var challenge = JsChallenge.Parse(html, SiteUrl);

            var jschl_answer = challenge.Solve();

            var clearancePage = $"{SiteUrl.Scheme}://{SiteUrl.Host}{challenge.Form.Action}";

            var solution = new JsChallengeSolution(clearancePage, challenge.Form, jschl_answer);

            await Task.Delay(challenge.Script.Delay + 100);

            return await SubmitJsSolution(solution);
        }

        private async Task<SolveResult> SubmitJsSolution(JsChallengeSolution solution)
        {
            PrepareHttpHandler();
            PrepareHttpHeaders();

            var response = await HttpClient.GetAsync(solution.ClearanceUrl);

            if (response.StatusCode == HttpStatusCode.Found)
            {
                var success = response.Headers.Contains("Set-Cookie");
                return new SolveResult(success, LayerJavaScript, success ? null : "response cookie not found", DetectResult, response); // "invalid submit response"
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // Captcha
            {
                return new SolveResult(false, LayerCaptcha, "captcha solver required", DetectResult, response);
            }
            else
            {
                return new SolveResult(false, LayerJavaScript, "something wrong happened", DetectResult, response);
            }
        }

    }
}
