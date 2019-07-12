using System.Text.RegularExpressions;

namespace CloudflareSolverRe.Types.Javascript
{
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
}