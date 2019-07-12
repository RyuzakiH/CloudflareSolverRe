using CloudflareSolverRe.CaptchaProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;

namespace CloudflareSolverRe.Tests
{
    [TestClass]
    public class WebsiteChallengeTests
    {
        [TestMethod]
        public void SolveWebsiteChallenge_uamhitmehardfun()
        {
            var cf = new CloudflareSolver();

            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler);

            var uri = new Uri("https://uam.hitmehard.fun/HIT");

            var result = cf.Solve(httpClient, httpClientHandler, uri, 3).Result;

            Assert.IsTrue(result.Success);

            var response = httpClient.GetAsync(uri).Result;
            var html = response.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("Dstat.cc is the best", html);
        }

        [TestMethod]
        public void SolveWebsiteChallenge_uamhitmehardfun_WithAntiCaptcha()
        {
            var cf = new CloudflareSolver(new AntiCaptchaProvider("YOUR_API_KEY"));

            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler);

            var uri = new Uri("https://uam.hitmehard.fun/HIT");

            var result = cf.Solve(httpClient, httpClientHandler, uri, 1).Result;

            Assert.IsTrue(result.Success);

            var response = httpClient.GetAsync(uri).Result;
            var html = response.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("Dstat.cc is the best", html);
        }

        [TestMethod]
        public void SolveWebsiteChallenge_uamhitmehardfun_With2Captcha()
        {
            var cf = new CloudflareSolver(new TwoCaptchaProvider("YOUR_API_KEY"));

            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler);

            var uri = new Uri("https://uam.hitmehard.fun/HIT");

            var result = cf.Solve(httpClient, httpClientHandler, uri, 1).Result;

            Assert.IsTrue(result.Success);

            var response = httpClient.GetAsync(uri).Result;
            var html = response.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("Dstat.cc is the best", html);
        }
    }
}
