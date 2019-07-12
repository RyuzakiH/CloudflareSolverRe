using System;
using System.Text.RegularExpressions;

namespace CloudflareSolverRe.Types.Javascript
{
    public class CharCodeCalculation : JsCalculation, IJsCalculation
    {
        private static readonly Regex CharCodeCalculationRegex = new Regex(@"\s*?\w+?\.\w+?(?<operator>[+\-*\/])=(?<charCode>(?<first>(?:\+|\(|\)|\!|\[|\])+?)/(?<second>\(\+\((?<second1>(?:\+|\(|\)|\!|\[|\])+)\)\+(?<second2>\(function.*?}\((?<p>.*?)\)\)\)));)", RegexOptions.Singleline | RegexOptions.Compiled);

        public new double Result { get => Solve(); }

        public string P { get; set; }
        public Uri SiteUrl { get; set; }

        private string second1;
        private int charCode;

        public CharCodeCalculation(string calculation, Uri siteUrl)
        {
            Type = CalculationType.CharCode;
            Value = calculation;
            SiteUrl = siteUrl;

            ExtractCalculationParts(calculation);
        }

        private void ExtractCalculationParts(string calculation)
        {
            var match = CharCodeCalculationRegex.Match(calculation);

            P = match.Groups["p"].Value;

            First = match.Groups["first"].Value;

            //Second = match.Groups["second1"].Value + $"{JsFuck.EncodeNumber(SiteUrl.Host[(int)JsFuck.DecodeNumber(P)])})";
            Second = "(" + match.Groups["second1"].Value + $"+{(int)SiteUrl.Host[(int)JsFuck.DecodeNumber(P)]})";

            second1 = match.Groups["second1"].Value;
            charCode = SiteUrl.Host[(int)JsFuck.DecodeNumber(P)];

            Operator = match.Groups["operator"].Value;
        }

        public new double Solve() => JsFuck.DecodeNumber(First) / (JsFuck.DecodeNumber(second1) + charCode);
    }
}