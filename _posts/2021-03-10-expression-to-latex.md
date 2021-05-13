---
title: Generating LaTeX from C# Expressions
tags: [ math, C# ]
---

In this blog I've been using [LaTeX](https://en.wikipedia.org/wiki/LaTeX) for rendering mathematics
since Github Hackdown doesn't cut it. I did this by making a copy of the template's
_layouts\default.html and adding this to the head and then using $$ to delimit the math:

```html
<script type="text/javascript" id="MathJax-script" defer src="https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-svg.js">
</script>
```

While writing blog entries I've been finding myself writing expressions in C# and then writing
formatting code to emit LaTeX versions of those expressions. I decided to play with using
[expression
trees](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/)
to automate this. There hasn't been much opportunity to use this but I thought it was fun.

Some examples of the results:

|C# Expression|LaTeX|Rendered|
|-------------|-----|--------|
|`LaTeX.From((double x) => Math.Log(x * Math.Sqrt(x - 1) * Math.Sqrt(x + 1)))`|\ln(x\sqrt{x-1}\sqrt{x+1})|$$\ln(x\sqrt{x-1}\sqrt{x+1})$$|
|`LaTeX.From((double x) => byte.MaxValue - Math.PI / (Math.E + x))`|255-\frac{\pi}{e+x}|$$255-\frac{\pi}{e+x}$$|
|`LaTeX.From((bool b) => !b && (b || false))`|\neg b\wedge (b\vee F)|$$\neg b\wedge (b\vee F)$$|
|`LaTeX.From((double z) => (+z) * (-z))`|z(-z)|$$z(-z)$$|

The code is easy and mostly tedious and so I won't go into the details here, but you can check it
out on Github (link above).