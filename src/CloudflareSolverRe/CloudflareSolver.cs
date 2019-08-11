using CloudflareSolverRe.CaptchaProviders;
using CloudflareSolverRe.Constants;
using CloudflareSolverRe.Extensions;
using CloudflareSolverRe.Solvers;
using CloudflareSolverRe.Types;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace CloudflareSolverRe
{
    public class CloudflareSolver : ICloudflareSolver
    {
        private readonly ICaptchaProvider captchaProvider;

        /// <summary>
        /// The default number of retries, if clearance fails.
        /// </summary>
        public static readonly int DefaultMaxTries = 3;

        /// <summary>
        /// The default number of captcha clearance tries.
        /// </summary>
        public static readonly int DefaultMaxCaptchaTries = 1;

        private HttpClient _httpClient;
        private CloudflareHandler _cloudflareHandler;
        private Uri _siteUrl;
        private CancellationToken? _cancellationToken;
        private List<DetectResult> _captchaDetectResults;

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


        public CloudflareSolver() : this(null) { }

        public CloudflareSolver(ICaptchaProvider captchaProvider)
        {
            this.captchaProvider = captchaProvider;

            _captchaDetectResults = new List<DetectResult>();
        }


        public async Task<SolveResult> Solve(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri siteUrl, CancellationToken? cancellationToken = null)
        {
            _cloudflareHandler = new CloudflareHandler(httpClientHandler);

            var result = await Solve(httpClient, siteUrl, cancellationToken);

            _cloudflareHandler.Dispose();

            return result;
        }

        internal async Task<SolveResult> Solve(HttpClient httpClient, CloudflareHandler cloudflareHandler, Uri siteUrl, CancellationToken? cancellationToken = null)
        {
            _cloudflareHandler = cloudflareHandler;
            return await Solve(httpClient, siteUrl, cancellationToken);
        }

        private async Task<SolveResult> Solve(HttpClient httpClient, Uri siteUrl, CancellationToken? cancellationToken = null)
        {
            _httpClient = httpClient.Clone(_cloudflareHandler, false);
            _siteUrl = siteUrl;
            _cancellationToken = cancellationToken;

            var result = await Solve();

            _httpClient.Dispose();
            _captchaDetectResults.Clear();

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

            for (var i = 0; i < tries; i++)
            {
                if (_cancellationToken.HasValue)
                    _cancellationToken.Value.ThrowIfCancellationRequested();

                result = await SolveJavascriptChallenge();

                if (result.Success)
                    return result;
            }

            return result;
        }

        private async Task<SolveResult> SolveWithCaptcha()
        {
            var result = default(SolveResult);

            for (int i = 0; i < MaxCaptchaTries; i++)
            {
                if (_cancellationToken.HasValue)
                    _cancellationToken.Value.ThrowIfCancellationRequested();

                //TODO: Don't waste Js challenge chances
                var captchaDetectResult = _captchaDetectResults.Count > i ? (DetectResult?)_captchaDetectResults[i] : null;
                result = await SolveCaptchaChallenge(captchaDetectResult);

                if (result.Success)
                    return result;
            }

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


        private async Task<SolveResult> SolveJavascriptChallenge(DetectResult? jsDetectResult = null)
        {
            var result = default(SolveResult);

            if (!jsDetectResult.HasValue)
            {
                jsDetectResult = await CloudflareDetector.Detect(_httpClient, _cloudflareHandler, _siteUrl);
                _siteUrl = ChangeUrlScheme(_siteUrl, jsDetectResult.Value.SupportsHttp);
            }

            var exceptional = IsExceptionalDetectionResult(jsDetectResult.Value);
            if (exceptional.Item1)
                result = exceptional.Item2;
            else if (jsDetectResult.Value.Protection.Equals(CloudflareProtection.JavaScript))
                result = await new JsChallengeSolver(_httpClient, _cloudflareHandler, _siteUrl, jsDetectResult.Value, ClearanceDelay)
                    .Solve();

            if (!result.Success && result.NewDetectResult.HasValue && result.NewDetectResult.Value.Protection.Equals(CloudflareProtection.Captcha))
                _captchaDetectResults.Add(result.NewDetectResult.Value);

            return result;
        }

        private async Task<SolveResult> SolveCaptchaChallenge(DetectResult? captchaDetectResult = null)
        {
            var result = default(SolveResult);

            if (!CaptchaSolvingEnabled)
                return result;

            if (!captchaDetectResult.HasValue)
            {
                captchaDetectResult = await CloudflareDetector.Detect(_httpClient, _cloudflareHandler, _siteUrl);
                _siteUrl = ChangeUrlScheme(_siteUrl, captchaDetectResult.Value.SupportsHttp);
            }

            var exceptional = IsExceptionalDetectionResult(captchaDetectResult.Value);
            if (exceptional.Item1)
                result = exceptional.Item2;
            else if (captchaDetectResult.Value.Protection.Equals(CloudflareProtection.Captcha))
                result = await new CaptchaChallengeSolver(_httpClient, _cloudflareHandler, _siteUrl, captchaDetectResult.Value, captchaProvider)
                    .Solve();
            else if (captchaDetectResult.Value.Protection.Equals(CloudflareProtection.JavaScript))
                result = await SolveJavascriptChallenge(captchaDetectResult);

            return result;
        }


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