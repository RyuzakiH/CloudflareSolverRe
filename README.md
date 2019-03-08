![](https://i.imgur.com/c4FeZHz.png)

## CloudflareSolver

![](https://img.shields.io/github/release/Zaczero/CloudflareSolver.svg)
![](https://img.shields.io/nuget/v/CloudflareSolver.svg)
![](https://img.shields.io/github/license/Zaczero/CloudflareSolver.svg)

Cloudflare JavaScript & ReCaptchaV2 challenge solving library (aka. *Under Attack Mode* bypass).

*Inspired by [CloudFlareUtilities
](https://github.com/elcattivo/CloudFlareUtilities)*

## Requirements

* This library is compiled with .NET Standard 1.3. Check full implementation support [here](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
* To use the ReCaptchaV2 solver you must create an account on [2Captcha](http://2captcha.com/?from=6591885)

## Download

* https://github.com/Zaczero/CloudflareSolver/releases/latest

## Getting started

```cs
////
// If you do not want to use the ReCaptchaV2 solver simply remove the parameter
////
var cf = new CloudflareSolver( YOUR_2CAPTCHA_KEY );
var uri = new Uri("https://uam.zaczero.pl/");

var httpClientHandler = new HttpClientHandler();
var httpClient = new HttpClient(httpClientHandler);

var result = cf.Solve(httpClient, httpClientHandler, uri).Result;
if (result.Success)
{
    Console.WriteLine($"Success! Protection bypassed: {result.DetectResult.Protection}");
}
else
{
    Console.WriteLine($"Fail :( => Reason: {result.FailReason}");
    return;
}

////
// Once the protection has been bypassed we can use that httpClient to send the requests as usual
////
var response = httpClient.GetAsync(uri).Result;
var html = response.Content.ReadAsStringAsync().Result;

Console.WriteLine($"Real response: {html}");
```

## Donate ❤️

* BTC: `1NjW3K26ZPZeveW4st4sC249MfyW2w5ZP8`
* ETH: `0x56b4ED755b7bDD75A954e168EB96f4501F75342d`

Thanks for your support!

## License
### CloudflareSolver

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

### Newtonsoft.Json ([GitHub](https://github.com/JamesNK/Newtonsoft.Json))

The MIT License (MIT)

Copyright (c) 2007 James Newton-King

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

### Jint ([GitHub](https://github.com/sebastienros/jint))

BSD 2-Clause License

Copyright (c) 2013, Sebastien Ros
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.