using _2CaptchaAPI;
using CloudflareSolverRe.Types.Captcha;
using System.Threading.Tasks;

namespace CloudflareSolverRe.CaptchaProviders
{
    public class TwoCaptchaProvider : ICaptchaProvider
    {
        public string Name { get; } = "2Captcha";

        private readonly _2Captcha twoCaptcha;

        public TwoCaptchaProvider(string apiKey) => twoCaptcha = new _2Captcha(apiKey);

        public async Task<CaptchaSolveResult> SolveCaptcha(string siteKey, string webUrl)
        {
            var result = await twoCaptcha.SolveReCaptchaV2(siteKey, webUrl);

            return new CaptchaSolveResult
            {
                Success = result.Success,
                Response = result.Response,
            };
        }
    }
}
