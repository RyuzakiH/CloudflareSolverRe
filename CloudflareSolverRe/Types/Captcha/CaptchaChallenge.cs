using System.Text.RegularExpressions;

namespace CloudflareSolverRe.Types.Captcha
{
    public class CaptchaChallenge
    {
        private static readonly Regex CaptchaFormRegex = new Regex(@"<form.+?action=""(?<action>\S+?)"".*?>.*?name=""s"" value=""(?<s>\S+)"".*?fallback\?\w+?=(?<siteKey>\S+)""", RegexOptions.Singleline | RegexOptions.Compiled);

        public string Action { get; set; }
        public string S { get; set; }
        public string SiteKey { get; set; }

        public static CaptchaChallenge Parse(string html)
        {
            var formMatch = CaptchaFormRegex.Match(html);

            return new CaptchaChallenge
            {
                Action = formMatch.Groups["action"].Value,
                S = formMatch.Groups["s"].Value,
                SiteKey = formMatch.Groups["siteKey"].Value
            };
        }

    }
}
