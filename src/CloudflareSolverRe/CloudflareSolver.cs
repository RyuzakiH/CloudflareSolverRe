using CloudflareSolverRe.CaptchaProviders;
using CloudflareSolverRe.Extensions;
using CloudflareSolverRe.Solvers;
using CloudflareSolverRe.Types;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudflareSolverRe
{
    public class CloudflareSolver: IClearanceDelayable, IRetriable
    {
        private readonly ICaptchaProvider captchaProvider;

        /// <summary>
        /// The default number of retries, if clearance fails.
        /// </summary>
        public static readonly int DefaultMaxRetries = 3;
        
        /// <summary>
        /// Gets or sets the number of clearance retries, if clearance fails.
        /// </summary>
        /// <remarks>A negative value causes an infinite amount of retries.</remarks>
        public int MaxRetries { get; set; } = DefaultMaxRetries;

        /// <summary>
        /// Gets or sets the number of milliseconds to wait before sending the clearance request.
        /// </summary>
        /// <remarks>
        /// Negative value or zero means to wait the delay time required by the challenge (like a browser).
        /// </remarks>
        public int ClearanceDelay { get; set; }


        public CloudflareSolver()
        {
            captchaProvider = null;
        }

        public CloudflareSolver(ICaptchaProvider captchaProvider)
        {
            this.captchaProvider = captchaProvider;
        }


        private bool IsCaptchaSolvingEnabled() => captchaProvider != null;


        public async Task<SolveResult> Solve(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri siteUrl, DetectResult? detectResult = null)
        {
            var result = default(SolveResult);
            var newDetectResult = default(DetectResult?);

            for (var i = 0; i < MaxRetries * (IsCaptchaSolvingEnabled() ? 2 : 1); i++)
            {
                if (!detectResult.HasValue)
                    detectResult = await CloudflareDetector.Detect(httpClient, httpClientHandler, siteUrl/*, i >= MaxRetries*/);

                siteUrl = ChangeUrlScheme(siteUrl, detectResult.Value.SupportsHttp);

                switch (detectResult.Value.Protection)
                {
                    case CloudflareProtection.NoProtection:
                        result = SolveResult.NoProtection;
                        result.DetectResult = detectResult.Value;
                        break;
                    case CloudflareProtection.JavaScript:

                        result = await new JsChallengeSolver(httpClient, httpClientHandler, siteUrl, detectResult.Value, clearanceDelay: ClearanceDelay).Solve();

                        if (!result.Success && result.NewDetectResult.HasValue && result.NewDetectResult.Value.Protection.Equals(CloudflareProtection.Captcha))
                            newDetectResult = result.NewDetectResult;

                        break;
                    case CloudflareProtection.Captcha:

                        if (i >= MaxRetries)
                            result = await new CaptchaChallengeSolver(httpClient, httpClientHandler, siteUrl, detectResult.Value, captchaProvider).Solve();

                        break;
                    case CloudflareProtection.Banned:
                        result = SolveResult.NoProtection;
                        result.DetectResult = detectResult.Value;
                        break;
                    case CloudflareProtection.Unknown:
                        result = SolveResult.Unknown;
                        result.DetectResult = detectResult.Value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (result.Success)
                    return result;

                detectResult = detectResult.Value.Equals(newDetectResult) ? null : newDetectResult;
            }

            return result;
        }

        private Uri ChangeUrlScheme(Uri uri, bool supportsHttp)
        {
            //if (i >= MaxRetries && siteUrl.Scheme.Equals("http"))
            //    siteUrl = siteUrl.ForceHttps();
            //else if (detectResult.Value.SupportsHttp && siteUrl.Scheme.Equals("https"))
            //    siteUrl = siteUrl.ForceHttp();

            if (uri.Scheme.Equals("http"))
                uri = uri.ForceHttps();
            else if (supportsHttp && uri.Scheme.Equals("https"))
                uri = uri.ForceHttp();

            return uri;
        }

    }
}