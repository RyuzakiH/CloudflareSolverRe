using System;
using System.Text.RegularExpressions;
using Jint;

namespace CloudflareSolverRe.Types.Javascript
{
    public class JsChallenge
    {
        private static readonly Regex JsCodeRegex = new Regex(@"setTimeout\s*\([^{]*{(?<js_code>.+\.submit\s*\(\s*\)\s*;)\s*}\s*,\s*(?<delay>\d+)\s*\)", RegexOptions.Singleline);
        public Uri SiteUrl { get; set; }
        public string JsCode { get; set; }
        public int Delay { get; set; }
        public JsForm Form { get; set; }
        private string JschlAnswer { get; set; }
        private static bool _debug;
        private static string _html;

        public static JsChallenge Parse(string html, Uri siteUrl, bool debug)
        {
            _debug = debug;
            return Parse(html, siteUrl);
        }

        public static JsChallenge Parse(string html, Uri siteUrl)
        {
            // remove html comments
            _html = Regex.Replace(html, "<!--.*?-->", "", RegexOptions.Singleline);

            // parse challenge
            var jsCodeMatch = JsCodeRegex.Match(_html);
            if (!jsCodeMatch.Success)
                throw new Exception("Error parsing JS challenge HTML");

            return new JsChallenge
            {
                SiteUrl = siteUrl,
                JsCode = jsCodeMatch.Groups["js_code"].Value,
                Delay = int.Parse(jsCodeMatch.Groups["delay"].Value),
                Form = new JsForm
                {
                    Action = System.Net.WebUtility.HtmlDecode(GetFieldAttrByAttr("id", "challenge-form", "action")),
                    R = GetFieldAttrByAttr("name", "r", "value"),
                    VerificationCode =  GetFieldAttrByAttr("name", "jschl_vc", "value"),
                    Pass =  GetFieldAttrByAttr("name", "pass", "value")
                }
            };
        }

        public string Solve()
        {
            var engine = new Engine().SetValue("invokeCSharp", this);

            // Jint only implements the Javascript language, we have to implement / mock the DOM methods
            // and some unimplemented methods like String.italics
            engine.Execute(@"
                //invokeCSharp.JsCallLog(""Example debug message from Javascript"");

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
            var innerHtml = GetInnerHtmlById(id);
            DebugLog($"JsCallInnerHtml id: {id} return: {innerHtml}");
            return innerHtml;
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

        private static string GetInnerHtmlById(string id)
        {
            var regEx = new Regex($@"<[^>]*id\s*=\s*""{id}""[^>]*>(?<innerHTML>\S+?)<\s*/", RegexOptions.Singleline);
            var match = regEx.Match(_html);
            if (!match.Success)
            {
                throw new Exception($"GetInnerHtmlById not found! id: {id}");
            }
            var innerHtml = match.Groups["innerHTML"].Value;
            DebugLog($"GetInnerHtmlById id: {id} return: {innerHtml}");
            return innerHtml;
        }

        private static string GetFieldAttrByAttr(string sAttrId, string sAttrValue, string returnAttr)
        {
            var regEx = new Regex($@"<[^>]*{sAttrId}\s*=\s*""{sAttrValue}""[^>]*>", RegexOptions.Singleline);
            var match = regEx.Match(_html);
            if (!match.Success)
            {
                throw new Exception($"GetInputValueByName element not found! {sAttrId}: {sAttrValue}");
            }
            var fHtml = match.Groups[0].Value;

            var regEx2 = new Regex($@"[^A-Za-z0-9]{returnAttr}\s*=\s*""([^""]*)""", RegexOptions.Singleline);
            var match2 = regEx2.Match(fHtml);
            if (!match2.Success)
            {
                throw new Exception($"GetInputValueByName attribute not found! returnAttr: {returnAttr} fHtml: {fHtml}");
            }
            var res = match2.Groups[1].Value;
            DebugLog($"GetFieldAttrByAttr {sAttrId}={sAttrValue} returnAttr: {returnAttr} return: {res}");
            return res;
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
