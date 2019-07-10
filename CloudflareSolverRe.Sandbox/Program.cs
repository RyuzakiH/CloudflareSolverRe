using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using CloudflareSolverRe.CaptchaProviders;

namespace CloudflareSolverRe.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            /* Example use with captcha provider:
             * var cf = new CloudflareSolver(new TwoCaptchaProvider("YOUR_API_KEY"));
             * var cf = new CloudflareSolver(new AntiCaptchaProvider("YOUR_API_KEY"));
             */
            //var socketsHttpHandler = new SocketsHttpHandler();

            

            var cf = new CloudflareSolver();
            //CookieContainer cookies = new CookieContainer();
            var httpClientHandler = new HttpClientHandler
            {
                //CookieContainer = cookies,
                //AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };
            var httpClient = new HttpClient(httpClientHandler);

            //var uri = new Uri("https://www.japscan.to");
            //var uri = new Uri("https://www.spacetorrent.cloud/");
            //var uri = new Uri("http://hdmovie8.com");
            //var uri = new Uri("https://github.com");
            //var uri = new Uri("https://www.mkvcage.ws/");
            //var uri = new Uri("http://codepen.io/");
            var uri = new Uri("https://uam.hitmehard.fun/HIT");

            var result = cf.Solve(httpClient, httpClientHandler, uri, 3).Result;
            if (result.Success)
            {
                Console.WriteLine($"[Success] Protection bypassed: {result.DetectResult.Protection}");
            }
            else
            {
                Console.WriteLine($"[Failed] Details: {result.FailReason}");
                return;
            }

            //uri = new Uri("https://hdmovie8.com/movies/young-sister-3/");

            // Once the protection has been bypassed we can use that httpClient to send the requests as usual
            var response = httpClient.GetAsync(uri).Result;
            var html = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"Server response: {html}");
        }
    }
}
