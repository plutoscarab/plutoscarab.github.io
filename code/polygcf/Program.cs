using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Sdcb.Arithmetic.Mpfr;

namespace PlutoScarab
{
    class Sigdig : IEquatable<Sigdig>, IComparable<Sigdig>
    {
        public const int Count = 20;

        private string s;

        public Sigdig(string s)
        {
            if (s[0] == '-') s = s[1..];
            s = s.Replace(".", "");
            if (!s.All(char.IsDigit)) throw new ArgumentException(nameof(s));
            var i = 0;
            while (s[i] == '0') i++;
            s = s[i..];
            if (s.Length < Count) throw new ArgumentException(nameof(s));
            this.s = s[..Count];
        }

        public override string ToString() => s;

        public override int GetHashCode() => s.GetHashCode();

        public bool Equals(Sigdig other) => s.Equals(other.s);

        public override bool Equals(object other) => other is Sigdig sd && s.Equals(sd);

        public int CompareTo(Sigdig other) => s.CompareTo(other.s);
    }

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

        static string LaTeXwrap(object n)
        {
            var s = n.ToString();

            if (s.Length == 1)
                return s;

            return "{" + s + "}";
        }

        static string LaTeXfrac(string a, string b)
        {
            if (b == "1") return a;
            return "\\frac" + LaTeXwrap(a) + LaTeXwrap(b);
        }

        static string LaTeXfracs(string a, string b)
        {
            if (b == "1") return a;
            return a + "/" + b;
        }

        static string LaTeXfrac(int a, int b)
        {
            var g = GCD(a, b);
            (a, b) = (a / g, b / g);
            return LaTeXfrac(a.ToString(), b.ToString());
        }

        static string LaTeXfracs(int a, int b)
        {
            var g = GCD(a, b);
            (a, b) = (a / g, b / g);
            return LaTeXfracs(a.ToString(), b.ToString());
        }

        static string LaTeXpow(string expr, int a, int b)
        {
            var f = LaTeXfracs(a, b);

            if (f == "0")
                return "1";

            if (f == "1")
                return expr;

            if (f == "1/2")
                return "\\sqrt" + LaTeXwrap(expr);

            return expr + "^" + LaTeXwrap(f);
        }

        static (int n, int f) SquareFree(int n)
        {
            var s = (int)(Math.Sqrt(n) + .5);

            if (s * s == n)
                return (1, s);

            var f = 1;

            for (var i = 2; i < s; i++)
            {
                var ii = i * i;

                if ((n % ii) == 0)
                {
                    f *= i;
                    n /= ii;
                }
            }

            return (n, f);
        }

        static string LaTeXsqrt(string n)
        {
            if (n == "1")
                return "1";

            return "\\sqrt" + LaTeXwrap(n);
        }

        static string LaTeXsqrt(int n)
        {
            (n, var f) = SquareFree(n);

            if (n == 1)
                return f.ToString();

            if (f > 1)
                return f + "\\sqrt" + LaTeXwrap(n);

            return "\\sqrt" + LaTeXwrap(n);
        }

        static string LaTeXprod(string a, string b)
        {
            if (a == "1")
                return b;

            if (b == "1")
                return a;

            return a + " " + b;    
        }

        static string LaTeXoverSqrt(int a, int b)
        {
            // a / sqrt(b)
            (b, var c) = SquareFree(b); // -> a / (c * sqrt(b))
                
            var d = GCD(a, b);  // (a/d)*sqrt(d) / (c * sqrt(b/d))
            (a, b) = (a / d, b / d); // a sqrt(d) / (c sqrt(b))

            var g = GCD(a, c);
            (a, c) = (a / g, c / g);

            if (d == 1)
                return LaTeXfrac(a.ToString(), LaTeXprod(c.ToString(), LaTeXsqrt(b)));

            if (b == 1)
                return LaTeXfrac(LaTeXprod(a.ToString(), LaTeXsqrt(d)), c.ToString());

            return LaTeXprod(LaTeXfrac(a, c), LaTeXsqrt(LaTeXfrac(d, b)));
        }

