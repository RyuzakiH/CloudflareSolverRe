using CloudflareSolverRe.Enums;
using CloudflareSolverRe.Types;
using CloudflareSolverRe.Types.Javascript;
using Jint;
using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareSolverRe.Solvers
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
            var challenge = JsChallenge.Parse(html);

            var preparedJsCode = PrepareJsScript(challenge);

            var clearancePage = $"{SiteUrl.Scheme}://{SiteUrl.Host}{challenge.Form.Action}";
            var jschl_answer = ExecuteJavaScript(preparedJsCode).Replace(',', '.');

            //var solution = new JsChallengeSolution(clearancePage, challenge.Form.S, challenge.Form.VerificationCode, challenge.Form.Pass, double.Parse(jschl_answer));
            var solution = new JsChallengeSolution(clearancePage, challenge.Form, double.Parse(jschl_answer));

            await Task.Delay(challenge.Script.Delay + 100);

            return await SubmitJsSolution(solution);
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
            var solveScriptStringBuilder = new StringBuilder($"var {challenge.Script.ClassName}={{\"{challenge.Script.PropertyName}\":{challenge.Script.PropertyValue}}};");

            //foreach (var calculation in challenge.Script.Calculations)
            //{
            //    if (calculation.EndsWith("}();") && calculation.Contains("eval(eval("))
            //    {
            //        solveScriptStringBuilder
            //            .Append(calculation.Substring(0, calculation.IndexOf("function", StringComparison.Ordinal)) + challenge.Cfdn + ";");
            //    }
            //    else if (calculation.EndsWith(")))));") && calculation.Contains("return eval("))
            //    {
            //        var match = CloudflareRegex.JsPParamRegex.Match(calculation);
            //        if (match.Success)
            //        {
            //            var p = match.Groups["p"].Value;
            //            var i = calculation.IndexOf("(function", StringComparison.Ordinal);
            //            //(int)targetUri.Host[p]
            //            solveScriptStringBuilder.Append(calculation.Substring(0, i) + $"'{SiteUrl.Host}'.charCodeAt({p})" + ");");
            //        }
            //    }
            //    else
            //    {
            //        solveScriptStringBuilder.Append(calculation);
            //    }
            //}

            if (challenge.Script.IsHostLength)
                solveScriptStringBuilder.Append($"{challenge.Script.ClassName}.{challenge.Script.PropertyName} += {SiteUrl.Host.Length};");

            solveScriptStringBuilder.Append($"{challenge.Script.ClassName}.{challenge.Script.PropertyName}.toFixed({challenge.Script.Round})");

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
