using CloudflareSolverRe.CaptchaProviders;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Types.Captcha
{
    public class CaptchaChallenge
    {
        private static readonly Regex CaptchaFormRegex = new Regex(@"<form.+?action=""(?<action>\S+?)"".*?>.*?name=""r"" value=""(?<r>[^""]*?)"".*?data-ray=""(?<dataRay>\S+)"".*?fallback\?\w+?=(?<siteKey>\S+)""", RegexOptions.Singleline/* | RegexOptions.Compiled*/);

        public string Action { get; set; }
        public string R { get; set; }
        public string DataRay { get; set; }
        public string SiteKey { get; set; }
        public Uri SiteUrl { get; set; }

        public static CaptchaChallenge Parse(string html, Uri siteUrl)
        {
            var formMatch = CaptchaFormRegex.Match(html);

            return new CaptchaChallenge
            {
                Action = formMatch.Groups["action"].Value,
                R = formMatch.Groups["r"].Value,
                DataRay = formMatch.Groups["dataRay"].Value,
                SiteKey = formMatch.Groups["siteKey"].Value,
                SiteUrl = siteUrl
            };
        }

        public async Task<CaptchaSolveResult> Solve(ICaptchaProvider captchaProvider)
        {
            return await captchaProvider.SolveCaptcha(SiteKey, SiteUrl.AbsoluteUri);
        }

    }
}
