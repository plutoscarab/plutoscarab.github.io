using System;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlutoScarab;

namespace tests
{
    [TestClass]
    public class ContinuedFractions
    {
        [TestMethod]
        public void NegateTypical()
        {
            var x = CF.Negate(new[] { 1, 2, 3, 4 });
            Assert.IsTrue(Enumerable.SequenceEqual(new BigInteger[] { -2, 1, 1, 3, 4 }, x));
        }

        [TestMethod]
        public void NegateCollapse()
        {
            var x = CF.Negate(new[] { 1, 1, 3, 4 });
            Assert.IsTrue(Enumerable.SequenceEqual(new BigInteger[] { -2, 4, 4 }, x));
        }

        [TestMethod]
        public void NegateInteger()
        {
            var x = CF.Negate(new[] { 18 });
            Assert.IsTrue(Enumerable.SequenceEqual(new BigInteger[] { -18 }, x));
        }

        [TestMethod]
        public void NegateZero()
        {
            var x = CF.Negate(new[] { 0 });
            Assert.IsTrue(Enumerable.SequenceEqual(new BigInteger[] { 0 }, x));
        }

        [TestMethod]
        public void NegateInfinity()
        {
            var x = CF.Negate(new BigInteger[0]);
            Assert.IsTrue(Enumerable.SequenceEqual(new BigInteger[0], x));
        }

        [TestMethod]
        public void WriteInfinity()
        {
            var x = CF.ToString(new int[0], 5);
            Assert.AreEqual("∞", x);
        }

        [TestMethod]
        public void WriteInteger()
        {
            var x = CF.ToString(new[] { 27 }, 5);
            Assert.AreEqual("27", x);
        }

        [TestMethod]
        public void WritePositive()
        {
            var x = CF.ToString(new[] { 1, 2, 3, 4 }, 5);
            Assert.AreEqual("1.43333", x);
        }

        [TestMethod]
        public void WriteTerminating()
        {
            var x = CF.ToString(new[] { 0, 8 }, 5);
            Assert.AreEqual("0.125", x);
        }

        [TestMethod]
        public void SimplifyE()
        {
            var e = CF.Simplify(CF.Nats().Select(n => 3 + n), CF.Nats().Select(n => -1 - n)).Take(10).ToList();
            Assert.IsTrue(Enumerable.SequenceEqual(new BigInteger[] { 2, 1, 2, 1, 1, 4, 1, 1, 6, 1 }, e));
        }

        [TestMethod]
        public void SimplifyΠ()
        {
            var odds = CF.Nats().Select(n => 2 * n + 1);
            var squares = CF.Nats().Select(n => 1 + n * (2 + n));
            var pi = CF.Transform(CF.Simplify(odds, squares), 4, 0, 0, 1).Take(10).ToList();
            Assert.IsTrue(Enumerable.SequenceEqual(new BigInteger[] { 3, 7, 15, 1, 292, 1, 1, 1, 2, 1, }, pi));
        }

        [TestMethod]
        public void AddInt()
        {
            var x = CF.Random("AddInt".GetHashCode());
            var xs = x.Take(20).ToArray();
            var y = CF.Transform(x, 11, 1, 1, 0);
            var ys = y.Take(20).ToArray();
            xs[0] += 11;
            Assert.IsTrue(Enumerable.SequenceEqual(xs, ys));
        }

        [TestMethod]
        public void MulInt()
        {
            var x = new BigInteger[] { 1, 2, 3 };
            var y = CF.Transform(x, 0, 17, 1, 0);
            Assert.IsTrue(Enumerable.SequenceEqual(new BigInteger[] { 24, 3, 2 }, y), CF.ToString(y, 20));
        }

        [TestMethod]
        public void IntoInt()
        {
            var odds = CF.Nats().Select(n => 1 + 2 * n);
            var squares = CF.Nats(1).Select(n => n * n);
            var fourOverPi = CF.Simplify(odds, squares);
            var pi = CF.Transform(fourOverPi, 4, 0, 0, 1);
            Assert.AreEqual("3.14159265358979", CF.ToString(pi, 14));
        }

        [TestMethod]
        public void FromPosPosRational()
        {
            var cf = CF.FromRatio(355, 113);
            var e = new BigInteger[] { 3, 7, 16 };
            Assert.IsTrue(Enumerable.SequenceEqual(e, cf));
        }

        [TestMethod]
        public void FromNegPosRational()
        {
            var cf = CF.FromRatio(-355, 113);
            var e = new BigInteger[] { -4, 1, 6, 16 };
            Assert.IsTrue(Enumerable.SequenceEqual(e, cf));
        }

        [TestMethod]
        public void FromPosNegRational()
        {
            var cf = CF.FromRatio(355, -113);
            var e = new BigInteger[] { -4, 1, 6, 16 };
            Assert.IsTrue(Enumerable.SequenceEqual(e, cf));
        }

        [TestMethod]
        public void FromNegNegRational()
        {
            var cf = CF.FromRatio(-355, -113);
            var e = new BigInteger[] { 3, 7, 16 };
            Assert.IsTrue(Enumerable.SequenceEqual(e, cf));
        }

        [TestMethod]
        public void ToRatio()
        {
            var r = ((Rational)Math.PI).Reduce();
            var cf = CF.FromRatio(r.p, r.q);
            var (p, q) = CF.ToRatio(cf);
            Assert.AreEqual(r.p, p);
            Assert.AreEqual(r.q, q);
        }

        [TestMethod]
        public void Normalize()
        {
            var rand = new Random("Normalize".GetHashCode());

            for (var i = 0; i < 1000; i++)
            {
                var cf = new BigInteger[10];

                for (var j = 0; j < cf.Length; j++)
                {
                    cf[j] = rand.Next(5) - 2;
                }

                var value = CF.ToRatio(cf);
                var ncf = CF.Normalize(cf);
                var nvalue = CF.ToRatio(ncf);
                Assert.AreEqual(value.Item1 * nvalue.Item2, nvalue.Item1 * value.Item2);

                for (var j = 1; j < ncf.Count; j++)
                {
                    if (ncf[j].Sign < 1)
                    {
                        Assert.Fail($"Term [{j}] of normalized CF isn't positive. {string.Join(", ", cf)} -> {string.Join(", ", ncf)}");
                    }
                }

                if (ncf.Count > 1)
                {
                    Assert.AreNotEqual(BigInteger.One, ncf[^1]);
                }
            }
        }
    }
}
