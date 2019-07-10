using System;

namespace CloudflareSolverRe.Exceptions
{
    public class CloudflareException : Exception
    {
        public CloudflareException(string message) : base(message)
        { }
    }
}
