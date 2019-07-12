using System;
using System.Text.RegularExpressions;

namespace CloudflareSolverRe.Types.Javascript
{
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