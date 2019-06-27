namespace Cloudflare.Structs
{
    internal struct InternalSolveResult
    {
        public readonly bool Success;
        public readonly string FailReason;

        public InternalSolveResult(bool success, string layer, string failReason)
        {
            Success = success;

            FailReason = !string.IsNullOrEmpty(failReason) ? $"Cloudflare [{layer}]: {failReason}" : null;
        }
    }
}
