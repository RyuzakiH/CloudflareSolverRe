using CloudflareSolverRe.Constants;
using System;

namespace CloudflareSolverRe.Extensions
{
    internal static class UriExtensions
    {
        public static Uri ForceHttp(this Uri uri)
        {
            var newUri = new UriBuilder(uri);

            var hadDefaultPort = newUri.Uri.IsDefaultPort;
            newUri.Scheme = General.UriSchemeHttp;
            newUri.Port = hadDefaultPort ? -1 : newUri.Port;

            return newUri.Uri;
        }

        public static Uri ForceHttps(this Uri uri)
        {
            var newUri = new UriBuilder(uri);

            var hadDefaultPort = newUri.Uri.IsDefaultPort;
            newUri.Scheme = General.UriSchemeHttps;
            newUri.Port = hadDefaultPort ? -1 : newUri.Port;

            return newUri.Uri;
        }
    }
}
