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
    record Info(string Expr, string Family);

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

        public static bool operator ==(Sigdig a, Sigdig b) => a.Equals(b);

        public static bool operator !=(Sigdig a, Sigdig b) => !a.Equals(b);
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

            if (s.Length == 1 && !char.IsLetter(s[0]))
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

        static Info Lookup(Sigdig sd, ConcurrentDictionary<Sigdig, Info> lookups, int[] p, int[] q)
        {
            var scf = string.Empty;
            var family = string.Empty;

            if (q.Length == 1 && p.Length == 1)
            {
                if ((p[0] & 1) == 0)
                    scf = "$$" + (p[0] / 2) + "+" + LaTeXsqrt(p[0] * p[0] / 4 + q[0]) + "$$";
                else
                    scf = "$$" + LaTeXfrac(p[0].ToString() + "+" + LaTeXsqrt(p[0] * p[0] + 4 * q[0]), "2") + "$$";

                family = "Surd";
            }
            else if (lookups.TryGetValue(sd, out var info))
            {
                scf = "$$" + info.Expr + "$$";
                family = info.Family;
            }
            else if (q.Length == 2 && q[0] == 0 && p.Length == 1)
            {
                var (P, Q) = (p[0], q[1]);
                var (a, b) = (P * P, 2 * Q);
                scf = "$$" + LaTeXfrac(LaTeXsqrt(2 * Q), LaTeXpow("e", a, b) + "\\Gamma\\left(\\frac12," + LaTeXfrac(a, b) + "\\right)") + "$$";
                family = "erfc";
            }
            else if (q.Length == 1 && q[0] < 0 && p.Length == 2 && p[0] == 3 && p[1] == 2)
            {
                var sq = Math.Sqrt(-q[0]);
                var z = sq * sq == -q[0]
                    ? sq.ToString()
                    : q[0] > -10 ? $"\\sqrt{-q[0]}" : $"\\sqrt{{{-q[0]}}}";
                scf = $"$${{{-q[0]}\\over 1-{z}\\cot{{{z}}}}}$$";
                family = "cot";
            }
            else if (q.Length == 3 && q[0] == 0 && q[1] == 0 && q[2] == 1 && p.Length == 3 && p[0] == 0 && p[1] == 2 && p[2] == 1)
            {
                scf = "$$\\frac1{1-J_0(2)}-1$$";
                family = "Bessel J0";
            }

            return new(scf, family);
        }

        static int[] PSLQ(double[] x)
        {
            var γ = 2 / Math.Sqrt(3);
            var n = x.Length;
            var A = new double[n + 1, n + 1];
            var B = new double[n + 1, n + 1];

            for (var i = 1; i <= n; i++)
                A[i, i] = B[i, i] = 1.0;

            var s = new double[n + 1];

            for (var k = 1; k <= n; k++)
                s[k] = Math.Sqrt(Enumerable.Range(k, n - k + 1).Select(j => x[j - 1] * x[j - 1]).Sum());

            var y = new double[n + 1];
            var t = s[1];

            for (var k = 1; k <= n; k++)
            {
                y[k] = x[k - 1] / t;
                s[k] /= t;
            }

            var H = new double[n + 1, n];

            for (var i = 1; i <= n; i++)
            {
                if (i < n) H[i, i] = s[i + 1] / s[i];

                for (var j = 1; j < i; j++)
                    H[i, j] = -y[i] * y[j] / (s[j] * s[j + 1]);
            }

            for (var i = 2; i <= n; i++)
            {
                for (var j = i - 1; j >= 1; j--)
                {
                    t = Math.Round(H[i, j] / H[j, j]);
                    y[j] += t * y[i];

                    for (var k = 1; k <= j; k++)
                        H[i, k] -= t * H[j, k];

                    for (var k = 1; k <= n; k++)
                    {
                        A[i, k] -= t * A[j, k];
                        B[k, j] += t * B[k, i];
                    }
                }
            }

            while (true)
            {
                var max = double.MinValue;
                var m = -1;

                for (var i = 1; i < n; i++)
                {
                    var q = Math.Pow(γ, i) * Math.Abs(H[i, i]);
                    if (q > max) { max = q; m = i; }
                }

                (y[m], y[m + 1]) = (y[m + 1], y[m]);

                for (var i = 1; i <= n; i++)
                    (A[m, i], A[m + 1, i]) = (A[m + 1, i], A[m, i]);

                for (var i = 1; i < n; i++)
                    (H[m, i], H[m + 1, i]) = (H[m + 1, i], H[m, i]);

                for (var i = 1; i <= n; i++)
                    (B[i, m], B[i, m + 1]) = (B[i, m + 1], B[i, m]);

                if (m <= n - 2)
                {
                    var t0 = Math.Sqrt(Math.Pow(H[m, m], 2) + Math.Pow(H[m, m + 1], 2));
                    var t1 = H[m, m] / t0;
                    var t2 = H[m, m + 1] / t0;

                    for (var i = m; i <= n; i++)
                    {
                        var t3 = H[i, m];
                        var t4 = H[i, m + 1];
                        H[i, m] = t1 * t3 + t2 * t4;
                        H[i, m + 1] = -t2 * t3 + t1 * t4;
                    }
                }

                for (var i = m + 1; i <= n; i++)
                {
                    for (var j = Math.Min(i - 1, m + 1); j >= 1; j--)
                    {
                        t = Math.Round(H[i, j] / H[j, j]);
                        y[j] += t * y[i];

                        for (var k = 1; k <= j; k++)
                            H[i, k] -= t * H[j, k];

                        for (var k = 1; k <= n; k++)
                        {
                            A[i, k] -= t * A[j, k];
                            B[k, j] += t * B[k, i];
                        }
                    }
                }

                var min = double.MaxValue;
                var c = -1;

                for (var i = 1; i <= n; i++)
                {
                    if (Math.Abs(y[i]) < min) { min = Math.Abs(y[i]); c = i; }
                }

                if (min < 1e-7)
                {
                    var result = new int[n];

                    for (var i = 1; i <= n; i++)
                        result[i - 1] = (int)Math.Round(B[i, c]);

                    return result;
                }
            }
        }

        static MpfrFloat AGM(MpfrFloat a, MpfrFloat b)
        {
            while (a != b)
            {
                (var old, a, b) = (a, (a + b) / 2, MpfrFloat.Sqrt(a * b));

                if (a == old || b == old)
                    break;
            }

            return a;
        }

        static void Main(string[] args)
        {
            MpfrFloat.DefaultPrecision = 256;
            var ϖ = MpfrFloat.Parse("2.6220575542921198104648395898911194136827549514316231628168");

            const int maxScore = 11;

            Dictionary<Sigdig, (int[], int[], string, int)> results = new();
            ConcurrentDictionary<Sigdig, Info> lookups = new();

            void MobiusOfConst(MpfrFloat x, string xs, string family, int max = 9)
            {
                foreach (var (a, b, c, d) in
                    from c in Enumerable.Range(-max, 2 * max + 1)
                    from d in Enumerable.Range(-max, 2 * max + 1)
                    from a in Enumerable.Range(-max, 2 * max + 1)
                    from b in Enumerable.Range(-max, 2 * max + 1)
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

                    lookups.TryAdd(sd, new(expr, family));
                }
            }

#if false

            // Lemniscate constant
            MobiusOfConst(ϖ, "\\varpi", "Lemniscate ϖ");

            // Gauss AGM(1, sqrt(2))
            MobiusOfConst(AGM(1, MpfrFloat.Sqrt(2)), "{\\operatorname{AGM}\\left(1,\\sqrt2\\right)}", "Gauss G");

            void MobiusOfPower(MpfrFloat f, string fs, string family)
            {
                foreach (var (a, b) in Seq.Rationals().Take(520).Where((r, _) => r.Item1 < 5 && r.Item2 < 5))
                {
                    var x = a / (MpfrFloat)b;
                    MobiusOfConst(MpfrFloat.Power(f, x), LaTeXpow(fs, a, b), family);
                }
            }

            MobiusOfPower(MpfrFloat.ConstPi(), "\\pi", "π");
            MobiusOfPower(MpfrFloat.Exp(1), "e", "e");
            MobiusOfPower(MpfrFloat.Exp(MpfrFloat.ConstPi()), "{e^\\pi}", "exp π");
            MobiusOfPower(MpfrFloat.Log(2), "{\\operatorname{log}2}", "log");
            MobiusOfPower(MpfrFloat.ConstCatalan(), "G", "Catalan");
            MobiusOfPower(MpfrFloat.ConstEuler(), "γ", "Euler Gamma");

            for (var z = 3; z <= 7; z += 2)
            {
                MobiusOfPower(MpfrFloat.Zeta(z), "\\zeta({z})", "Zeta");
            }

            // first Feigenbaum constant
            MobiusOfPower(MpfrFloat.Parse("4.669201609102990671853203820466201617258185577475768632745"), "\\delta_F", "Feigenbaum δ");

            // Feigenbaum alpha
            MobiusOfPower(MpfrFloat.Parse("2.502907875095892822283902873218215786381271376727149977336"), "\\alpha_F", "Feigenbaum α");

            // Conway's constant
            MobiusOfPower(MpfrFloat.Parse("1.303577269034296391257099112152551890730702504659404875754"), "\\lambda_C", "Conway λ");

            // Khinchin's constant
            MobiusOfPower(MpfrFloat.Parse("2.685452001065306445309714835481795693820382293994462953051"), "K", "Khinchin");

            // Glaisher-Kinkelin constant 
            MobiusOfPower(MpfrFloat.Parse("1.2824271291006226368753425688697917277676889273250011920637"), "A", "Glaisher-Kinkelin");

            // Meissel–Mertens constant
            MobiusOfPower(MpfrFloat.Parse("0.2614972128476427837554268386086958590515666482611992061920"), "M", "Meissel–Mertens");

            // Golomb-Dickman constant
            MobiusOfPower(MpfrFloat.Parse("0.6243299885435508709929363831008372441796426201805292869735"), "\\lambda_GD", "Golomb-Dickman λ");

            Sigdig StrIndex(MpfrFloat f)
            {
                var s = MpfrFloat.Abs(f, null).ToString();
                return new(s);
            }

            foreach (var (p, q) in Seq.Rationals().TakeWhile(_ => _.Item2 < 100))
            {
                if (p < q)
                {
                    var agm = AGM(p, q);
                    var s = agm.ToString();
                    Sigdig sd = new(s);
                    lookups.TryAdd(sd, new("{\\operatorname{AGM}\\left(" + p + "," + q + "\\right)}", "AGM"));
                }

                var x = (MpfrFloat.ConstPi() * p) / q;
                var num = Poly.ToFactoredString(new[] { 0, p }, "\\pi");
                var frac = LaTeXfrac(num, q.ToString());

                void Add(string trig, Func<MpfrFloat, int?, MpfrRounding?, MpfrFloat> func)
                {
                    //try
                    {
                        var y = func(x, null, null);
                        var s = y.ToString();

                        if (s.Length >= Sigdig.Count)
                        {
                            Sigdig sd = new(s);

                            lookups.TryAdd(sd, new(trig + "\\left(" + frac + "\\right)", trig));
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
                Add("ln", MpfrFloat.Log);
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
                frac = LaTeXfrac(p, q);

                Add("tan", MpfrFloat.Tan);
                Add("sin", MpfrFloat.Sin);
                Add("cos", MpfrFloat.Cos);
                Add("cot", (x, _, _) => 1 / MpfrFloat.Tan(x));
                Add("csc", (x, _, _) => 1 / MpfrFloat.Sin(x));
                Add("sec", (x, _, _) => 1 / MpfrFloat.Cos(x));
                Add("ln", MpfrFloat.Log);
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
                        var family = "BesselJ";

                        for (var k = 0; k < 10; k++)
                        {
                            y = MpfrFloat.JN(k + 1, x) / MpfrFloat.JN(k, x);
                            s = StrIndex(y);

                            if (d == 1)
                                lookups[s] = new($"\\frac {{J_{k + 1}({n})}} {{J_{k}({n})}}", family);
                            else
                                lookups[s] = new($"\\frac {{J_{k + 1}(\\frac {n} {d})}} {{J_{k}(\\frac {n} {d})}}", family);

                            s = StrIndex(1 / y);

                            if (d == 1)
                                lookups[s] = new($"\\frac {{J_{k}({n})}} {{J_{k + 1}({n})}}", family);
                            else
                                lookups[s] = new($"\\frac {{J_{k}(\\frac {n} {d})}} {{J_{k + 1}(\\frac {n} {d})}}", family);

                            y = x * MpfrFloat.JN(k + 1, x) / MpfrFloat.JN(k, x);
                            s = StrIndex(y);

                            if (d == 1)
                                if (n == 1)
                                    lookups[s] = new($"\\frac {{J_{k + 1}({n})}} {{J_{k}({n})}}", family);
                                else
                                    lookups[s] = new($"\\frac {{{n} J_{k + 1}({n})}} {{J_{k}({n})}}", family);
                            else
                                if (n == 1)
                                lookups[s] = new($"\\frac {{J_{k + 1}(\\frac {n} {d})}} {{{d} J_{k}(\\frac {n} {d})}}", family);
                            else
                                lookups[s] = new($"\\frac {{{n} J_{k + 1}(\\frac {n} {d})}} {{{d} J_{k}(\\frac {n} {d})}}", family);
                        }

                        y = BesselI(x, .5) / BesselI(x - 1, .5);
                        s = StrIndex(y);
                        family = "BesselI";

                        if (d == 1)
                            lookups[s] = new($"\\frac {{I_{{{n}}}(\\frac 1 2)}} {{I_{{{n - d}}}(\\frac 1 2)}}", family);
                        else
                            lookups[s] = new($"\\frac {{I_{{\\frac {n} {d}}}(\\frac 1 2)}} {{I_{{\\frac {n - d} {d}}}(\\frac 1 2)}}", family);

                        y = BesselI(x, 1) / BesselI(x - 1, 1);
                        s = StrIndex(y);

                        if (d == 1)
                            lookups[s] = new($"\\frac {{I_{{{n}}}(1)}} {{I_{{{n - d}}}(1)}}", family);
                        else
                            lookups[s] = new($"\\frac {{I_{{\\frac {n} {d}}}(1)}} {{I_{{\\frac {n - d} {d}}}(1)}}", family);

                        y = BesselI(x, (MpfrFloat)2 / d) / BesselI(x - 1, (MpfrFloat)2 / d);
                        s = StrIndex(y);

                        if (d == 1)
                            lookups[s] = new($"\\frac {{I_{{{n}}}(2)}} {{I_{{{n - d}}}(2)}}", family);
                        else if (d == 2)
                            lookups[s] = new($"\\frac {{I_{{\\frac {n} 2}}(1)}} {{I_{{\\frac {n - d} 2}}(1)}}", family);
                        else if ((d & 1) == 0)
                            lookups[s] = new($"\\frac {{I_{{\\frac {n} {d}}}(\\frac 1 {d / 2})}} {{I_{{\\frac {n - d} {d}}}(\\frac 1 {d / 2})}}", family);
                        else
                            lookups[s] = new($"\\frac {{I_{{\\frac {n} {d}}}(\\frac 2 {d})}} {{I_{{\\frac {n - d} {d}}}(\\frac 2 {d})}}", family);
                    }
                }
            }

            {
                using var file = File.CreateText("lookups.txt");

                foreach (var lkp in lookups)
                {
                    var e = lkp.Value.Expr.Replace("{{", "{ {"); // Fixes rendering of "log2" instead of "log 2"
                    file.WriteLine($"{lkp.Key}\t{e}\t{lkp.Value.Family}");
                }
            }
#else
#if false
            {
                using var file = File.OpenText("lookups.txt");
                string s;

                while ((s = file.ReadLine()) != null)
                {
                    var t = s.Split('\t');
                    Sigdig key = new(t[0]);
                    Info value = new(t[1].Replace("{{", "{ {"), t[2]); // Fixes rendering of "log2" instead of "log 2"
                    lookups[key] = value;
                }
            }
#endif
#endif

            var families = lookups.Values.Select(v => v.Family).Distinct().ToList();

#if false
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
                var cfk = cf.Take(20).ToList();
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
                    var info = Lookup(sd, lookups, p, q);
                    results[sd] = (p, q, info.Expr, termsUsed);
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
                        var qs = Poly.ToFactoredString(q);
                        var ps = Poly.ToFactoredString(p);
                        file.WriteLine($"|{value}|{qs}|{ps}|{scf}|{tu}|");
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

#if false
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
                        var info = Lookup(sd, lookups, p, q);
                        var scf = info.Expr;

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
#endif

            bool TryComputeGCF(IEnumerable<BigInteger> ps, IEnumerable<BigInteger> qs, int digits, out string s, out int terms)
            {
                s = default;
                terms = default;
                var pterms = 0;
                var qterms = 0;
                ps = ps.Select(t => { pterms++; return t; });
                qs = qs.Select(t => { qterms++; return t; });
                var cf = CF.Simplify(ps, qs);
                var start = cf.Take(1).ToList();

                if (start.Count == 0)
                    return false;

                if (start[0].Sign < 0)
                    return false;

                pterms = qterms = 0;
                s = CF.Digits(cf, digits);

                if (s.EndsWith(CF.InvalidDigit))
                    return false;

                terms = Math.Max(pterms, qterms);
                return true;
            }

#if true
            Dictionary<string, StreamWriter> files = new();

            var consts = new[]
            {
                (MpfrFloat.ConstPi(), "\\pi", "π"),
                (MpfrFloat.Power(MpfrFloat.ConstPi(), 2), "\\pi^2", "π²"),
                (AGM(1, MpfrFloat.Sqrt(2)), "{\\operatorname{agm}(1,\\sqrt2)}", "AGM"),
                (ϖ, "\\varpi", "ϖ"),
                (MpfrFloat.Exp(1.0), "e", "e"),
                (MpfrFloat.Exp(MpfrFloat.ConstPi()), "e^\\pi", "e^π"),
                (MpfrFloat.Log(2.0), "\\operatorname{log}2", "log2"),
                (MpfrFloat.ConstCatalan(), "G", "Catalan"),
                (MpfrFloat.ConstEuler(), "γ", "γ"),
            };

            int Content(int[] a)
            {
                a = a.Select(Math.Abs).Where(i => i != 0).ToArray();
                if (a.Length == 1) return a[0];
                return a.Aggregate(GCD);
            }

            const int pDegree = 1;
            const int qDegree = 2;
            var folder = $"degree{qDegree}over{pDegree}";
            System.IO.Directory.CreateDirectory(folder);

            IEnumerable<(int[], int[])> Pairs()
            {
                BigInteger n = 0;
                var filename = folder + "\\index.txt";

                if (System.IO.File.Exists(filename))
                {
                    n = BigInteger.Parse(System.IO.File.ReadAllText(filename));
                }

                Console.WriteLine("Press any key to stop and save progress");

                while (!Console.KeyAvailable)
                {
                    yield return Poly.WithDegree(pDegree, qDegree, n);
                    n++;
                }

                System.IO.File.WriteAllText(filename, n.ToString());
                Console.ReadKey();
                Console.WriteLine("Completing existing threads");
            }

            Parallel.ForEach(Pairs(), (pq, _) =>
            {
                MpfrFloat.DefaultPrecision = 256;
                var (p, q) = pq;
                var contentP = Content(p);
                var contentQ = Content(q);

                if (contentP > 1 && contentP * contentP == contentQ)
                    return;

                var ps = CF.Nats().Select(n => Poly.Eval(p, n));
                var qs = CF.Nats().Skip(1).Select(n => Poly.Eval(q, n));

                if (!TryComputeGCF(ps, qs, Sigdig.Count, out var s, out var termsUsed))
                    return;

                var y = double.Parse(s);
                Sigdig sd = new(s);
                string precise = default;

                foreach (var (x, xs, family) in consts)
                {
                    var pslq = PSLQ(new[] { 1.0, (double)x, -y, -y * (double)x });

                    // Skip rational values
                    if (pslq[0] * pslq[3] == pslq[1] * pslq[2])
                        continue;

                    var ecf = (pslq[0] + x * pslq[1]) / (pslq[2] + x * pslq[3]);
                    var ecfs = ecf.ToString();

                    if (ecfs.Length < 2 * Sigdig.Count)
                        return;

                    Sigdig cd = new(ecfs);

                    if (cd == sd)
                    {
                        // Confirm equivalence with higher precision
                        if (precise is null)
                        {
                            if (!TryComputeGCF(ps, qs, Sigdig.Count * 2, out precise, out var ignore))
                                break;
                        }

                        if (ecfs[..(Sigdig.Count * 2)] != precise[..(Sigdig.Count * 2)])
                            continue;

                        if (pslq[0] + x * pslq[1] < 0)
                        {
                            // Negate all coefficients
                            for (var i = 0; i < pslq.Length; i++) pslq[i] *= -1;
                        }

                        string scf;

                        if (pslq[2] == 0)
                        {
                            var num = Poly.ToFactoredString(new[] { pslq[0] }, xs);
                            var den = Poly.ToFactoredString(new[] { 0, pslq[3] }, xs);

                            if (pslq[1] == 0)
                                scf = LaTeXfrac(num, den);
                            else if (pslq[1] * pslq[3] < 0)
                                scf = LaTeXfrac(num, den) + "-" + LaTeXfrac(-pslq[1], pslq[3]);
                            else
                                scf = LaTeXfrac(num, den) + "+" + LaTeXfrac(pslq[1], pslq[3]);
                        }
                        else if (pslq[3] == 0)
                        {
                            var num = Poly.ToFactoredString(new[] { 0, pslq[1] }, xs);
                            var den = Poly.ToFactoredString(new[] { pslq[2] }, xs);

                            if (pslq[0] == 0)
                                scf = LaTeXfrac(num, den);
                            else if (pslq[0] * pslq[2] < 0)
                                scf = LaTeXfrac(num, den) + "-" + LaTeXfrac(-pslq[0], pslq[2]);
                            else
                                scf = LaTeXfrac(num, den) + "+" + LaTeXfrac(pslq[0], pslq[2]);
                        }
                        else
                        {
                            var num = Poly.ToFactoredString(new[] { pslq[0], pslq[1] }, xs);
                            var den = Poly.ToFactoredString(new[] { pslq[2], pslq[3] }, xs);
                            scf = LaTeXfrac(num, den);
                        }

                        var k = "\\operatornamewithlimits{\\huge K}_{n=1}^\\infty" + LaTeXfrac(Poly.ToFactoredString(q, "n"), Poly.ToFactoredString(p, "n"));

                        if (p[0] != 0)
                            k = p[0] + "+" + k;

                        var line = $"|{sd}|$${k}$$|$${scf}$$|{termsUsed}|";
                        StreamWriter file;

                        lock (files)
                        {
                            if (!files.TryGetValue(family, out file))
                            {
                                var filename = folder + "\\" + family.Replace(" ", "_") + ".md";
                                var needHeader = !File.Exists(filename);
                                files[family] = file = File.AppendText(filename);
                                file.AutoFlush = true;

                                if (needHeader)
                                {
                                    file.WriteLine("|Significant digits|Continued fraction|Expression|Terms|");
                                    file.WriteLine("|--------------|:-------:|:-------:|-----|");
                                }
                            }
                        }

                        lock (Console.Out)
                        {
                            Console.WriteLine(line);
                        }

                        lock (file)
                        {
                            file.WriteLine(line);
                        }

                        break;
                    }
                }
            });
#endif
        }
    }
}
