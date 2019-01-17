using System;

namespace Cloudflare
{
    public class CloudflareException : Exception
    {
        public CloudflareException(string message) : base(message)
        { }
    }
}
