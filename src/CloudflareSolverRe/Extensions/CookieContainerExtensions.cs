using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace CloudflareSolverRe.Extensions
{
    internal static class CookieContainerExtensions
    {
        public static IEnumerable<Cookie> GetCookiesByName(this CookieContainer container, Uri uri, params string[] names) =>
            container.GetCookies(uri).Cast<Cookie>().Where(c => names.Contains(c.Name)).ToList();

        public static Cookie GetCookie(this CookieContainer cookieContainer, Uri uri, string name) =>
            cookieContainer.GetCookiesByName(uri, name).FirstOrDefault();

    }
}
