using System;
using System.Globalization;

namespace Cloudflare.Structs
{
    /// <summary>
    /// Holds the information, which is required to pass the Cloudflare clearance.
    /// </summary>
    public struct CaptchaChallengeSolution : IEquatable<CaptchaChallengeSolution>
    {
        public string ClearancePage { get; }

        public string RecaptchaResponse { get; }

        public string S { get; }

        public string ClearanceUrl => !string.IsNullOrEmpty(S) ?
            $"{ClearancePage}?s={Uri.EscapeDataString(S)}&g-recaptcha-response={Uri.EscapeDataString(RecaptchaResponse)}" :
            $"{ClearancePage}?g-recaptcha-response={Uri.EscapeDataString(RecaptchaResponse)}";


        public CaptchaChallengeSolution(string clearancePage, string s, string recaptchaResponse)
        {
            ClearancePage = clearancePage;
            S = s;
            RecaptchaResponse = recaptchaResponse;
        }
        
        public static bool operator ==(CaptchaChallengeSolution solutionA, CaptchaChallengeSolution solutionB)
        {
            return solutionA.Equals(solutionB);
        }

        public static bool operator !=(CaptchaChallengeSolution solutionA, CaptchaChallengeSolution solutionB)
        {
            return !(solutionA == solutionB);
        }

        public override bool Equals(object obj)
        {
            var other = obj as CaptchaChallengeSolution?;
            return other.HasValue && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return ClearanceUrl.GetHashCode();
        }

        public bool Equals(CaptchaChallengeSolution other)
        {
            return other.ClearanceUrl == ClearanceUrl;
        }
    }
}