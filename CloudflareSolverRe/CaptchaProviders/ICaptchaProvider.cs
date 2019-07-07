using Cloudflare.Structs;
using System.Threading.Tasks;

namespace Cloudflare.CaptchaProviders
{
    public interface ICaptchaProvider
    {
        string Name { get; }

        Task<CaptchaSolveResult> SolveCaptcha(string siteKey, string webUrl);
    }
}
