CloudflareSolverRe
==================
[![AppVeyor](https://img.shields.io/appveyor/ci/RyuzakiH/CloudflareSolverRe/master.svg?maxAge=60)](https://ci.appveyor.com/project/RyuzakiH/CloudflareSolverRe)
[![NuGet](https://img.shields.io/nuget/v/CloudflareSolverRe.svg?maxAge=60)](https://www.nuget.org/packages/CloudflareSolverRe)
[![NuGet](https://img.shields.io/nuget/v/CloudflareSolverRe.Captcha.svg?maxAge=60)](https://www.nuget.org/packages/CloudflareSolverRe.Captcha)

Cloudflare Javascript & reCaptcha v2 challenge (Under Attack Mode) solving/bypass .NET Standard library.

_Reawakening of [CloudflareSolver](https://www.nuget.org/packages/CloudflareSolver) (removed) adding the capabilities ([DelegatingHandler](https://msdn.microsoft.com/en-us/library/system.net.http.delegatinghandler(v=vs.110).aspx)) of [CloudFlareUtilities](https://github.com/elcattivo/CloudFlareUtilities) (not working)._

# Features
- [.NET Standard 1.1](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard1.1.md)
- Two ways of solving (CloudflareSolver, ClearanceHandler)
- Captcha challenge solving using any [captcha provider](#implement-a-captcha-provider)
- No Javascript interpreter required

# Installation
Full-Featured library:

`PM> Install-Package CloudflareSolverRe.Captcha`

Or get just javascript challenge solver without the captcha features:

`PM> Install-Package CloudflareSolverRe`

# Usage

- ### ClearanceHandler

A [DelegatingHandler](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.delegatinghandler?view=netstandard-1.1) that
handles the challenge solution.

> A type for HTTP handlers that delegate the processing of HTTP response messages to another handler, called the inner handler.

It checks on every request if the clearance is required or not, if required, it solves the challenge in background then returns the response.

```csharp

var target = new Uri("https://uam.hitmehard.fun/HIT");

var handler = new ClearanceHandler
{
    MaxTries = 3, // Default value is 3
    ClearanceDelay = 3000 // Default value is the delay time determined in challenge code
};

var client = new HttpClient(handler);
client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

try
{
    var content = client.GetStringAsync(target).Result;
    Console.WriteLine(content);
}
catch (AggregateException ex) when (ex.InnerException is CloudFlareClearanceException)
{
    // After all retries, clearance still failed.
    Console.WriteLine(ex.InnerException.Message);
}
catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
{
    // Looks like we ran into a timeout. Too many clearance attempts?
    // Maybe you should increase client.Timeout as each attempt will take about five seconds.
}
```

With Captcha Provider

```csharp

var handler = new ClearanceHandler(new TwoCaptchaProvider("YOUR_API_KEY"))
{
    MaxTries = 3, // Default value is 3
    MaxCaptchaTries = 2, // Default value is 1
    //ClearanceDelay = 3000  // Default value is the delay time determined in challenge code (not required in captcha)
};

```

To provide an inner handler, there are two ways

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


- ### CloudflareSolver

The internal challenge solver, that's what happens inside the ClearanceHandler, you can use it directly.

```csharp

var target = new Uri("https://uam.hitmehard.fun/HIT");

var cf = new CloudflareSolver
{
    MaxTries = 3, // Default value is 3
    ClearanceDelay = 3000  // Default value is the delay time determined in challenge code
};

var httpClientHandler = new HttpClientHandler();
var httpClient = new HttpClient(httpClientHandler);
httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");

var result = cf.Solve(httpClient, httpClientHandler, target).Result;
if (result.Success)
{
    Console.WriteLine($"[Success] Protection bypassed: {result.DetectResult.Protection}");
}
else
{
    Console.WriteLine($"[Failed] Details: {result.FailReason}");
    return;
}

// Once the protection has been bypassed we can use that httpClient to send the requests as usual
var response = httpClient.GetAsync(target).Result;
var html = response.Content.ReadAsStringAsync().Result;

Console.WriteLine($"Server response: {html}");
```

With Captcha Provider

```csharp

var cf = new CloudflareSolver(new TwoCaptchaProvider("YOUR_API_KEY"))
{
    MaxTries = 3, // Default value is 3
    MaxCaptchaTries = 1, // Default value is 1
    //ClearanceDelay = 3000  // Default value is the delay time determined in challenge code (not required in captcha)
};

```

Full Samples [Here](https://github.com/RyuzakiH/CloudflareSolverRe/tree/master/sample/CloudflareSolverRe.Sample)

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
- [hitmehard](https://uam.hitmehard.fun/HIT)
- [hdmovie8](https://hdmovie8.com)
- [japscan](https://www.japscan.to)
- [spacetorrent.cloud](https://www.spacetorrent.cloud)
- [codepen](https://codepen.io) (captcha challenges only - js challenges not allowed)
- [temp-mail](https://temp-mail.org) (not always using cloudflare)
