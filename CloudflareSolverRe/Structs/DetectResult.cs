using Cloudflare.Enums;
using System;

namespace Cloudflare.Structs
{
    public struct DetectResult : IEquatable<DetectResult>
    {
        public CloudflareProtection Protection { get; set; }
        public string Html { get; set; }
        public bool SupportsHttp { get; set; }

        public override string ToString() => Protection.ToString();

        public static bool operator ==(DetectResult resultA, DetectResult resultB)
        {
            return resultA.Equals(resultB);
        }

        public static bool operator !=(DetectResult resultA, DetectResult resultB)
        {
            return !(resultA == resultB);
        }

        public override bool Equals(object obj)
        {
            var other = obj as DetectResult?;
            return other.HasValue && Equals(other.Value);
        }

        public bool Equals(DetectResult other) => other.Protection.Equals(Protection) && other.Html.Equals(Html);

        public override int GetHashCode() => Protection.GetHashCode();
    }
}
