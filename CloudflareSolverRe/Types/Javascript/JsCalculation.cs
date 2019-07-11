using System;
using System.Text.RegularExpressions;

namespace CloudflareSolverRe.Types.Javascript
{
    public interface IJsCalculation
    {
        CalculationType Type { get; }
        string Operator { get; }
        string First { get; }
        string Second { get; }
        string Value { get; }
        double Result { get; }
        string ToCode();
        double Solve();
    }

    public class JsCalculation : IJsCalculation
    {
        public CalculationType Type { get; protected set; }
        public string Operator { get; protected set; }
        public string First { get; protected set; }
        public string Second { get; protected set; }
        public string Value { get; protected set; }
        public double Result { get; protected set; }

        public virtual string ToCode() => Value;
        public virtual double Solve() => JsFuck.DecodeNumber(First) / JsFuck.DecodeNumber(Second);
    }


    public enum CalculationType
    {
        Normal,
        CharCode,
        Cfdn
    }

    public class NormalCalculation : JsCalculation, IJsCalculation
    {
        private static readonly Regex NormalCalculationRegex = new Regex(@"\s*?\w+?\.\w+?(?<operator>[+\-*\/]{0,1})=(?<normal>(?<first>(?:\+|\(|\)|\!|\[|\])+?)/(?<second>(?:\+|\(|\)|\!|\[|\])+?);)", RegexOptions.Singleline | RegexOptions.Compiled);
        
        public NormalCalculation(string calculation)
        {
            Type = CalculationType.Normal;
            Value = calculation;

            ExtractCalculationParts(calculation);
        }

        private void ExtractCalculationParts(string calculation)
        {
            var match = NormalCalculationRegex.Match(calculation);

            First = match.Groups["first"].Value;
            Second = match.Groups["second"].Value;
            Operator = match.Groups["operator"].Value;
        }

    }

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

        public new string ToCode() => Value.Substring(0, Value.IndexOf("(function", StringComparison.Ordinal))
                + $"'{SiteUrl.Host}'.charCodeAt({P})" + ");";

        public new double Solve() => JsFuck.DecodeNumber(First) / (JsFuck.DecodeNumber(second1) + charCode);
    }

    public class CfdnCalculation : JsCalculation, IJsCalculation
    {
        private static readonly Regex CharCodeCalculationRegex = new Regex(@"\s*?\w+?\.\w+?(?<operator>[+\-*\/])=(?<cfdn>function\(.\)\{var.*?;\s.*?;)", RegexOptions.Singleline | RegexOptions.Compiled);

        public new double Result { get => Solve(); }

        public string Cfdn { get; set; }

        public CfdnCalculation(string calculation, string cfdn)
        {
            Type = CalculationType.Cfdn;
            Value = calculation;
            Cfdn = cfdn;
            First = Cfdn;
            Operator = CharCodeCalculationRegex.Match(calculation).Groups["operator"].Value;
        }

        public new string ToCode() => Value.Substring(0, Value.IndexOf("function", StringComparison.Ordinal)) + Cfdn + ";";

        public new double Solve() => new NormalCalculation($"test.temp{Operator}={Cfdn};").Solve(); //JsFuck.DecodeNumber(First);
    }
}