using _2Captcha;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cloudflare
{
    public class CloudflareSolver
    {
        private readonly TwoCaptcha _twoCaptcha;
        private readonly HashSet<int> _statusCodeWhitelist = new HashSet<int>
        {
            200,
            301, 307, 308,
            404, 410,
        };
        
        public CloudflareSolver(string _2CaptchaKey = null)
        {
            if (!string.IsNullOrEmpty(_2CaptchaKey))
                _twoCaptcha = new TwoCaptcha(_2CaptchaKey);

            if (!IsNodeInstalled())
                throw new CloudflareException("Node.js doesn't seem to be installed");
        }

        private bool IsNodeInstalled()
        {
            var psi = new ProcessStartInfo
            {
                Arguments = "-v",
                CreateNoWindow = true,
                FileName = "node",
                UseShellExecute = false,
            };

            try
            {
                Process.Start(psi);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private async Task<string> RunNodeScript(string script)
        {
            var tmpFileName = Path.GetTempFileName() + ".js";
            File.WriteAllText(tmpFileName, script);

            var psi = new ProcessStartInfo
            {
                Arguments = tmpFileName,
                CreateNoWindow = true,
                FileName = "node",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            var process = Process.Start(psi);
            var output = await process.StandardOutput.ReadToEndAsync();
            File.Delete(tmpFileName);

            return output.Trim();
        }

        private bool Is2CaptchaEnabled()
        {
            return _twoCaptcha != null;
        }
        
        private void PrepareHttpHandler(HttpClientHandler httpClientHandler)
        {
            httpClientHandler.AllowAutoRedirect = false;
        }

        private void PrepareHttpHeaders(HttpRequestHeaders headers, Uri targetUri)
        {
            if (headers.Accept.Count == 0)
                headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");

            if (headers.AcceptLanguage.Count == 0) 
                headers.AcceptLanguage.ParseAdd("en,en-US;q=0.9");

            if (headers.Host == null)
                headers.Host = targetUri.Host;

            if (headers.Referrer == null)
                headers.Referrer = targetUri;

            if (headers.UserAgent.Count == 0)
                headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
        }

        private string PrepareJsScript(Uri targetUri, Match defineMatch, MatchCollection calcMatches)
        {
            var solveScriptStringBuilder = new StringBuilder(defineMatch.Value);

            foreach (Match calcMatch in calcMatches)
            {
                solveScriptStringBuilder.Append(calcMatch.Value);
            }
            solveScriptStringBuilder.Append($"console.log(+{defineMatch.Groups["className"].Value}.{defineMatch.Groups["propName"].Value}.toFixed(10) + {targetUri.Host.Length})");
            
            return solveScriptStringBuilder.ToString();
        }

        public async Task<CloudflareSolveResult> Solve(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri targetUri, CloudflareDetectResult detectResult = null)
        {
            if (detectResult == null)
                detectResult = await Detect(httpClient, httpClientHandler, targetUri);

            switch (detectResult.Protection)
            {
                case CloudflareProtection.NoProtection:
                    return new CloudflareSolveResult
                    {
                        Success = true,
                        FailReason = "No protection detected",
                        DetectResult = detectResult,
                    };
                case CloudflareProtection.JavaScript:
                {
                    var solve = await SolveJs(httpClient, targetUri, detectResult.Html);
                    return new CloudflareSolveResult
                    {
                        Success = solve.Item1,
                        FailReason = solve.Item2,
                        DetectResult = detectResult,
                    };
                }
                case CloudflareProtection.Captcha:
                {
                    if (!Is2CaptchaEnabled())
                    {
                        return new CloudflareSolveResult
                        {
                            Success = false,
                            FailReason = "Missing 2Captcha API key to solve the captcha",
                            DetectResult = detectResult,
                        };
                    }

                    var solve = await SolveCaptcha(httpClient, targetUri, detectResult.Html);
                    return new CloudflareSolveResult
                    {
                        Success = solve.Item1,
                        FailReason = solve.Item2,
                        DetectResult = detectResult,
                    };
                }
                case CloudflareProtection.Banned:
                    return new CloudflareSolveResult
                    {
                        Success = false,
                        FailReason = "This IP address is banned on the website",
                        DetectResult = detectResult,
                    };
                case CloudflareProtection.Unknown:
                    return new CloudflareSolveResult
                    {
                        Success = false,
                        FailReason = "Unknown protection detected",
                        DetectResult = detectResult,
                    };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<CloudflareDetectResult> Detect(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri targetUri)
        {
            PrepareHttpHandler(httpClientHandler);
            PrepareHttpHeaders(httpClient.DefaultRequestHeaders, targetUri);

            var response = await httpClient.GetAsync(targetUri);

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                var html = await response.Content.ReadAsStringAsync();
                if (html.Contains("<title>Just a moment...</title>"))
                {
                    return new CloudflareDetectResult
                    {
                        Protection = CloudflareProtection.JavaScript,
                        Html = html,
                    };
                }

                return new CloudflareDetectResult
                {
                    Protection = CloudflareProtection.Unknown,
                    Html = html,
                };
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                var html = await response.Content.ReadAsStringAsync();
                if (html.Contains("<title>Attention Required! |"))
                {
                    return new CloudflareDetectResult
                    {
                        Protection = CloudflareProtection.Captcha,
                        Html = html,
                    };
                }

                if (html.Contains("<title>Access denied |"))
                {
                    return new CloudflareDetectResult
                    {
                        Protection = CloudflareProtection.Banned,
                        Html = html,
                    };
                }

                return new CloudflareDetectResult
                {
                    Protection = CloudflareProtection.Unknown,
                    Html = html,
                };
            }

            if (response.Headers.Contains("CF-RAY") && (response.IsSuccessStatusCode || _statusCodeWhitelist.Contains((int) response.StatusCode)))
            {
                return new CloudflareDetectResult
                {
                    Protection = CloudflareProtection.NoProtection,
                };
            }
            else
            {
                return new CloudflareDetectResult
                {
                    Protection = CloudflareProtection.Unknown,
                };
            }
        }

        private async Task<(bool, string)> SolveJs(HttpClient httpClient, Uri targetUri, string html)
        {
            var formMatch = CloudflareRegex.JsFormRegex.Match(html);
            if (!formMatch.Success)
            {
                return (false, "Cloudflare (JS): form tag not found");
            }

            var scriptMatch = CloudflareRegex.ScriptRegex.Match(html);
            if (!scriptMatch.Success)
            {
                return (false, "Cloudflare (JS): script tag not found");
            }

            var script = scriptMatch.Groups["script"].Value;

            var defineMatch = CloudflareRegex.JsDefineRegex.Match(script);
            if (!defineMatch.Success)
            {
                return (false, "Cloudflare (JS): define variable not found");
            }
            
            var calcMatches = CloudflareRegex.JsCalcRegex.Matches(script);
            if (calcMatches.Count == 0)
            {
                return (false, "Cloudflare (JS): challenge not found");
            }

            var solveJsScript = PrepareJsScript(targetUri, defineMatch, calcMatches);

            var action = $"{targetUri.Scheme}://{targetUri.Host}{formMatch.Groups["action"]}";
            var s = formMatch.Groups["s"].Success ? formMatch.Groups["s"].Value : null;
            var jschl_vc = formMatch.Groups["jschl_vc"].Value;
            var pass = formMatch.Groups["pass"].Value;
            var jschl_answer = await RunNodeScript(solveJsScript);

            await Task.Delay(4000 + 100);
            
            return await SubmitJsSolution(httpClient, action, s, jschl_vc, pass, jschl_answer);
        }

        private async Task<(bool, string)> SubmitJsSolution(HttpClient httpClient, string action, string s, string jschl_vc, string pass, string jschl_answer)
        {
            var query = $"jschl_vc={Uri.EscapeDataString(jschl_vc)}&pass={Uri.EscapeDataString(pass)}&jschl_answer={Uri.EscapeDataString(jschl_answer)}";

            // query order is very important, 's' must go first if exists
            if (s != null)
                query = $"s={Uri.EscapeDataString(s)}&{query}";
            
            var response = await httpClient.GetAsync($"{action}?{query}");
            if (response.StatusCode != HttpStatusCode.Found)
            {
                return (false, "Cloudflare (JS): invalid submit response");
            }
            
            var success = response.Headers.Contains("Set-Cookie");
            return (success, success ? null : "Cloudflare (JS): response cookie not found");
        }

        private async Task<(bool, string)> SolveCaptcha(HttpClient httpClient, Uri targetUri, string html)
        {
            var formMatch = CloudflareRegex.CaptchaFormRegex.Match(html);
            if (!formMatch.Success)
            {
                return (false, "Cloudflare (Captcha): form tag not found");
            }
            
            var action = $"{targetUri.Scheme}://{targetUri.Host}{formMatch.Groups["action"]}";
            var siteKey = formMatch.Groups["siteKey"].Value;

            var captchaResult = await _twoCaptcha.SolveReCaptchaV2(siteKey, targetUri.AbsoluteUri);
            if (!captchaResult.Success)
            {
                return (false, $"Cloudflare (Captcha): 2Captcha error ({captchaResult.Response})");
            }

            return await SubmitCaptchaSolution(httpClient, action, captchaResult.Response);
        }

        private async Task<(bool, string)> SubmitCaptchaSolution(HttpClient httpClient, string action, string captchaResponse)
        {
            var query = $"g-recaptcha-response={Uri.EscapeDataString(captchaResponse)}";
            
            var response = await httpClient.GetAsync($"{action}?{query}");
            if (response.StatusCode != HttpStatusCode.Found)
            {
                return (false, "Cloudflare (Captcha): invalid submit response");
            }
            
            var success = response.Headers.Contains("Set-Cookie");
            return (success, success ? null : "Cloudflare (Captcha): response cookie not found");
        }

    }
}
