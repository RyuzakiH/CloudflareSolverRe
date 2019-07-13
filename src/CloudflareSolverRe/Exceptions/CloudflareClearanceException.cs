using System;
using System.Net.Http;

namespace CloudflareSolverRe.Exceptions
{
    /// <summary>
    /// The exception that is thrown if CloudFlare clearance failed after the declared number of attempts.
    /// </summary>
    public class CloudFlareClearanceException : HttpRequestException
    {
        public CloudFlareClearanceException(int attempts) : this(attempts, $"Clearance failed after {attempts} attempt(s).") { }

        public CloudFlareClearanceException(int attempts, string message) : base(message)
        {
            Attempts = attempts;
        }

        public CloudFlareClearanceException(int attempts, string message, Exception inner) : base(message, inner)
        {
            Attempts = attempts;
        }

        /// <summary>
        /// Returns the number of failed clearance attempts.
        /// </summary>
        public int Attempts { get; }
    }
}