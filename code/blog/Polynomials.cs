using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PlutoScarab
{
    public static class Poly
    {
        public static IEnumerable<int[]> All() =>
            from score in Enumerable.Range(1, int.MaxValue - 1)
            from poly in WithScore(score)
            select poly;

        public static IEnumerable<int[]> Monic() =>
            All().Select(p => AddTerm(p, p.Length, 1));

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

        private static string Super(int n) =>
            new string(n.ToString().Select(c => "⁰¹²³⁴⁵⁶⁷⁸⁹"[c - '0']).ToArray());

        public static string ToString(int[] poly) => ToString(poly, "𝑛");

        public static string ToString(int[] poly, string indeterminate)
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

                        s.Append(indeterminate);

                        if (power > 1)
                        {
                            s.Append(Super(power));
                        }
                    }
                }
            }

            return s.ToString();
        }

        public static string ToFactoredString(int[] poly) => ToFactoredString(poly, "𝑛");

        public static string ToFactoredString(int[] poly, string indeterminate)
        {
            if (poly.Count(coeff => coeff != 0) == 1)
            {
                return ToString(poly, indeterminate);
            }

            var gcd = 0;

            foreach (var coeff in poly.Where(_ => _ != 0))
            {
                gcd = PolyI.GCD(coeff, gcd);
            }

            gcd = Math.Abs(gcd);

            for (var i = 0; i < poly.Length; i++)
            {
                poly[i] /= gcd;
            }

            var n = poly.TakeWhile(coeff => coeff == 0).Count();

            if (n > 0)
            {
                var arr = new int[poly.Length - n];
                Array.Copy(poly, n, arr, 0, arr.Length);
                poly = arr;
            }

            var s = new StringBuilder();

            if (gcd == -1)
            {
                s.Append("−");
            }
            else if (gcd != 1)
            {
                s.Append(gcd.ToString().Replace("-", "−"));
            }

            if (n > 0)
            {
                s.Append(indeterminate);

                if (n > 1)
                {
                    s.Append(Super(n));
                }
            }

            if (poly.Length > 1 || poly[0] != 1)
            {
                if (s.Length > 0)
                {
                    s.Append("(");
                    s.Append(ToString(poly, indeterminate));
                    s.Append(")");
                }
                else
                {
                    s.Append(ToString(poly, indeterminate));
                }
            }

            return s.ToString();
        }

        public static BigInteger Eval(int[] p, BigInteger n)
        {
            var sum = BigInteger.Zero;

            for (var i = p.Length - 1; i >= 0; i--)
            {
                sum = sum * n + p[i];
            }

            return sum;
        }
    }
}