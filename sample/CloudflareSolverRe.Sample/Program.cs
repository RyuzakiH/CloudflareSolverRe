using System.Threading.Tasks;

namespace CloudflareSolverRe.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            ClearanceHandlerSample.Sample().Wait();

            Task.Delay(5000).Wait();

            CloudflareSolverSample.Sample().Wait();

            Task.Delay(5000).Wait();

            IntegrationSample.WebClientSample().Wait();

            Task.Delay(5000).Wait();

            IntegrationSample.HttpWebRequestSample().Wait();
        }
    }
}