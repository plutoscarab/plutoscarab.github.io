---
title: Converting Generalized Continued Fractions to Simple Continued Fractions in C#
tags: [math, C#]
---

In the [last post](/2021/02/21/continued-fractions-to-decimal) I showed how to convert a continued fraction of the form

$$
x = t_0 + \cfrac 1 {t_1 + \cfrac 1 {t_2 + \cfrac 1 {t_3 + \cdots}}}
$$

into a decimal fraction of arbitrary precision. This post is about converting a generalized continued fraction of the form

$$
x = t_0 + \cfrac {u_0} {t_1 + \cfrac {u_1} {t_2 + \cfrac {u_2} {t_3 + \cdots}}}
$$

into a simple continued fraction. There are a lot of interesting constants such as $$\pi$$ that have nice pattern for
the generalized continued fraction terms but have no discernable pattern in their simple continued fraction. For example, we
have

$$
\frac 4 \pi = 1 + \cfrac 1 {3 + \cfrac {2^2} {5 + \cfrac {3^2} {7 + \cfrac {4^2} {9 + \cfrac {5^2} {11 + \cdots}}}}}
$$

We'll use the same trick for "consuming" terms one-at-a-time. We represent the value as

$$
f(x) = \frac {a+bx}{c+dx}
$$

with starting values $$a=0, b=1, c=1, d=0$$.  Instead of a single sequence of terms $$t_i$$ and $$x=t_0+\frac 1 y$$ we instead
have two sequences $$t_i$$ and $$u_i$$ and $$x=t_0+\frac {u_0} y$$. Plugging this into $$f(x)$$ we get

$$
\begin{align}
f(x) &= \frac {a + b(t_0 + \frac {u_0} y)} {c + d(t_0 + \frac {u_0} y)} \\
&= \frac {ay + b(t_0y + u_0)} {cy + d(t_0y + u_0)} \\
&= \frac {u_0b + (a+t_0b)y} {u_0d + (c+t_0d)y}
\end{align}
$$

This transforms the state variables in the following way:

$$
\begin{align}
a &\to u_0b \\
b &\to a+t_0b \\
c &\to u_0d \\
d &\to c+t_0d 
\end{align}
$$

When we wanted to generate decimal digits, we repeatedly subtracted the integer part of the value and multiplied by 10. Instead, we want to extract
the simple continued fraction terms, so we'll repeatedly subtract the integer part of the value and then take the reciprocal. Here's the final result.

```csharp
    public static IEnumerable<BigInteger> Simplify(
        IEnumerable<BigInteger> ts,
        IEnumerable<BigInteger> us)
    {
        BigInteger a = 0, b = 1, c = 1, d = 0;
        using (var e = us.GetEnumerator())
        {
            foreach (var t in ts)
            {
                var u = e.MoveNext() ? e.Current : BigInteger.One;
                (a, b) = (u * b, a + t * b);
                (c, d) = (u * d, c + t * d);

                while (!c.IsZero && !d.IsZero)
                {
                    var m = BigInteger.DivRem(a, c, out var r);
                    var n = BigInteger.DivRem(b, d, out var s);

                    if (m != n)
                        break;

                    yield return n;
                    (a, c) = (c, r);
                    (b, d) = (d, s);
                }
            }

            while (!b.IsZero && !d.IsZero)
            {
                var n = BigInteger.DivRem(b, d, out var s);
                yield return n;
                (b, d) = (d, s);
            }
        }
    }
```

We can try it out for $$\frac 4 \pi$$.

```csharp
    static void Main(string[] args)
    {
        BigInteger two = 2;
        var odds = Enumerable.Range(0, int.MaxValue).Select(i => two * i + 1);
        var squares = Enumerable.Range(1, int.MaxValue).Select(i => i * (BigInteger)i);
        var fourOverPi = Simplify(odds, squares);
        Write(fourOverPi, 50, Console.Out);
        Console.WriteLine();
    }
```

This produces the following output, which is correct!

```
1.27323954473516268615107010698011489627567716592365
```

The `Simplify` function can sometimes return terms that are not valid for simple continued fractions. For example, if negative inputs are provided it can
produce negative outputs. Whether or not this is an issue depends on the application.

To produce the continued fraction terms for $$\pi$$ from the terms for $$\frac 4 \pi$$ we just need to be able to compute $$\frac 4 x$$ for a continued fraction $$x$$.
That's straightforward with the same technique we've used before, using $$f(x)=\frac{a+bx}{c+dx}$$. We just have to use $$a=4, b=0, c=0, d=1$$. So let's make
a general-purpose C# function for $$f(x)$$.

```csharp
    public static IEnumerable<BigInteger> Transform(
        IEnumerable<BigInteger> terms,
        BigInteger a, BigInteger b,
        BigInteger c, BigInteger d)
    {
        foreach (var term in terms)
        {
            (a, b) = (b, a + term * b);
            (c, d) = (d, c + term * d);

            while (!c.IsZero && !d.IsZero)
            {
                var m = BigInteger.DivRem(a, c, out var r);
                var n = BigInteger.DivRem(b, d, out var s);

                if (m != n)
                    break;

                yield return n;
                (a, c) = (c, r);
                (b, d) = (d, s);
            }
        }

        while (!b.IsZero && !d.IsZero)
        {
            var n = BigInteger.DivRem(b, d, out var s);
            yield return n;
            (b, d) = (d, s);
        }
    }
```

And then we can generate the terms for $$pi$$.

```csharp
        var pi = Transform(fourOverPi, 4, 0, 0, 1);

        foreach (var term in pi.Take(1000))
        {
            Console.Write($"{term} ");
        }

        Console.WriteLine();
```

which produces

```
3 7 15 1 292 1 1 1 2 1 3 1 14 2 1 1 2 2 2 2 1 84 2 1 1 15 3 13 1 4 2 6 6 99 1 2 2 6 3 5 1 1 6 8 1 7 1 2 3 7 1 2 1 1 12 1 1 1 3 1 1 8 1 1 2 1 6 1 1 5 2 2 3 1 2 4 4 16 1 161 45 1 22 1 2 2 1 4 1 2 24 1 2 1 3 1 2 1 1 10 2 5 4 1 2 2 8 1 5 2 2 26 1 4 1 1 8 2 42 2 1 7 3 3 1 1 7 2 4 9 7 2 3 1 57 1 18 1 9 19 1 2 18 1 3 7 30 1 1 1 3 3 3 1 2 8 1 1 2 1 15 1 2 13 1 2 1 4 1 12 1 1 3 3 28 1 10 3 2 20 1 1 1 1 4 1 1 1 5 3 2 1 6 1 4 1 120 2 1 1 3 1 23 1 15 1 3 7 1 16 1 2 1 21 2 1 1 2 9 1 6 4 127 14 5 1 3 13 7 9 1 1 1 1 1 5 4 1 1 3 1 1 29 3 1 1 2 2 1 3 1 1 1 3 1 1 10 3 1 3 1 2 1 12 1 4 1 1 1 1 7 1 1 2 1 11 3 1 7 1 4 1 48 16 1 4 5 2 1 1 4 3 1 2 3 1 2 2 1 2 5 20 1 1 5 4 1 436 8 1 2 2 1 1 1 1 1 5 1 2 1 3 6 11 4 3 1 1 1 2 5 4 6 9 1 5 1 5 15 1 11 24 4 4 5 2 1 4 1 6 1 1 1 4 3 2 2 1 1 2 1 58 5 1 2 1 2 1 1 2 2 7 1 15 1 4 8 1 1 4 2 1 1 1 3 1 1 1 2 1 1 1 1 1 9 1 4 3 15 1 2 1 13 1 1 1 3 24 1 2 4 10 5 12 3 3 21 1 2 1 34 1 1 1 4 15 1 4 44 1 4 20776 1 1 1 1 1 1 1 23 1 7 2 1 94 55 1 1 2 1 1 3 1 1 32 5 1 14 1 1 1 1 1 3 50 2 16 5 1 2 1 4 6 3 1 3 3 1 2 2 2 5 2 2 2 28 1 1 13 1 5 43 1 4 3 5 3 1 4 1 1 2 2 1 1 19 2 7 1 72 3 1 2 3 7 11 1 2 1 1 2 2 1 1 2 1 1 1 1 1 33 7 19 1 19 3 1 4 1 1 1 1 2 3 1 3 2 2 2 2 4 1 1 1 4 2 3 1 1 1 1 11 1 1 2 1 2 1 2 2 1 7 2 27 1 1 6 2 1 9 6 26 1 1 3 2 1 1 1 1 1 15 1 36 4 2 2 1 22 2 1 106 2 2 1 3 1 12 10 7 1 2 1 1 1 1 8 2 4 5 3 2 1 4 23 1 18 2 10 3 1 6 6 13 8 6 2 2 2 2 1 1 1 3 1 7 17 1 1 1 2 5 5 1 1 2 11 1 6 1 6 1 29 4 29 3 5 3 1 141 1 2 7 7 2 2 7 1 1 7 1 7 1 2 4 1 1 1 30 1 12 4 18 10 2 8 1 2 2 2 4 13 1 5 4 1 6 1 1 11 2 4 2 1 1 3 3 12 1 1 39 5 1 1 16 125 1 4 1 2 1 19 1 4 1 1 2 1 4 1 10 1 4 2 1 1 1 5 10 4 14 1 13 41 1 4 1 8 1 1 2 1 3 1 6 1 3 2 2 2 1 4 1 14 1 2 8 1 8 3 3 3 1 37 4 2 4 1 3 4 25 4 27 2 7 1 1 2 6 1 1 1 12 1 2 2 2 13 12 1 3 1 6 1 1 33 1 5 3 1 5 15 8 8 47 1 3 2 12 2 12 1 12 1 2 5 3 1 1 1 1 2 3 5 4 2 1 1 5 1 9 14 1 1 3 2 1 9 3 22 13 1 1 3 20 1 1 61 1 376 2 107 1 10 3 2 2 31 1 2 10 2 2 62 2 2 7 4 5 6 1 1 1 1 2 8 2 73 3 5 42 1 3 2 1 1 59 6 1 1 1 5 1 6 1 2 6 1 1 1 1 3 2 1 3 1 8 1 4 2 5 4 7 1 4 2 2 6 1 1 2 2 1 1 1 1 1 2 1 2 2 5 1 2 1 1 10 1 6 1 129 1 4 65 2 4 4 3 2 3 1 1 5 1 1 1 1 1 2 2 1 2 1 1 2 2 1 2 3 1 2 1 2 4 2 1 2 27 6 2 
```
