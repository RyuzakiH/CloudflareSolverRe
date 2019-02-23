using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cloudflare.Test
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void Relay()
        {
            var cf = new CloudflareSolver();

            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler);
            var uri = new Uri("https://cf.zaczero.pl/");
            
            var result = cf.Solve(httpClient, httpClientHandler, uri).Result;
            Assert.IsTrue(result.Success);

            var response = httpClient.GetAsync(uri).Result;
            var html = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("OK", html);
        }

        [TestMethod]
        public void JavaScript()
        {
            var cf = new CloudflareSolver();

            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler);
            var uri = new Uri("https://uam.zaczero.pl/");
            
            var result = cf.Solve(httpClient, httpClientHandler, uri).Result;
            Assert.IsTrue(result.Success);

            var response = httpClient.GetAsync(uri).Result;
            var html = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("OK", html);
        }
    }
}
