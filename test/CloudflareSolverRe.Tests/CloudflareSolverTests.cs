using CloudflareSolverRe.CaptchaProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Tests
{
    [TestClass]
    public class CloudflareSolverTests
    {
        private static readonly Uri uamhitmehardfunUri = new Uri("https://uam.hitmehard.fun/HIT");

        [TestMethod]
        public async Task SolveWebsiteChallenge_uamhitmehardfun()
        {
            var cf = new CloudflareSolver
            {
                MaxTries = 3,
                ClearanceDelay = 3000
            };

            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);

            var result = await cf.Solve(client, handler, uamhitmehardfunUri);

            Assert.IsTrue(result.Success);

            var content = await client.GetStringAsync(uamhitmehardfunUri);

            Assert.AreEqual("Dstat.cc is the best", content);
        }

        [TestMethod]
        public async Task SolveWebsiteChallenge_uamhitmehardfun_WithAntiCaptcha()
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

            var result = await cf.Solve(client, handler, uamhitmehardfunUri);

            Assert.IsTrue(result.Success);

            var content = await client.GetStringAsync(uamhitmehardfunUri);

            Assert.AreEqual("Dstat.cc is the best", content);
        }

        [TestMethod]
        public async Task SolveWebsiteChallenge_uamhitmehardfun_With2Captcha()
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

            var result = await cf.Solve(client, handler, uamhitmehardfunUri);

            Assert.IsTrue(result.Success);

            var content = await client.GetStringAsync(uamhitmehardfunUri);

            Assert.AreEqual("Dstat.cc is the best", content);
        }
    }
}