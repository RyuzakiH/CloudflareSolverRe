using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace CloudflareSolverRe.Extensions
{
    internal static class CookieExtensions
    {
        public static string ToHeaderValue(this Cookie cookie) => $"{cookie.Name}={cookie.Value}";

        public static IEnumerable<Cookie> GetCookiesByName(this CookieContainer container, Uri uri, params string[] names) =>
            container.GetCookies(uri).Cast<Cookie>().Where(c => names.Contains(c.Name)).ToList();
    }
}