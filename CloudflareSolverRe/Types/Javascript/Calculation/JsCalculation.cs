namespace CloudflareSolverRe.Types.Javascript
{
    public class JsCalculation : IJsCalculation
    {
        public CalculationType Type { get; protected set; }
        public string Operator { get; protected set; }
        public string First { get; protected set; }
        public string Second { get; protected set; }
        public string Value { get; protected set; }
        public double Result { get => Solve(); }

        public virtual double Solve() => JsFuck.DecodeNumber(First) / JsFuck.DecodeNumber(Second);
    }
}