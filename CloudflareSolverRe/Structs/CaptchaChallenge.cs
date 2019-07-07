using System;
using System.Collections.Generic;
using System.Text;

namespace Cloudflare.Structs
{
    public class CaptchaChallenge
    {
        public string Action { get; set; }
        public string S { get; set; }
        public string SiteKey { get; set; }

    }
}
