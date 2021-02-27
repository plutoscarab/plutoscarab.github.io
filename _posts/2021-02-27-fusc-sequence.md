---
title: Fusc and Ruler Sequences as Recursive Iterators in C#
tags: [ math, C# ]
---

The [fusc](https://www.cs.utexas.edu/~EWD/transcriptions/EWD05xx/EWD570.html) function is defined for integer $$n > 0$$ as

$$
\begin{align}
fusc(0) &= 0 \\
fusc(1) &= 1 \\
fusc(2n) &= fusc(n) \\
fusc(2n+1) &= fusc(n) + fusc(n+1) \\
\end{align}
$$

For n=0,1,2,... it takes on values

0 1 1 2 1 3 2 3 1 4 3 5 2 5 3 4 1 5 4 7 3 8 5 7 2 7 5 8 3 7 4 5 1 6 5 9 4 11 7 10 ...

Evaluation of fusc with a large argument results in an explosion of recursive calls.
Recursion can be eliminated by the use of the continued fraction formed from the 
bit pattern of n, but this is complicated and if all you need is the fusc *sequence*
and not the function then there is a very simple way to get it.

(This sequence is also called [Stern's diatomic series](https://oeis.org/A002487) or the Stern-Brocot sequence.)

Notice the sequence of *every other* value, highlighted in bold:

**0** 1 **1** 2 **1** 3 **2** 3 **1** 4 **3** 5 **2** 5 **3** 4 **1** 5 **4** 7 **3** 8 **5** 7 **2** 7 **5** 8 **3** 7 **4** 5 **1** 6 **5** 9 **4** 11 **7** 10 ...

This is just the original fusc sequence! And the in-between (non-bold) terms are
just the sum of the terms on either side of them. To generate the sequence we might
be tempted to do

```csharp
// Not this
IEnumerable<int> Fusc()
{
    var n = Fusc().First();
    
    foreach (var f in Fusc().Skip(1))
    {
        yield return n;
        yield return n + f;
        n = f;
    }
}
```

The problem with this is that you get a stack overflow because of infinite recursion.
We can't get `Fusc().First()` without knowing `Fusc().First()` and we also need the second
term to enter the `foreach` loop. We need to jump-start things by emitting the first 
few terms by hand:

```csharp
IEnumerable<int> Fusc()
{
    yield return 0;
    yield return 1;
    yield return 1;
    var n = 1;

    foreach (var f in Fusc().Skip(2))
    {
        yield return n + f;
        yield return f;
        n = f;
    }
}
```

This still does recursion, but it's much more efficient than enumerating fusc(n) for increasing
values of n. It's pretty efficient overall, and certainly a lot simpler than other methods.

You can use this sequence to generate all positive rational numbers, in reduced form and without
duplicates:

```csharp
IEnumerable<(int, int)> AllRationals()
{
    var n = 1;
    
    foreach (var f in Fusc().Skip(2))
    {
        yield return (n, f);
        n = f;
    }
}
```

This is the depth-first traversal of the [Calkin-Wilf tree](https://en.wikipedia.org/wiki/Calkin%E2%80%93Wilf_tree)
which starts like this:

1/1, 1/2, 2/1, 1/3, 3/2, 2/3, 3/1, 1/4, 4/3, 3/5, 5/2, 2/5, 5/3, 3/4, 4/1, 1/5, 5/4, 4/7, 7/3, 3/8 

## Ruler Sequence

Another sequence that is easy to generate using a recursive iterator is the
[ruler sequence](https://oeis.org/A007814) which looks like

0 1 0 2 0 1 0 3 0 1 0 2 0 1 0 4 0 1 0 2 0 1 0 3 0 1 0 2 0 1 0 5 0

You can see that it is recursive if you add 1 to each term and then precede
each term with a 0. So this is even easier to implement than fusc. We still
need to prime the pump by hard-coding the first term.

```csharp
IEnumerable<int> Ruler()
{
    yield return 0;

    foreach (var n in Ruler())
    {
        yield return n + 1;
        yield return 0;
    }
}
```

There are a number of uses for this. One that is interesting to me is its
use in generating [low-discrepency sequences](https://en.wikipedia.org/wiki/Low-discrepancy_sequence)
such as [Sobol sequences](https://en.wikipedia.org/wiki/Sobol_sequence) and
[Van der Corput sequences](https://en.wikipedia.org/wiki/Van_der_Corput_sequence),
which are useful in numerical sampling and integration.

```csharp
IEnumerable<double> VanDerCorput()
{
    var bits = 0ul;
    var scale = Math.Pow(2.0, -64);

    foreach (var r in Ruler())
    {
        bits ^= 1ul << (63 - r);
        yield return bits * scale;
    }
}
```

which produces

0.5, 0.75, 0.25, 0.375, 0.875, 0.625, 0.125, 0.1875, 0.6875, 0.9375, 0.4375, 0.3125, 0.8125, 0.5625, 
0.0625, 0.09375, 0.59375, 0.84375, 0.34375, 0.46875, ...