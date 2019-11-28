using System;
using System.Collections.Generic;

namespace CloudflareSolverRe.Types.Captcha
{
    /// <summary>
    /// Holds the information, which is required to pass the Cloudflare clearance.
    /// </summary>
    public class CaptchaChallengeSolution : IEquatable<CaptchaChallengeSolution>
    {
        public string ClearancePage { get; }
        public string RecaptchaResponse { get; }
        public string R { get; }
        public string Id { get; }

        public string ClearanceUrl => ClearancePage;

        public Dictionary<string, string> ClearanceBody => new Dictionary<string, string>
        {
            { "r", Uri.EscapeDataString(R) },
            { "id", Uri.EscapeDataString(Id) },
            { "g-recaptcha-response", Uri.EscapeDataString(RecaptchaResponse)}
        };

        public CaptchaChallengeSolution(string clearancePage, string s, string id, string recaptchaResponse)
        {
            ClearancePage = clearancePage;
            R = s;
            Id = id;
            RecaptchaResponse = recaptchaResponse;
        }

        public CaptchaChallengeSolution(CaptchaChallenge challenge, string recaptchaResponse)
        {
            ClearancePage = $"{challenge.SiteUrl.Scheme}://{challenge.SiteUrl.Host}{challenge.Action}";
            R = challenge.R;
            RecaptchaResponse = recaptchaResponse;
        }

        public static bool operator ==(CaptchaChallengeSolution solution1, CaptchaChallengeSolution solution2) =>
            (solution1 is null) ? (solution2 is null) : solution1.Equals(solution2);

        public static bool operator !=(CaptchaChallengeSolution solution1, CaptchaChallengeSolution solution2) => !(solution1 == solution2);

        public override bool Equals(object obj) => Equals(obj as CaptchaChallengeSolution);

        public bool Equals(CaptchaChallengeSolution other) => other != null && other.ClearanceUrl == ClearanceUrl;

        public override int GetHashCode() => ClearanceUrl.GetHashCode();

    }
}