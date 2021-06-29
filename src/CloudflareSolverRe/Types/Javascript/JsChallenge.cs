using CloudflareSolverRe.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CloudflareSolverRe.Types.Javascript
{
    public class JsChallenge
    {
        private static readonly Regex JsChallengeRegex = new Regex(@"<script.*?>(?<script>.*?var s,t,o,p,b,r,e,a,k,i,n,g,\w, (?<className>\w+?)={""(?<propName>\w+?)"":(?<propValue>.*?)};.*?(?<calculations>\s*?\w+?\.\w+?[+\-*\/]=(?:(?<normal>(?:\+|\(|\)|\!|\[|\]|\/)+?;)|(?<charCode>(?:\+|\(|\)|\!|\[|\]|\/)+?\(function.*?}\(.*?\)\)\);)|(?<cfdn>function\(.\)\{var.*?;\s.*?;)))+.*?a\.value\s=\s\(\+\w+\.\w+(\s\+\s(?<addHostLength>t\.length))*?\)\.toFixed\((?<round>\d+)\);.*?},\s*(?<delay>\d+)\);.*?)<\/script>.*?<form.+?action=""(?<action>\S+?)"".*?>.*?name=""r"" value=""(?<r>\S+)"".*?name=""jschl_vc"" value=""(?<jschl_vc>[a-z0-9]{32})"".*?name=""pass"" value=""(?<pass>\S+?)"".*?</form>.*?(id=""cf-dn-\S+"">(?<cf_dn>.*?)</div>\s+<div.*?){0,1}\s+</div>", RegexOptions.Singleline/* | RegexOptions.Compiled*/);

        public JsScript Script { get; set; }
        public JsForm Form { get; set; }
        public string Cfdn { get; set; }
        public Uri SiteUrl { get; set; }


        public double Solve() =>
            Math.Round(Script.Calculations.Aggregate(0d, ApplyCalculation), Script.Round) + (Script.IsHostLength ? SiteUrl.Host.Length : 0);

        private static double ApplyCalculation(double number, IJsCalculation calculation)
        {
            switch (calculation.Operator)
            {
                case "":
                    return calculation.Result;
                case "+":
                    return number + calculation.Result;
                case "-":
                    return number - calculation.Result;
                case "*":
                    return number * calculation.Result;
                case "/":
                    return number / calculation.Result;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown operator: {calculation.Operator}");
            }
        }


        public static JsChallenge Parse(string html, Uri siteUrl)
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
                        .eePrepend(new NormalCalculation($"{challengeMatch.Groups["className"].Value}.{challengeMatch.Groups["propName"].Value}={challengeMatch.Groups["propValue"].Value};")),
                    IsHostLength = challengeMatch.Groups["addHostLength"].Success,
                    Round = int.Parse(challengeMatch.Groups["round"].Value),
                    Delay = int.Parse(challengeMatch.Groups["delay"].Value),
                },
                Form = new JsForm
                {
                    Action = System.Net.WebUtility.HtmlDecode(challengeMatch.Groups["action"].Value),
                    R = challengeMatch.Groups["r"].Value,
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
            var cfdn = challengeMatch.Groups["cf_dn"].Value;

            return challengeMatch.Groups["calculations"].Captures.Cast<Capture>()
                .Select(capture => GetCalculation(capture, normalCaptures, charCodeCaptures, cfdn, siteUrl));
        }

        private static IJsCalculation GetCalculation(Capture capture, IEnumerable<Capture> normalCaptures, IEnumerable<Capture> charCodeCaptures, string cfdn, Uri siteUrl)
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

    }
}
