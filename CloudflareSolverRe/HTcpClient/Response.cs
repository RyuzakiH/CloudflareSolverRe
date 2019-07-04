using System;
using System.Net;
using System.Text.RegularExpressions;

namespace HTcp
{
    public class Response
    {
        public HttpStatusCode StatusCode { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public string Body { get; set; }
        public Version ProtocolVersion { get; set; }
        public CookieContainer Cookies { get; set; }

        public Response()
        {
            Headers = new WebHeaderCollection();
            Cookies = new CookieContainer();
        }

        public static Response Parse(string raw_response)
        {
            var match = Regex.Match(raw_response, @"HTTP/(?<protocolVersion>\d\.\d)\s(?<response_code>.*?)\s(?<message>.*?)(\r\n|\n)((?<header_name>.*?):\s(?<header_value>.*?)(\r\n|\n))*(\r\n|\n)(?<body>(.|\r\n|\n)*)");

            var response = new Response();

            response.StatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), match.Groups["response_code"].Value);

            response.ProtocolVersion = Version.Parse(match.Groups["protocolVersion"].Value);

            response.Headers = new WebHeaderCollection();
            for (int i = 0; i < match.Groups["header_name"].Captures.Count; i++)
                response.Headers.Add(match.Groups["header_name"].Captures[i].Value, match.Groups["header_value"].Captures[i].Value);

            response.Body = match.Groups["body"].Value;

            return response;
        }


        
    }
}