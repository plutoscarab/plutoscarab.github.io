using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace PlutoScarab
{
    class Program
    {
        static int GCD(int m, int n)
        {
            var sign = m < 0 && n < 0 ? -1 : 1;

            while (n != 0)
            {
                (m, n) = (n, m % n);
            }

            return Math.Abs(m) * sign;
        }

        static IEnumerable<int> SternDiatomic()
        {
            yield return 1;
            yield return 1;
            var n = 1;

            foreach (var m in SternDiatomic().Skip(1))
            {
                yield return n + m;
                yield return m;
                n = m;
            }
        }

        static IEnumerable<(int, int)> Rationals()
        {
            var n = 1;

            foreach (var m in SternDiatomic().Skip(1))
            {
                yield return (n, m);
                n = m;
            }
        }

        static void Main(string[] args)
        {
            const int maxScore = 11;

            var results = new Dictionary<string, (int[], int[], string, int)>();

            // Surds
            var surds =
                from y in Enumerable.Range(2, maxScore - 2)
                from twox in Enumerable.Range(1, (maxScore - y) / 2)
                select (twox, y);

            foreach (var (twox, y) in surds)
            {
                var p = new[] { twox };
                var q = new[] { y };
                int pterms = 0, qterms = 0;
                var ps = CF.Nats().Select(n => { pterms++; return Poly.Eval(p, n); });
                var qs = CF.Nats().Select(n => { qterms++; return Poly.Eval(q, n); });
                var cf = CF.Simplify(ps, qs);
                var s = CF.Digits(cf, 25);
                var termsUsed = Math.Max(pterms, qterms);

                if (s.EndsWith(CF.InvalidDigit))
                    continue;

                var scf = "[" + cf.First() + "; " + string.Join(", ", cf.Skip(1).Take(5)) + ", ...]";
                var d = twox * twox + 4 * y;

                if ((d % 4) != 0)
                {
                    scf += " = $$\\frac{" + twox + "+\\sqrt{" + d + "}}2$$";
                }
                else if ((twox % 2) != 0)
                {
                    scf += " = $$\\frac{" + twox + "+2\\sqrt{" + (d / 4) + "}}2$$";
                }
                else
                {
                    scf += " = $$" + (twox / 2) + "+\\sqrt{" + (d / 4) + "}$$";
                }

                if (!results.ContainsKey(s))
                {
                    results[s] = (p, q, scf, termsUsed);
                }
            }

            var lookups = new Dictionary<string, string>();

            // e
            var e = new BigInteger[] { 2 }.Concat(CF.Nats(1).SelectMany(n => new[] { 1, 2 * n, 1 }));
            var sqrtE = CF.Nats().SelectMany(n => new[] { 1, 1 + 4 * n, 1 });

            var es =
                from a in Enumerable.Range(-9, 19)
                from b in Enumerable.Range(-9, 19)
                from c in Enumerable.Range(-9, 19)
                from d in Enumerable.Range(-9, 19)
                let g = GCD(GCD(a, b), GCD(c, d))
                let q = c + d * Math.E
                where q > 0
                let v = (a + b * Math.E) / q
                where g == 1 && b * d != 0 && v > 0 && v != 1
                select (a / g, b / g, c / g, d / g);

            foreach (var (a, b, c, d) in es)
            {
                var cf = CF.Transform(e, a, b, c, d);
                var s = CF.Digits(cf, 25);

                if (s.EndsWith(CF.InvalidDigit))
                    continue;

                var num = Poly.ToFactoredString(new[] { a, b }, "e");
                var den = Poly.ToFactoredString(new[] { c, d }, "e");
                var expr = "\\frac{" + num + "}{" + den + "}";

                if (!lookups.ContainsKey(s))
                {
                    lookups[s] = expr;
                }
            }

            es =
                from c in Enumerable.Range(-9, 19)
                from d in Enumerable.Range(-9, 19)
                let q = c + d * Math.E
                where q > 0
                from a in Enumerable.Range(-9, 19)
                from b in Enumerable.Range(-9, 19)
                let g = GCD(GCD(a, b), GCD(c, d))
                let v = (a + b * Math.Sqrt(Math.E)) / (c + d * Math.E)
                where g == 1 && b * d != 0 && v > 0 && v != 1
                select (a / g, b / g, c / g, d / g);

            foreach (var (a, b, c, d) in es)
            {
                var cf = CF.Transform(sqrtE, a, b, c, d);
                var s = CF.Digits(cf, 25);

                if (s.EndsWith(CF.InvalidDigit))
                    continue;

                if (!lookups.ContainsKey(s))
                {
                    var num = Poly.ToFactoredString(new[] { a, b }, "\\sqrt e");
                    var den = Poly.ToFactoredString(new[] { c, d }, "e");
                    var expr = "\\frac{" + num + "}{" + den + "}";
                    lookups[s] = expr;
                }
            }

            var pairs =
                from score in Enumerable.Range(2, maxScore - 1)
                from score1 in Enumerable.Range(1, score - 1)
                let score2 = score - score1
                from p in Poly.WithScore(score1)
                from q in Poly.WithScore(score2)
                select (p, q);

            foreach (var (p, q) in pairs)
            {
                if (q.Length == 1 && q[0] == 1)
                    continue; // simple continued fraction

                int pterms = 0, qterms = 0;
                var ps = CF.Nats().Select(n => { pterms++; return Poly.Eval(p, n); });
                var qs = CF.Nats().Select(n => { qterms++; return Poly.Eval(q, n); });
                var cf = CF.Simplify(ps, qs);

                if (!cf.Any())
                    continue;

                var first = cf.First();

                if (first.Sign < 0)
                    continue;

                pterms = qterms = 0;
                var s = CF.Digits(cf, 25);
                var termsUsed = Math.Max(pterms, qterms);

                if (s.EndsWith(CF.InvalidDigit))
                    continue;

                // Console.WriteLine(s);
                var scf = "[" + first + "; " + string.Join(", ", cf.Skip(1).Take(5)) + ", ...]";

                if (lookups.TryGetValue(s, out var expr))
                {
                    scf += " = $$" + expr + "$$";
                }

                if (!results.TryGetValue(s, out var result) || result.Item4 > termsUsed)
                {
                    results[s] = (p, q, scf, termsUsed);
                    Console.WriteLine($"{s}\t{termsUsed}");
                }
            }

            var list = results.ToList();
            results = null;
            list.Sort((a, b) => decimal.Parse(a.Key).CompareTo(decimal.Parse(b.Key)));

            using (var file = File.CreateText("../../polygcf.md"))
            {
                file.WriteLine("---");
                file.WriteLine("title: Values of Generalized Continued Fractions with Polynomial Terms");
                file.WriteLine("tag: math");
                file.WriteLine("---");
                file.WriteLine();
                file.WriteLine("Intended to be found by search engines when a value or terms of a simple");
                file.WriteLine("continued fraction are known but the generalized continued fraction is unknown.");
                file.WriteLine($"These are the first {list.Count:N0} values with polynomials of lowest total score.");
                file.WriteLine("Polynomal score is equal to its degree plus the sum of absolute values of its coefficients.");
                file.WriteLine();
                file.WriteLine("$$");
                file.WriteLine("x = f(0) + \\cfrac {g(0)} {f(1) + \\cfrac {g(1)} {f(2) + \\ddots}}");
                file.WriteLine("$$");
                file.WriteLine();
                file.WriteLine("The 'Terms' column is the number of continued fraction terms needed to calculate");
                file.WriteLine("the value to the precision shown.");
                file.WriteLine();
                file.WriteLine("|Value of $$x$$|f(n)|g(n)|Simple CF|Terms|");
                file.WriteLine("|--------------|----|----|---------|-----|");

                foreach (var pair in list)
                {
                    var value = pair.Key;
                    var (p, q, scf, tu) = pair.Value;
                    file.WriteLine($"|{value}|{Poly.ToFactoredString(p)}|{Poly.ToFactoredString(q)}|{scf}|{tu}|");
                }
            }
        }
    }
}
