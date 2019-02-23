using System;
using System.Net.Http;

namespace Cloudflare.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            ////
            // If you do not want to use the ReCaptchaV2 solver simply remove the parameter
            ////
            var cf = new CloudflareSolver("YOUR_2CAPTCHA_KEY");

            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler);
            var uri = new Uri("https://uam.zaczero.pl/");
            
            var result = cf.Solve(httpClient, httpClientHandler, uri).Result;
            if (result.Success)
            {
                Console.WriteLine($"Success! Protection bypassed: {result.DetectResult.Protection}");
            }
            else
            {
                Console.WriteLine($"Fail :( => Reason: {result.FailReason}");
                return;
            }

            ////
            // Once the protection has been bypassed we can use that httpClient to send the requests as usual
            ////
            var response = httpClient.GetAsync(uri).Result;
            var html = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"Real response: {html}");
        }
    }
}
