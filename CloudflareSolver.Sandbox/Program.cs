using System;
using System.Net.Http;

namespace Cloudflare.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            // If you don't want to use the ReCaptchaV2 solver - simply remove the parameter
            var cf = new CloudflareSolver("YOUR_2CAPTCHA_KEY");

            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler);
            
            var result = cf.Solve(httpClient, httpClientHandler, new Uri("https://uam.hitmehard.fun/HIT")).Result;
            if (result.Success)
            {
                Console.WriteLine($"Success! Protection bypassed: {result.DetectResult.Protection}");
            }
            else
            {
                Console.WriteLine($"Fail :( => Reason: {result.FailReason}");
            }
        }
    }
}
