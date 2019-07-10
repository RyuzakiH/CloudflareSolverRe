using System;
using System.Collections.Generic;
using System.Text;

namespace CloudflareSolverRe.Extensions
{
    public static class UriExtensions
    {
        public static Uri ForceHttp(this Uri uri)
        {
            var newUri = new UriBuilder(uri);

            var hadDefaultPort = newUri.Uri.IsDefaultPort;
            newUri.Scheme = "http";
            newUri.Port = hadDefaultPort ? -1 : newUri.Port;

            return newUri.Uri;
        }

        public static Uri ForceHttps(this Uri uri)
        {
            var newUri = new UriBuilder(uri);

            var hadDefaultPort = newUri.Uri.IsDefaultPort;
            newUri.Scheme = "https";
            newUri.Port = hadDefaultPort ? -1 : newUri.Port;

            return newUri.Uri;
        }
    }
}
