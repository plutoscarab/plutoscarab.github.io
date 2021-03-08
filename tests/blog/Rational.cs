using System;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlutoScarab;

namespace tests
{
    [TestClass]
    public class RationalTests
    {
        private static string Num(BigInteger n)
        {
            var s = new StringBuilder(n.ToString());

            if (s.Length > 5)
            {
                var i = s.Length - 3;

                while (i > 0)
                {
                    s.Insert(i, "\\ ");
                    i -= 3;
                }
            }

            return s.ToString();
        }

        [TestMethod]
        public void FromDouble()
        {
            var e = Math.PI;
            var r = (Rational)e;
            Trace.WriteLine($"{r.p}/{r.q}");
            var cf = CF.FromRatio(r.p, r.q);
            Trace.WriteLine(string.Join(", ", cf));
            var lo = new Rational(r.p * 2 - 1, r.q * 2);
            var hi = new Rational(r.p * 2 + 1, r.q * 2);
            Trace.WriteLine(lo);
            Trace.WriteLine(hi);
            Trace.WriteLine(string.Join(", ", CF.FromRatio(lo.p, lo.q)));
            Trace.WriteLine(string.Join(", ", CF.FromRatio(hi.p, hi.q)));
            Trace.WriteLine($"\\frac {{{Num(lo.p)}}} {{{Num(lo.q)}}} \\lt");
            Trace.WriteLine($"\\frac {{{Num(r.p)}}} {{{Num(r.q)}}} \\lt");
            Trace.WriteLine($"\\frac {{{Num(hi.p)}}} {{{Num(hi.q)}}}");

            e = 1.0 / 3;
            r = (Rational)e;
            Trace.WriteLine($"{r.p}/{r.q}");
            cf = CF.FromRatio(r.p, r.q);
            Trace.WriteLine(string.Join(", ", cf));
            lo = new Rational(r.p * 2 - 1, r.q * 2);
            hi = new Rational(r.p * 2 + 1, r.q * 2);
            Trace.WriteLine(lo);
            Trace.WriteLine(hi);
            Trace.WriteLine(string.Join(", ", CF.FromRatio(lo.p, lo.q)));
            Trace.WriteLine(string.Join(", ", CF.FromRatio(hi.p, hi.q)));
        }

        [TestMethod]
        public void BestDouble()
        {
            var r = Rational.Best(Math.PI);
            Trace.WriteLine($"{r.p} / {r.q}");
            Trace.WriteLine($"\\frac {{{Num(r.p)}}} {{{Num(r.q)}}}");
            Assert.AreEqual(Math.PI, (double)r.p / (double)r.q);
            var rand = new Random();

            for (var i = 0; i < 10000; i++)
            {
                var x = 1 / (rand.NextDouble() - .5);
                r = Rational.Best(x);
                Assert.AreEqual(x, (double)r.p / (double)r.q);
            }
        }

        [TestMethod]
        public void BestFloat()
        {
            var r = Rational.Best((float)Math.PI);
            Trace.WriteLine($"{r.p} / {r.q}");
            Trace.WriteLine($"\\frac {{{Num(r.p)}}} {{{Num(r.q)}}}");
            Assert.AreEqual((float)Math.PI, (float)r.p / (float)r.q);
            var rand = new Random();

            for (var i = 0; i < 1000; i++)
            {
                var x = 1 / ((float)rand.NextDouble() - .5f);
                r = Rational.Best(x);
                Assert.AreEqual(x, (float)r.p / (float)r.q);

                while (r.q > 1)
                {
                    r = new Rational((BigInteger)((double)(r.q - 1) * x + .5), r.q - 1);
                    Assert.AreNotEqual(x, (float)r.p / (float)r.q);
                }
            }
        }


        [TestMethod]
        public void TableOfBest()
        {
            void Row(string name, double d)
            {
                Console.Write($"|$${name}$$|{d}|");
                var r = Rational.Best(d);
                Console.Write("$$\\frac {" + Num(r.p) + "} {" + Num(r.q) + "}$$|");
                r = Rational.Best((float)d);
                Console.WriteLine("$$\\frac {" + Num(r.p) + "} {" + Num(r.q) + "}$$|");
            }

            Row("\\pi", Math.PI);
            Row("e", Math.E);
            Row("\\sqrt 2", Math.Sqrt(2));
            Row("\\phi", (Math.Sqrt(5) + 1) / 2);
            Row("ln(2)", Math.Log(2));
            Row("\\gamma", 0.57721566490153286);
            Row("G", 0.915965594177219015054603514932384110774);
            Row("\\zeta(3)", 1.202056903159594);
            Row("\\sqrt {2\\pi}", Math.Sqrt(2 * Math.PI));
        }
    }
}