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
}