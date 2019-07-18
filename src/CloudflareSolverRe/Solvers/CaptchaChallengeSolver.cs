using CloudflareSolverRe.CaptchaProviders;
using CloudflareSolverRe.Constants;
using CloudflareSolverRe.Types;
using CloudflareSolverRe.Types.Captcha;
using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Solvers
{
    public class CaptchaChallengeSolver : ChallengeSolver
    {
        private readonly ICaptchaProvider captchaProvider;

        public CaptchaChallengeSolver(HttpClient client, HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, ICaptchaProvider captchaProvider)
            : base(client, handler, siteUrl, detectResult)
        {
            this.captchaProvider = captchaProvider;
        }

        public CaptchaChallengeSolver(HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, ICaptchaProvider captchaProvider)
            : base(handler, siteUrl, detectResult)
        {
            this.captchaProvider = captchaProvider;
        }


        private bool IsCaptchaSolvingEnabled() => captchaProvider != null;


        public new async Task<SolveResult> Solve()
        {
            if (!IsCaptchaSolvingEnabled())
            {
                return new SolveResult
                {
                    Success = false,
                    FailReason = Errors.MissingCaptchaProvider,
                    DetectResult = DetectResult,
                };
            }

            var solve = await SolveChallenge(DetectResult.Html);
            return new SolveResult
            {
                Success = solve.Success,
                FailReason = solve.FailReason,
                DetectResult = DetectResult,
            };
        }

        private async Task<SolveResult> SolveChallenge(string html)
        {
            var challenge = CaptchaChallenge.Parse(html, SiteUrl);

            var result = await challenge.Solve(captchaProvider);

            if (!result.Success)
                return new SolveResult(false, LayerCaptcha, $"captcha provider error ({result.Response})", DetectResult);

            var solution = new CaptchaChallengeSolution(challenge, result.Response);

            return await SubmitCaptchaSolution(solution);
        }

        private async Task<SolveResult> SubmitCaptchaSolution(CaptchaChallengeSolution solution)
        {
            PrepareHttpHandler(HttpClientHandler);

            var request = CreateRequest(new Uri(solution.ClearanceUrl));
            var response = await HttpClient.SendAsync(request);

            if (response.StatusCode.Equals(HttpStatusCode.Found))
            {
                var success = response.Headers.Contains(HttpHeaders.SetCookie);
                return new SolveResult(success, LayerCaptcha, success ? null : Errors.ClearanceCookieNotFound, DetectResult, response);
            }
            else
            {
                return new SolveResult(false, LayerCaptcha, Errors.SomethingWrongHappened, DetectResult, response); //"invalid submit response"
            }
        }

    }
}
