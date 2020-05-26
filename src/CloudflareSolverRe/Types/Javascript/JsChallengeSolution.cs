using System;
using System.Collections.Generic;

namespace CloudflareSolverRe.Types.Javascript
{
    /// <summary>
    /// Holds the information, which is required to pass the CloudFlare clearance.
    /// </summary>
    public class JsChallengeSolution : IEquatable<JsChallengeSolution>
    {
        public string ClearancePage { get; }
        public string VerificationCode { get; }
        public string Pass { get; }
        public string R { get; }
        public string Answer { get; }

        public string ClearanceUrl => ClearancePage;

        public Dictionary<string, string> ClearanceBody => new Dictionary<string, string>
        {
            { "r", Uri.EscapeDataString(R) },
            { "jschl_vc", VerificationCode},
            { "pass", Pass },
            { "jschl_answer", Answer }
        };

        public JsChallengeSolution(string clearancePage, string r, string verificationCode, string pass, string answer)
        {
            ClearancePage = clearancePage;
            R = r;
            VerificationCode = verificationCode;
            Pass = pass;
            Answer = answer;
        }

        public JsChallengeSolution(string clearancePage, JsForm form, string answer)
        {
            ClearancePage = clearancePage;
            R = form.R;
            VerificationCode = form.VerificationCode;
            Pass = form.Pass;
            Answer = answer;
        }

        public JsChallengeSolution(Uri siteUrl, JsForm form, string answer)
        {
            ClearancePage = $"{siteUrl.Scheme}://{siteUrl.Host}{form.Action}";
            R = form.R;
            VerificationCode = form.VerificationCode;
            Pass = form.Pass;
            Answer = answer;
        }

        public static bool operator ==(JsChallengeSolution solution1, JsChallengeSolution solution2) =>
            (solution1 is null) ? (solution2 is null) : solution1.Equals(solution2);

        public static bool operator !=(JsChallengeSolution solution1, JsChallengeSolution solution2) => !(solution1 == solution2);

        public override bool Equals(object obj) => Equals(obj as JsChallengeSolution);

        public bool Equals(JsChallengeSolution other) => other != null && other.ClearanceUrl == ClearanceUrl;

        public override int GetHashCode() => ClearanceUrl.GetHashCode();
    }
}