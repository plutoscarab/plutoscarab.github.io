using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlutoScarab;
using System;

namespace tests
{
    [TestClass]
    public class LaTeXTests
    {
        [TestMethod]
        public void FromExpression()
        {
            var paramExpr = LaTeX.From((double x) => x);
            Assert.AreEqual("x", paramExpr);
            var subtractExpr = LaTeX.From((double x) => x - 1);
            Assert.AreEqual("x-1", subtractExpr);
            var addExpr = LaTeX.From((double x) => x + 1);
            Assert.AreEqual("x+1", addExpr);
            var sqrtExpr1 = LaTeX.From((double x) => Math.Sqrt(x - 1));
            Assert.AreEqual("\\sqrt{" + subtractExpr + "}", sqrtExpr1);
            var sqrtExpr2 = LaTeX.From((double x) => Math.Sqrt(x + 1));
            Assert.AreEqual("\\sqrt{" + addExpr + "}", sqrtExpr2);
            var mulMulExpr = LaTeX.From((double x) => x * Math.Sqrt(x - 1) * Math.Sqrt(x + 1));
            Assert.AreEqual("x" + sqrtExpr1 + sqrtExpr2, mulMulExpr);
            var logExpr = LaTeX.From((double x) => Math.Log(1 + x));
            Assert.AreEqual("\\ln(1+x)", logExpr);
            var s = LaTeX.From((double x) => Math.Log(x * Math.Sqrt(x - 1) * Math.Sqrt(x + 1)));
            var e = "\\ln(x\\sqrt{x-1}\\sqrt{x+1})";
            Assert.AreEqual(e, s);
        }

        [TestMethod]
        public void FracAddPiTauByteE()
        {
            var s = LaTeX.From((double x) => byte.MaxValue - Math.PI / (Math.E + x));
            var e = "255-\\frac{\\pi}{e+x}";
            Assert.AreEqual(e, s);
        }

        [TestMethod]
        public void AndOrNot()
        {
            var s = LaTeX.From((bool b) => !b && (b || false));
            var e = "\\neg b\\wedge (b\\vee F)";
            Assert.AreEqual(e, s);
        }

        [TestMethod]
        public void UnaryPlusMinus()
        {
            var s = LaTeX.From((double z) => (+z) * (-z));
            var e = "z(-z)";
            Assert.AreEqual(e, s);
        }

        [TestMethod]
        public void WeirdMath()
        {
            var s = LaTeX.From((double x) => Math.IEEERemainder(x, 3));
            var e = "IEEERemainder(x,3)";
            Assert.AreEqual(e, s);
        }

        [TestMethod]
        public void ProductOfSums()
        {
            var s = LaTeX.From((double x) => (x + 1) * (x - 2));
            var e = "(x+1)(x-2)";
            Assert.AreEqual(e, s);
            s = LaTeX.From((double x) => 2 * (x + 1));
            e = "2(x+1)";
            Assert.AreEqual(e, s);
        }
    }
}