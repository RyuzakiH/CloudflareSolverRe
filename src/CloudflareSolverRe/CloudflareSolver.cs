using CloudflareSolverRe.CaptchaProviders;
using CloudflareSolverRe.Extensions;
using CloudflareSolverRe.Solvers;
using CloudflareSolverRe.Types;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudflareSolverRe
{
    public class CloudflareSolver
    {
        private const int DefaultMaxRetries = 3;

        private readonly ICaptchaProvider captchaProvider;


        public CloudflareSolver()
        {
            captchaProvider = null;
        }

        public CloudflareSolver(ICaptchaProvider captchaProvider)
        {
            this.captchaProvider = captchaProvider;
        }


        private bool IsCaptchaSolvingEnabled() => captchaProvider != null;


        public async Task<SolveResult> Solve(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri siteUrl, int maxRetry = DefaultMaxRetries, DetectResult? detectResult = null)
        {
            var result = default(SolveResult);
            var newDetectResult = default(DetectResult?);

            for (var i = 0; i < maxRetry * (IsCaptchaSolvingEnabled() ? 2 : 1); i++)
            {
                if (!detectResult.HasValue)
                    detectResult = await CloudflareDetector.Detect(httpClient, httpClientHandler, siteUrl, i >= maxRetry);

                if (i >= maxRetry && siteUrl.Scheme.Equals("http"))
                    siteUrl = siteUrl.ForceHttps();
                else if (detectResult.Value.SupportsHttp && siteUrl.Scheme.Equals("https"))
                    siteUrl = siteUrl.ForceHttp();

                switch (detectResult.Value.Protection)
                {
                    case CloudflareProtection.NoProtection:
                        result = SolveResult.NoProtection;
                        result.DetectResult = detectResult.Value;
                        break;
                    case CloudflareProtection.JavaScript:

                        result = await new JsChallengeSolver(httpClient, httpClientHandler, siteUrl, detectResult.Value).Solve();

                        if (!result.Success && result.NewDetectResult.HasValue && result.NewDetectResult.Value.Protection.Equals(CloudflareProtection.Captcha))
                            newDetectResult = result.NewDetectResult;

                        break;
                    case CloudflareProtection.Captcha:

                        if (i >= maxRetry)
                            result = await new CaptchaChallengeSolver(httpClient, httpClientHandler, siteUrl, detectResult.Value, captchaProvider).Solve();

                        break;
                    case CloudflareProtection.Banned:
                        result = SolveResult.NoProtection;
                        result.DetectResult = detectResult.Value;
                        break;
                    case CloudflareProtection.Unknown:
                        result = SolveResult.Unknown;
                        result.DetectResult = detectResult.Value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (result.Success)
                    return result;

                detectResult = detectResult.Value.Equals(newDetectResult) ? null : newDetectResult;
            }

            return result;
        }

    }
}