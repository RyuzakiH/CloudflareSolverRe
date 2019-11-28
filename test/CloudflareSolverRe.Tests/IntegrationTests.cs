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
        private static readonly Uri soundparkUri = new Uri("https://sound-park.world/");

        [TestMethod]
        public async Task SolveWebsiteChallenge_soundpark_WebClient()
        {
            var cf = new CloudflareSolver
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var result = await cf.Solve(soundparkUri);

            Assert.IsTrue(result.Success);

            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Cookie, result.Cookies.AsHeaderString());
            client.Headers.Add(HttpRequestHeader.UserAgent, result.UserAgent);

            var content = await client.DownloadStringTaskAsync(soundparkUri);

            Assert.IsTrue(content.Contains("Music Torrent Tracker"));
        }

        [TestMethod]
        public async Task SolveWebsiteChallenge_soundpark_HttpWebRequest()
        {
            var cf = new CloudflareSolver
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var result = await cf.Solve(soundparkUri);

            Assert.IsTrue(result.Success);

            var request = (HttpWebRequest)WebRequest.Create(soundparkUri);
            request.Headers.Add(HttpRequestHeader.Cookie, result.Cookies.AsHeaderString());
            request.Headers.Add(HttpRequestHeader.UserAgent, result.UserAgent);

            var response = (HttpWebResponse)await request.GetResponseAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}