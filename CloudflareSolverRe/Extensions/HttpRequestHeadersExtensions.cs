using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace Cloudflare.Extensions
{
    public static class HttpRequestHeadersExtensions
    {
        public static void AddWithoutValidation(this HttpRequestHeaders headers, string name, string value)
        {
            if (name == "Accept")
            {
                headers.Add(name, "text/html");

                var mtq = headers.Accept.LastOrDefault();
                var _mediaType = mtq?.GetType().GetTypeInfo().BaseType.GetField("_mediaType", BindingFlags.NonPublic | BindingFlags.Instance);
                _mediaType.SetValue(mtq, value);
                mtq.Parameters.Clear();
            }
            else if (name == "Accept-Language")
            {
                headers.Add(name, "en-US");

                var sq = headers.AcceptLanguage.LastOrDefault();
                var _value = sq.GetType().GetTypeInfo().GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance);
                _value.SetValue(sq, value);
            }
            else
            {
                headers.Add(name, value);
            }
        }



    }
}
