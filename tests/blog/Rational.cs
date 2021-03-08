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
            var i = s.Length - 3;

            while (i > 0)
            {
                s.Insert(i, "\\ ");
                i -= 3;
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

            var x = 165707065.0 / 52746197;
            Assert.AreEqual(Math.PI, x);
        }

        [TestMethod]
        public void Best()
        {
            var r = Rational.Best(Math.PI);
            Trace.WriteLine($"{r.p} / {r.q}");
            Trace.WriteLine($"\\frac {{{Num(r.p)}}} {{{Num(r.q)}}}");
            Assert.AreEqual(Math.PI, (double)r.p / (double)r.q);
            var rand = new Random();

            while (true)
            {
                var x = rand.NextDouble();
                r = Rational.Best(x);
                Assert.AreEqual(x, (double)r.p / (double)r.q);
            }
        }
    }
}