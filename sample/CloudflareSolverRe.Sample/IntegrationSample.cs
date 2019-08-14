using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Sample
{
    public class IntegrationSample
    {
        private static readonly Uri target = new Uri("https://uam.hitmehard.fun/HIT");

        public static async Task WebClientSample()
        {
            var cf = new CloudflareSolver
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var result = await cf.Solve(target);

            if (!result.Success)
            {
                Console.WriteLine($"[Failed] Details: {result.FailReason}");
                return;
            }

            // Add session cookies, user-agent and proxy (if used) to the WebClient headers
            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Cookie, result.Cookies.AsHeaderString());
            client.Headers.Add(HttpRequestHeader.UserAgent, result.UserAgent);

            // Once the protection has been bypassed we can use that WebClient to send the requests as usual
            var content = await client.DownloadStringTaskAsync(target);
            Console.WriteLine($"Server response: {content}");
        }

        public static async Task HttpWebRequestSample()
        {
            var cf = new CloudflareSolver
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var result = await cf.Solve(target);

            if (!result.Success)
            {
                Console.WriteLine($"[Failed] Details: {result.FailReason}");
                return;
            }

            // Add session cookies, user-agent and proxy (if used) to the HttpWebRequest headers
            var request = (HttpWebRequest)WebRequest.Create(target);
            request.Headers.Add(HttpRequestHeader.Cookie, result.Cookies.AsHeaderString());
            request.Headers.Add(HttpRequestHeader.UserAgent, result.UserAgent);

            // Once the protection has been bypassed we can use that HttpWebRequest to send the requests as usual
            var response = (HttpWebResponse)await request.GetResponseAsync();
            var content = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
            Console.WriteLine($"Server response: {content}");
        }        
    }
}
