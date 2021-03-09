---
title: Best Rational Representation of Double-Precision Floating Point Number
tags: [ math, C# ]
---

IEEE double-precision floating-point numbers are rational numbers. Their denominator
is always a power of two. For example, the double-precision representation of 1/3 is
actually

$$
\frac {6\ 004\ 799\ 503\ 160\ 661} {2^{54}} = \frac {6\ 004\ 799\ 503\ 160\ 661} {1\ 801\ 439\ 850\ 948\ 1984}
$$

but in a more concise form where the exponent of the denominator is stored instead of
the full denominator.
In C# we can use `BitConverter` to extract the bits from a `double` to reveal its 
secrets.

```csharp
public record Rational(BigInteger p, BigInteger q)
{
    public static explicit operator Rational(double d)
    {
        const int exponentBits = 11;
        const int mantissaBits = 63 - exponentBits;
        const int exponentMask = (1 << exponentBits) - 1;
        const int exponentBias = exponentMask / 2 + mantissaBits;
        const long mantissaMsb = 1L << mantissaBits;
        const long mantissaMask = mantissaMsb - 1;

        var bits = BitConverter.DoubleToInt64Bits(d);
        var sign = bits < 0 ? -1 : +1;
        var exponent = (int)(bits >> mantissaBits) & exponentMask;
        var mantissa = bits & mantissaMask;

        if (exponent == 0)
        {
            exponent = 1;

            if (mantissa == 0)
            {
                return new Rational(BigInteger.Zero, BigInteger.One);
            }
        }
        else
        {
            mantissa |= mantissaMsb;
        }

        exponent -= exponentBias;

        if (exponent >= 0)
        {
            return new Rational(BigInteger.Pow(2, exponent) * mantissa * sign, BigInteger.One);
        }

        var p = (BigInteger)mantissa;
        var q = BigInteger.Pow(2, -exponent);
        var g = BigInteger.GreatestCommonDivisor(p, q);
        return new Rational(p * sign / g, q / g);
    }
}
```

There are an infinite number of possible rational numbers that result in the same
double-precision value for 1/3. The rational 1/3 could be considered the "best" of these
in the sense that it has the smallest numerator or denominator of all of them.

If we want to recover the "best" rational representation of a double-precision number
we just need to find the one with the smallest denominator that is within the range
of the number plus-or-minus a half [ULP](https://en.wikipedia.org/wiki/Unit_in_the_last_place).
We can get the bounds of this range by taking the exact rational value, doubling the
numerator and denominator, and then subtracting or adding one to the numerator.

Continued fractions give us a way to find the rational with the smallest
denominator that is within this range. But first we need to know how to get the continued
fraction representation of a rational number. This is straightforward from the definition
of a simple continued fraction: just take the integer portion (via division) as the 
first term, then loop using the reciprocal of the fractional portion.

Note the special handling for negative numbers due to the behavior of `DivRem`. This
uses the `Negate` function from a [previous post](2021-02-21-continued-fractions-to-decimal).

```csharp
public static IEnumerable<BigInteger> FromRatio(BigInteger p, BigInteger q)
{
    if (p.Sign * q.Sign == -1)
    {
        foreach (var d in Negate(FromRatio(BigInteger.Abs(p), BigInteger.Abs(q))))
        {
            yield return d;
        }

        yield break;
    }

    while (true)
    {
        var d = BigInteger.DivRem(p, q, out var r);
        yield return d;

        if (r.IsZero)
        {
            yield break;
        }

        (p, q) = (q, r);
    }
}
```

If we apply this to the exact ratio for the double-precision expression 1.0 / 3 we get

```
[0; 3, 6004799503160661]
```

which is telling us that the double-precision representation is off by the
desired value by 1 part in 6 quadrillion. Close enough for most applications but
certainly not all.

The end-points of the half-ULP range are

$$
\frac {12\ 009\ 599\ 006\ 321\ 321} {36\ 028\ 797\ 018\ 963\ 968} \lt 
\frac 1 3 \lt 
\frac {12\ 009\ 599\ 006\ 321\ 323} {36\ 028\ 797\ 018\ 963\ 968}
$$

The continued-fraction representations of these endpoints are

```
[0; 3, 2401919801264264, 5]
[0; 2, 1, 12009599006321322]
```

If we want to create the most concise continued fraction that is between these two values,
it's clear that it starts with [0; ...] and the next term must be either 2 or 3. But
[0; 2] is outside the range and [0; 3] is inside the range, so we choose [0; 3]. We got the obvious value 1/3 as the simplest rational number within the range.

For a more interesting case, let's use $$\pi$$. Using `Math.PI` we get the ratios

$$
\frac {14\ 148\ 475\ 504\ 056\ 879} {4\ 503\ 599\ 627\ 370\ 496} \lt
\frac {7\ 074\ 237\ 752\ 028\ 440} {2\ 251\ 799\ 813\ 685\ 248} \lt
\frac {14\ 148\ 475\ 504\ 056\ 881} {4\ 503\ 599\ 627\ 370\ 496}
$$

Note that the middle fraction isn't in reduced form! We need this non-reduced form to correctly
calculate the ULP endpoint values.
The $$\pi$$ value and endpoints have continued-fraction representations

```
Math.PI - ½ ULP: [3; 7, 15, 1, 292, 1, 1, 1, 2, 1, 3, 1, 14, 6, 2, 14, ...]
Math.PI:         [3; 7, 15, 1, 292, 1, 1, 1, 2, 1, 3, 1, 14, 3, 3, 2, ...]
Math.PI + ½ ULP: [3; 7, 15, 1, 292, 1, 1, 1, 2, 1, 3, 1, 14, 2, 5, 11, ...]
```

Notice that the terms disagree with $$\pi$$ right where they disagree with the +/- half
ULP terms. The actual terms of $$\pi$$ start with

```
[3; 7, 15, 1, 292, 1, 1, 1, 2, 1, 3, 1, 14, 2, 1, 1, ...]
```

The simplest continued fraction between the two endpoints is

```
[3; 7, 15, 1, 292, 1, 1, 1, 2, 1, 3, 1, 14, 3]
```

where we take one more than the minimum of the first disagreeing terms.
The value of the truncated continued fraction is 

$$
\frac {245\ 850\ 922} {78\ 256\ 779}
$$

This is considerably more concise than the exact rational double-precision value and yet it has the exact same double-precision representation.

```csharp
public static Rational Best(double d) => Best((Rational)d);

public static Rational Best(float f) => Best((Rational)f);

private static Rational Best(Rational r) => Best(
    new Rational(2 * r.p - 1, 2 * r.q), 
    new Rational(2 * r.p + 1, 2 * r.q));

public static Rational Best(Rational lo, Rational hi)
{
    var clo = FromRatio(lo.p, lo.q).ToList();
    var chi = FromRatio(hi.p, hi.q).ToList();
    var matching = clo.Zip(chi).TakeWhile(_ => _.First == _.Second).Count();
    var even = (matching & 1) == 0;
    var cf = clo.Take(matching).ToList();
    var min = BigInteger.Min(clo[matching], chi[matching]);
    cf.Add(min + 1);
    var (p, q) = ToRatio(cf);
    return new Rational(p, q);
}
```

A table of some common values:

|Value|Approximate|Double-precision|Single-precision|
|-----|-----------|:--------------:|:--------------:|
|$$\pi$$|3.141592653589793|$$\frac {245\ 850\ 922} {78\ 256\ 779}$$|$$\frac {93343} {29712}$$|
|$$e$$|2.718281828459045|$$\frac {268\ 876\ 667} {98\ 914\ 198}$$|$$\frac {2721} {1001}$$|
|$$\sqrt {2\pi}$$|2.5066282746310002|$$\frac {127\ 095\ 877} {50\ 703\ 919}$$|$$\frac {4349} {1735}$$|
|$$\phi$$|1.618033988749895|$$\frac {165\ 580\ 141} {102\ 334\ 155}$$|$$\frac {4181} {2584}$$|
|$$\sqrt 2$$|1.4142135623730951|$$\frac {131\ 836\ 323} {93\ 222\ 358}$$|$$\frac {4756} {3363}$$|
|$$\zeta(3)$$|1.202056903159594|$$\frac {89\ 952\ 803} {74\ 832\ 400}$$|$$\frac {1987} {1653}$$|
|$$G$$|0.915965594177219|$$\frac {105\ 640\ 241} {115\ 332\ 106}$$|$$\frac {9690} {10579}$$|
|$$ln(2)$$|0.6931471805599453|$$\frac {49\ 180\ 508} {70\ 952\ 475}$$|$$\frac {2731} {3940}$$|
|$$\gamma$$|0.5772156649015329|$$\frac {240\ 627\ 391} {416\ 876\ 058}$$|$$\frac {3035} {5258}$$|

