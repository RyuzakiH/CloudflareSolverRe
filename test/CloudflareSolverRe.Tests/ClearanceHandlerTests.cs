using CloudflareSolverRe.CaptchaProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Tests
{
    [TestClass]
    public class ClearanceHandlerTests
    {
        private static readonly Uri speedcdUri = new Uri("https://speed.cd/");
        private static readonly Uri japscanUri = new Uri("https://japscan.to");

        [TestMethod]
        public async Task SolveWebsiteChallenge_speedcd()
        {
            var handler = new ClearanceHandler
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var client = new HttpClient(handler);

            HttpResponseMessage response = await client.GetAsync(speedcdUri);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task SolveWebsiteChallenge_speedcd_WithAntiCaptcha()
        {
            if (Settings.AntiCaptchaApiKey.Equals("YOUR_API_KEY"))
                return;

            var handler = new ClearanceHandler(new AntiCaptchaProvider(Settings.AntiCaptchaApiKey))
            {
                MaxTries = 2,
                MaxCaptchaTries = 2
            };

            var client = new HttpClient(handler);

            HttpResponseMessage response = await client.GetAsync(speedcdUri);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task SolveWebsiteChallenge_speedcd_With2Captcha()
        {
            if (Settings.TwoCaptchaApiKey.Equals("YOUR_API_KEY"))
                return;

            var handler = new ClearanceHandler(new TwoCaptchaProvider(Settings.TwoCaptchaApiKey))
            {
                MaxTries = 2,
                MaxCaptchaTries = 2
            };

            var client = new HttpClient(handler);

            HttpResponseMessage response = await client.GetAsync(speedcdUri);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }


        [TestMethod]
        public async Task SolveWebsiteChallenge_japscan()
        {
            var handler = new ClearanceHandler
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var client = new HttpClient(handler);

            HttpResponseMessage response = await client.GetAsync(japscanUri);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task SolveWebsiteChallenge_japscan_WithAntiCaptcha()
        {
            if (Settings.AntiCaptchaApiKey.Equals("YOUR_API_KEY"))
                return;

            var handler = new ClearanceHandler(new AntiCaptchaProvider(Settings.AntiCaptchaApiKey))
            {
                MaxTries = 2,
                MaxCaptchaTries = 2
            };

            var client = new HttpClient(handler);

            HttpResponseMessage response = await client.GetAsync(japscanUri);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task SolveWebsiteChallenge_japscan_With2Captcha()
        {
            if (Settings.TwoCaptchaApiKey.Equals("YOUR_API_KEY"))
                return;

            var handler = new ClearanceHandler(new TwoCaptchaProvider(Settings.TwoCaptchaApiKey))
            {
                MaxTries = 2,
                MaxCaptchaTries = 2
            };

            var client = new HttpClient(handler);

            HttpResponseMessage response = await client.GetAsync(japscanUri);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }


        [TestMethod]
        public async Task SolveWebsiteChallenge_github()
        {
            var target = new Uri("https://github.com/RyuzakiH/CloudflareSolverRe");

            var handler = new ClearanceHandler
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var client = new HttpClient(handler);

            var content = await client.GetStringAsync(target);

            Assert.IsTrue(content.Contains("RyuzakiH"));
        }
    }
}