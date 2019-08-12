using CloudflareSolverRe.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace CloudflareSolverRe.Types
{
    public class SessionCookies : IEquatable<SessionCookies>
    {
        internal const string IdCookieName = "__cfduid";
        internal const string ClearanceCookieName = "cf_clearance";

        private readonly Uri siteUri;

        public Cookie Cfduid { get; set; }
        public Cookie Cf_Clearance { get; set; }
        public bool Valid => Cfduid != null && Cf_Clearance != null;

        public SessionCookies(Uri siteUri)
        {
            this.siteUri = siteUri;
        }

        public SessionCookies(Cookie cfduid, Cookie cf_clearance, Uri siteUri)
        {
            this.Cfduid = cfduid;
            this.Cf_Clearance = cf_clearance;
            this.siteUri = siteUri;
        }


        public static SessionCookies FromCookieContainer(CookieContainer cookieContainer, Uri uri)
        {
            return new SessionCookies(uri)
            {
                Cfduid = cookieContainer.GetCookie(uri, IdCookieName),
                Cf_Clearance = cookieContainer.GetCookie(uri, ClearanceCookieName)
            };
        }

        public static SessionCookies FromCookieCollection(CookieCollection cookieCollection, Uri uri)
        {
            var cookies = cookieCollection.Cast<Cookie>();

            return new SessionCookies(uri)
            {
                Cfduid = cookies.FirstOrDefault(c => c.Name.Equals(IdCookieName)),
                Cf_Clearance = cookies.FirstOrDefault(c => c.Name.Equals(ClearanceCookieName))
            };
        }


        public CookieContainer AsCookieContainer()
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(siteUri, Cfduid);
            cookieContainer.Add(siteUri, Cf_Clearance);
            return cookieContainer;
        }

        public CookieCollection AsCookieCollection => new CookieCollection { Cfduid, Cf_Clearance };


        public override bool Equals(object obj) => Equals(obj as SessionCookies);

        public bool Equals(SessionCookies other) =>
            other != null && this.Cfduid == other.Cfduid && this.Cf_Clearance == other.Cf_Clearance;

        public override int GetHashCode()
        {
            var hashCode = 238132315;
            hashCode = hashCode * -1521134295 + EqualityComparer<Cookie>.Default.GetHashCode(this.Cfduid);
            hashCode = hashCode * -1521134295 + EqualityComparer<Cookie>.Default.GetHashCode(this.Cf_Clearance);
            return hashCode;
        }

        public static bool operator ==(SessionCookies cookies1, SessionCookies cookies2) =>
            (cookies1 is null) ? (cookies2 is null) : cookies1.Equals(cookies2);

        public static bool operator !=(SessionCookies cookies1, SessionCookies cookies2) => !(cookies1 == cookies2);
    }
}
