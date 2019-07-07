using System;
using System.Globalization;

namespace Cloudflare.Structs
{
    /// <summary>
    /// Holds the information, which is required to pass the CloudFlare clearance.
    /// </summary>
    public struct JsChallengeSolution : IEquatable<JsChallengeSolution>
    {
        public string ClearancePage { get; }

        public string VerificationCode { get; }

        public string Pass { get; }

        public string S { get; }

        public double Answer { get; }

        // Using .ToString("R") to reduse answer rounding
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


        public static bool operator ==(JsChallengeSolution solutionA, JsChallengeSolution solutionB)
        {
            return solutionA.Equals(solutionB);
        }

        public static bool operator !=(JsChallengeSolution solutionA, JsChallengeSolution solutionB)
        {
            return !(solutionA == solutionB);
        }

        public override bool Equals(object obj)
        {
            var other = obj as JsChallengeSolution?;
            return other.HasValue && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return ClearanceUrl.GetHashCode();
        }

        public bool Equals(JsChallengeSolution other)
        {
            return other.ClearanceUrl == ClearanceUrl;
        }
    }
}