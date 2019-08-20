CloudflareSolverRe
==================
[![AppVeyor](https://img.shields.io/appveyor/ci/RyuzakiH/CloudflareSolverRe/master.svg?maxAge=60)](https://ci.appveyor.com/project/RyuzakiH/CloudflareSolverRe)
[![NuGet](https://img.shields.io/nuget/v/CloudflareSolverRe.svg?maxAge=60)](https://www.nuget.org/packages/CloudflareSolverRe)
[![NuGet](https://img.shields.io/nuget/v/CloudflareSolverRe.Captcha.svg?maxAge=60)](https://www.nuget.org/packages/CloudflareSolverRe.Captcha)

Cloudflare Javascript & reCaptcha challenge (I'm Under Attack Mode or IUAM) solving/bypass .NET Standard library.

_Reawakening of [CloudflareSolver](https://www.nuget.org/packages/CloudflareSolver) (removed) adding the capabilities ([DelegatingHandler](https://msdn.microsoft.com/en-us/library/system.net.http.delegatinghandler(v=vs.110).aspx)) of [CloudFlareUtilities](https://github.com/elcattivo/CloudFlareUtilities) (not working)._

This can be useful if you wish to scrape or crawl a website protected with Cloudflare. Cloudflare's IUAM page currently just checks if the client supports JavaScript or requires solving captcha challenge. Fortunately, this library supports both.

For reference, this is the default message Cloudflare uses for these sorts of pages:

```
Checking your browser before accessing website.com.

This process is automatic. Your browser will redirect to your requested content shortly.

Please allow up to 5 seconds...
```

The first visit to any site with Cloudflare IUAM enabled will sleep for 4 to 5 seconds until the challenge is solved, though no delay will occur after the first request.

# Installation
Full-Featured library:

`PM> Install-Package CloudflareSolverRe.Captcha`

Or get just javascript challenge solver without the captcha features:

`PM> Install-Package CloudflareSolverRe`

# Dependencies
- No dependencies (no javaScript interpreter required)
- [.NET Standard 1.1](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard1.1.md)

In case you need to use captcha solvers:
#### CloudflareSolverRe.Captcha
- [2CaptchaAPI](https://www.nuget.org/packages/2CaptchaAPI/)
- [AntiCaptchaAPI](https://www.nuget.org/packages/AntiCaptchaAPI/)

If you want to use another captcha provider, see [How to implement a captcha provider?](#implement-a-captcha-provider)

# Issues
Cloudflare regularly modifies their IUAM protection challenge and improves their bot detection capabilities.

If you notice that the anti-bot page has changed, or if library suddenly stops working, please create a GitHub issue so that I can update the code accordingly.

Before submitting an issue, just be sure that you have the latest version of the library.

# Usage

- ### ClearanceHandler

A [DelegatingHandler](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.delegatinghandler?view=netstandard-1.1) that
handles the challenge solution automatically.

> A type for HTTP handlers that delegate the processing of HTTP response messages to another handler, called the inner handler.

It checks on every request if the clearance is required or not, if required, it solves the challenge in background then returns the response.

Websites not using Cloudflare will be treated normally. You don't need to configure or call anything further, and you can effectively treat all websites as if they're not protected with anything.

```csharp
var target = new Uri("https://uam.hitmehard.fun/HIT");

var handler = new ClearanceHandler
{
    MaxTries = 3,
    ClearanceDelay = 3000
};

var client = new HttpClient(handler);

var content = client.GetStringAsync(target).Result;
Console.WriteLine(content);
```


- ### CloudflareSolver
The internal challenge solver, that's what happens inside the ClearanceHandler, you can use it directly.

Use it when you already have a HttpClient and you want to solve the challenge manually for some specific website so you can scrape it freely.

```csharp
var target = new Uri("https://uam.hitmehard.fun/HIT");

var cf = new CloudflareSolver
{
    MaxTries = 3,
    ClearanceDelay = 3000
};

var handler = new HttpClientHandler();
var client = new HttpClient(handler);

var result = cf.Solve(client, handler, target).Result;

if (!result.Success)
{
    Console.WriteLine($"[Failed] Details: {result.FailReason}");
    return;
}

// Once the protection has been bypassed we can use that HttpClient to send the requests as usual
var content = client.GetStringAsync(target).Result;
Console.WriteLine($"Server response: {content}");
```

**Full Samples [Here](https://github.com/RyuzakiH/CloudflareSolverRe/tree/master/sample/CloudflareSolverRe.Sample)**

# Options
### Message Handlers
To use a message handler ([HttpClientHandler](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclienthandler), [SocketsHttpHandler](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.socketshttphandler), etc.) with ClearanceHandler, provide it as an inner handler.

To provide  an inner handler, there are two ways

1. By setting the InnerHandler property of the ClearanceHandler:

```csharp
var handler = new ClearanceHandler
{
    InnerHandler = new HttpClientHandler
    {
        CookieContainer = cookieContainer,
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        Proxy = proxy
    },
    MaxTries = 5,
    ClearanceDelay = 3000
};
```

2. By passing the inner handler to the constructor of the ClearanceHandler.

```csharp
var httpHandler = new HttpClientHandler
{
    CookieContainer = cookieContainer,
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
    Proxy = proxy
};

var handler = new ClearanceHandler(httpHandler)
{
    MaxTries = 5,
    ClearanceDelay = 3000
};
```

### Maximum Tries
The maximum number of challenge solving tries. Most of the time the minimum tries required are 2 or 3, so default value is 3.
If you would like to override this number to make sure it always succeed, change MaxTries property.

```csharp
var handler = new ClearanceHandler
{
    MaxTries = 10
};
```
```csharp
var cf = new CloudflareSolver
{
    MaxTries = 10
};
```

Cloudflare challenges are not always javascript challenges, it may be reCaptcha challenges and this library also provides a way to solve these challenges using captcha solvers (2captcha, anti-captcha, etc.).

So, there's MaxCaptchaTries property to set the max number of captcha challenge solving tries (default is 1).

```csharp
handler.MaxCaptchaTries = 2;
```
```csharp
cf.MaxCaptchaTries = 2;
```

### Delays
Normally, when a browser is faced with a Cloudflare IUAM challenge page, Cloudflare requires the browser to wait 4 seconds (default delay) before submitting the challenge answer. If you would like to override this delay, change ClearanceDelay property.

```csharp
var handler = new ClearanceHandler
{
    ClearanceDelay = 5000
};
```
```csharp
var cf = new CloudflareSolver
{
    ClearanceDelay = 5000
};
```

### User-Agent
**User-Agent must be the same as the one used to solve the challenge, otherwise Cloudflare will flag you as a bot.**

You can set the user-agent of ClearanceHandler once and it will be used along with the handler.
All requests made using that handler will have this user-agent, cannot be changed even if you set the user-agent header explicitly.
```csharp
var handler = new ClearanceHandler(
    userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");
```
If you didn't set it, a random user-agent will be generated, also cannot be changed even if you set the user-agent header explicitly.

Also, you can set the user-agent of CloudflareSolver once and it will be used to solve every challenge.
But unlike the ClearanceHandler, this user-agent can be changed to a specific value or set to random.
```csharp
var cf = new CloudflareSolver(
    userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");
```
To use a random user-agent set randomUserAgent to true when calling Solve method.
```csharp
var result = await cf.Solve(client, handler, target, randomUserAgent: true);
```
To use a specific user-agent set userAgent.
```csharp
var result = await cf.Solve(client, handler, target, userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");
```


# Integration
It's easy to integrate CloudflareSolverRe with other applications and tools. Cloudflare uses two cookies as tokens: 

1. [\__cfuid](https://support.cloudflare.com/hc/en-us/articles/200170156-What-does-the-Cloudflare-cfduid-cookie-do-)
> The \__cfduid cookie is used to identify individual clients behind a shared IP address and apply security settings on a per-client basis.

2. [cf_clearance](https://blog.cloudflare.com/cloudflare-supports-privacy-pass/)
> Clearance cookies are like authentication cookies, but instead of being tied to an identity, they are tied to the fact that you solved a challenge sometime in the past.

To bypass the challenge page, simply include both of these cookies (with the appropriate user-agent) in all HTTP requests you make.

**To retrieve working cookies and a user-agent for a specific website:**

```csharp
var target = new Uri("https://uam.hitmehard.fun/HIT");

var cf = new CloudflareSolver
{
    MaxTries = 3,
    ClearanceDelay = 3000
};

var result = await cf.Solve(target);
```

Can be used with a proxy:
```csharp
IWebProxy proxy = new WebProxy("51.83.15.1:8080");

var result = await cf.Solve(target, proxy);
```

User-agent can be set to
1. random:
```csharp
var result = await cf.Solve(target, randomUserAgent: true);
```
2. specific value
```csharp
var result = await cf.Solve(target, userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");
```

CancellationToken can be passed to control solving operation cancelling:
```csharp
var cancellationToken = new CancellationTokenSource();

var result = await cf.Solve(target, cancellationToken: cancellationToken.Token);
```

Returns [SolveResult](https://github.com/RyuzakiH/CloudflareSolverRe/blob/master/src/CloudflareSolverRe/Types/SolveResult.cs) struct:
```csharp
public struct SolveResult
{
    public bool Success; // indicates that challenge solve process succeeded
    public string FailReason // indicates clearance fail reason, if failed
    public DetectResult DetectResult; // indicates the protection type found (js, captcha, banned, unknown, no-protection)
    public string UserAgent; // the user-agent used to solve the challenge (must be used during the session)
    public SessionCookies Cookies; // the two tokens/cookies to use during the session indicating that we passed the challenge.
}
```

Session cookies can be retrieved as
```csharp
var cookiesHeaderValue = result.Cookies.AsHeaderString(); // __cfduid=anyvalue;cf_clearance=anyvalue;
var cookieCollection = result.Cookies.AsCookieCollection();
var cookieContainer = result.Cookies.AsCookieContainer();
```

Remember, you must always use the same user-agent when retrieving or using these cookies.

### WebClient
Here is an example of integrating CloudflareSolver with WebClient. As you can see, all you have to do is pass the cookies and user-agent to the webclient headers.

```csharp
var target = new Uri("https://uam.hitmehard.fun/HIT");

var cf = new CloudflareSolver
{
	MaxTries = 3,
	ClearanceDelay = 3000
};

var result = cf.Solve(target).Result;

if (!result.Success)
{
    Console.WriteLine($"[Failed] Details: {result.FailReason}");
    return;
}

var client = new WebClient();
client.Headers.Add(HttpRequestHeader.Cookie, result.Cookies.AsHeaderString());
client.Headers.Add(HttpRequestHeader.UserAgent, result.UserAgent);

var content = client.DownloadString(target);
Console.WriteLine($"Server response: {content}");
```

### HttpWebRequest
Here is an example of integrating CloudflareSolver with HttpWebRequest. As you can see, all you have to do is pass the cookies and user-agent to the HttpWebRequest headers.

```csharp
var target = new Uri("https://uam.hitmehard.fun/HIT");

var cf = new CloudflareSolver
{
	MaxTries = 3,
	ClearanceDelay = 3000
};

var result = cf.Solve(target).Result;

if (!result.Success)
{
    Console.WriteLine($"[Failed] Details: {result.FailReason}");
    return;
}

var request = (HttpWebRequest)WebRequest.Create(target);
request.Headers.Add(HttpRequestHeader.Cookie, result.Cookies.AsHeaderString());
request.Headers.Add(HttpRequestHeader.UserAgent, result.UserAgent);

var response = (HttpWebResponse)request.GetResponse();
var content = new StreamReader(response.GetResponseStream()).ReadToEnd();
Console.WriteLine($"Server response: {content}");
```


# Captcha
To use captcha solving capabilities, you can install [CloudflareSolverRe.Captcha](https://www.nuget.org/packages/CloudflareSolverRe.Captcha) package which supports the following captcha providers.

- [Anti-Captcha](https://anti-captcha.com)
- [2captcha](https://2captcha.com)

_If you want to use another captcha solver, see [How to implement a captcha provider?](#implement-a-captcha-provider)_

- ### ClearanceHandler

```csharp
var handler = new ClearanceHandler(new TwoCaptchaProvider("YOUR_API_KEY"))
{
    MaxTries = 3,
    MaxCaptchaTries = 2
};
```

- ### CloudflareSolver

```csharp
var cf = new CloudflareSolver(new TwoCaptchaProvider("YOUR_API_KEY"))
{
    MaxTries = 3,
    MaxCaptchaTries = 1
};
```


# Implement a Captcha Provider
Implement [ICaptchaProvider](https://github.com/RyuzakiH/CloudflareSolverRe/blob/master/src/CloudflareSolverRe/Types/Captcha/ICaptchaProvider.cs) interface.

```csharp
public interface ICaptchaProvider
{
    string Name { get; }

    Task<CaptchaSolveResult> SolveCaptcha(string siteKey, string webUrl);
}
```

Example [AntiCaptchaProvider](https://github.com/RyuzakiH/CloudflareSolverRe/blob/master/src/CloudflareSolverRe.Captcha/AntiCaptchaProvider.cs)

```csharp
public class AntiCaptchaProvider : ICaptchaProvider
{
    public string Name { get; } = "AntiCaptcha";

    private readonly AntiCaptcha antiCaptcha;

    public AntiCaptchaProvider(string apiKey) => antiCaptcha = new AntiCaptcha(apiKey);

    public async Task<CaptchaSolveResult> SolveCaptcha(string siteKey, string webUrl)
    {
        var result = await antiCaptcha.SolveReCaptchaV2(siteKey, webUrl);

        return new CaptchaSolveResult
        {
            Success = result.Success,
            Response = result.Response,
        };
    }
}
```

# Tested Sites
- [uam.hitmehard.fun](https://uam.hitmehard.fun/HIT)
- [hdmovie8.com](https://hdmovie8.com)
- [japscan.to](https://www.japscan.to)
- [spacetorrent.cloud](https://www.spacetorrent.cloud)
- [codepen.io](https://codepen.io) (captcha challenges only - js challenges not allowed)
- [temp-mail](https://temp-mail.org) (not always using cloudflare)
- [hidemy.name](https://hidemy.name/en/proxy-list/)
- [speed.cd](https://speed.cd)
- [gktorrent.biz](https://www.gktorrent.biz/)
- [humanweb.fr](https://www.humanweb.fr/)
- [steamdb.info](https://steamdb.info/)
