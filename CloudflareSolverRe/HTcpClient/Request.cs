using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
//using System.Web;

namespace HTcp
{
    public class Request
    {
        public HttpMethod Method { get; set; }
        public string Path { get; set; }
        public string Host => Headers.Get("Host");
        public Version ProtocolVersion { get; set; }

        public WebHeaderCollection Headers { get; set; }

        public string Body { get; set; }


        public Request()
        {
            Headers = new WebHeaderCollection();
        }

        public static Request Parse(string raw_request)
        {
            var match = Regex.Match(raw_request, @"(?<method>.*?)\s(?<path>.*?)\sHTTP/(?<protocolVersion>\d\.\d)(\r\n|\n)((?<header_name>.*?):\s(?<header_value>.*?)(\r\n|\n))*(\r\n|\n)(?<body>(.|\r\n|\n)*)");

            var request = new Request();

            request.Method = new HttpMethod(match.Groups["method"].Value);

            request.Path = match.Groups["path"].Value;

            request.ProtocolVersion = Version.Parse(match.Groups["protocolVersion"].Value);

            request.Headers = new WebHeaderCollection();
            for (int i = 0; i < match.Groups["header_name"].Captures.Count; i++)
                request.Headers.Add(match.Groups["header_name"].Captures[i].Value, match.Groups["header_value"].Captures[i].Value);

            request.Body = match.Groups["body"].Value;

            return request;
        }


        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{Method.Method} {Path} HTTP/{ProtocolVersion.ToString()}");

            foreach (string header in Headers)
                sb.AppendLine($"{header}: {Headers[header]}");

            var str = sb.ToString();

            sb.AppendLine();

            if (!string.IsNullOrEmpty(Body))
                sb.Append(Body);

            return sb.ToString();
        }
    }
}
