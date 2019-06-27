using System;

namespace Cloudflare.Exceptions
{
    public class CloudflareException : Exception
    {
        public CloudflareException(string message) : base(message)
        { }
    }
}
