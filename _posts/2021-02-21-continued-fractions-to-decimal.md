---
tag: math
---
It's surprisingly easy to compute the decimal value of simple continued fractions of the form

$$
x = a_0 + \cfrac 1 {a_1 + \cfrac 1 {a_2 + \cfrac 1 {a_3 + \cdots}}}
$$

without a lot of code, assuming you have a "big integer" data type available. I'll show you how to do it in C# using `System.Numerics.BigInteger`, and later we'll build on this to create a full-fledged continued fraction data type that we can use to do arithmetic.

We'll assume that we know how to convert integers into their decimal string representation, e.g. `BigInteger.ToString()`. Let's walk through an example using the `double` data type, and then we'll look at how to change this to use continued fractions.

```csharp
public static void Write(double x, TextWriter writer)
{
```

Our method will take a `double` value and write it to the provided `TextWriter`. Later we'll also want to be able to control the number of decimal places and specify number format properties for globalization, but let's start with something simple.

First let's handle negative numbers and zeros.

```csharp
    if (x < 0)
    {
        writer.Write('-');
        Write(-x, writer);
        return;
    }
    
    if (x == 0)
    {
      writer.Write('0');
      return;
    }
```

Next we'll write out the integer portion, to the left of the decimal point. This implementation doesn't handle the "e" exponent notation so it will mess up with numbers that are too large, but this is just to show the general idea.
    
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
    while (x != 0)
    {
        x *= 10;
        n = Math.Floor(x);
        writer.Write(n);
        x -= n;
    }
}
```

