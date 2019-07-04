using Cloudflare.Enums;
using Cloudflare.Interfaces;
using Cloudflare.Solvers;
using Cloudflare.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cloudflare
{
    public class CloudflareSolver
    {

        private const int DefaultMaxRetries = 2;

        private readonly ICaptchaProvider captchaProvider;


        public CloudflareSolver()
        {
            captchaProvider = null;
        }

        public CloudflareSolver(ICaptchaProvider captchaProvider)
        {
            this.captchaProvider = captchaProvider;
        }



        


        public async Task<SolveResult> Solve(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri siteUrl, int maxRetry = DefaultMaxRetries, bool validateCloudflare = true, DetectResult? detectResult = null)
        {
            var result = default(SolveResult);

            if (!detectResult.HasValue)
                detectResult = await Detector.Detect(httpClient, httpClientHandler, siteUrl);

            for (var i = 0; i < maxRetry; i++)
            {
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

                        if (!result.Success && result.FailReason.Contains("captcha"))
                        {
                            detectResult = result.DetectResult;
                            i--;
                        }

                        break;
                    case CloudflareProtection.Captcha:
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
            }

            return result;
        }

    }
}