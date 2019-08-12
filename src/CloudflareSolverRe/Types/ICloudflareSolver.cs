namespace CloudflareSolverRe.Types
{
    internal interface ICloudflareSolver
    {
        //int MaxJavascriptTries { get; set; }
        int MaxTries { get; set; }
        int MaxCaptchaTries { get; set; }
        int ClearanceDelay { get; set; }
    }
}