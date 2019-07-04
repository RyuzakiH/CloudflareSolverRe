using Cloudflare.Enums;

namespace Cloudflare.Structs
{
    public struct DetectResult
    {
        public CloudflareProtection Protection;
        public string Html;

        public override string ToString() => Protection.ToString();
    }
}
