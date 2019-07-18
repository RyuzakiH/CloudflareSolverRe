using CloudflareSolverRe.CaptchaProviders;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace CloudflareSolverRe.Sample
{
    public class CloudflareSolverSample
    {

        public static void Sample()
        {
            var target = new Uri("https://uam.hitmehard.fun/HIT");

            var cf = new CloudflareSolver
            {
                MaxTries = 3, // Default value is 3
                ClearanceDelay = 3000  // Default value is the delay time determined in challenge code
            };

            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

            var result = cf.Solve(httpClient, httpClientHandler, target).Result;
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
            var response = httpClient.GetAsync(target).Result;
            var html = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"Server response: {html}");
        }


        public static void Sample_2Captcha()
        {
            var target = new Uri("https://uam.hitmehard.fun/HIT");

            var cf = new CloudflareSolver(new TwoCaptchaProvider("YOUR_API_KEY"))
            {
                MaxTries = 3, // Default value is 3
                MaxCaptchaTries = 1, // Default value is 1
                //ClearanceDelay = 3000  // Default value is the delay time determined in challenge code (not required in captcha)
            };

            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

            var result = cf.Solve(httpClient, httpClientHandler, target).Result;
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
            var response = httpClient.GetAsync(target).Result;
            var html = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"Server response: {html}");
        }


        public static void Sample_AntiCaptcha()
        {
            var target = new Uri("https://uam.hitmehard.fun/HIT");

            var cf = new CloudflareSolver(new AntiCaptchaProvider("YOUR_API_KEY"))
            {
                MaxTries = 3, // Default value is 3
                MaxCaptchaTries = 1, // Default value is 1
                //ClearanceDelay = 3000  // Default value is the delay time determined in challenge code (not required in captcha)
            };

            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

            var result = cf.Solve(httpClient, httpClientHandler, target).Result;
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
            var response = httpClient.GetAsync(target).Result;
            var html = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"Server response: {html}");
        }
    }
}
