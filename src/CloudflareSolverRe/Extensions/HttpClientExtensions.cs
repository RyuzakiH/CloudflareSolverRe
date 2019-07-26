using System.Net.Http;
using System.Runtime.InteropServices;

namespace CloudflareSolverRe.Extensions
{
    internal static class HttpClientExtensions
    {
        internal static HttpClient Clone(this HttpClient httpClient)
        {
            return httpClient.Clone(null, null);
        }

        internal static HttpClient Clone(this HttpClient httpClient, HttpMessageHandler handler)
        {
            return httpClient.Clone(handler, null);
        }

        internal static HttpClient Clone(this HttpClient httpClient, HttpMessageHandler handler, bool disposeHandler)
        {
            return httpClient.Clone(handler, disposeHandler);
        }

        private static HttpClient Clone(this HttpClient httpClient, [Optional]HttpMessageHandler handler, [Optional]bool? disposeHandler)
        {
            HttpClient client;

            if (handler != null && disposeHandler.HasValue)
                client = new HttpClient(handler, disposeHandler.Value);
            else if (handler != null)
                client = new HttpClient(handler);
            else
                client = new HttpClient();

            client.BaseAddress = httpClient.BaseAddress;
            client.MaxResponseContentBufferSize = httpClient.MaxResponseContentBufferSize;
            client.Timeout = httpClient.Timeout;

            //client.DefaultRequestHeaders.ToList().ForEach(header => cc.DefaultRequestHeaders.Add(header.Key, header.Value));
            foreach (var header in httpClient.DefaultRequestHeaders)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);

            return client;
        }
    }
}
