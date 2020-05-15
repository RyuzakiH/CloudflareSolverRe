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
            var siteUrl = new Uri("https://btdb.io/test_path/#test_hash");

            // parse the challenge and assert known values
            var challenge = JsChallenge.Parse(html, siteUrl, true);

            Assert.AreEqual(challenge.SiteUrl, siteUrl);
            Assert.IsTrue(challenge.JsCode.Contains("s,t,o,p") && challenge.JsCode.Contains("submit("));
            Assert.AreEqual(challenge.Delay, 4000);
            Assert.AreEqual(challenge.Form.Action, "/test_path/?__cf_chl_jschl_tk__=142a09148b70368f7502c61ab4c22049b79114d8-1588359276-0-ASdixwI-NcWH2aobBI9NVMlCTAuBaAuDxMhGGOlab3Py-oclbbnLdYhIdorGxZt8at-EJNAhrD6642ijNYHzCSJUrvH8ag7UpOcXNqOrxfq2od59C8ieQJ8jS1K31Rv9KKeggTusGhvfqMGoqp2Navpr2FP5FBNSeHCtvYAANZX01FFHCiA9maPQM7FeyT3UZDz_r-FGXzUZJxKj4d_CyXNYbIZoQiEzL4S7CqcU2jV2DZWhgw1CfI3Yxj9SFa68DZanfVT4JJq4rW8KCFmUR6_Oe8olOlryLORY9ZPJWCcA");
            Assert.AreEqual(challenge.Form.Pass, "1588359280.159-HK5EwitrRG");
            Assert.AreEqual(challenge.Form.R, "59d0b63d0345d1391c395963e1536a3501a6a1d0-1588359276-0-Ac0Z0b+wDc09p7eCTVFfT2O9k3+ky00bQvPY0FD5WCTY0PUJB5nqu2mM4PxuWUlXS5c7g3Zd/f468832zFunQAMoJRxTw3g+G4X4x+bGz1jDGJ7dUQYAO2T+V0sOkHCAr9rxePZdDsa5qNFzXuspFQIOKvI1EX6O5baSmQPDc4H3RBEgcrvF7HzrK+dqCd9lFILTOf3VYQVxtBUOxgHwmkNyx0j4IMVX/+k8uBsoN3F4jz88Qd8OSzKVxuERMwekyqdyJVBwwl/eueZD5XZi8LZDS7kUmPNhdFqXMm6rixmcDtBOb0LyVmAOewn/cgJ3JjzVohO3yZbAJgeik5gaAsSJiA3cg52mjovkX1/DwjCHD6KhdG7AH/Yv09Uvic9s9T1DUwJePjYutpQ1nR3v0pLMUZPmm1dPONPuivJERw1k5EMFkmohF/avs4lTY05/2hN4j3WZ008yYbnJoeUc6gH4QufJVIg7EwQmLqLaU11WXiXxr8+7vhB8gLYcwtXbCkCUVAC0zBqZmY67WWinyQJXbdsyhxYqZT46+7uNmBeLHRE1K9le9E4aey0kP+JarzVy5xFwE9aDJOP+Gr0oeXpZuVOIQgPLX78gJHWre2tsnW6vVzi4d3W3Lrh7oEOgeQcZvpsFjcB5xs+x44Pq8TKFo2VOap0DYfKkXSum+pxn1XoyxYt0UgzrwahOTGAPQsSiZwvJ+n7sE5CRlGcmHH8ZYIc9Th/9sGPxjhhEtTUOpyDNQngWSl9GWSwERIpgspRa79jdelVEThiTldMVyKKF1+m8kbA6Lim6sOMi9vemGgeiLGyHC1QLachDcJcc9hDLEgwpXMZaux7OjuLdDRoVpp5/99zzDb+RhrqMgiL67+qUWsYtzQTX42MjCcuSXzQB9u6X/u5Dur5MYqr4nUrt5w97quCGrIeGbsBhRhAcUlEEVJx6fPMzezjLvtTBvZCePxraHICqUpjVWVdWUcx1GZPrOird9A4OJn6/5zpZ4r9Wm8JgLl8/yOlr0quxUFA2NMjV5BCCQPaibLZ3YUMlmZKiINExV7lEd0TWsGOPAJwvniCsUWBRfZkz64i88eRb45Rr8/u6tGYcuyL/gMKSPIstGC6tw+ao4pXRYIwp/POJB5lC2HHnYI+4wZ+hDbHrStX3S6Sm1+3s//DudnIt0q8sf6froVbv75Bkrkb0ZWk9SIOPov7jNDCIw5MUQPIoOm3hoReN+V+obchvwHSK3tOqJhcGKIi+PSDx1ZGjFyi+9WofK4ESxiAsUVF3T9DMFXKKdNc8Z3i9fGpGzRyx0zhyjwk0rKlTW29OsTSQmQK/CyTSF289moDCNxY0wTDGsl/BCAU1ErIhJuzs0mJ3c+C8IX8b38kpH98aAj9ELtUimdw5rXAKa8mZ0iFZ6KsnSHp6KilJRja/DR6x70M=");
            Assert.AreEqual(challenge.Form.VerificationCode, "2d676315e436ddfeb0e5657ea150c450");
            Assert.AreEqual(challenge.CfDn, "+((!+[]+(!![])+!![]+!![]+!![]+!![]+!![]+!![]+[])+(+!![])+(!+[]+(!![])+!![]+!![]+!![]+!![]+!![])+(!+[]+(!![])+!![]+!![]+!![])+(!+[]-(!![]))+(!+[]+(!![])+!![]+!![])+(!+[]+(!![])-[])+(!+[]+(!![])+!![]+!![]+!![])+(!+[]+(!![])+!![]))/+((!+[]+(!![])+!![]+!![]+!![]+!![]+!![]+[])+(!+[]+(!![])+!![]+!![]+!![]+!![])+(!+[]+(!![])+!![]+!![]+!![]+!![]+!![])+(!+[]+(!![])+!![]+!![]+!![]+!![]+!![])+(!+[]+(!![])+!![]+!![]+!![]+!![]+!![])+(!+[]+(!![])+!![]+!![])+(!+[]+(!![])+!![]+!![]+!![]+!![]+!![]+!![]+!![])+(!+[]+(!![])+!![])+(!+[]+(!![])+!![]+!![]+!![]+!![]+!![]+!![]+!![]))");

            // solve the challenge and assert known values
            var jschlAnswer = challenge.Solve();

            // the js code changes the form action to include the #hash (if there is hash)
            Assert.AreEqual(challenge.Form.Action, "/test_path/?__cf_chl_jschl_tk__=142a09148b70368f7502c61ab4c22049b79114d8-1588359276-0-ASdixwI-NcWH2aobBI9NVMlCTAuBaAuDxMhGGOlab3Py-oclbbnLdYhIdorGxZt8at-EJNAhrD6642ijNYHzCSJUrvH8ag7UpOcXNqOrxfq2od59C8ieQJ8jS1K31Rv9KKeggTusGhvfqMGoqp2Navpr2FP5FBNSeHCtvYAANZX01FFHCiA9maPQM7FeyT3UZDz_r-FGXzUZJxKj4d_CyXNYbIZoQiEzL4S7CqcU2jV2DZWhgw1CfI3Yxj9SFa68DZanfVT4JJq4rW8KCFmUR6_Oe8olOlryLORY9ZPJWCcA#test_hash");
            Assert.AreEqual(jschlAnswer, "-5.9314365906");
        }
    }
}