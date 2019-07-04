using System.Net.Http;
using System.Runtime.InteropServices;

namespace Cloudflare.Structs
{
    public struct SolveResult
    {
        public bool Success;
        public string FailReason;
        public DetectResult DetectResult;
        internal HttpResponseMessage Response;

        public SolveResult(bool success, string layer, string failReason, DetectResult detectResult, [Optional]HttpResponseMessage response)
        {
            Success = success;

            FailReason = !string.IsNullOrEmpty(failReason) ? $"Cloudflare [{layer}]: {failReason}" : null;

            DetectResult = detectResult;

            Response = response;
        }

        public SolveResult(bool success, string failReason, DetectResult detectResult, [Optional]HttpResponseMessage response)
        {
            Success = success;

            FailReason = failReason;

            DetectResult = detectResult;

            Response = response;
        }
    }
}
