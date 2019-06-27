using System;
using System.Net;
using System.Net.Http;
using Cloudflare.CaptchaProviders;

namespace Cloudflare.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            /* Example use with captcha provider:
             * var cf = new CloudflareSolver(new TwoCaptchaProvider("YOUR_API_KEY"));
             * var cf = new CloudflareSolver(new AntiCaptchaProvider("YOUR_API_KEY"));
             */
            var cf = new CloudflareSolver();

            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };
            var httpClient = new HttpClient(httpClientHandler);
            var uri = new Uri("https://uam.zaczero.pl/");

            var result = cf.Solve(httpClient, httpClientHandler, uri).Result;
            if (result.Success)
            {
                Console.WriteLine($"[Success] Protection bypassed: {result.DetectResult.Protection}");
            }
            else
            {
                Console.WriteLine($"[Failed] Details: {result.FailReason}");
                return;
            }

            // Once the protection has been bypassed we can use that httpClient to send the requests as usual
            var response = httpClient.GetAsync(uri).Result;
            var html = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"Server response: {html}");
        }
    }
}
