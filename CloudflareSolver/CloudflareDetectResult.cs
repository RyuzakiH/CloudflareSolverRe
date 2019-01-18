namespace Cloudflare
{
    public class CloudflareDetectResult
    {
        public CloudflareProtection Protection;
        public string Html;

        public override string ToString()
        {
            return Protection.ToString();
        }
    }
}
