using Cloudflare.Structs;
using System.Threading.Tasks;

namespace Cloudflare.Interfaces
{
    public interface ICaptchaProvider
    {
        string Name { get; }

        Task<CaptchaSolveResult> SolveCaptcha(string siteKey, string webUrl);
    }
}
