using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlutoScarab;

namespace tests
{
    [TestClass]
    public class RationalTests
    {
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
    }
}