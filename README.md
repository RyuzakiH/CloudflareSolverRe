![](https://i.imgur.com/c4FeZHz.png)

## CloudflareSolver

Cloudflare JavaScript & ReCaptchaV2 challenge solving library (aka. *Under Attack Mode* bypass).

*Inspired by [CloudFlareUtilities
](https://github.com/elcattivo/CloudFlareUtilities)*

## Requirements

* To use the JavaScript solver you must install [Node.js](https://nodejs.org/)
* To use the ReCaptchaV2 solver you must create an account on [2Captcha](http://2captcha.com/?from=6591885)

## Download

* https://github.com/Zaczero/CloudflareSolver/releases/latest

## Getting started

```cs
// If you don't want to use the ReCaptchaV2 solver - simply remove the parameter
var cf = new CloudflareSolver( YOUR_2CAPTCHA_KEY );

var httpClientHandler = new HttpClientHandler();
var httpClient = new HttpClient(httpClientHandler);

var result = cf.Solve(httpClient, httpClientHandler, new Uri("https://uam.hitmehard.fun/HIT")).Result;
if (result.Success)
{
    Console.WriteLine($"Success! Protection bypassed: {result.DetectResult.Protection}");
}
else
{
    Console.WriteLine($"Fail :( => Reason: {result.FailReason}");
}
```

## Donate ❤️

* BTC: `1JN8Ez4SiUU16znKyVJ8Bndu25x619ym2w`
* ETH: `0x56b4ED755b7bDD75A954e168EB96f4501F75342d`
* BCH: `qrx5sz2wzdrjn5an2euvqgf677gu7trxyc9jf3ldkn`
* XLM: `GAQVHFBRGXHLZHSDLR4GNG7O4TJ2WQUC7IFZYGAET4VDOYYOPNJWSGIB`

## License

MIT License

Copyright (c) 2019 Kamil Monicz

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.