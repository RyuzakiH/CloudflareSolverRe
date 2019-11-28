using CloudflareSolverRe.CaptchaProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Tests
{
    [TestClass]
    public class CloudflareSolverTests
    {
        private static readonly Uri soundparkUri = new Uri("https://sound-park.world/");

        [TestMethod]
        public async Task SolveWebsiteChallenge_soundpark()
        {
            var cf = new CloudflareSolver
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);

            var result = await cf.Solve(client, handler, soundparkUri);

            Assert.IsTrue(result.Success);

            HttpResponseMessage response = await client.GetAsync(soundparkUri);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task SolveWebsiteChallenge_soundpark_WithAntiCaptcha()
        {
            if (Settings.AntiCaptchaApiKey.Equals("YOUR_API_KEY"))
                return;

            var cf = new CloudflareSolver(new AntiCaptchaProvider(Settings.AntiCaptchaApiKey))
            {
                MaxTries = 2,
                MaxCaptchaTries = 2
            };

            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);

            var result = await cf.Solve(client, handler, soundparkUri);

            Assert.IsTrue(result.Success);

            HttpResponseMessage response = await client.GetAsync(soundparkUri);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task SolveWebsiteChallenge_soundpark_With2Captcha()
        {
            if (Settings.TwoCaptchaApiKey.Equals("YOUR_API_KEY"))
                return;

            var cf = new CloudflareSolver(new TwoCaptchaProvider(Settings.TwoCaptchaApiKey))
            {
                MaxTries = 2,
                MaxCaptchaTries = 2
            };

            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);

            var result = await cf.Solve(client, handler, soundparkUri);

            Assert.IsTrue(result.Success);

            HttpResponseMessage response = await client.GetAsync(soundparkUri);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

    }
}