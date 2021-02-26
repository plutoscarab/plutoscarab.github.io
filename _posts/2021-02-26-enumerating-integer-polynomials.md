---
title: Enumerating Integer Polynomials in C#
tags: [math, C#]
---

Polynomials with integer coefficients are [countable](https://en.wikipedia.org/wiki/Countable_set).
[Gowers's Weblog](https://gowers.wordpress.com/2008/07/30/recognising-countable-sets/) shows a way to
prove that and I'll show you how to enumerate them in C# using the idea from that proof.

The method involves assigning a score to a polynomial equal to the sum of the absolute values of the coefficients
plus the degree of the polynomial. For example the score for $$n^4 - 3n^2 + 5$$ is 13 because $$13=4+\abs{1}+\abs{-3}+\abs{5}$$.
We just have to enumerate the polynomials with each score starting with zero.

We'll represent a polynomial of degree n as an integer array of length n + 1. The element at index i represents
the coefficient of degree i. So the polynomial above is represented by the array [5, 0, -3, 0, 1]. We'll ignore
the edge cases of the empty array and the zero polynomial [0]. 

If we had a suitable function `PolysWithScore` it's clear that this would do the job.

```csharp
        var allPolys =
            from score in Enumerable.Range(1, int.MaxValue - 1)
            from poly in PolysWithScore(score)
            select poly;
```

The function is also straightforward to write, assume we have a suitable `PolysWithTotalAndDegree` function.
The "Total" in the name refers to the sum of the absolute values of the coefficients. 

```csharp
        IEnumerable<int[]> PolysWithScore(int score) =>
            from degree in Enumerable.Range(0, score)
            from poly in PolysWithTotalAndDegree(score - degree, degree)
            select poly;
```

To implement `PolysWithTotalAndDegree` we can implement a recursive function. First we emit the monomials
with the specified degree with the coefficient equal to the positive and negative total, and then we
emit all the polynomials of lower degrees for each lesser total and to each of those polynomaisl we
add the max-degree term with the coefficient equal to whatever is left over from the total.

```csharp
        IEnumerable<int[]> PolysWithTotalAndDegree(int coeffTotal, int degree) =>
            new[] { Monomial(degree, coeffTotal), Monomial(degree, -coeffTotal) }
            .Concat((
                from coeff in Enumerable.Range(1, coeffTotal - 1)
                from d in Enumerable.Range(0, degree)
                from poly in PolysWithTotalAndDegree(coeffTotal - coeff, d)
                select new[] { AddTerm(poly, degree, coeff), AddTerm(poly, degree, -coeff) })
            .SelectMany(_ => _));
```

This uses two helper functions, one for creating a monomial, and one for extending a lower-degree
polynomial to include a higher-degree term.

```csharp
        int[] Monomial(int degree, int coeff)
        {
            var result = new int[degree + 1];
            result[degree] = coeff;
            return result;
        }        
        
        int[] AddTerm(int[] poly, int degree, int coeff)
        {
            var result = new int[degree + 1];
            Array.Copy(poly, result, poly.Length);
            result[degree] = coeff;
            return result;
        }
```

A helper function for making the array look like an actual polynomial is straightforward and a bit tedious:

```csharp
        string PolyToString(int[] poly)
        {
            var s = new StringBuilder();

            for (var power = 0; power < poly.Length; power++)
            {
                var coeff = poly[power];

                if (coeff != 0)
                {
                    if (s.Length > 0)
                    {
                        s.Append(" ");
                    }

                    if (power == 0)
                    {
                        s.Append(coeff.ToString().Replace("-", "−"));
                    }
                    else
                    {
                        if (coeff < 0)
                        {
                            s.Append("−");

                            if (s.Length > 1)
                            {
                                s.Append(" ");
                            }
                        }
                        else if (s.Length > 0)
                        {
                            s.Append("+ ");
                        }

                        if (Math.Abs(coeff) != 1)
                        {
                            s.Append(Math.Abs(coeff));
                        }

                        s.Append("𝑛");

                        if (power > 1)
                        {
                            s.Append(new string(power.ToString().Select(c => "⁰¹²³⁴⁵⁶⁷⁸⁹"[c - '0']).ToArray()));
                        }
                    }
                }
            }

            return s.ToString();
        }
```

Then we pull it all together and enumerate the low-scoring polynomials.

```csharp
        foreach (var p in allPolys.TakeWhile(p => p[0] != 6))
        {
            Console.WriteLine(PolyToString(p));
        }
```

This produces

```
1
−1
2
−2
n
−n
3
−3
2n
−2n
1 + n
1 − n
−1 + n
−1 − n
n²
−n²
4
−4
3n
−3n
2 + n
2 − n
−2 + n
−2 − n
1 + 2n
1 − 2n
−1 + 2n
−1 − 2n
2n²
−2n²
1 + n²
1 − n²
−1 + n²
−1 − n²
n + n²
n − n²
−n + n²
−n − n²
n³
−n³
5
−5
4n
−4n
3 + n
3 − n
−3 + n
−3 − n
2 + 2n
2 − 2n
−2 + 2n
−2 − 2n
1 + 3n
1 − 3n
−1 + 3n
−1 − 3n
3n²
−3n²
2 + n²
2 − n²
−2 + n²
−2 − n²
2n + n²
2n − n²
−2n + n²
−2n − n²
1 + n + n²
1 + n − n²
1 − n + n²
1 − n − n²
−1 + n + n²
−1 + n − n²
−1 − n + n²
−1 − n − n²
1 + 2n²
1 − 2n²
−1 + 2n²
−1 − 2n²
n + 2n²
n − 2n²
−n + 2n²
−n − 2n²
2n³
−2n³
1 + n³
1 − n³
−1 + n³
−1 − n³
n + n³
n − n³
−n + n³
−n − n³
n² + n³
n² − n³
−n² + n³
−n² − n³
n⁴
−n⁴
```
