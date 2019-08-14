using CloudflareSolverRe.CaptchaProviders;
using CloudflareSolverRe.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Tests
{
    [TestClass]
    public class ClearanceHandlerTests
    {
        private static readonly Uri uamhitmehardfunUri = new Uri("https://uam.hitmehard.fun/HIT");

        [TestMethod]
        public async Task SolveWebsiteChallenge_uamhitmehardfun()
        {
            var target = new Uri("https://uam.hitmehard.fun/HIT");

            var handler = new ClearanceHandler
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var client = new HttpClient(handler);

            var content = await client.GetStringAsync(uamhitmehardfunUri);

            Assert.AreEqual("Dstat.cc is the best", content);
        }

        [TestMethod]
        public async Task SolveWebsiteChallenge_uamhitmehardfun_WithAntiCaptcha()
        {
            if (Settings.AntiCaptchaApiKey.Equals("YOUR_API_KEY"))
                return;

            var handler = new ClearanceHandler(new AntiCaptchaProvider(Settings.AntiCaptchaApiKey))
            {
                MaxTries = 2,
                MaxCaptchaTries = 2
            };

            var client = new HttpClient(handler);

            var content = await client.GetStringAsync(uamhitmehardfunUri);

            Assert.AreEqual("Dstat.cc is the best", content);
        }

        [TestMethod]
        public async Task SolveWebsiteChallenge_uamhitmehardfun_With2Captcha()
        {
            if (Settings.TwoCaptchaApiKey.Equals("YOUR_API_KEY"))
                return;

            var handler = new ClearanceHandler(new TwoCaptchaProvider(Settings.TwoCaptchaApiKey))
            {
                MaxTries = 2,
                MaxCaptchaTries = 2
            };

            var client = new HttpClient(handler);

            var content = await client.GetStringAsync(uamhitmehardfunUri);

            Assert.AreEqual("Dstat.cc is the best", content);
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