using System;
using System.Threading.Tasks;

using CloudflareSolverRe.Types.Captcha;
using CapMonsterCloud;
using CapMonsterCloud.Models.CaptchaTasks;
using CapMonsterCloud.Models.CaptchaTasksResults;
using CapMonsterCloud.Exceptions;

namespace CloudflareSolverRe.CaptchaProviders
{
    public class CapMonsterProvider : ICaptchaProvider
    {
        public string Name { get; } = "CapMonsterProvider";

        private readonly CapMonsterClient capMonsterClient;

        public CapMonsterProvider(string clientKey)
        {
            capMonsterClient = new CapMonsterClient(clientKey);
        }

        public async Task<CaptchaSolveResult> SolveCaptcha(string siteKey, string webUrl)
        {
            var captchaTask = new NoCaptchaTaskProxyless
            {
                WebsiteUrl = webUrl,
                WebsiteKey = siteKey
            };
            try
            {
                int taskId = await capMonsterClient.CreateTaskAsync(captchaTask);
                var solution = await capMonsterClient.GetTaskResultAsync<NoCaptchaTaskProxylessResult>(taskId);
                return new CaptchaSolveResult
                {
                    Success = true,
                    Response = solution.GRecaptchaResponse
                };
            }
            catch (CapMonsterException)
            {
                return new CaptchaSolveResult
                {
                    Success = false
                };
            }
        }
    }
}
