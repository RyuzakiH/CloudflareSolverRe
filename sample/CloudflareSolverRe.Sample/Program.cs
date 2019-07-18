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
                        
        }
    }
}
