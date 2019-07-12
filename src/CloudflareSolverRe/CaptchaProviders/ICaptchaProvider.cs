using CloudflareSolverRe.Types.Captcha;
using System.Threading.Tasks;

namespace CloudflareSolverRe.CaptchaProviders
{
    public interface ICaptchaProvider
    {
        string Name { get; }

        Task<CaptchaSolveResult> SolveCaptcha(string siteKey, string webUrl);
    }
}
