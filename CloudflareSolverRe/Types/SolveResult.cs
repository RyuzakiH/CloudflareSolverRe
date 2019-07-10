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
            FailReason = "No protection detected",
        };

        public static SolveResult Banned = new SolveResult
        {
            Success = false,
            FailReason = "IP address is banned",
        };

        public static SolveResult Unknown = new SolveResult
        {
            Success = false,
            FailReason = "Unknown protection detected",
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
