using Cloudflare.Interfaces;
using Cloudflare.Structs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cloudflare.Solvers
{
    public class CaptchaChallengeSolver
    {
        private const string LayerCaptcha = "Captcha";


        public HttpClient HttpClient { get; }
        public HttpClientHandler HttpClientHandler { get; }
        public DetectResult DetectResult { get; private set; }
        public Uri SiteUrl { get; }


        private readonly ICaptchaProvider captchaProvider;


        public CaptchaChallengeSolver(HttpClient client, HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, ICaptchaProvider captchaProvider)
        {
            HttpClient = client;
            HttpClientHandler = handler;
            SiteUrl = siteUrl;
            DetectResult = detectResult;
            this.captchaProvider = captchaProvider;
        }

        public CaptchaChallengeSolver(HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, ICaptchaProvider captchaProvider)
        {
            HttpClient = new HttpClient(handler);
            HttpClientHandler = handler;
            DetectResult = detectResult;
            this.captchaProvider = captchaProvider;
        }



        private bool IsCaptchaSolvingEnabled()
        {
            return captchaProvider != null;
        }
        

        public async Task<SolveResult> Solve()
        {
            if (!IsCaptchaSolvingEnabled())
            {
                return new SolveResult
                {
                    Success = false,
                    FailReason = "Missing captcha provider",
                    DetectResult = DetectResult,
                };
            }

            var solve = await SolveCaptcha(HttpClient, SiteUrl, DetectResult.Html);
            return new SolveResult
            {
                Success = solve.Success,
                FailReason = solve.FailReason,
                DetectResult = DetectResult,
            };
        }

        private async Task<SolveResult> SolveCaptcha(HttpClient httpClient, Uri targetUri, string html)
        {
            var formMatch = CloudflareRegex.CaptchaFormRegex.Match(html);
            if (!formMatch.Success)
                return new SolveResult(false, LayerCaptcha, "form tag not found", DetectResult);

            var action = $"{targetUri.Scheme}://{targetUri.Host}{formMatch.Groups["action"]}";
            var s = formMatch.Groups["s"].Value;
            var siteKey = formMatch.Groups["siteKey"].Value;

            var captchaResult = await captchaProvider.SolveCaptcha(siteKey, targetUri.AbsoluteUri);
            if (!captchaResult.Success)
                return new SolveResult(false, LayerCaptcha, $"captcha provider error ({captchaResult.Response})", DetectResult);

            return await SubmitCaptchaSolution(httpClient, action, s, captchaResult.Response);
        }

        private async Task<SolveResult> SubmitCaptchaSolution(HttpClient httpClient, string action, string s, string captchaResponse)
        {
            var query = $"s={Uri.EscapeDataString(s)}" +
                        $"&g-recaptcha-response={Uri.EscapeDataString(captchaResponse)}";

            var response = await httpClient.GetAsync($"{action}?{query}");
            if (response.StatusCode != HttpStatusCode.Found)
            {
                return new SolveResult(false, LayerCaptcha, "invalid submit response", DetectResult, response);
            }

            var success = response.Headers.Contains("Set-Cookie");
            return new SolveResult(success, LayerCaptcha, success ? null : "response cookie not found", DetectResult, response);
        }

    }
}
