using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CloudflareSolverRe.CaptchaProviders;
using CloudflareSolverRe.Exceptions;

namespace CloudflareSolverRe.Sample
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


            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://uam.hitmehard.fun/HIT");
            //request.Headers.Host = "uam.hitmehard.fun";
            //request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");
            //request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            //request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.5");
            //request.Headers.Connection.ParseAdd("keep-alive");
            //request.Headers.Add("Upgrade-Insecure-Requests", "1");

            //var handlerss = new HttpClientHandler();
            //var pp = new HttpClient(handlerss).SendAsync(request).Result;
            //var oo = pp.Content.ReadAsStringAsync().Result;


            var target = new Uri("https://uam.hitmehard.fun/HIT");
            //var target = new Uri("https://www.spacetorrent.cloud/");

            var handler = new ClearanceHandler
            {
                //MaxRetries = 3,
                //ClearanceDelay = 3000
            };

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");
            
            try
            {
                //var content = client.GetStringAsync(target).Result;
                //Console.WriteLine(content);
            }
            catch (AggregateException ex) when (ex.InnerException is CloudFlareClearanceException)
            {
                // After all retries, clearance still failed.
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                // Looks like we ran into a timeout. Too many clearance attempts?
                // Maybe you should increase client.Timeout as each attempt will take about five seconds.
            }



            var cf = new CloudflareSolver()
            {
                MaxRetries = 1
            };
            //CookieContainer cookies = new CookieContainer();
            var httpClientHandler = new HttpClientHandler
            {
                //CookieContainer = cookies,
                //AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };
            var httpClient = new HttpClient(httpClientHandler);

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.5");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");
            httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            httpClient.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");

            //var uri = new Uri("https://www.japscan.to");
            //var uri = new Uri("https://www.spacetorrent.cloud/");
            var uri = new Uri("https://hdmovie8.com");
            //var uri = new Uri("https://github.com");
            //var uri = new Uri("https://www.mkvcage.ws/");
            //var uri = new Uri("http://codepen.io/");
            //var uri = new Uri("https://uam.hitmehard.fun/HIT");
            
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
