using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using CloudflareSolverRe.Types.Javascript;

namespace CloudflareSolverRe.Tests
{
    [TestClass]
    public class JsChallengeTests
    {
        [TestMethod]
        public void JsChallengeUnitTest()
        {
            var htmlFile = Path.Combine("resources", "js_challenge.html");

            var html = File.ReadAllText(htmlFile);
            var siteUrl = new Uri("https://btdb.io/search/harry/0/#test_hash");

            // parse the challenge and assert known values
            var challenge = JsChallenge.Parse(html, siteUrl, true);

            Assert.AreEqual(challenge.SiteUrl, siteUrl);
            Assert.IsTrue(challenge.JsCode.Contains("String.fromCharCode") && challenge.JsCode.Contains("submit("));
            Assert.AreEqual(challenge.Delay, 4000);

            Assert.AreEqual(challenge.Form.Action, "/search/harry/0/?__cf_chl_jschl_tk__=111b63f2aa783fa1458a09cd562685cf6dcf28e8-1590518849-0-ASgHSNOYGTw7DJpKQcXxwYce-WrnT3c_j4iCBD_xmYH524kCC78aJVtx4_HecIhTYJRf012fmJuijjoHiCyOKxHa3YDbO3KhE6DBv5SMaIyNBdFrIpvHdKG5luYmWbYX7JH39Nv7QqGmbntmUnRpnioF4EJ0exYbpoluiSLqPKROL5UPRw_mm0DTxZi7wIGKWFCkYRx3agmVaPVTJElS9I8jsVj8__KCMs3b5C6r7TlDTdpIEO3CNYMy3uHdH3c-RmPYXb7YkQmrA-XOcWm9_jjduFlE-lEkWA5eF9GjaMfn");
            Assert.AreEqual(challenge.Form.Pass, "1590518853.067-5CNneVTlD1");
            Assert.AreEqual(challenge.Form.R, "c484b68db43ade37765408609249e35cbd805676-1590518849-0-Ae7887BqC1RYufgoYTaFOxcPptetydp7cdmrcPlil13gtGomFZwrejb40qWVZZ+WGKGDnBUkzIb56RdLSCKnlVjX1Uc8OW8poDo+U4E53xN6QknTWHFx4LnMNjzuZ8dpAzLoWLhPRkxp1uuERt5P0haaIY5bqeTssoI0YHer2JPZekGHHQtG3Ny2DQxmGO7e9STWE/OoCVpDxFmxdM3r7Qf/yYrpt1KUagghsxf4KTLPgKRUuBjuc6BS48wl1t/Xu9KcPhVFjWGqL8i3/z/Art4aqFmW5wNWoDFSWxvfaRCfeokAeHte7/yVogeJNGRBzNeLg6mg9cak2ZNgwbytBjRWHzTWpmWn27byCV/boEfjbWobQwTI4xfJvWxWDw0il2e9P5HVMTzFTK8M/q1fW+PfkJ9skkOnY0ygEtw4+M4o/VszJCDB3dp4EciRf2Rowp4k8JWi3x6fehwDETmd8vN58EAqVTyyHLuJD/FM21NMzZd8rSVuO8/2OKHfDZ0hFWc+SfAYEJApeswvH5IhhWin0Opgnfw5dMc3tgcYWqicIxS6geV000b0cpJE7QHqJGVqQjkWiUMz64UY8Tu2hxPsyunG2TOV1rk6Bi1p4Q8eHjJd0t7AOhiuRkch6cPBMNKhVVVColMyH3HAvL4sOIkMq0n+QLIzy4OHEquSjqEuWWPNk7i1jVVub3/FYe0JDrfAmJwy4T317pbklcAoHdLLuVsJkV72Veo2zvsnHOXgjxlYx4ale9+XIkZJ1Wp2LwcIsDAgc0syJc9BJrsQ8J8I3pc5ngOZZSpJnvx4vhLeV1C6i/nVOI9dNbyPLnLTX9qXsJSnSMC9f6Tx8NxGx/Z6E+aAg6sCQAv9jnufJxsgYaJa3BDIkv9LA/SBgH4Yp9YYLAh5p0FAsw+I80n8jiMT83eU1Yst9mrWhKblLUQ65TjumXIhOL+3zF9R64ich2pEaBALx74s7CEGyPshwNmbXtDUdN/XskqhQArx8O85RBfjbZjg+22fIGW1RMxjw3YwOxtfMR9SXCphsfQrO/bCjUL91jTJ8jFuX50lX1Gvvjpx9NRWIVdVm3cReJMPewIOopY5LQc9mM5gXcVrnM4hm4YRP4w9sTrMBN4zOh1y1lkj9mlwz9wZmzzdDRPLErad+LmrJIeOuc7QpyuQjuWgZS4rRxbEINvnf7x6Q+NzKxJN2Tl9q2EwBhKydryS/hFWGbwvecduhm9k+IOs97SPR8Js/tWp6GFemtHmkbZzDvrU1rv96KD3za3UAkUNdNVFhv6/J12ZCZypxFvcB5yEWL5CxCGir16MlsbC3QneoqOhJRPs5XbqV8M1jm0gszKT1fJZxW8i0iyhQWwDnGCznjh5Hu00PmBUv4kmAS7jHzJkofhG3UbO7ZaZj1pxF+W/qyddG+11HDqOBeofTpM=");
            Assert.AreEqual(challenge.Form.VerificationCode, "84b59970d68c15b786b5a85483d27cb4");

            // solve the challenge and assert known values
            var jschlAnswer = challenge.Solve();

            // the js code changes the form action to include the #hash (if there is hash)
            Assert.AreEqual(challenge.Form.Action, "/search/harry/0/?__cf_chl_jschl_tk__=111b63f2aa783fa1458a09cd562685cf6dcf28e8-1590518849-0-ASgHSNOYGTw7DJpKQcXxwYce-WrnT3c_j4iCBD_xmYH524kCC78aJVtx4_HecIhTYJRf012fmJuijjoHiCyOKxHa3YDbO3KhE6DBv5SMaIyNBdFrIpvHdKG5luYmWbYX7JH39Nv7QqGmbntmUnRpnioF4EJ0exYbpoluiSLqPKROL5UPRw_mm0DTxZi7wIGKWFCkYRx3agmVaPVTJElS9I8jsVj8__KCMs3b5C6r7TlDTdpIEO3CNYMy3uHdH3c-RmPYXb7YkQmrA-XOcWm9_jjduFlE-lEkWA5eF9GjaMfn#test_hash");
            Assert.AreEqual(jschlAnswer, "18.0511703099");
        }
    }
}