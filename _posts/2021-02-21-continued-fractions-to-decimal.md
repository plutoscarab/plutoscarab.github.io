---
title: Converting Continued Fractions to Decimal Fractions Using Arbitrary Precision in C#
tags: [math, C#]
---
It's surprisingly easy to compute the decimal value of simple continued fractions of the form

$$
x = t_0 + \cfrac 1 {t_1 + \cfrac 1 {t_2 + \cfrac 1 {t_3 + \cdots}}}
$$

without a lot of code, assuming you have a "big integer" data type available. I'll show you how to do it in C# using `System.Numerics.BigInteger`, and later we'll build on this to create a full-fledged continued fraction data type that we can use to do arithmetic.

We'll assume that we know how to convert integers into their decimal string representation, e.g. `BigInteger.ToString()`. Let's walk through an example using the `double` data type, and then we'll look at how to change this to use continued fractions.

```csharp
public static void Write(double x, int places, TextWriter writer)
{
```

Our method will take a `double` value and write it to the provided `TextWriter` with the specified number of decimal places of precision. Later we'll also want to be able to specify number format properties for globalization, but let's start with something simple.

First let's handle negative numbers and zeros. We'll ignore argument validation such as null checking for now.

```csharp
    if (x < 0)
    {
        writer.Write('-');
        Write(-x, places, writer);
        return;
    }
    
    if (x == 0)
    {
      writer.Write('0');
      return;
    }
```

Next we'll write out the integer portion, to the left of the decimal point. This implementation doesn't handle the "e" exponent notation like `1.1e37` so it will mess up numbers that are too large or too small, but this is just to show the general idea.
    
```csharp
    var n = Math.Floor(x);
    writer.Write(n);
    x -= n;
```

Now that we've done that and subtracted the integer portion out, we're left with a value $$0 <= x < 1$$. If we're at zero then we're done, otherwise we need to write the decimal point.

```csharp
    if (x == 0)
    {
        return;
    }
    
    writer.Write('.');
```

For the rest of the number we're going to extract one digit at a time. We do that by multiplying by 10 to shift the decimal point one place to the right, and then extracting the integer portion again. 

```csharp
    while (x != 0 && places-- > 0)
    {
        x *= 10;
        n = Math.Floor(x);
        writer.Write(n);
        x -= n;
    }
}
```

Because of how floating-point math works, there will be many values where this loop will continue a long time (or forever) if we didn't stop after the specified number of digits. For example, if you call it with `Math.PI` it will output the expected digits and then a bunch of noise digits. We also haven't handled rounding correctly in the case that the final digit is 5 or more. That's okay, though, as long as you get the general idea which is to keep multiplying by 10 and chopping off the integer portion.

Let's test it out:

```csharp
static void Main(string[] args)
{
    Write(Math.PI, 10, Console.Out);
    Console.WriteLine();
}
```

```
3.1415926535
```

To get this to work with continued fractions, we need to be able to extract the integer portion of the value (which is easy), to negate the value (which is not too hard), and to be able to multiply by 10 which is not nearly as easy as it sounds. 

## Negating a continued fraction

In simple continued fractions only $$t_0$$ can be negative. All the other terms $$t_1, t_2, \dots$$ are assumed to be strictly positive. Let's think of our continued fraction $$x = [t_0; t_1, t_2, \cdots]$$ as $$x = t_0 + \frac 1 y$$ where $$y = [t_1; t_2, t_3, \cdots]$$.

$$
\begin{align}
-x &= -t_0 - \cfrac 1 y \\
&= -t_0 - 1 + 1 - \cfrac 1 y \\
\end{align}
$$

It's easy to show that

$$
1 - \cfrac 1 y = \cfrac 1 {1 + \cfrac 1 {y - 1}}
$$

so that

$$
\begin{align}
-x &= -t_0 - 1 + \cfrac 1 {1 + \cfrac 1 {y - 1}} \\
&= [-t_0 - 1; 1, t_1 - 1, t_2, \cdots]
\end{align}
$$

Now if $$t_1 = 1$$ then the $$t_1 - 1$$ term becomes zero, and we don't allow fraction terms that aren't positive. In that case we can use the identity

$$
[\cdots, a, 0, b, \cdots] = [\cdots, a + b, \cdots]
$$

to eliminate the zero and arrive at

$$
-x = [-t_0 - 1; t_2 + 1, t_3, \cdots]
$$

for the $$t_1 = 1$$ case. 

In code, we change from `double` to a series of terms represented by `IEnumerable<BigInteger>`.

```csharp
public static IEnumerable<BigInteger> Negate(IEnumerable<BigInteger> terms)
{
    var arr = terms.Take(3).ToArray();
    
    if (arr.Length == 0)
    {
        return Enumerable.Empty<BigInteger>();
    }
    
    if (arr.Length == 1)
    {
        return new[] { -arr[0] };
    }
    
    if (arr[1] == BigInteger.One)
    {
        return new[] { -arr[0] - BigInteger.One, arr[2] + BigInteger.One }
            .Concat(terms.Skip(3));
    }
    
    return new[] { -arr[0] - BigInteger.One, BigInteger.One, 
        arr[1] - BigInteger.One }.Concat(terms.Skip(2));
}

public static void Write(IEnumerable<BigInteger> terms, int places, 
    TextWriter writer)
{
    if (!terms.Any())
    {
        writer.Write('∞');
        return;
    }
    
    if (terms.First().Sign < 0)
    {
        writer.Write('-');
        Write(Negate(terms), places, writer);
        return;
    }
    
    // Write the integer portion.
    writer.Write(terms.First());
    
    // We're done if there is no fractional part.
    if (terms.Take(2).Count() == 1)
    {
        return;
    }
    
    // Now we're ready to do the decimal fraction part.
    writer.Write('.');
```

## Multiplying by 10

Consider the continued fraction $$[1; 2, 3, 4]$$ which is short-hand for

$$
1 + \cfrac 1 {2 + \cfrac 1 {3 + \cfrac 1 4}} = \frac {43} {30} \approx 1.43333
$$

Multiply by 10 and you should get $$[14; 3]$$ which is nothing like $$[10; 20, 30, 40]$$ or $$[12; 3, 4]$$ or whatever else you might guess $$10 \cdot [1; 2, 3, 4]$$ would look like.

We could compute the ratio $$\frac {43} {30}$$ from the continued fraction, multiply that by 10, and then convert back to continued fraction, but I want to be able to use this on infinite continued fractions, using lazy computation to process one term at a time. So that approach won't work.

To calculate $$10x$$ using term-by-term computation, we really need to be able to compute $$10 \cdot (t_0 + \frac 1 y)$$ where $$t_0$$ is the first term of $$x$$, and $$y$$ represents the remaining terms.  For example, for $$x = [1; 2, 3, 4]$$ we have $$t_0 = 1$$ and $$y = [2; 3, 4]$$.  Mathematician Bill Gosper figured out the trick to this. We actually compute

$$
f(x) = \frac {a + bx} {c + dx}
$$

When $$a = 0, b = 10, c = 1, d = 0$$ we get $$10x$$. So we have

$$
\begin{align}
f(x) = f(t_0 + \frac 1 y) &= \frac {a + b(t_0 + \frac 1 y)} {c + d(t_0 + \frac 1 y)} \\
&= \frac {ay + b(t_0y +1)} {cy + d(t_0y+1)} \\
&= \frac {b + (a + t_0b)y} {d + (c + t_0d)y}
\end{align}
$$

This has the same form as $$f(x)$$ but with the coefficients transformed.

$$
\begin{array}{rcl}
a & \rightarrow & b  \\
b & \rightarrow & a + t_0b \\
c & \rightarrow & d \\
d & \rightarrow & c + t_0d \\
\end{array}
$$

So we just need to maintain four variables, and to consume one term of $$x$$ we can just do

```csharp
BigInteger a = 0, b = 10, c = 1, d = 0;

foreach (var term in terms)
{
    (a, b) = (b, a + b * term);
    (c, d) = (d, c + d * term);
```

We're using `BigInteger` because these values can grow arbitrarily large.

But one tweak we need to make is that we've already written out the first term as the integer part of the value, so we need to skip it. But to get the right result for the fractional part, we need to compute the rest of the value assuming the first term was zero. In other words, we can't just emit the digits of [2; 3, 4], we need to emit the digits of [0; 2, 3, 4]. The digits are completely different. So we'll skip the first term but initialize our four variables with what they would have been had we consumed a zero at the start of the sequence. We also want to break out of the loop once we've written enough decimal digits.

```csharp
BigInteger a = 10, b = 0, c = 0, d = 1;

foreach (var term in terms.Skip(1).TakeWhile(_ => places > 0))
{
    (a, b) = (b, a + b * term);
    (c, d) = (d, c + d * term);
```


To determine the next digit to write, we need to know the integer portion of $$f(x)$$. If all we know is that $$x$$ isn't negative (we actually know more than that, but we don't need to), then we know that

