using System.Net;

namespace CloudflareSolverRe.Extensions
{
    internal static class CookieExtensions
    {
        public static string ToHeaderValue(this Cookie cookie) => $"{cookie.Name}={cookie.Value}";
        
    }
}