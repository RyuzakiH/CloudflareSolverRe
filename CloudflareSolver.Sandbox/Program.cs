using System;
using System.Diagnostics;
using System.Net.Http;

namespace Cloudflare.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            // If you don't want to use the ReCaptchaV2 solver - simply remove the parameter
            var cf = new CloudflareSolver("YOUR_2CAPTCHA_KEY");

            cf.OnCloudflareSolveStatus += (status, message) =>
            {
                // all details go here
                Debug.WriteLine($"{Enum.GetName(typeof(CloudflareSolveStatus), status)} => {message}");
            };

            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler);

            var isBypassed = cf.Solve(httpClient, httpClientHandler, new Uri("https://uam.hitmehard.fun/HIT")).Result;
            // true = httpClient is now ready to send requests (successfully bypassed)
            // false = check what went wrong in the OnCloudflareSolveStatus event
        }
    }
}
