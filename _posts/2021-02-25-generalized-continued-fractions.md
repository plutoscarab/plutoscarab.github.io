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
