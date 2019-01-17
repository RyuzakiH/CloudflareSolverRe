using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using _2Captcha;

namespace Cloudflare
{
    public class CloudflareSolver
    {
        private readonly TwoCaptcha _twoCaptcha;

        public delegate void CloudflareSolveStatusHandler(CloudflareSolveStatus status, string message = null);
        public event CloudflareSolveStatusHandler OnCloudflareSolveStatus;

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

        public async Task<bool> Solve(HttpClient httpClient, HttpClientHandler httpClientHandler, Uri targetUri)
        {
            PrepareHttpHandler(httpClientHandler);
            PrepareHttpHeaders(httpClient.DefaultRequestHeaders, targetUri);

            var response = await httpClient.GetAsync(targetUri);

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                var html = await response.Content.ReadAsStringAsync();
                if (html.Contains("<title>Just a moment...</title>"))
                {
                    OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.SolvingJavaScript);
                    if (await SolveJs(httpClient, targetUri, html))
                    {
                        OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.Success);
                        return true;
                    }
                    else
                        return false;
                }

                OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.ProtectionNotRecognized, "Cloudflare title tag not found");
                return false;
            }

            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                var html = await response.Content.ReadAsStringAsync();
                if (html.Contains("<title>Attention Required! | Cloudflare</title>"))
                {
                    if (Is2CaptchaEnabled())
                    {
                        OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.SolvingCaptcha);
                        if (await SolveCaptcha(httpClient, targetUri, html))
                        {
                            OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.Success);
                            return true;
                        }
                        else
                            return false;
                    }
                    else
                    {
                        OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.Fail, "Captcha resolving service is not enabled");
                        return false;
                    }
                }

                OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.ProtectionNotRecognized, "Cloudflare title tag not found");
                return false;
            }

            OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.ProtectionNotRecognized, $"Invalid server status: {response.StatusCode}");
            return response.IsSuccessStatusCode;
        }

        private async Task<bool> SolveJs(HttpClient httpClient, Uri targetUri, string html)
        {
            var formMatch = CloudflareRegex.JsFormRegex.Match(html);
            if (!formMatch.Success)
            {
                OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.Fail, "Cloudflare JS form not found");
                return false;
            }

            var scriptMatch = CloudflareRegex.ScriptRegex.Match(html);
            if (!scriptMatch.Success)
            {
                OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.Fail, "Cloudflare JS script tag not found");
                return false;
            }

            var script = scriptMatch.Groups["script"].Value;

            var defineMatch = CloudflareRegex.JsDefineRegex.Match(script);
            if (!defineMatch.Success)
            {
                OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.Fail, "Cloudflare JS define variable not found");
                return false;
            }
            
            var calcMatches = CloudflareRegex.JsCalcRegex.Matches(script);
            if (calcMatches.Count == 0)
            {
                OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.Fail, "Cloudflare JS calc not found");
                return false;
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

        private async Task<bool> SubmitJsSolution(HttpClient httpClient, string action, string s, string jschl_vc, string pass, string jschl_answer)
        {
            var query = $"jschl_vc={Uri.EscapeDataString(jschl_vc)}&pass={Uri.EscapeDataString(pass)}&jschl_answer={Uri.EscapeDataString(jschl_answer)}";

            // query order is very important, 's' must go first if exists
            if (s != null)
                query = $"s={Uri.EscapeDataString(s)}&{query}";

            OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.SubmittingResult, query);
            var response = await httpClient.GetAsync($"{action}?{query}");
            if (response.StatusCode != HttpStatusCode.Found)
            {
                OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.Fail, $"Cloudflare invalid JS solve server status: {response.StatusCode}");
                return false;
            }
            
            return response.Headers.Contains("Set-Cookie");
        }

        private async Task<bool> SolveCaptcha(HttpClient httpClient, Uri targetUri, string html)
        {
            var formMatch = CloudflareRegex.CaptchaFormRegex.Match(html);
            if (!formMatch.Success)
            {
                OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.Fail, "Cloudflare captcha form not found");
                return false;
            }
            
            var action = $"{targetUri.Scheme}://{targetUri.Host}{formMatch.Groups["action"]}";
            var siteKey = formMatch.Groups["siteKey"].Value;

            var captchaResult = await _twoCaptcha.SolveReCaptchaV2(siteKey, targetUri.AbsoluteUri);
            if (!captchaResult.Success)
            {
                OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.Fail, $"Cloudflare captcha solve failed: {captchaResult.Response}");
                return false;
            }

            return await SubmitCaptchaSolution(httpClient, action, captchaResult.Response);
        }

        private async Task<bool> SubmitCaptchaSolution(HttpClient httpClient, string action, string captchaResponse)
        {
            var query = $"g-recaptcha-response={Uri.EscapeDataString(captchaResponse)}";

            OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.SubmittingResult, query);
            var response = await httpClient.GetAsync($"{action}?{query}");
            if (response.StatusCode != HttpStatusCode.Found)
            {
                OnCloudflareSolveStatus?.Invoke(CloudflareSolveStatus.Fail, $"Cloudflare invalid captcha solve server status: {response.StatusCode}");
                return false;
            }
            
            return response.Headers.Contains("Set-Cookie");
        }

    }
}
