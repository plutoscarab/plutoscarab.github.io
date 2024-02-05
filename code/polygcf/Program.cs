using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Sdcb.Arithmetic.Mpfr;

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

        static void Main(string[] args)
        {
            const int maxScore = 11;

            var results = new Dictionary<string, (int[], int[], string, int)>();
            var lookups = new Dictionary<string, string>();

            // e
            var e = new BigInteger[] { 2 }.Concat(CF.Nats(1).SelectMany(n => new[] { 1, 2 * n, 1 }));
            var sqrtE = CF.Nats().SelectMany(n => new[] { 1, 1 + 4 * n, 1 });

            var es =
                from c in Enumerable.Range(-9, 19)
                from d in Enumerable.Range(-9, 19)
                let q = c + d * Math.E
                where q > 0
                from a in Enumerable.Range(-9, 19)
                from b in Enumerable.Range(-9, 19)
                let g = GCD(GCD(a, b), GCD(c, d))
                let v = (a + b * Math.E) / q
                where g == 1 && v > 0 && v != 1
                select (a / g, b / g, c / g, d / g);

            foreach (var (a, b, c, d) in es)
            {
                var cf = CF.Transform(e, a, b, c, d);
                var s = CF.Digits(cf, 25);

                if (s.EndsWith(CF.InvalidDigit))
                    continue;

                var num = Poly.ToFactoredString(new[] { a, b }, "e");
                var den = Poly.ToFactoredString(new[] { c, d }, "e");
                var expr = den == "1" ? num : "\\frac{" + num + "}{" + den + "}";

                if (!lookups.ContainsKey(s))
                {
                    lookups[s] = expr;
                }
            }

            es =
                from c in Enumerable.Range(-9, 19)
                from d in Enumerable.Range(-9, 19)
                let q = c + d * Math.Sqrt(Math.E)
                where q > 0
                from a in Enumerable.Range(-9, 19)
                from b in Enumerable.Range(-9, 19)
                let g = GCD(GCD(a, b), GCD(c, d))
                let v = (a + b * Math.Sqrt(Math.E)) / q
                where g == 1 && v > 0 && v != 1
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
                    var den = Poly.ToFactoredString(new[] { c, d }, "\\sqrt e");
                    var expr = den == "1" ? num : "\\frac{" + num + "}{" + den + "}";
                    lookups[s] = expr;
                }
            }

            // pi
            var odds = CF.Nats().Select(n => 2 * n + 1);
            var squares = CF.Nats().Select(n => (n + 1) * (n + 1));
            var pi = CF.Transform(CF.Simplify(odds, squares), 4, 0, 0, 1);

            var pis =
                from c in Enumerable.Range(-9, 19)
                from d in Enumerable.Range(-9, 19)
                let q = c + d * Math.PI
                where q > 0
                from a in Enumerable.Range(-9, 19)
                from b in Enumerable.Range(-9, 19)
                let g = GCD(GCD(a, b), GCD(c, d))
                let v = (a + b * Math.PI) / q
                where g == 1 && v > 0 && v != 1
                select (a / g, b / g, c / g, d / g);

            foreach (var (a, b, c, d) in pis)
            {
                var cf = CF.Transform(pi, a, b, c, d);
                var s = CF.Digits(cf, 25);

                if (s.EndsWith(CF.InvalidDigit))
                    continue;

                var num = Poly.ToFactoredString(new[] { a, b }, "\\pi");
                var den = Poly.ToFactoredString(new[] { c, d }, "\\pi");
                var expr = den == "1" ? num : "\\frac{" + num + "}{" + den + "}";

                if (!lookups.ContainsKey(s))
                {
                    lookups[s] = expr;
                }
            }

            var maybes = new Dictionary<string, string>();
            MpfrFloat.DefaultPrecision = 256;

            string StrIndex(MpfrFloat f)
            {
                var s = MpfrFloat.Abs(f, null).ToString() + "0000000000000000000000000";
                return s[..(s.IndexOf('.') + 26)];
            }

            // mixed surds
            var surds =
                from q in new[] { 2, 3, 5, 6, 7, 10, 11, 13, 14, 15, 17, 19, 21, 22, 23, 26, 29, 30, 31, 33, 34, 35, 37, 38, 39 }
                where Math.Sqrt(q) != (int)Math.Sqrt(q)
                from a in Enumerable.Range(-9, 19)
                from b in Enumerable.Range(-9, 19)
                where a > 0 || b > 0
                from c in Enumerable.Range(1, 2)
                where b != 0 
                let g = GCD(GCD(a, b), c)
                where g == 1
                select (q, a, b, c);
            
            foreach (var (q, a, b, c) in surds)
            {
                var sq = MpfrFloat.Sqrt(q);
                var y = (a + b * sq) / c;

                if (y != (int)y)
                {
                    var s = StrIndex(y);
                    var lk = "";
                    if (a != 0) lk = a.ToString();
                    if (a != 0 && b > 0) lk += "+";
                    if (b != 1) lk += b.ToString();
                    lk += "\\sqrt{" + q + "}";
                    if (c != 1) lk = "\\frac{" + lk + "}" + c;

                    if (!lookups.ContainsKey(s) || lookups[s].Length > lk.Length)
                    {
                        lookups[s] = lk;
                    }
                }
            }

            foreach (var (p, q) in Seq.Rationals().TakeWhile(_ => _.Item2 < 100))
            {
                var x = (Math.PI * p) / q;
                var num = Poly.ToFactoredString(new[] { 0, p }, "\\pi");
                var frac = q == 1 ? num : "\\frac{" + num + "}{" + q + "}";

                void Add(string trig, Func<double, double> func)
                {
                    try
                    {
                        var y = ((decimal)func(x)).ToString();
                        y = y.Substring(0, y.Length - 1);

                        if (y.Length >= 15)
                        {
                            y = y.Substring(0, 15);

                            if (!maybes.ContainsKey(y))
                            {
                                maybes[y] = trig + "(" + frac + ")";
                            }
                        }
                    }
                    catch
                    {
                        Debugger.Break();
                    }
                }

                Add("tan", Math.Tan);
                Add("sin", Math.Sin);
                Add("cos", Math.Cos);
                Add("cot", x => 1 / Math.Tan(x));
                Add("csc", x => 1 / Math.Sin(x));
                Add("sec", x => 1 / Math.Cos(x));
                Add("exp", Math.Exp);
                Add("ln", Math.Log);
                Add("sqrt", Math.Sqrt);
                Add("tanh", Math.Tanh);
                Add("sinh", Math.Sinh);
                Add("cosh", Math.Cosh);
                Add("tan^{-1}", Math.Atan);
                Add("sinh^{-1}", Math.Asinh);

                if (x < 1) 
                {
                    Add("tanh^{-1}", Math.Atanh);
                    Add("cos^{-1}", Math.Acos);
                    Add("sin^{-1}", Math.Asin);
                }
                else if (x > 1)
                {
                    Add("cosh^{-1}", Math.Acosh);
                }

                x = p / (double)q;
                frac = q == 1 ? p.ToString() : "\\frac{" + p + "}{" + q + "}";

                Add("tan", Math.Tan);
                Add("sin", Math.Sin);
                Add("cos", Math.Cos);
                Add("cot", x => 1 / Math.Tan(x));
                Add("csc", x => 1 / Math.Sin(x));
                Add("sec", x => 1 / Math.Cos(x));
                Add("exp", Math.Exp);
                Add("ln", Math.Log);
                Add("sqrt", Math.Sqrt);
                Add("tanh", Math.Tanh);
                Add("sinh", Math.Sinh);
                Add("cosh", Math.Cosh);
                Add("tan^{-1}", Math.Atan);
                Add("sinh^{-1}", Math.Asinh);

                if (x < 1) 
                {
                    Add("tanh^{-1}", Math.Atanh);
                    Add("cos^{-1}", Math.Acos);
                    Add("sin^{-1}", Math.Asin);
                }
                else if (x > 1)
                {
                    Add("cosh^{-1}", Math.Acosh);
                }
            }

            MpfrFloat BesselI(MpfrFloat a, MpfrFloat x)
            {
                MpfrFloat sum = 0;
                uint m = 0;

                while (true)
                {
                    var t = MpfrFloat.Power(x / 2, 2 * m + a) / MpfrFloat.Factorial(m) / MpfrFloat.Gamma(m + a + 1);
                    sum += t;

                    if (MpfrFloat.Abs(t, null) < 1e-50)
                        break;

                    ++m;
                }

                return sum;
            }

            for (var n = 1; n < 20; n++)
            {
                for (var d = 1; d < 20; d++)
                {
                    var g = GCD(n, d);

                    if (g == 1)
                    {
                        var x = (MpfrFloat)n / (MpfrFloat)d;
                        MpfrFloat y;
                        string s;

                        for (var k = 0; k < 10; k++)
                        {
                            y = MpfrFloat.JN(k + 1, x) / MpfrFloat.JN(k, x);
                            s = StrIndex(y);

                            if (d == 1)
                                lookups[s] = $"\\frac {{J_{k + 1}({n})}} {{J_{k}({n})}}";
                            else
                                lookups[s] = $"\\frac {{J_{k + 1}(\\frac {n} {d})}} {{J_{k}(\\frac {n} {d})}}";
                                
                            s = StrIndex(1 / y);

                            if (d == 1)
                                lookups[s] = $"\\frac {{J_{k}({n})}} {{J_{k + 1}({n})}}";
                            else
                                lookups[s] = $"\\frac {{J_{k}(\\frac {n} {d})}} {{J_{k + 1}(\\frac {n} {d})}}";
                        }

                        y = BesselI(x, .5) / BesselI(x - 1, .5);
                        s = StrIndex(y);
                        lookups[s] = $"\\frac {{I_{{\\frac {n} {d}}}(\\frac 1 2)}} {{I_{{\\frac {n - d} {d}}}(\\frac 1 2)}}";

                        y = BesselI(x, 1) / BesselI(x - 1, 1);
                        s = StrIndex(y);

                        if (d == 1)
                            lookups[s] = $"\\frac {{I_{{{n}}}(1)}} {{I_{{{n - d}}}(1)}}";
                        else
                            lookups[s] = $"\\frac {{I_{{\\frac {n} {d}}}(1)}} {{I_{{\\frac {n - d} {d}}}(1)}}";
                    }
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
                //if (q.Length == 1 && q[0] == 1)
                //    continue; // simple continued fraction

                int pterms = 0, qterms = 0;
                var ps = CF.Nats().Select(n => { pterms++; return Poly.Eval(p, n); });
                var qs = CF.Nats().Skip(1).Select(n => { qterms++; return Poly.Eval(q, n); });
                var cf = CF.Simplify(ps, qs);
                List<BigInteger> capture = null;

                IEnumerable<BigInteger> Captured(IEnumerable<BigInteger> terms)
                {
                    capture = new List<BigInteger>();

                    foreach (var term in terms)
                    {
                        capture.Add(term);
                        yield return term;
                    }
                }

                if (!cf.Any())
                    continue;

                var first = cf.First();

                if (first.Sign < 0)
                    continue;

                pterms = qterms = 0;
                var s = CF.Digits(Captured(cf), 25);

                if (s.EndsWith(CF.InvalidDigit))
                    continue;

                var termsUsed = Math.Max(pterms, qterms);
                cf = CF.Normalize(capture);
                var scf = ""; //"[" + first + "; " + string.Join(", ", cf.Skip(1).Take(5)) + ", ...]";

                if (lookups.TryGetValue(s, out var expr))
                {
                    scf = "$$" + expr + "$$";
                }
                else if (maybes.TryGetValue(s.Substring(0, 15), out expr))
                {
                    scf = "$$" + expr + "$$";
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
                file.WriteLine("Polynomial score is equal to its degree plus the sum of absolute values of its coefficients.");
                file.WriteLine();
                file.WriteLine("$$");
                file.WriteLine("x = b_0 + \\cfrac {a_1} {b_1 + \\cfrac {a_2} {b_2 + \\ddots}}");
                file.WriteLine("$$");
                file.WriteLine();
                file.WriteLine("The 'Terms' column is the number of continued fraction terms needed to calculate");
                file.WriteLine("the value to the precision shown.");
                file.WriteLine();
                file.WriteLine("|Value of $$x$$|a<sub>n</sub>|b<sub>n</sub>|Expression|Terms|");
                file.WriteLine("|--------------|----|----|---------|-----|");

                foreach (var pair in list)
                {
                    var value = pair.Key;
                    var (p, q, scf, tu) = pair.Value;
                    file.WriteLine($"|{value}|{Poly.ToFactoredString(q)}|{Poly.ToFactoredString(p)}|{scf}|{tu}|");
                }
            }
        }
    }
}
