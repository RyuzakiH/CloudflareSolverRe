using Cloudflare.Enums;
using Cloudflare.Extensions;
using Cloudflare.Structs;
using Jint;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cloudflare.Solvers
{
    public class JsChallengeSolver
    {
        private const string LayerJavaScript = "JavaScript";
        private const string LayerCaptcha = "Captcha";

        private const int DefaultMaxRetries = 2;

        public HttpClient HttpClient { get; }
        public HttpClientHandler HttpClientHandler { get; }
        public DetectResult DetectResult { get; private set; }
        public Uri SiteUrl { get; }

        public int MaxRetries { get; set; } = DefaultMaxRetries;

        public JsChallengeSolver(HttpClient client, HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, [Optional]int maxRetries)
        {
            HttpClient = client;
            HttpClientHandler = handler;
            SiteUrl = siteUrl;
            DetectResult = detectResult;

            if (maxRetries != default(int))
                MaxRetries = maxRetries;
        }

        public JsChallengeSolver(HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, [Optional]int maxRetries)
        {
            HttpClient = new HttpClient(handler);
            HttpClientHandler = handler;
            DetectResult = detectResult;

            if (maxRetries != default(int))
                MaxRetries = maxRetries;
        }



        public async Task<SolveResult> Solve()
        {
            var solve = await SolveChallenge(DetectResult.Html);

            if (!solve.Success && solve.FailReason.Contains("captcha"))
            {
                solve.DetectResult = new DetectResult
                {
                    Protection = CloudflareProtection.Captcha,
                    Html = await solve.Response.Content.ReadAsStringAsync()
                };
            }

            return solve;
        }


        private static void PrepareHttpHandler(HttpClientHandler httpClientHandler)
        {
            httpClientHandler.AllowAutoRedirect = false;
            httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        }

        private void PrepareHttpHeaders()
        {
            if (HttpClient.DefaultRequestHeaders.Host == null)
                HttpClient.DefaultRequestHeaders.Host = SiteUrl.Host;

            if (!HttpClient.DefaultRequestHeaders.UserAgent.Any())
                HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

            if (!HttpClient.DefaultRequestHeaders.Accept.Any())
                HttpClient.DefaultRequestHeaders.AddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            if (!HttpClient.DefaultRequestHeaders.AcceptLanguage.Any())
                HttpClient.DefaultRequestHeaders.AddWithoutValidation("Accept-Language", "en-US,en;q=0.5");

            //if (!HttpClient.DefaultRequestHeaders.AcceptEncoding.Any())
            //    HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");

            if (HttpClient.DefaultRequestHeaders.Referrer == null)
                HttpClient.DefaultRequestHeaders.Referrer = SiteUrl;

            if (!HttpClient.DefaultRequestHeaders.Connection.Any())
                HttpClient.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");

            //if (!headers.Contains("DNT"))
            //    headers.Add("DNT", "1");

            if (!HttpClient.DefaultRequestHeaders.Contains("Upgrade-Insecure-Requests"))
                HttpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        }


        private static string PrepareJsScript(Uri targetUri, JsChallenge challenge)
        {
            var solveScriptStringBuilder = new StringBuilder(challenge.ClassDefinition);

            foreach (var calculation in challenge.Calculations)
            {
                if (calculation.EndsWith("}();") && calculation.Contains("eval(eval("))
                {
                    var i = calculation.IndexOf("function", StringComparison.Ordinal);
                    solveScriptStringBuilder.Append(calculation.Substring(0, i) + challenge.CfdnHidden + ";");
                }
                else if (calculation.EndsWith(")))));") && calculation.Contains("return eval("))
                {
                    var match = CloudflareRegex.JsPParamRegex.Match(calculation);
                    if (match.Success)
                    {
                        var p = match.Groups["p"].Value;
                        var i = calculation.IndexOf("(function", StringComparison.Ordinal);
                        solveScriptStringBuilder.Append(calculation.Substring(0, i) + $"'{targetUri.Host}'.charCodeAt({p})" + ");");
                    }
                }
                else
                {
                    solveScriptStringBuilder.Append(calculation);
                }
            }

            if (challenge.IsHostLength)
                solveScriptStringBuilder.Append($"{challenge.ClassName}.{challenge.PropertyName} += {targetUri.Host.Length};");

            solveScriptStringBuilder.Append($"{challenge.ClassName}.{challenge.PropertyName}.toFixed(10)");

            var temp = solveScriptStringBuilder.ToString(); // for debugging
            return solveScriptStringBuilder.ToString();
        }

        private static string ExecuteJavaScript(string script)
        {
            return new Engine()
                .Execute(script)
                .GetCompletionValue()
                .AsString();
        }


        private async Task<SolveResult> SolveChallenge(string html)
        {
            var challenge = ExtractJsChallenge(html);

            var preparedJsCode = PrepareJsScript(SiteUrl, challenge);

            var clearancePage = $"{SiteUrl.Scheme}://{SiteUrl.Host}{challenge.Form.Action}";
            var jschl_answer = ExecuteJavaScript(preparedJsCode).Replace(',', '.');

            var solution = new JsChallengeSolution(clearancePage, challenge.Form.S, challenge.Form.VerificationCode, challenge.Form.Pass, double.Parse(jschl_answer));

            await Task.Delay(challenge.Delay + 100);

            return await SubmitJsSolution(solution);
        }

        private static JsChallenge ExtractJsChallenge(string html)
        {
            var formMatch = CloudflareRegex.JsFormRegex.Match(html);
            var script = CloudflareRegex.ScriptRegex.Match(html).Groups["script"].Value;
            var defineMatch = CloudflareRegex.JsDefineRegex.Match(script);

            return new JsChallenge
            {
                Form = new JsForm
                {
                    Action = formMatch.Groups["action"].Value,
                    S = formMatch.Groups["s"].Value,
                    VerificationCode = formMatch.Groups["jschl_vc"].Value,
                    Pass = formMatch.Groups["pass"].Value
                },
                Script = script,
                ClassDefinition = defineMatch.Value,
                ClassName = defineMatch.Groups["className"].Value,
                PropertyName = defineMatch.Groups["propName"].Value,
                Calculations = CloudflareRegex.JsCalcRegex.Matches(script).Cast<Match>().Select(m => m.Value),
                IsHostLength = CloudflareRegex.JsResultRegex.Match(script).Groups["addHostLength"].Success,
                CfdnHidden = CloudflareRegex.JsHtmlHiddenRegex.Match(html).Groups["inner"].Value,
                Delay = int.Parse(CloudflareRegex.JsDelayRegex.Match(script).Groups["delay"].Value)
            };
        }

        private async Task<SolveResult> SubmitJsSolution(JsChallengeSolution solution)
        {
            PrepareHttpHeaders();

            var response = await HttpClient.GetAsync(solution.ClearanceUrl);
            
            if (response.StatusCode == HttpStatusCode.Found)
            {
                var success = response.Headers.Contains("Set-Cookie");
                return new SolveResult(success, LayerJavaScript, success ? null : "response cookie not found", DetectResult, response); // "invalid submit response"
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) // Captcha
            {
                return new SolveResult(false, LayerCaptcha, "captcha solver required", DetectResult, response);
            }
            else
            {
                return new SolveResult(false, LayerJavaScript, "something wrong happened", DetectResult, response);
            }            
        }



    }
}
