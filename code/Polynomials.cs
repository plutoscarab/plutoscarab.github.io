using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Poly
{
    public static IEnumerable<int[]> All() =>
        from score in Enumerable.Range(1, int.MaxValue - 1)
        from poly in WithScore(score)
        select poly;

    public static IEnumerable<int[]> WithScore(int score) =>
        from degree in Enumerable.Range(0, score)
        from poly in WithTotalAndDegree(score - degree, degree)
        select poly;

    public static IEnumerable<int[]> WithTotalAndDegree(int coeffTotal, int degree) =>
        (
            from coeff in Enumerable.Range(1, coeffTotal - 1)
            from d in Enumerable.Range(0, degree)
            from poly in WithTotalAndDegree(coeffTotal - coeff, d)
            select new[] { AddTerm(poly, degree, coeff), AddTerm(poly, degree, -coeff) })
        .SelectMany(_ => _)
        .Concat(new[] { Monomial(degree, coeffTotal), Monomial(degree, -coeffTotal) });

    public static int[] Monomial(int degree, int coeff)
    {
        if (degree < 0) throw new ArgumentOutOfRangeException(nameof(degree));
        var result = new int[degree + 1];
        result[degree] = coeff;
        return result;
    }

    public static int[] AddTerm(int[] poly, int degree, int coeff)
    {
        if (poly is null) throw new ArgumentNullException(nameof(poly));
        if (degree < poly.Length) throw new ArgumentOutOfRangeException(nameof(degree));
        var result = new int[degree + 1];
        Array.Copy(poly, result, poly.Length);
        result[degree] = coeff;
        return result;
    }

    public static string ToString(int[] poly)
    {
        if (poly is null) throw new ArgumentOutOfRangeException(nameof(poly));
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
}