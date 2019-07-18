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

            ClearanceHandlerSample.Sample();

            CloudflareSolverSample.Sample();
            
            //var uri = new Uri("https://www.japscan.to");
            //var uri = new Uri("https://www.spacetorrent.cloud/");
            //var uri = new Uri("https://hdmovie8.com");
            //var uri = new Uri("https://github.com");
            //var uri = new Uri("https://www.mkvcage.ws/");
            //var uri = new Uri("https://codepen.io/");
            //var uri = new Uri("https://uam.hitmehard.fun/HIT");
            
        }
    }
}
