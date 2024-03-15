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

        public static BigInteger Binomial(BigInteger n, BigInteger k)
        {
            if (n < k) return 0;
            if (k == 0) return 1;
            k = BigInteger.Min(k, n - k);
            if (k == 1) return n;
            BigInteger i = 0, p = 1;

            while (true)
            {
                if (i >= k)
                    return p;

                (i, p) = (i + 1, (n - i) * p / (i + 1));
            }
        }

        private static BigInteger UpperBinomial(BigInteger k, BigInteger n)
        {
            var to = k;

            while (Binomial(to, k) <= n + k)
            {
                to *= 2;
            }

            var from = to / 2;

            while (from != to)
            {
                var mid = (from + to) / 2;

                if (Binomial(mid, k) > n)
                    to = mid;
                else
                    from = mid + 1;
            }

            return from;
        }

        private static List<BigInteger> BinomialDigits(BigInteger k, BigInteger n)
        {
            List<BigInteger> list = [];

            while (!k.IsZero)
            {
                var m = UpperBinomial(k, n) - 1;
                list.Add(m);
                n -= Binomial(m, k--);
            }

            return list;
        }

        private static int[] ToTuple(BigInteger n, int length)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length));

            var list = BinomialDigits(length, n);
            var arr = new int[length];
            arr[0] = (int)list[^1];

            for (var i = 1; i < length; i++)
            {
                arr[i] = (int)(list[length - 1 - i] - list[length - i] - 1);
            }

            return arr;
        }

        public static IEnumerable<int[]> WithDegree(int degree)
        {
            BigInteger n = 0;

            while (true)
            {
                var t = ToTuple(n, degree + 1);
                t[^1]++;
                t = t.Select(c => (c & 1) == 0 ? -c / 2 : c / 2 + 1).ToArray();
                yield return t;
                n++;
            }
        }

        public static (int[], int[]) WithDegree(int degree1, int degree2, BigInteger n)
        {
            var degree = degree1 + degree2 + 1;
            var t = ToTuple(n, degree + 1);
            t[degree1]++;
            t[^1]++;
            t = t.Select(c => (c & 1) == 0 ? -c / 2 : c / 2 + 1).ToArray();
            return (t[..(degree1 + 1)], t[(degree1 + 1)..]);
        }

        public static IEnumerable<(int[], int[])> WithDegree(int degree1, int degree2)
        {
            BigInteger n = 0;

            while (true)
            {
                yield return WithDegree(degree1, degree2, n);
                n++;
            }
        }

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

            var nonz = poly.Where(coeff => coeff != 0).ToList();
            var powers = Enumerable.Range(0, poly.Length).ToList();

            if (nonz.Count == 2 && nonz[0] < 0 && nonz[1] > 0)
            {
                powers.Reverse();
            }

            foreach (var power in powers)
            {
                var coeff = poly[power];

                if (coeff != 0)
                {
                    if (s.Length > 0)
                    {
                        s.Append(" ");
                    }

                    if (coeff < 0)
                    {
                        s.Append("-");

                        if (s.Length > 1)
                        {
                            s.Append(" ");
                        }
                    }
                    else if (s.Length > 0)
                    {
                        s.Append("+ ");
                    }

                    if (Math.Abs(coeff) != 1 || power == 0)
                    {
                        s.Append(Math.Abs(coeff));
                    }

                    if (power > 0)
                    {
                        s.Append(indeterminate);
                    }

                    if (power > 1)
                    {
                        s.Append('^');
                        if (power > 9)
                            s.Append($"{{{power}}}");
                        else
                            s.Append(power);
                    }
                }
            }

            return s.ToString();
        }

        public static string ToFactoredString(int[] poly) => ToFactoredString(poly, "𝑛");

        public static string ToFactoredString(int[] poly, string indeterminate)
        {
            poly = (int[])poly.Clone();

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
                s.Append("-");
            }
            else if (gcd != 1)
            {
                s.Append(gcd.ToString());
            }

            if (n > 0)
            {
                s.Append(indeterminate);

                if (n > 1)
                {
                    if (n > 9)
                        s.Append($"^{{{n}}}");
                    else
                        s.Append($"^{n}");
                }
            }

            if (poly.Length > 1 || poly[0] != 1)
            {
                string ps = ToString(poly, indeterminate);

                if (poly.Length == 3)
                {
                    var a = poly[2];
                    var b = poly[1];
                    var c = poly[0];
                    var d = b * b - 4 * a * c;

                    if (d == 0)
                    {
                        var g = PolyI.GCD(b, 2 * a);
                        ps = "(" + ToString([b / g, 2 * a / g], indeterminate) + ")^2";
                    }
                    else if (d > 0)
                    {
                        var sd = (int)Math.Round(Math.Sqrt(d));

                        if (sd * sd == d)
                        {
                            var g = PolyI.GCD(b - sd, 2 * a);
                            ps = "(" + ToString([(b - sd) / g, 2 * a / g], indeterminate) + ")";
                            g = PolyI.GCD(b + sd, 2 * a);
                            ps += "(" + ToString([(b + sd) / g, 2 * a / g], indeterminate) + ")";
                        }
                    }
                }

                if (s.Length > 0 && ps[0] != '(')
                {
                    s.Append("(" + ps + ")");
                }
                else
                {
                    s.Append(ps);
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