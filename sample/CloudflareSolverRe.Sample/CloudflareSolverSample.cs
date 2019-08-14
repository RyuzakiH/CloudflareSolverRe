using CloudflareSolverRe.CaptchaProviders;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Sample
{
    public class CloudflareSolverSample
    {
        private static readonly Uri target = new Uri("https://uam.hitmehard.fun/HIT");

        public static async Task Sample()
        {
            var cf = new CloudflareSolver
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);

            var result = await cf.Solve(client, handler, target);

            if (!result.Success)
            {
                Console.WriteLine($"[Failed] Details: {result.FailReason}");
                return;
            }

            // Once the protection has been bypassed we can use that HttpClient to send the requests as usual
            var content = await client.GetStringAsync(target);
            Console.WriteLine($"Server response: {content}");
        }

        public async static Task Sample_2Captcha()
        {
            var cf = new CloudflareSolver(new TwoCaptchaProvider("YOUR_API_KEY"))
            {
                MaxTries = 5,
                MaxCaptchaTries = 2
            };

            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);

            var result = await cf.Solve(client, handler, target);

            if (!result.Success)
            {
                Console.WriteLine($"[Failed] Details: {result.FailReason}");
                return;
            }

            // Once the protection has been bypassed we can use that httpClient to send the requests as usual
            var content = await client.GetStringAsync(target);
            Console.WriteLine($"Server response: {content}");
        }

        public async static Task Sample_AntiCaptcha()
        {
            var cf = new CloudflareSolver(new AntiCaptchaProvider("YOUR_API_KEY"))
            {
                MaxTries = 5,
                MaxCaptchaTries = 2
            };

            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);

            var result = await cf.Solve(client, handler, target);

            if (!result.Success)
            {
                Console.WriteLine($"[Failed] Details: {result.FailReason}");
                return;
            }

            // Once the protection has been bypassed we can use that httpClient to send the requests as usual
            var content = await client.GetStringAsync(target);
            Console.WriteLine($"Server response: {content}");
        }
    }
}