$$
\frac { a + bx } { c + dx }
$$

is somewhere between $$\left\lfloor \frac a c \right\rfloor$$ when $$x=0$$ and $$\left\lfloor \frac b d \right\rfloor$$ when $$x$$ is large. If these two values are the same, we know that's the integer portion of the value.

If the integer portion is $$n$$, we can subtract it as follows.

$$
\begin{align}
\frac {a+bx} {c+dx} - n &= \frac {a+bx} {c+dx} - \frac {n(c+dx)} {c+dx}  \\
&= \frac {a - cn + (b-dn)x} {c+dx}
\end{align}
$$

But because $$n = \lfloor \frac a c \rfloor = \lfloor \frac b d \rfloor$$ we have

$$
\frac {a - c \lfloor \frac a c \rfloor + (b - d \lfloor \frac b d \rfloor)x} {c + dx}
$$

If we realize that $$a - c \lfloor \frac a c \rfloor$$ is another way to say the remainder after dividing $$a$$ by $$c$$, we can simplify the division and remainder operations by taking advantage of `BigInteger.DivRem`.

```csharp
    while  (!c.IsZero && !d.IsZero && places > 0)
    {
        // Compute the two quotients and remainders.
        var m = BigInteger.DivRem(a, c, out var r);
        var n = BigInteger.DivRem(b, d, out var s);
        
        // If the quotients aren't the same we don't know the digit yet.
        if (m != n)
            break;
            
        // Write the digit. 
        writer.Write((int)n);
        places--;
        
        // Take the remainder and multiply by 10 again to set up for next digit.
        a = r * 10;
        b = s * 10;
    }
```

If the continued fraction terminates and we run out of terms, we might have more digits to emit. In this case we pretend that the rest of the terms are infinite (which doesn't affect the value since $$1/\infty=0$$), so we can ignore the $$a$$ and $$c$$ values and just deal with $$b$$ and $$d$$.

```csharp
} // done with terms

while (!b.IsZero && !d.IsZero && places > 0)
{
    var n = BigInteger.DivRem(b, d, out var s);
    writer.Write((int)n);
    places--;
    b = s * 10;
}
```
