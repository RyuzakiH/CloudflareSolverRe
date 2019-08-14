using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        private static readonly Uri uamhitmehardfunUri = new Uri("https://uam.hitmehard.fun/HIT");

        [TestMethod]
        public async Task SolveWebsiteChallenge_uamhitmehardfun_WebClient()
        {
            var cf = new CloudflareSolver
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var result = await cf.Solve(uamhitmehardfunUri);

            Assert.IsTrue(result.Success);

            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Cookie, result.Cookies.AsHeaderString());
            client.Headers.Add(HttpRequestHeader.UserAgent, result.UserAgent);

            var content = await client.DownloadStringTaskAsync(uamhitmehardfunUri);

            Assert.AreEqual("Dstat.cc is the best", content);
        }

        [TestMethod]
        public async Task SolveWebsiteChallenge_uamhitmehardfun_HttpWebRequest()
        {
            var cf = new CloudflareSolver
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var result = await cf.Solve(uamhitmehardfunUri);

            Assert.IsTrue(result.Success);

            var request = (HttpWebRequest)WebRequest.Create(uamhitmehardfunUri);
            request.Headers.Add(HttpRequestHeader.Cookie, result.Cookies.AsHeaderString());
            request.Headers.Add(HttpRequestHeader.UserAgent, result.UserAgent);

            var response = (HttpWebResponse)await request.GetResponseAsync();
            var content = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();

            Assert.AreEqual("Dstat.cc is the best", content);
        }
    }
}