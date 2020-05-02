using System;
using System.Text.RegularExpressions;
using Jint;

namespace CloudflareSolverRe.Types.Javascript
{
    public class JsChallenge
    {
        private static readonly Regex JsChallengeRegex = new Regex(@"setTimeout\s*\([^{]*{(?<js_code>.+\.submit\s*\(\s*\)\s*;)\s*}\s*,\s*(?<delay>\d+)\s*\).*?<form.+?action=""(?<action>\S+?)"".*?>.*?name=""r"" value=""(?<r>\S+)"".*?value=""(?<jschl_vc>[a-z0-9]{32})"".*?name=""jschl_vc"".*?name=""pass"" value=""(?<pass>\S+?)"".*?</form>.*?id=""cf-dn-[^>]+>(?<cf_dn>.*?)</div>", RegexOptions.Singleline);
        public Uri SiteUrl { get; set; }
        public string JsCode { get; set; }
        public int Delay { get; set; }
        public JsForm Form { get; set; }
        public string CfDn { get; set; }
        private string JschlAnswer { get; set; }
        private static bool _debug;

        public static JsChallenge Parse(string html, Uri siteUrl, bool debug)
        {
            _debug = debug;
            return Parse(html, siteUrl);
        }

        public static JsChallenge Parse(string html, Uri siteUrl)
        {
            var challengeMatch = JsChallengeRegex.Match(html);
            if (!challengeMatch.Success)
                throw new Exception("Error parsing JS challenge HTML");

            return new JsChallenge
            {
                SiteUrl = siteUrl,
                JsCode = challengeMatch.Groups["js_code"].Value,
                Delay = int.Parse(challengeMatch.Groups["delay"].Value),
                Form = new JsForm
                {
                    Action = System.Net.WebUtility.HtmlDecode(challengeMatch.Groups["action"].Value),
                    R = challengeMatch.Groups["r"].Value,
                    VerificationCode = challengeMatch.Groups["jschl_vc"].Value,
                    Pass = challengeMatch.Groups["pass"].Value
                },
                CfDn = challengeMatch.Groups["cf_dn"].Value
            };
        }

        public string Solve()
        {
            var engine = new Engine().SetValue("invokeCSharp", this);

            // Jint only implements the Javascript language, we have to implement / mock the DOM methods
            // and some unimplemented methods like String.italics
            engine.Execute(@"
                invokeCSharp.JsCallLog(""Example debug message from Javascript"");

                document = {
                    getElementById: function(id) {
                        var fakeObj = {
                            submit: function () {}
                        }
                        Object.defineProperty(fakeObj, ""innerHTML"", {
                            get: function () { return invokeCSharp.JsCallInnerHtml(id); }
                        });
                        Object.defineProperty(fakeObj, ""value"", {
                            get: function () { return invokeCSharp.JsCallGetAttribute(id, ""value""); },
                            set: function(value) { invokeCSharp.JsCallSetAttribute(id, ""value"", value); }
                        });
                        Object.defineProperty(fakeObj, ""action"", {
                            get: function () { return invokeCSharp.JsCallGetAttribute(id, ""action""); },
                            set: function(value) { invokeCSharp.JsCallSetAttribute(id, ""action"", value); }
                        });
                        return fakeObj;
                    },
                    createElement: function(element) {
                        return {
                            innerHTML: """",
                            firstChild: {
                                href:  invokeCSharp.JsCallGetHref()
                            }
                        }
                    }
                };

                location = {
                    hash: invokeCSharp.JsCallGetHash()
                }

                String.prototype.italics = function() {
                    return ""<i>"" + this + ""</i>"";
                };" + JsCode);

            return JschlAnswer;
        }

        // This method is used to print traces from Javascript code
        // ReSharper disable once UnusedMember.Global
        public static void JsCallLog(string message)
        {
            DebugLog($"JsCallLog message: {message}");
        }

        // ReSharper disable once UnusedMember.Global
        public string JsCallGetHref()
        {
            // currently only used to get the base url (js: t.firstChild.href)
            var href = $"{SiteUrl.Scheme}://{SiteUrl.Host}/";
            DebugLog($"JsCallGetHref return: {href}");
            return href;
        }

        // ReSharper disable once UnusedMember.Global
        public string JsCallGetHash()
        {
            // currently only used to get url hash (js: f.action += location.hash)
            var hash = SiteUrl.Fragment;
            DebugLog($"JsCallGetHash return: {hash}");
            return hash;
        }

        // ReSharper disable once UnusedMember.Global
        public string JsCallInnerHtml(string id)
        {
            // currently only used to get the value of <div ... id="cf-dn- ...
            DebugLog($"JsCallInnerHtml id: {id}");
            return CfDn;
        }

        // ReSharper disable once UnusedMember.Global
        public string JsCallGetAttribute(string id, string attr)
        {
            // currently only used in form.action (js: f.action += location.hash;)
            DebugLog($"JsCallGetAttribute id: {id} attr: {attr}");
            return Form.Action;
        }

        // ReSharper disable once UnusedMember.Global
        public void JsCallSetAttribute(string id, string attr, string value)
        {
            // currently only used in:
            // <input ... name="jschl_answer" (js: a.value = ...)
            // form.action (js: f.action += location.hash;)
            DebugLog($"JsCallSetAttribute id: {id} attr: {attr} value: {value}");
            switch (attr)
            {
                case "value":
                    JschlAnswer = value;
                    break;
                case "action":
                    Form.Action = value;
                    break;
                default:
                    DebugLog("JsCallSetAttribute unexpected attr");
                    break;
            }
        }

        private static void DebugLog(string message)
        {
            if (_debug)
            {
                Console.WriteLine($"JsChallenge DEBUG {message}");
            }
        }
    }
}