        static string Lookup(Sigdig sd, ConcurrentDictionary<Sigdig, string> lookups, int[] p, int[] q)
        {
            var scf = string.Empty;

            if (q.Length == 1 && p.Length == 1)
            {
                if ((p[0] & 1) == 0)
                    scf = "$$" + (p[0] / 2) + "+" + LaTeXsqrt(p[0] * p[0] / 4 + q[0]) + "$$";
                else
                    scf = "$$" + LaTeXfrac(p[0].ToString() + "+" + LaTeXsqrt(p[0] * p[0] + 4 * q[0]), "2") + "$$";
            }
            else if (lookups.TryGetValue(sd, out var expr))
            {
                scf = "$$" + expr + "$$";
            }
            else if (q.Length == 2 && q[0] == 0 && p.Length == 1)
            {
                var (P, Q) = (p[0], q[1]);
                var (a, b) = (P * P, 2 * Q);
                scf = "$$" + LaTeXfrac(LaTeXsqrt(2 * Q), LaTeXpow("e", a, b) + "\\Gamma\\left(\\frac12," + LaTeXfrac(a, b) + "\\right)") + "$$";
            }
            else if (q.Length == 1 && q[0] < 0 && p.Length == 2 && p[0] == 3 && p[1] == 2)
            {
                var sq = Math.Sqrt(-q[0]);
                var z = sq * sq == -q[0]
                    ? sq.ToString()
                    : q[0] > -10 ? $"\\sqrt{-q[0]}" : $"\\sqrt{{{-q[0]}}}";
                scf = $"$${{{-q[0]}\\over 1-{z}\\cot{{{z}}}}}$$";
            }
            else if (q.Length == 3 && q[0] == 0 && q[1] == 0 && q[2] == 1 && p.Length == 3 && p[0] == 0 && p[1] == 2 && p[2] == 1)
            {
                scf = "$$\\frac1{1-J_0(2)}-1$$";
            }

            return scf;
        }

