using System.Text.RegularExpressions;

namespace Cloudflare
{
    internal static class CloudflareRegex
    {
        public static readonly Regex ScriptRegex = new Regex(@"<script.*?>(?<script>\s*\/\/.*?)<\/script>", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex JsDefineRegex = new Regex(@"var s,t,o,p,b,r,e,a,k,i,n,g,\w, (?<className>\w+?)={""(?<propName>\w+?)"":.*?};", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex JsCalcRegex = new Regex(@"\s*?\w+?\.\w+?[+\-*\/]=.*?;(?:\s.*?;)?", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex JsHtmlHiddenRegex = new Regex(@"id=""cf-dn-\S+"">(?<inner>.*?)<\/div>", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex JsResultRegex = new Regex(@"a\.value\s=\s\(\+\w+\.\w+(\s\+\s(?<addHostLength>t\.length))?\)\.toFixed\(\d+\);", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex JsPParamRegex = new Regex(@"}\((?<p>.*?)\)\)\);", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex JsFormRegex = new Regex(@"<form.+?action=""(?<action>\S+?)"".*?>.*?name=""s"" value=""(?<s>\S+)"".*?name=""jschl_vc"" value=""(?<jschl_vc>[a-z0-9]{32})"".*?name=""pass"" value=""(?<pass>\S+?)""", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex JsDelayRegex = new Regex(@"},\s*(?<delay>\d+)\);", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex CaptchaFormRegex = new Regex(@"<form.+?action=""(?<action>\S+?)"".*?>.*?name=""s"" value=""(?<s>\S+)"".*?fallback\?\w+?=(?<siteKey>\S+)""", RegexOptions.Singleline | RegexOptions.Compiled);
    }
}
