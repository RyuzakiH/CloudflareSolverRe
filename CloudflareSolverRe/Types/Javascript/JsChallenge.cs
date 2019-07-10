using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CloudflareSolverRe.Types.Javascript
{
    public class JsChallenge
    {
        private static readonly Regex JsChallengeRegex = new Regex(@"<script.*?>(?<script>.*?var s,t,o,p,b,r,e,a,k,i,n,g,\w, (?<className>\w+?)={""(?<propName>\w+?)"":(?<propValue>.*?)};.*?(?<calculations>\s*?\w+?\.\w+?[+\-*\/]=(?:(?<normal>(?:\+|\(|\)|\!|\[|\]|\/)+?;)|(?<charCode>(?:\+|\(|\)|\!|\[|\]|\/)+?\(function.*?}\((?<p>.*?)\)\)\);)|(?<cfdn>function\(.\)\{var.*?;\s.*?;)))+.*?a\.value\s=\s\(\+\w+\.\w+(\s\+\s(?<addHostLength>t\.length))*?\)\.toFixed\((?<round>\d+)\);.*?},\s*(?<delay>\d+)\);.*?)<\/script>.*?<form.+?action=""(?<action>\S+?)"".*?>.*?name=""s"" value=""(?<s>\S+)"".*?name=""jschl_vc"" value=""(?<jschl_vc>[a-z0-9]{32})"".*?name=""pass"" value=""(?<pass>\S+?)"".*?id=""cf-dn-\S+"">(?<cf_dn>.*?)<\/div>", RegexOptions.Singleline | RegexOptions.Compiled);

        public JsScript Script { get; set; }

        public JsForm Form { get; set; }

        public string Cfdn { get; set; }

        public static JsChallenge Parse(string html)
        {
            var challengeMatch = JsChallengeRegex.Match(html);

            if (!challengeMatch.Success)
                throw new Exception("Error parsing JS challenge html");

            return new JsChallenge
            {
                Script = new JsScript
                {
                    ClassName = challengeMatch.Groups["className"].Value,
                    PropertyName = challengeMatch.Groups["propName"].Value,
                    PropertyValue = challengeMatch.Groups["propValue"].Value,
                    Calculations = GetCalculations(challengeMatch),
                    IsHostLength = challengeMatch.Groups["addHostLength"].Success,
                    Round = int.Parse(challengeMatch.Groups["round"].Value),
                    Delay = int.Parse(challengeMatch.Groups["delay"].Value),
                },
                Form = new JsForm
                {
                    Action = challengeMatch.Groups["action"].Value,
                    S = challengeMatch.Groups["s"].Value,
                    VerificationCode = challengeMatch.Groups["jschl_vc"].Value,
                    Pass = challengeMatch.Groups["pass"].Value
                },
                Cfdn = challengeMatch.Groups["cf_dn"].Value
            };
        }

        private static IEnumerable<JsCalculation> GetCalculations(Match challengeMatch)
        {
            var normalCaptures = challengeMatch.Groups["normal"].Captures.Cast<Capture>();
            var charCodeCaptures = challengeMatch.Groups["charCode"].Captures.Cast<Capture>();

            return challengeMatch.Groups["calculations"].Captures.Cast<Capture>()
                .Select(capture => new JsCalculation
                {
                    Type = GetCalculationType(capture, normalCaptures, charCodeCaptures),
                    Value = capture.Value
                });
        }

        private static JsCalculationType GetCalculationType(Capture capture, IEnumerable<Capture> normalCaptures, IEnumerable<Capture> charCodeCaptures)
        {
            return normalCaptures.Any(Equals(capture)) ? JsCalculationType.Normal :
                (charCodeCaptures.Any(Equals(capture)) ?
                JsCalculationType.CharCode : JsCalculationType.Cfdn);
        }

        private static Func<Capture, bool> Equals(Capture capture) => cap => capture.Value.Contains(cap.Value);

    }
}
