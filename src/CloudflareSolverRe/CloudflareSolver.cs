using CloudflareSolverRe.CaptchaProviders;
using CloudflareSolverRe.Constants;
using CloudflareSolverRe.Extensions;
using CloudflareSolverRe.Solvers;
using CloudflareSolverRe.Types;
using CloudflareSolverRe.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CloudflareSolverRe
{
    public class CloudflareSolver : ICloudflareSolver
    {
        private static readonly SemaphoreLocker _locker = new SemaphoreLocker();

        /// <summary>
        /// The default number of retries, if clearance fails.
        /// </summary>
        public static readonly int DefaultMaxTries = 3;

        /// <summary>
        /// The default number of captcha clearance tries.
        /// </summary>
        public static readonly int DefaultMaxCaptchaTries = 1;


        private readonly ICaptchaProvider captchaProvider;
        private string userAgent;
        private string defaultUserAgent;

        private HttpClient httpClient;
        private CloudflareHandler cloudflareHandler;
        private Uri siteUrl;
        private CancellationToken? cancellationToken;
        private List<DetectResult> captchaDetectResults;

        /// <summary>
        /// Gets or sets the number of clearance retries, if clearance fails.
        /// </summary>
        /// <remarks>A negative value causes an infinite amount of retries.</remarks>
        public int MaxTries { get; set; } = DefaultMaxTries;

        /// <summary>
        /// Gets or sets the max number of captcha clearance tries.
        /// </summary>
        public int MaxCaptchaTries { get; set; } = DefaultMaxCaptchaTries;

        /// <summary>
        /// Gets or sets the number of milliseconds to wait before sending the clearance request.
        /// </summary>
        /// <remarks>
        /// Negative value or zero means to wait the delay time required by the challenge (like a browser).
        /// </remarks>
        public int ClearanceDelay { get; set; }

        private bool CaptchaSolvingEnabled => captchaProvider != null;


        public CloudflareSolver([Optional]string userAgent) : this(null, userAgent) { }

        public CloudflareSolver(ICaptchaProvider captchaProvider, [Optional]string userAgent)
        {
            this.captchaProvider = captchaProvider;
            defaultUserAgent = userAgent ?? Utils.GetGenerateRandomUserAgent();
            captchaDetectResults = new List<DetectResult>();
        }


        /// <summary>
        /// Solves cloudflare challenge protecting a specific website.
        /// </summary>
        /// <param name="siteUrl">Uri of the website.</param>
        /// <param name="userAgent">The user-agent which will be used to solve the challenge.</param>
        /// <param name="proxy">Proxy to use while solving the challenge.</param>
        /// <param name="cancellationToken">CancellationToken to contol solving operation cancelling.</param>
        public async Task<SolveResult> Solve(Uri siteUrl, string userAgent, [Optional]IWebProxy proxy, [Optional]CancellationToken cancellationToken)
        {
            SolveResult result = default(SolveResult);

            await _locker.LockAsync(async () =>
            {
                this.userAgent = userAgent;
                cloudflareHandler = new CloudflareHandler(userAgent);
                cloudflareHandler.HttpClientHandler.Proxy = proxy;
                httpClient = new HttpClient(cloudflareHandler);
                this.siteUrl = siteUrl;
                this.cancellationToken = cancellationToken;

                result = await Solve();

                httpClient.Dispose();
                captchaDetectResults.Clear();
            });

            return result;
        }

        /// <summary>
        /// Solves cloudflare challenge protecting a specific website.
        /// </summary>
        /// <param name="siteUrl">Uri of the website.</param>
        /// <param name="proxy">Proxy to use while solving the challenge.</param>
        /// <param name="cancellationToken">CancellationToken to contol solving operation cancelling.</param>
        /// <param name="randomUserAgent">Use a new random user-agent.</param>
        public async Task<SolveResult> Solve(Uri siteUrl, [Optional]IWebProxy proxy, [Optional]CancellationToken cancellationToken, bool randomUserAgent = false)
        {
            SolveResult result = default(SolveResult);

            await _locker.LockAsync(async () =>
            {
                userAgent = randomUserAgent ? Utils.GetGenerateRandomUserAgent() : defaultUserAgent;
                cloudflareHandler = new CloudflareHandler(userAgent);
                cloudflareHandler.HttpClientHandler.Proxy = proxy;
                httpClient = new HttpClient(cloudflareHandler);
                this.siteUrl = siteUrl;
                this.cancellationToken = cancellationToken;

                result = await Solve();

                httpClient.Dispose();
                captchaDetectResults.Clear();
            });

            return result;
        }

        /// <summary>
        /// Solves cloudflare challenge protecting a specific website.
        /// </summary>
        /// <param name="httpClient">HttpClient to use in challenge solving process.</param>
        /// <param name="httpClientHandler">HttpClientHandler of the HttpClient.</param>
        /// <param name="siteUrl">Uri of the website.</param>
        /// <param name="userAgent">The user-agent which will be used to solve the challenge.</param>
        /// <param name="cancellationToken">CancellationToken to contol solving operation cancelling.</param>
        public async Task<SolveResult> Solve(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri siteUrl, string userAgent, [Optional]CancellationToken cancellationToken)
        {
            SolveResult result = default(SolveResult);

            await _locker.LockAsync(async () =>
            {
                this.userAgent = userAgent;
                var cloudflareHandler = new CloudflareHandler(httpClientHandler, userAgent);
                result = await Solve(httpClient, cloudflareHandler, siteUrl, cancellationToken);

                httpClient.DefaultRequestHeaders.UserAgent.Clear();
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(result.UserAgent);
            });

            return result;
        }

        /// <summary>
        /// Solves cloudflare challenge protecting a specific website.
        /// </summary>
        /// <param name="httpClient">HttpClient to use in challenge solving process.</param>
        /// <param name="httpClientHandler">HttpClientHandler of the HttpClient.</param>
        /// <param name="siteUrl">Uri of the website.</param>
        /// <param name="cancellationToken">CancellationToken to contol solving operation cancelling.</param>
        /// <param name="randomUserAgent">Use a new random user-agent.</param>
        public async Task<SolveResult> Solve(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri siteUrl, [Optional]CancellationToken cancellationToken, bool randomUserAgent = false)
        {
            SolveResult result = default(SolveResult);

            await _locker.LockAsync(async () =>
            {
                userAgent = randomUserAgent ? Utils.GetGenerateRandomUserAgent() : defaultUserAgent;
                var cloudflareHandler = new CloudflareHandler(httpClientHandler, userAgent);
                result = await Solve(httpClient, cloudflareHandler, siteUrl, cancellationToken);

                httpClient.DefaultRequestHeaders.UserAgent.Clear();
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(result.UserAgent);
            });

            return result;
        }

        /// <summary>
        /// Solves cloudflare challenge protecting a specific website.
        /// </summary>
        /// <param name="httpClient">HttpClient to use in challenge solving process.</param>
        /// <param name="cloudflareHandler">CloudflareHandler of the HttpClient</param>
        /// <param name="siteUrl">Uri of the website.</param>
        /// <param name="cancellationToken">CancellationToken to contol solving operation cancelling.</param>
        internal async Task<SolveResult> Solve(HttpClient httpClient, CloudflareHandler cloudflareHandler, Uri siteUrl, [Optional]CancellationToken cancellationToken)
        {
            this.cloudflareHandler = cloudflareHandler;
            this.httpClient = httpClient.Clone(this.cloudflareHandler, false);
            this.siteUrl = siteUrl;
            this.cancellationToken = cancellationToken;

            var result = await Solve();

            this.httpClient.Dispose();
            captchaDetectResults.Clear();

            return result;
        }

        private async Task<SolveResult> Solve()
        {
            var result = await SolveWithJavascript(MaxTries - (CaptchaSolvingEnabled ? MaxCaptchaTries : 0));

            if (!result.Success && CaptchaSolvingEnabled)
                result = await SolveWithCaptcha();

            return result;
        }


        private async Task<SolveResult> SolveWithJavascript(int tries)
        {
            var result = default(SolveResult);

            for (var i = 0; i < tries && !result.Success; i++)
            {
                if (cancellationToken.HasValue)
                    cancellationToken.Value.ThrowIfCancellationRequested();

                result = await SolveJavascriptChallenge();
            }

            return result;
        }

        private async Task<SolveResult> SolveWithCaptcha()
        {
            var result = default(SolveResult);

            for (int i = 0; i < MaxCaptchaTries && !result.Success; i++)
            {
                if (cancellationToken.HasValue)
                    cancellationToken.Value.ThrowIfCancellationRequested();

                var captchaDetectResult = captchaDetectResults.Count > i ? (DetectResult?)captchaDetectResults[i] : null;
                result = await SolveCaptchaChallenge(captchaDetectResult);
            }

            return result;
        }


        private async Task<SolveResult> SolveJavascriptChallenge(DetectResult? jsDetectResult = null)
        {
            var result = default(SolveResult);

            if (!jsDetectResult.HasValue)
            {
                jsDetectResult = await CloudflareDetector.Detect(httpClient, siteUrl);
                siteUrl = ChangeUrlScheme(siteUrl, jsDetectResult.Value.SupportsHttp);
            }

            var exceptional = IsExceptionalDetectionResult(jsDetectResult.Value);
            if (exceptional.Item1)
                result = exceptional.Item2;
            else if (jsDetectResult.Value.Protection.Equals(CloudflareProtection.JavaScript))
                result = await new JsChallengeSolver(httpClient, cloudflareHandler, siteUrl, jsDetectResult.Value, userAgent, ClearanceDelay)
                    .Solve();

            if (!result.Success && result.NewDetectResult.HasValue && result.NewDetectResult.Value.Protection.Equals(CloudflareProtection.Captcha))
                captchaDetectResults.Add(result.NewDetectResult.Value);

            return result;
        }

        private async Task<SolveResult> SolveCaptchaChallenge(DetectResult? captchaDetectResult = null)
        {
            var result = default(SolveResult);

            if (!CaptchaSolvingEnabled)
                return result;

            if (!captchaDetectResult.HasValue)
            {
                captchaDetectResult = await CloudflareDetector.Detect(httpClient, siteUrl);
                siteUrl = ChangeUrlScheme(siteUrl, captchaDetectResult.Value.SupportsHttp);
            }

            var exceptional = IsExceptionalDetectionResult(captchaDetectResult.Value);
            if (exceptional.Item1)
                result = exceptional.Item2;
            else if (captchaDetectResult.Value.Protection.Equals(CloudflareProtection.Captcha))
                result = await new CaptchaChallengeSolver(httpClient, cloudflareHandler, siteUrl, captchaDetectResult.Value, userAgent, captchaProvider)
                    .Solve();
            else if (captchaDetectResult.Value.Protection.Equals(CloudflareProtection.JavaScript))
                result = await SolveJavascriptChallenge(captchaDetectResult);

            return result;
        }


        private Tuple<bool, SolveResult> IsExceptionalDetectionResult(DetectResult detectResult)
        {
            var result = default(SolveResult);

            if (IsNotProtected(detectResult))
                result = SolveResult.NoProtection;
            else if (IsBanned(detectResult))
                result = SolveResult.Banned;
            else if (IsUnknown(detectResult))
                result = SolveResult.Unknown;

            result.DetectResult = detectResult;

            var done = result.Success || !string.IsNullOrEmpty(result.FailReason);

            return Tuple.Create(done, result);
        }

        private bool IsNotProtected(DetectResult detectResult) => detectResult.Protection.Equals(CloudflareProtection.NoProtection);

        private bool IsBanned(DetectResult detectResult) => detectResult.Protection.Equals(CloudflareProtection.Banned);

        private bool IsUnknown(DetectResult detectResult) => detectResult.Protection.Equals(CloudflareProtection.Unknown);


        private Uri ChangeUrlScheme(Uri uri, bool supportsHttp)
        {
            if (!supportsHttp && uri.Scheme.Equals(General.UriSchemeHttp))
                uri = uri.ForceHttps();
            else if (supportsHttp && uri.Scheme.Equals(General.UriSchemeHttps))
                uri = uri.ForceHttp();

            return uri;
        }
    }
}