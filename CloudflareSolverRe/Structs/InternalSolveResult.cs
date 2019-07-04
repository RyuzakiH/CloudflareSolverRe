using System.Net.Http;

namespace Cloudflare.Structs
{
    internal struct InternalSolveResult
    {
        public readonly bool Success;
        public readonly string FailReason;
        public HttpResponseMessage Response;

        public InternalSolveResult(bool success, string layer, string failReason, HttpResponseMessage response = null)
        {
            Success = success;

            FailReason = !string.IsNullOrEmpty(failReason) ? $"Cloudflare [{layer}]: {failReason}" : null;

            Response = response;
        }
    }
}
