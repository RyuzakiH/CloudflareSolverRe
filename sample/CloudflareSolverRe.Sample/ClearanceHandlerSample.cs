using CloudflareSolverRe.CaptchaProviders;
using CloudflareSolverRe.Exceptions;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Sample
{
    public class ClearanceHandlerSample
    {

        public static void Sample()
        {
            var target = new Uri("https://uam.hitmehard.fun/HIT");

            var handler = new ClearanceHandler
            {
                MaxTries = 3, // Default value is 3
                ClearanceDelay = 3000 // Default value is the delay time determined in challenge code
            };

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

            try
            {
                var content = client.GetStringAsync(target).Result;
                Console.WriteLine(content);
            }
            catch (AggregateException ex) when (ex.InnerException is CloudflareClearanceException)
            {
                // After all retries, clearance still failed.
                Console.WriteLine(ex.InnerException.Message);
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                // Looks like we ran into a timeout. Too many clearance attempts?
                // Maybe you should increase client.Timeout as each attempt will take about five seconds.
            }
        }


        public static void Sample_2Captcha()
        {
            var target = new Uri("https://uam.hitmehard.fun/HIT");

            var handler = new ClearanceHandler(new TwoCaptchaProvider("YOUR_API_KEY"))
            {
                MaxTries = 3, // Default value is 3
                MaxCaptchaTries = 2, // Default value is 1
                //ClearanceDelay = 3000  // Default value is the delay time determined in challenge code (not required in captcha)
            };
            
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

            try
            {
                var content = client.GetStringAsync(target).Result;
                Console.WriteLine(content);
            }
            catch (AggregateException ex) when (ex.InnerException is CloudflareClearanceException)
            {
                // After all retries, clearance still failed.
                Console.WriteLine(ex.InnerException.Message);
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                // Looks like we ran into a timeout. Too many clearance attempts?
                // Maybe you should increase client.Timeout as each attempt will take about five seconds.
            }
        }


        public static void Sample_AntiCaptcha()
        {
            var target = new Uri("https://uam.hitmehard.fun/HIT");

            var handler = new ClearanceHandler(new AntiCaptchaProvider("YOUR_API_KEY"))
            {
                MaxTries = 3, // Default value is 3
                MaxCaptchaTries = 2, // Default value is 1
                //ClearanceDelay = 3000  // Default value is the delay time determined in challenge code (not required in captcha)
            };

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

            try
            {
                var content = client.GetStringAsync(target).Result;
                Console.WriteLine(content);
            }
            catch (AggregateException ex) when (ex.InnerException is CloudflareClearanceException)
            {
                // After all retries, clearance still failed.
                Console.WriteLine(ex.InnerException.Message);
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                // Looks like we ran into a timeout. Too many clearance attempts?
                // Maybe you should increase client.Timeout as each attempt will take about five seconds.
            }
        }

    }
}
