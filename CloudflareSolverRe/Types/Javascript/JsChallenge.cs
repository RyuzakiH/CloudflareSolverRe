using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace CloudflareSolverRe.Types.Javascript
{
    public class JsChallenge
    {
        private static readonly Regex JsChallengeRegex = new Regex(@"<script.*?>(?<script>.*?var s,t,o,p,b,r,e,a,k,i,n,g,\w, (?<className>\w+?)={""(?<propName>\w+?)"":(?<propValue>.*?)};.*?(?<calculations>\s*?\w+?\.\w+?(?<operator>[+\-*\/])=(?:(?<normal>(?:\+|\(|\)|\!|\[|\]|\/)+?;)|(?<charCode>(?:\+|\(|\)|\!|\[|\]|\/)+?\(function.*?}\((?<p>.*?)\)\)\);)|(?<cfdn>function\(.\)\{var.*?;\s.*?;)))+.*?a\.value\s=\s\(\+\w+\.\w+(\s\+\s(?<addHostLength>t\.length))*?\)\.toFixed\((?<round>\d+)\);.*?},\s*(?<delay>\d+)\);.*?)<\/script>.*?<form.+?action=""(?<action>\S+?)"".*?>.*?name=""s"" value=""(?<s>\S+)"".*?name=""jschl_vc"" value=""(?<jschl_vc>[a-z0-9]{32})"".*?name=""pass"" value=""(?<pass>\S+?)"".*?id=""cf-dn-\S+"">(?<cf_dn>.*?)<\/div>", RegexOptions.Singleline | RegexOptions.Compiled);

        public JsScript Script { get; set; }
        public JsForm Form { get; set; }
        public string Cfdn { get; set; }
        public Uri SiteUrl { get; set; }

        public static JsChallenge Parse(string html, [Optional]Uri siteUrl)
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
                    Calculations = GetCalculations(challengeMatch, siteUrl)
                        .Prepend(new NormalCalculation($"{challengeMatch.Groups["className"].Value}.{challengeMatch.Groups["propName"].Value}={challengeMatch.Groups["propValue"].Value};")),
                    P = challengeMatch.Groups["p"].Value,
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
                Cfdn = challengeMatch.Groups["cf_dn"].Value,
                SiteUrl = siteUrl
            };
        }

        private static IEnumerable<IJsCalculation> GetCalculations(Match challengeMatch, Uri siteUrl)
        {
            var normalCaptures = challengeMatch.Groups["normal"].Captures.Cast<Capture>();
            var charCodeCaptures = challengeMatch.Groups["charCode"].Captures.Cast<Capture>();
            var operatorCaptures = challengeMatch.Groups["operator"].Captures.Cast<Capture>();
            var p = challengeMatch.Groups["p"].Value;
            var cfdn = challengeMatch.Groups["cf_dn"].Value;

            return challengeMatch.Groups["calculations"].Captures.Cast<Capture>()
                .Select((capture, index) => GetCalculation(capture, index, normalCaptures, charCodeCaptures, operatorCaptures, p, cfdn, siteUrl));
        }

        private static IJsCalculation GetCalculation(Capture capture, int index, IEnumerable<Capture> normalCaptures, IEnumerable<Capture> charCodeCaptures, IEnumerable<Capture> operatorCaptures, string p, string cfdn, Uri siteUrl)
        {
            var type = GetCalculationType(capture, normalCaptures, charCodeCaptures);

            if (type.Equals(CalculationType.Normal))
            {
                return new NormalCalculation(capture.Value);
            }
            else if (type.Equals(CalculationType.CharCode))
            {
                return new CharCodeCalculation(capture.Value, siteUrl);
            }
            else
            {
                return new CfdnCalculation(capture.Value, cfdn);
            }
        }

        private static CalculationType GetCalculationType(Capture capture, IEnumerable<Capture> normalCaptures, IEnumerable<Capture> charCodeCaptures)
        {
            return normalCaptures.Any(Equals(capture)) ? CalculationType.Normal :
                (charCodeCaptures.Any(Equals(capture)) ?
                CalculationType.CharCode : CalculationType.Cfdn);
        }

        private static Func<Capture, bool> Equals(Capture capture) => cap => capture.Value.Contains(cap.Value);


        public double Solve() =>
            Math.Round(Script.Calculations.Aggregate(0d, ApplyCalculation), Script.Round) + (Script.IsHostLength ? SiteUrl.Host.Length : 0);

        private static double ApplyCalculation(double number, IJsCalculation calculation)
        {
            switch (calculation.Operator)
            {
                case "":
                    return calculation.Solve();
                case "+":
                    return number + calculation.Solve();
                case "-":
                    return number - calculation.Solve();
                case "*":
                    return number * calculation.Solve();
                case "/":
                    return number / calculation.Solve();
                default:
                    throw new ArgumentOutOfRangeException($"Unknown operator: {calculation.Operator}");
            }
        }

    }
}
