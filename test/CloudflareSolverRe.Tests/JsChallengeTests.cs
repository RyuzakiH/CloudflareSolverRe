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

            Assert.AreEqual(challenge.Form.Action, "/search/harry/0/?__cf_chl_jschl_tk__=17b0d0c134f396ab5c0c5d762e979166506c60c4-1589552412-0-AZWR07FASW_2aSkfJOKexq8OuE58LcBmUUfLGsynpwxGOmyeBGgV0C0_IwUGEWMxwXElEBOZNxbQ5CFb4-gAK_g46Ku2yyUviOOo_v8seF3EtyT6Fy11QgJqMdrzrAYsYz1IjyhP3SKfMQbV9HQoS7NlaP0bzt4AZEeDoxHie0GTojf-sfqwBHBO5zdb3ucYqlvZ2Z4JVNw04NPiwayhOtH6_04SK3BM7PJKWNFkEMDPpUDSHxlwStg7ga9yyW0yd4bEd8v0LXXfZcQd_0EbQ23HIZk8R0uv8g7LKQv_kZqt");
            Assert.AreEqual(challenge.Form.Pass, "1589552416.106-DyI3gUa+pM");
            Assert.AreEqual(challenge.Form.R, "6bf123a2463a5fdc591b6597ec4d6d13423bdc2c-1589552412-0-AR0zK6J7JOO9P+Z4pdlkxvfRNPKgSxuAzQmEMF/hxn2LVx2oRKsM9H7BLAm52g3e6d/miF9sPfqWFdpKWIRCA/o3Ab7e04BpzzLfn7QWuuZxAkOQxihhcs+g71lhLT78craeQjox/KCw1H2QkWPlUw4PR8WlrIaZoqcXboliBLexCB9+TGAHp9OjiREskxBru3RtIM75xXGmKx19tuchKU9BQtkJRFAXRwiBARJhnzrKXZ5NZtdYvrylPoZnsnNsH8dLwqvjSmDBN2f+46yJlH7VUs4WAL0ZQh0YWCOgW/WNRCv4g0FlL2slQMlKgWEpGKRtySMi0vAKwzlNWs6Or+alg5Y1xj8aq+5ce9NKtwR1xuRjl5rdUa673ZCNL8UFsZgwV7F8veOBuVW/l+w5MebxY/9+IETu2M/lA3QK/TV7KiPbaE5RIvflyOJcRJqNZkWxj2kjBvucdL/BWXeK4wUJwECp9/VbwolohFqCkF/ABvvEdkh3BTf0v+6OII1VyT4G+fssXeOEGhlHHFUHbagpYhZhEtHrn8sDiIkd9EW4ZAuUz3cIHtc6DXXTujhSHJIZbf+xcUiwKsWQFW6oWqhywQA6FrHAzPyYWsWGlkZ03XqX/hGBnmiR9bEmGNkkc4+EabJ2R+xJ7vFQkwkKmZiAjSyEgc3mG3Hg5H9eF4GyPqi7Xe7xzm5r+kzxovJPi9DvRrh/bcYnaz3jU9kGHn2H00HBseaMig5MAND6RjiLNOnhYXW2QcbTaCvrWtm/KrcCxX5PdXk9OuWQnkhbxT9NyVWlzK1ZnE3RRbIjf20C281x0Xw4wBSeDqcQAnZf61e5SCmjfE6xMdqodB7/Vo7QEDYhNZpvT5E+pglSrrFToP8qkH5nhQ1bubxOdczx3gA1ZSIYSg10tR7RVE71qznDxu5e3hosu/j1bW99rJqtnpVCHQ0kEP/3rmyUHLMKDilt08/W/NThqs5d4ImRLuGTVrEzdOf1UNj++RfVqnr5+SwjLql2P3d73Uzy/7MfPn56CKoWlNYr07bnQbOau0GDEYVkk2BI11G0a3fyPw0y1gbPK9uqzpHIoP7mrHwd+iIjb7TSRW6TeGXPzAIYI5OrxCsdmpThArUekxQhtmpobG/uSlqy7qixISHnYQ7bwsmCiGftjj0wZr9SBMQ5L35V5Qp6RuuRLRgNVhQz7LEkYoFU8pLoAfA33bD3OGbh766PW97PYvgSXiZlgj9eavcHIKt+VCbKRtpEa5XEM1nP");
            Assert.AreEqual(challenge.Form.VerificationCode, "97d95831ffd5985c399d571b44a4bd68");

            // solve the challenge and assert known values
            var jschlAnswer = challenge.Solve();

            // the js code changes the form action to include the #hash (if there is hash)
            Assert.AreEqual(challenge.Form.Action, "/search/harry/0/?__cf_chl_jschl_tk__=17b0d0c134f396ab5c0c5d762e979166506c60c4-1589552412-0-AZWR07FASW_2aSkfJOKexq8OuE58LcBmUUfLGsynpwxGOmyeBGgV0C0_IwUGEWMxwXElEBOZNxbQ5CFb4-gAK_g46Ku2yyUviOOo_v8seF3EtyT6Fy11QgJqMdrzrAYsYz1IjyhP3SKfMQbV9HQoS7NlaP0bzt4AZEeDoxHie0GTojf-sfqwBHBO5zdb3ucYqlvZ2Z4JVNw04NPiwayhOtH6_04SK3BM7PJKWNFkEMDPpUDSHxlwStg7ga9yyW0yd4bEd8v0LXXfZcQd_0EbQ23HIZk8R0uv8g7LKQv_kZqt#test_hash");
            Assert.AreEqual(jschlAnswer, "9.2875653032");
        }
    }
}