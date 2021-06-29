﻿using AntiCaptchaAPI;
using CloudflareSolverRe.Types.Captcha;
using System.Threading.Tasks;

namespace CloudflareSolverRe.CaptchaProviders
{
    public class AntiCaptchaProvider : ICaptchaProvider
    {
        public string Name { get; } = "AntiCaptcha";

        private readonly AntiCaptcha antiCaptcha;

        public AntiCaptchaProvider(string apiKey) => antiCaptcha = new AntiCaptcha(apiKey);

        public async Task<CaptchaSolveResult> SolveCaptcha(string siteKey, string webUrl)
        {
            var result = await antiCaptcha.SolveReCaptchaV2(siteKey, webUrl);

            return new CaptchaSolveResult
            {
                Success = result.Success,
                Response = result.Response,
            };
        }
    }
}
