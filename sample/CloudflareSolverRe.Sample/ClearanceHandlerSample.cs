using CloudflareSolverRe.CaptchaProviders;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Sample
{
    public class ClearanceHandlerSample
    {
        private static readonly Uri target = new Uri("https://uam.hitmehard.fun/HIT");

        public async static Task Sample()
        {
            var handler = new ClearanceHandler
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var client = new HttpClient(handler);

            var content = await client.GetStringAsync(target);
            Console.WriteLine(content);
        }

        public async static void Sample_2Captcha()
        {
            var handler = new ClearanceHandler(new TwoCaptchaProvider("YOUR_API_KEY"))
            {
                MaxTries = 5,
                MaxCaptchaTries = 2
            };

            var client = new HttpClient(handler);

            // You can use the HttpClient to send requests as usual, any challenge will be solved automatically
            var content = await client.GetStringAsync(target);
            Console.WriteLine(content);
        }

        public async static Task Sample_AntiCaptcha()
        {
            var handler = new ClearanceHandler(new AntiCaptchaProvider("YOUR_API_KEY"))
            {
                MaxTries = 5,
                MaxCaptchaTries = 2
            };

            var client = new HttpClient(handler);

            // You can use the HttpClient to send requests as usual, any challenge will be solved automatically
            var content = await client.GetStringAsync(target);
            Console.WriteLine(content);
        }
    }
}