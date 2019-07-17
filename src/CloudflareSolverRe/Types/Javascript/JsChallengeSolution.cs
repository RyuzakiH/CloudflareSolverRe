using System;
using System.Globalization;

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
        public string S { get; }
        public double Answer { get; }

        // Using .ToString("R") to reduce answer rounding
        public string ClearanceUrl => !(string.IsNullOrEmpty(S)) ?
            $"{ClearancePage}?s={Uri.EscapeDataString(S)}&jschl_vc={VerificationCode}&pass={Pass}&jschl_answer={Answer.ToString("R", CultureInfo.InvariantCulture)}" :
            $"{ClearancePage}?jschl_vc={VerificationCode}&pass={Pass}&jschl_answer={Answer.ToString("R", CultureInfo.InvariantCulture)}";

        public JsChallengeSolution(string clearancePage, string s, string verificationCode, string pass, double answer)
        {
            ClearancePage = clearancePage;
            S = s;
            VerificationCode = verificationCode;
            Pass = pass;
            Answer = answer;
        }

        public JsChallengeSolution(string clearancePage, JsForm form, double answer)
        {
            ClearancePage = clearancePage;
            S = form.S;
            VerificationCode = form.VerificationCode;
            Pass = form.Pass;
            Answer = answer;
        }

        public JsChallengeSolution(Uri siteUrl, JsForm form, double answer)
        {
            ClearancePage = $"{siteUrl.Scheme}://{siteUrl.Host}{form.Action}";
            S = form.S;
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