        static void Main(string[] args)
        {
            const int maxScore = 11;

            var results = new Dictionary<Sigdig, (int[], int[], string, int)>();
            var lookups = new ConcurrentDictionary<Sigdig, string>();
            MpfrFloat.DefaultPrecision = 256;

            void MobiusOfConst(MpfrFloat x, string xs)
            {
                foreach (var (a, b, c, d) in
                    from c in Enumerable.Range(-9, 19)
                    from d in Enumerable.Range(-9, 19)
                    from a in Enumerable.Range(-9, 19)
                    from b in Enumerable.Range(-9, 19)
                    select (a, b, c, d))
                {
                    var q = c + d * x;
                    if (q <= 0) continue;
                    var g1 = GCD(c, d);
                    if (g1 == 5 || g1 == -5) continue;
                    var v = (a + b * x) / q;
                    if (v <= 0 || v == 1) continue;
                    var g2 = GCD(a, b);
                    if (GCD(g1, g2) != 1) continue;
                    if (v == g2 / (MpfrFloat)g1) continue;

                    var num = Poly.ToFactoredString(new[] { a, b }, xs);
                    var den = Poly.ToFactoredString(new[] { c, d }, xs);
                    var expr = den == "1" ? num : "\\frac{" + num + "}{" + den + "}";
                    var s = ((a + b * x) / (c + d * x)).ToString();
                    Sigdig sd = new(s);

                    lookups.TryAdd(sd, expr);
                }
            }

#if BUILD_LOOKUPS
            foreach (var (a, b) in Seq.Rationals().Take(520).Where((r, _) => r.Item1 < 5 && r.Item2 < 5))
            {
                var x = a / (MpfrFloat)b;
                MobiusOfConst(MpfrFloat.Power(MpfrFloat.ConstPi(), x), LaTeXpow("\\pi", a, b));
                MobiusOfConst(MpfrFloat.Power(MpfrFloat.Exp(1), x), LaTeXpow("e", a, b));
                MobiusOfConst(MpfrFloat.Power(MpfrFloat.ConstCatalan(), x), LaTeXpow("G", a, b));

                for (var z = 3; z <= 7; z += 2)
                {
                    MobiusOfConst(MpfrFloat.Power(MpfrFloat.Zeta(z), x), LaTeXpow($"\\zeta({z})", a, b));
                }
            }

            {
                using var file = File.CreateText("lookups.txt");

                foreach (var lkp in lookups)
                {
                    file.WriteLine($"{lkp.Key}\t{lkp.Value}");
                }
            }
#else
            {
                using var file = File.OpenText("lookups.txt");
                string s;

                while ((s = file.ReadLine()) != null)
                {
                    var t = s.IndexOf('\t');
                    Sigdig key = new(s[..t]);
                    var value = s[(t+1)..];
                    lookups[key] = value;
                }
            }
#endif

            Sigdig StrIndex(MpfrFloat f)
            {
                var s = MpfrFloat.Abs(f, null).ToString();
                return new(s);
            }

            /*
            // mixed surds
            var surds =
                from q in new[] { 2, 3, 5, 6, 7, 10, 11, 13, 14, 15, 17, 19, 21, 22, 23, 26, 29, 30, 31, 33, 34, 35, 37, 38, 39 }
                where Math.Sqrt(q) != (int)Math.Sqrt(q)
                from a in Enumerable.Range(-9, 19)
                from b in Enumerable.Range(-9, 19)
                where (a > 0 || b > 0) && b != 5
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

                    lookups.AddOrUpdate(s, s => lk, (s, old) => old.Length > lk.Length ? lk : old);
                }
            }
            */

            foreach (var (p, q) in Seq.Rationals().TakeWhile(_ => _.Item2 < 100))
            {
                var x = (MpfrFloat.ConstPi() * p) / q;
                var num = Poly.ToFactoredString(new[] { 0, p }, "\\pi");
                var frac = q == 1 ? num : "\\frac{" + num + "}{" + q + "}";

                void Add(string trig, Func<MpfrFloat, int?, MpfrRounding?, MpfrFloat> func)
                {
                    //try
                    {
                        var y = func(x, null, null);
                        var s = y.ToString();

                        if (s.Length >= Sigdig.Count)
                        {
                            Sigdig sd = new(s);

                            lookups.TryAdd(sd, trig + "(" + frac + ")");
                        }
                    }
                    //catch
                    {
                        //Debugger.Break();
                    }
                }

                Add("tan", MpfrFloat.Tan);
                Add("sin", MpfrFloat.Sin);
                Add("cos", MpfrFloat.Cos);
                Add("cot", (x, _, _) => 1 / MpfrFloat.Tan(x));
                Add("csc", (x, _, _) => 1 / MpfrFloat.Sin(x));
                Add("sec", (x, _, _) => 1 / MpfrFloat.Cos(x));
                Add("exp", MpfrFloat.Exp);
                Add("ln", MpfrFloat.Log);
                Add("sqrt", MpfrFloat.Sqrt);
                Add("tanh", MpfrFloat.Tanh);
                Add("sinh", MpfrFloat.Sinh);
                Add("cosh", MpfrFloat.Cosh);
                Add("tan^{-1}", MpfrFloat.Atan);
                Add("sinh^{-1}", MpfrFloat.Asinh);

                if (x < 1)
                {
                    Add("tanh^{-1}", MpfrFloat.Atanh);
                    Add("cos^{-1}", MpfrFloat.Acos);
                    Add("sin^{-1}", MpfrFloat.Asin);
                }
                else if (x > 1)
                {
                    Add("cosh^{-1}", MpfrFloat.Acosh);
                }

                x = p / (MpfrFloat)q;
                frac = q == 1 ? p.ToString() : "\\frac{" + p + "}{" + q + "}";

                Add("tan", MpfrFloat.Tan);
                Add("sin", MpfrFloat.Sin);
                Add("cos", MpfrFloat.Cos);
                Add("cot", (x, _, _) => 1 / MpfrFloat.Tan(x));
                Add("csc", (x, _, _) => 1 / MpfrFloat.Sin(x));
                Add("sec", (x, _, _) => 1 / MpfrFloat.Cos(x));
                Add("exp", MpfrFloat.Exp);
                Add("ln", MpfrFloat.Log);
                Add("sqrt", MpfrFloat.Sqrt);
                Add("tanh", MpfrFloat.Tanh);
                Add("sinh", MpfrFloat.Sinh);
                Add("cosh", MpfrFloat.Cosh);
                Add("tan^{-1}", MpfrFloat.Atan);
                Add("sinh^{-1}", MpfrFloat.Asinh);

                if (x < 1)
                {
                    Add("tanh^{-1}", MpfrFloat.Atanh);
                    Add("cos^{-1}", MpfrFloat.Acos);
                    Add("sin^{-1}", MpfrFloat.Asin);
                }
                else if (x > 1)
                {
                    Add("cosh^{-1}", MpfrFloat.Acosh);
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
                for (var d = 1; d < 19; d++)
                {
                    var g = GCD(n, d);

                    if (g == 1)
                    {
                        var x = (MpfrFloat)n / (MpfrFloat)d;
                        MpfrFloat y;
                        Sigdig s;

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

                            y = x * MpfrFloat.JN(k + 1, x) / MpfrFloat.JN(k, x);
                            s = StrIndex(y);

                            if (d == 1)
                                if (n == 1)
                                    lookups[s] = $"\\frac {{J_{k + 1}({n})}} {{J_{k}({n})}}";
                                else
                                    lookups[s] = $"\\frac {{{n} J_{k + 1}({n})}} {{J_{k}({n})}}";
                            else
                                if (n == 1)
                                lookups[s] = $"\\frac {{J_{k + 1}(\\frac {n} {d})}} {{{d} J_{k}(\\frac {n} {d})}}";
                            else
                                lookups[s] = $"\\frac {{{n} J_{k + 1}(\\frac {n} {d})}} {{{d} J_{k}(\\frac {n} {d})}}";
                        }

                        y = BesselI(x, .5) / BesselI(x - 1, .5);
                        s = StrIndex(y);

                        if (d == 1)
                            lookups[s] = $"\\frac {{I_{{{n}}}(\\frac 1 2)}} {{I_{{{n - d}}}(\\frac 1 2)}}";
                        else
                            lookups[s] = $"\\frac {{I_{{\\frac {n} {d}}}(\\frac 1 2)}} {{I_{{\\frac {n - d} {d}}}(\\frac 1 2)}}";

                        y = BesselI(x, 1) / BesselI(x - 1, 1);
                        s = StrIndex(y);

                        if (d == 1)
                            lookups[s] = $"\\frac {{I_{{{n}}}(1)}} {{I_{{{n - d}}}(1)}}";
                        else
                            lookups[s] = $"\\frac {{I_{{\\frac {n} {d}}}(1)}} {{I_{{\\frac {n - d} {d}}}(1)}}";

                        y = BesselI(x, (MpfrFloat)2 / d) / BesselI(x - 1, (MpfrFloat)2 / d);
                        s = StrIndex(y);

                        if (d == 1)
                            lookups[s] = $"\\frac {{I_{{{n}}}(2)}} {{I_{{{n - d}}}(2)}}";
                        else if (d == 2)
                            lookups[s] = $"\\frac {{I_{{\\frac {n} 2}}(1)}} {{I_{{\\frac {n - d} 2}}(1)}}";
                        else if ((d & 1) == 0)
                            lookups[s] = $"\\frac {{I_{{\\frac {n} {d}}}(\\frac 1 {d / 2})}} {{I_{{\\frac {n - d} {d}}}(\\frac 1 {d / 2})}}";
                        else
                            lookups[s] = $"\\frac {{I_{{\\frac {n} {d}}}(\\frac 2 {d})}} {{I_{{\\frac {n - d} {d}}}(\\frac 2 {d})}}";
                    }
                }
            }

#if true
            var pairs =
                from score in Enumerable.Range(2, maxScore - 1)
                from score1 in Enumerable.Range(1, score - 1)
                let score2 = score - score1
                from p in Poly.WithScore(score1)
                from q in Poly.WithScore(score2)
                select (p, q);

            foreach (var (p, q) in pairs)
            {
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
                var s = CF.Digits(Captured(cf), Sigdig.Count);

                if (s.EndsWith(CF.InvalidDigit))
                    continue;

                var termsUsed = Math.Max(pterms, qterms);
                Sigdig sd = new(s);
                
                if (!results.TryGetValue(sd, out var result) || result.Item4 > termsUsed)
                {
                    var scf = Lookup(sd, lookups, p, q);
                    results[sd] = (p, q, scf, termsUsed);
                    Console.WriteLine($"{s}\t{termsUsed}");
                }
            }

            var list = results.ToList();
            results = null;
            list.Sort((a, b) => a.Key.CompareTo(b.Key));

            using (var file = File.CreateText("../../polygcf.md"))
            {
                file.WriteLine("---");
                file.WriteLine("title: Values of Generalized Continued Fractions with Polynomial Terms");
                file.WriteLine("tag: math");
                file.WriteLine("---");
                file.WriteLine();
                file.WriteLine("Intended to be found by search engines when a value is known but the generalized continued fraction is unknown.");
                file.WriteLine($"These are the first {list.Count:N0} converging values with polynomials of lowest total score.");
                file.WriteLine("Polynomial score is equal to its degree plus the sum of absolute values of its coefficients.");
                file.WriteLine();
                file.WriteLine("$$");
                file.WriteLine("x = b_0 + \\cfrac {a_1} {b_1 + \\cfrac {a_2} {b_2 + \\ddots}}");
                file.WriteLine("$$");
                file.WriteLine();
                file.WriteLine("The 'Terms' column is the number of continued fraction terms needed to calculate");
                file.WriteLine("the value to the precision shown.");
                file.WriteLine();
                file.WriteLine("|Digits of $$x$$|a<sub>n</sub>|b<sub>n</sub>|Expression|Terms|");
                file.WriteLine("|--------------|----|----|---------|-----|");

                foreach (var pair in list)
                {
                    var value = pair.Key;
                    var (p, q, scf, tu) = pair.Value;

                    if (scf.Length > 0)
                    {
                        file.WriteLine($"|{value}|{Poly.ToFactoredString(q)}|{Poly.ToFactoredString(p)}|{scf}|{tu}|");
                    }
                }

                file.WriteLine();
                file.WriteLine("Entries with unknown closed-form expression:");
                file.WriteLine();
                file.WriteLine("|Digits of $$x$$|a<sub>n</sub>|b<sub>n</sub>|Terms|");
                file.WriteLine("|--------------|----|----|-----|");

                foreach (var pair in list)
                {
                    var value = pair.Key;
                    var (p, q, scf, tu) = pair.Value;

                    if (scf.Length == 0)
                    {
                        file.WriteLine($"|{value}|{Poly.ToFactoredString(q)}|{Poly.ToFactoredString(p)}|{tu}|");
                    }
                }
            }
#endif

            foreach (var score in Enumerable.Range(19, short.MaxValue))
            {
                using var file = File.CreateText($"../../polygcf{score}.md");
                file.AutoFlush = true;
                file.WriteLine("|Digits of $$x$$|a<sub>n</sub>|b<sub>n</sub>|Expression|Terms|");
                file.WriteLine("|--------------|----|----|---------|-----|");

                var pq =
                    from score1 in Enumerable.Range(1, score - 1)
                    let score2 = score - score1
                    from q in Poly.WithScore(score2)
                    from p in Poly.WithScore(score1)
                    select (p, q);

                Parallel.ForEach(Partitioner.Create(pq).GetPartitions(Environment.ProcessorCount - 1), part =>
                {
                    while (part.MoveNext())
                    {
                        (int[] p, int[] q) = part.Current;

                        if (p.Length == 1 && q.Length == 1)
                            continue;

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
                        var s = CF.Digits(Captured(cf), Sigdig.Count);

                        if (s.EndsWith(CF.InvalidDigit))
                            continue;

                        var termsUsed = Math.Max(pterms, qterms);
                        Sigdig sd = new(s);
                        var scf = Lookup(sd, lookups, p, q);

                        if (scf.Length > 0)
                        {
                            var line = $"|{sd}|{Poly.ToFactoredString(q)}|{Poly.ToFactoredString(p)}|{scf}|{termsUsed}|";

                            lock (Console.Out)
                            {
                                Console.WriteLine(line);
                                file.WriteLine(line);
                            }
                        }
                    }
                });

                file.WriteLine();
                file.WriteLine("Done");
                file.Dispose();
            }
        }
    }
}
