using CloudflareSolverRe.CaptchaProviders;
using CloudflareSolverRe.Constants;
using CloudflareSolverRe.Types;
using CloudflareSolverRe.Types.Captcha;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Solvers
{
    internal class CaptchaChallengeSolver : ChallengeSolver
    {
        private readonly ICaptchaProvider captchaProvider;

        private bool CaptchaSolvingEnabled => captchaProvider != null;


        internal CaptchaChallengeSolver(HttpClient client, CloudflareHandler handler, Uri siteUrl, DetectResult detectResult, ICaptchaProvider captchaProvider)
            : base(client, handler, siteUrl, detectResult)
        {
            this.captchaProvider = captchaProvider;
        }

        internal CaptchaChallengeSolver(CloudflareHandler handler, Uri siteUrl, DetectResult detectResult, ICaptchaProvider captchaProvider)
            : base(handler, siteUrl, detectResult)
        {
            this.captchaProvider = captchaProvider;
        }


        public new async Task<SolveResult> Solve()
        {
            if (!CaptchaSolvingEnabled)
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
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(solution.ClearanceUrl));
            request.Headers.Referrer = SiteUrl;

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
