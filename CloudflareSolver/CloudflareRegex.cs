using System.Text.RegularExpressions;

namespace Cloudflare
{
    internal static class CloudflareRegex
    {
        public static readonly Regex ScriptRegex = new Regex(@"<script.*?>(?<script>.*?)<\/script>", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex JsDefineRegex = new Regex(@"var s,t,o,p,b,r,e,a,k,i,n,g,\w, (?<className>\w+?)={""(?<propName>\w+?)"":.*?};", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex JsCalcRegex = new Regex(@"\s*?\w+?\.\w+?[+\-*\/]=.*?;", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex JsFormRegex = new Regex(@"<form.+?action=""(?<action>\S+?)"".*?>.*?(?:name=""s"" value=""(?<s>\S+)"".*?)?name=""jschl_vc"" value=""(?<jschl_vc>[a-z0-9]{32})"".*?name=""pass"" value=""(?<pass>\S+?)""", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex CaptchaFormRegex = new Regex(@"<form.+?action=""(?<action>\S+?)"".*?>.*?fallback\?\w+?=(?<siteKey>\S+)""", RegexOptions.Singleline | RegexOptions.Compiled);
    }
}
