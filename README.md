
CloudflareSolverRe
==================
[![AppVeyor](https://img.shields.io/appveyor/ci/RyuzakiH/CloudflareSolverRe/master.svg?maxAge=60)](https://ci.appveyor.com/project/RyuzakiH/CloudflareSolverRe)
[![NuGet](https://img.shields.io/nuget/v/CloudflareSolverRe.svg?maxAge=60)](https://www.nuget.org/packages/CloudflareSolverRe)
[![NuGet](https://img.shields.io/nuget/v/CloudflareSolverRe.Captcha.svg?maxAge=60)](https://www.nuget.org/packages/CloudflareSolverRe.Captcha)

Cloudflare JavaScript & ReCaptchaV2 challenge (Under Attack Mode) solving/bypass .NET Standard library.

_Reawakening of [CloudflareSolver](https://www.nuget.org/packages/CloudflareSolver) (removed) adding the capabilities ([DelegatingHandler](https://msdn.microsoft.com/en-us/library/system.net.http.delegatinghandler(v=vs.110).aspx)) of [CloudFlareUtilities](https://github.com/elcattivo/CloudFlareUtilities) (not working)._

# Features
- [.NET Standard 1.1](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard1.1.md)
- Two ways of solving (CloudflareSolver, ClearanceHandler)
- Captcha challenge solving using any [captcha provider](#implement-a-captcha-provider)

# Installation
Full-Featured library:

`PM> Install-Package CloudflareSolverRe.Captcha`

Or get just javascript challenge solver without the captcha features:

`PM> Install-Package CloudflareSolverRe`

# Usage

CloudflareSolver Example

```csharp

var target = new Uri("https://uam.hitmehard.fun/HIT");

/*
// With captcha provider:
var cf = new CloudflareSolver(new TwoCaptchaProvider("YOUR_API_KEY"))
{
    MaxTries = 3, // Default value is 3
    MaxCaptchaTries = 1, // Default value is 1
    //ClearanceDelay = 3000  // Default value is the delay time determined in challenge code (not required in captcha)
};
*/

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

ClearanceHandler Example

```csharp

var target = new Uri("https://uam.hitmehard.fun/HIT");

/*
// With captcha provider:
var handler = new ClearanceHandler(new TwoCaptchaProvider("YOUR_API_KEY"))
{
    MaxTries = 3, // Default value is 3
    MaxCaptchaTries = 2, // Default value is 1
    //ClearanceDelay = 3000  // Default value is the delay time determined in challenge code (not required in captcha)
};
*/

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

Full Samples [Here](https://github.com/RyuzakiH/Temp-Mail-API/blob/master/sample/CloudflareSolverRe.Sample)

# Implement a Captcha Provider
Implement [ICaptchaProvider](https://github.com/RyuzakiH/Temp-Mail-API/blob/master/src/CloudflareSolverRe/Types/Captcha/ICaptchaProvider.cs) interface.

Example [AntiCaptchaProvider](https://github.com/RyuzakiH/Temp-Mail-API/blob/master/src/CloudflareSolverRe.Captcha/AntiCaptchaProvider.cs)
