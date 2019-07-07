using Cloudflare.CaptchaProviders;
using Cloudflare.Enums;
using Cloudflare.Extensions;
using Cloudflare.Solvers;
using Cloudflare.Structs;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cloudflare
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


        public async Task<SolveResult> Solve(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri siteUrl, int maxRetry = DefaultMaxRetries, bool validateCloudflare = true, DetectResult? detectResult = null)
        {
            var result = default(SolveResult);
            var newDetectResult = default(DetectResult?);

            for (var i = 0; i < maxRetry * (IsCaptchaSolvingEnabled() ? 2 : 1); i++)
            {
                if (!detectResult.HasValue)
                    detectResult = await CloudflareDetector.Detect(httpClient, httpClientHandler, siteUrl, i >= maxRetry);

                if (i >= maxRetry && siteUrl.Scheme.Equals(Uri.UriSchemeHttp))
                    siteUrl = siteUrl.ForceHttps();
                else if (detectResult.Value.SupportsHttp && siteUrl.Scheme.Equals(Uri.UriSchemeHttps))
                    siteUrl = siteUrl.ForceHttp();

                switch (detectResult.Value.Protection)
                {
                    case CloudflareProtection.NoProtection:
                        result = new SolveResult
                        {
                            Success = true,
                            FailReason = "No protection detected",
                            DetectResult = detectResult.Value,
                        };
                        break;
                    case CloudflareProtection.JavaScript:

                        result = await new JsChallengeSolver(httpClient, httpClientHandler, siteUrl, detectResult.Value).Solve();

                        if (!result.Success && result.NewDetectResult.HasValue && result.NewDetectResult.Value.Protection.Equals(CloudflareProtection.Captcha))
                        {
                            newDetectResult = result.NewDetectResult;
                            //i--;
                        }
                        break;
                    case CloudflareProtection.Captcha:
                        //if (i != (maxRetry - 1))
                        if (i < maxRetry)
                            break;

                        result = await new CaptchaChallengeSolver(httpClient, httpClientHandler, siteUrl, detectResult.Value, captchaProvider).Solve();
                        break;
                    case CloudflareProtection.Banned:
                        result = new SolveResult
                        {
                            Success = false,
                            FailReason = "IP address is banned",
                            DetectResult = detectResult.Value,
                        };
                        break;
                    case CloudflareProtection.Unknown:
                        result = new SolveResult
                        {
                            Success = false,
                            FailReason = "Unknown protection detected",
                            DetectResult = detectResult.Value,
                        };
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