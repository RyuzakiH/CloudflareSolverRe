namespace CloudflareSolverRe.Types.Javascript
{
    public class JsCalculation
    {
        public JsCalculationType Type { get; set; }

        public string Value { get; set; }

    }

    public enum JsCalculationType
    {
        Normal,
        CharCode,
        Cfdn
    }
}