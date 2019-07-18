using CloudflareSolverRe.Constants;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace CloudflareSolverRe.Types
{
    public struct SolveResult
    {
        public bool Success;
        public string FailReason;
        public DetectResult DetectResult;
        internal DetectResult? NewDetectResult;
        internal HttpResponseMessage Response;

        public static SolveResult NoProtection = new SolveResult
        {
            Success = true,
        };

        public static SolveResult Banned = new SolveResult
        {
            Success = false,
            FailReason = Errors.IpAddressIsBanned,
        };

        public static SolveResult Unknown = new SolveResult
        {
            Success = false,
            FailReason = Errors.UnknownProtectionDetected,
        };

        public SolveResult(bool success, string layer, string failReason, DetectResult detectResult, [Optional]HttpResponseMessage response)
        {
            Success = success;

            FailReason = !string.IsNullOrEmpty(failReason) ? $"Cloudflare [{layer}]: {failReason}" : null;

            DetectResult = detectResult;

            NewDetectResult = null;

            Response = response;
        }

        public SolveResult(bool success, string failReason, DetectResult detectResult, [Optional]HttpResponseMessage response)
        {
            Success = success;

            FailReason = failReason;

            DetectResult = detectResult;

            NewDetectResult = null;

            Response = response;
        }
    }
}
