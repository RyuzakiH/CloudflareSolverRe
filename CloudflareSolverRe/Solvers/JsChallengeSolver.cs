using Cloudflare.Enums;
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
    public class JsChallengeSolver : ChallengeSolver
    {
        public JsChallengeSolver(HttpClient client, HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, [Optional]int maxRetries)
            : base(client, handler, siteUrl, detectResult, maxRetries)
        {            
        }

        public JsChallengeSolver(HttpClientHandler handler, Uri siteUrl, DetectResult detectResult, [Optional]int maxRetries)
            : base(handler, siteUrl, detectResult, maxRetries)
        {
        }


        public new async Task<SolveResult> Solve()
        {
            var solution = default(SolveResult);

            for (int i = 0; (i < MaxRetries) && !solution.Success; i++)
                solution = await SolveChallenge(DetectResult.Html);

            if (!solution.Success && solution.FailReason.Contains("captcha"))
            {
                solution.NewDetectResult = new DetectResult
                {
                    Protection = CloudflareProtection.Captcha,
                    Html = await solution.Response.Content.ReadAsStringAsync()
                };
            }

            return solution;
        }       

        private async Task<SolveResult> SolveChallenge(string html)
        {
            var challenge = ExtractJsChallenge(html);

            var preparedJsCode = PrepareJsScript(challenge);

            var clearancePage = $"{SiteUrl.Scheme}://{SiteUrl.Host}{challenge.Form.Action}";
            var jschl_answer = ExecuteJavaScript(preparedJsCode).Replace(',', '.');

            //var solution = new JsChallengeSolution(clearancePage, challenge.Form.S, challenge.Form.VerificationCode, challenge.Form.Pass, double.Parse(jschl_answer));
            var solution = new JsChallengeSolution(clearancePage, challenge.Form, double.Parse(jschl_answer));

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
                Delay = int.Parse(CloudflareRegex.JsDelayRegex.Match(script).Groups["delay"].Value),
                CfdnHidden = CloudflareRegex.JsHtmlHiddenRegex.Match(html).Groups["inner"].Value
            };
        }

        private async Task<SolveResult> SubmitJsSolution(JsChallengeSolution solution)
        {
            PrepareHttpHandler();
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


        private string PrepareJsScript(JsChallenge challenge)
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
                        //(int)targetUri.Host[p]
                        solveScriptStringBuilder.Append(calculation.Substring(0, i) + $"'{SiteUrl.Host}'.charCodeAt({p})" + ");");
                    }
                }
                else
                {
                    solveScriptStringBuilder.Append(calculation);
                }
            }

            if (challenge.IsHostLength)
                solveScriptStringBuilder.Append($"{challenge.ClassName}.{challenge.PropertyName} += {SiteUrl.Host.Length};");

            solveScriptStringBuilder.Append($"{challenge.ClassName}.{challenge.PropertyName}.toFixed(10)");

            //var temp = solveScriptStringBuilder.ToString(); // for debugging
            return solveScriptStringBuilder.ToString();
        }

        private static string ExecuteJavaScript(string script)
        {
            return new Engine()
                .Execute(script)
                .GetCompletionValue()
                .AsString();
        }


    }
}
