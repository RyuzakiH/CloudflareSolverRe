namespace CloudflareSolverRe.Types
{
    internal interface ICloudflareSolver : IClearanceDelayable, IRetriable
    {
        //int MaxJavascriptTries { get; set; }
        int MaxCaptchaTries { get; set; }
    }
}
