namespace CloudflareSolverRe.Types
{
    internal interface ICloudflareSolver : IClearanceDelayable
    {
        //int MaxJavascriptTries { get; set; }
        int MaxTries { get; set; }
        int MaxCaptchaTries { get; set; }
    }
}
