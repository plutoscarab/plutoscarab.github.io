using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlutoScarab;
using System.Linq;

namespace tests
{
    [TestClass]
    public class PolyITests
    {
        [TestMethod]
        public void Multiply()
        {
            var a = new PolyI(3, 1);
            var b = new PolyI(-1, 1);
            var c = new PolyI(4, 1);
            var d = new PolyI(-5, 1);
            var p = a * b * c * d;
            Assert.AreEqual("60 − 37𝑛 − 25𝑛² + 𝑛³ + 𝑛⁴", p.ToString());
        }

        [TestMethod]
        public void Subtract()
        {
            var a = new PolyI(3, 1);
            var b = new PolyI(-1, 1);
            var c = new PolyI(4, 1);
            var d = new PolyI(-5, 1);
            var p = a * b - c * d;
            Assert.AreEqual("17 + 3𝑛", p.ToString());
        }

        [TestMethod]
        public void Evaluate()
        {
            var p = new PolyI(60, -37, -25, 1, 1);
            var x = p.At(-7);
            Assert.AreEqual(60 - 7 * (-37 - 7 * (-25 - 7 * (1 - 7))), x);
        }

        [TestMethod]
        public void Pow()
        {
            var p = new PolyI(-3, 1) ^ 7;
            Assert.AreEqual(new PolyI(-2187, 5103, -5103, 2835, -945, 189, -21, 1), p);
        }

        [TestMethod]
        public void Derivative()
        {
            var p = new PolyI(5, -4, 3, -2, 1);
            var d = p.Derivative();
            Assert.AreEqual(new PolyI(-4, 6, -6, 4), d);
        }

        [TestMethod]
        public void ContentAllPositive()
        {
            var p = new PolyI(3, 9, 6);
            var n = p.Content();
            Assert.AreEqual(3, n);
        }

        [TestMethod]
        public void ContentMixed()
        {
            var p = new PolyI(3, -9, 6);
            var n = p.Content();
            Assert.AreEqual(3, n);
        }

        [TestMethod]
        public void ContentAllNegative()
        {
            var p = new PolyI(-3, -6, -6);
            var n = p.Content();
            Assert.AreEqual(-3, n);
        }

        [TestMethod]
        public void ContentWithZeros()
        {
            var p = new PolyI(2, 0, 4, 0, 2);
            var n = p.Content();
            Assert.AreEqual(2, n);
        }

        [TestMethod]
        public void PrimitivePart()
        {
            var p = new PolyI(3, 0, -6, 6).PrimitivePart();
            Assert.AreEqual(new PolyI(1, 0, -2, 2), p);
        }

        [TestMethod]
        public void FactorsNoContent()
        {
            var p = new PolyI(2, 3).Factors().ToList();
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual(new PolyI(2, 3), p[0]);
        }
        
        [TestMethod]
        public void FactorWithContent()
        {
            var p = new PolyI(2, 4).Factors().ToList();
            Assert.AreEqual(2, p.Count);
            Assert.AreEqual(new PolyI(2), p[0]);
            Assert.AreEqual(new PolyI(1, 2), p[1]);
        }
        
        [TestMethod]
        public void FactorWithMonomial()
        {
            var p = new PolyI(0, 2, 4).Factors().ToList();
            Assert.AreEqual(2, p.Count);
            Assert.AreEqual(new PolyI(0, 2), p[0]);
            Assert.AreEqual(new PolyI(1, 2), p[1]);
        }

        [TestMethod]
        public void DivNoRem()
        {
            var x = new PolyI(2, 1);
            var y = new PolyI(1, -5, 3);
            var p = x * y;  // (2 + n)(1 - 5n + 3n^2) = 2 - 9n + n^2 + 3n^3
            var d = p.DivRem(x, out var r);
            Assert.AreEqual(y, d);
            Assert.AreEqual(PolyI.Zero, r);
            d = p.DivRem(y, out r);
            Assert.AreEqual(x, d);
            Assert.AreEqual(PolyI.Zero, r);
        }

        [TestMethod]
        public void DivRem()
        {
            var x = new PolyI(2, 1);
            var y = new PolyI(1, -5, 3);
            var z = new PolyI(7);
            var p = x * y + z;  // (2 + n)(1 - 5n + 3n^2) + 7 = 9 - 9n + n^2 + 3n^3
            var d = p.DivRem(x, out var r);
            Assert.AreEqual(y, d);
            Assert.AreEqual(z, r);
            d = p.DivRem(y, out r);
            Assert.AreEqual(x, d);
            Assert.AreEqual(z, r);
        }
    }

    [TestClass]
    public class PolyLTests
    {
        [TestMethod]
        public void Multiply()
        {
            var a = new PolyL(3, 1);
            var b = new PolyL(-1, 1);
            var c = new PolyL(4, 1);
            var d = new PolyL(-5, 1);
            var p = a * b * c * d;
            Assert.AreEqual("60 − 37𝑛 − 25𝑛² + 𝑛³ + 𝑛⁴", p.ToString());
        }

        [TestMethod]
        public void Subtract()
        {
            var a = new PolyL(3, 1);
            var b = new PolyL(-1, 1);
            var c = new PolyL(4, 1);
            var d = new PolyL(-5, 1);
            var p = a * b - c * d;
            Assert.AreEqual("17 + 3𝑛", p.ToString());
        }

        [TestMethod]
        public void Evaluate()
        {
            var p = new PolyL(60, -37, -25, 1, 1);
            var x = p.At(-7);
            Assert.AreEqual(60 - 7 * (-37 - 7 * (-25 - 7 * (1 - 7))), x);
        }

        [TestMethod]
        public void Pow()
        {
            var p = new PolyL(-3, 1) ^ 7;
            Assert.AreEqual(new PolyL(-2187, 5103, -5103, 2835, -945, 189, -21, 1), p);
        }

        [TestMethod]
        public void Derivative()
        {
            var p = new PolyL(5, -4, 3, -2, 1);
            var d = p.Derivative();
            Assert.AreEqual(new PolyL(-4, 6, -6, 4), d);
        }

        [TestMethod]
        public void ContentAllPositive()
        {
            var p = new PolyL(3, 9, 6);
            var n = p.Content();
            Assert.AreEqual(3, n);
        }

        [TestMethod]
        public void ContentMixed()
        {
            var p = new PolyL(3, -9, 6);
            var n = p.Content();
            Assert.AreEqual(3, n);
        }

        [TestMethod]
        public void ContentAllNegative()
        {
            var p = new PolyL(-3, -6, -6);
            var n = p.Content();
            Assert.AreEqual(-3, n);
        }

        [TestMethod]
        public void ContentWithZeros()
        {
            var p = new PolyL(2, 0, 4, 0, 2);
            var n = p.Content();
            Assert.AreEqual(2, n);
        }

        [TestMethod]
        public void PrimitivePart()
        {
            var p = new PolyL(3, 0, -6, 6).PrimitivePart();
            Assert.AreEqual(new PolyL(1, 0, -2, 2), p);
        }

        [TestMethod]
        public void FactorsNoContent()
        {
            var p = new PolyL(2, 3).Factors().ToList();
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual(new PolyL(2, 3), p[0]);
        }
        
        [TestMethod]
        public void FactorWithContent()
        {
            var p = new PolyL(2, 4).Factors().ToList();
            Assert.AreEqual(2, p.Count);
            Assert.AreEqual(new PolyL(2), p[0]);
            Assert.AreEqual(new PolyL(1, 2), p[1]);
        }
        
        [TestMethod]
        public void FactorWithMonomial()
        {
            var p = new PolyL(0, 2, 4).Factors().ToList();
            Assert.AreEqual(2, p.Count);
            Assert.AreEqual(new PolyL(0, 2), p[0]);
            Assert.AreEqual(new PolyL(1, 2), p[1]);
        }

        [TestMethod]
        public void DivNoRem()
        {
            var x = new PolyL(2, 1);
            var y = new PolyL(1, -5, 3);
            var p = x * y;  // (2 + n)(1 - 5n + 3n^2) = 2 - 9n + n^2 + 3n^3
            var d = p.DivRem(x, out var r);
            Assert.AreEqual(y, d);
            Assert.AreEqual(PolyL.Zero, r);
            d = p.DivRem(y, out r);
            Assert.AreEqual(x, d);
            Assert.AreEqual(PolyL.Zero, r);
        }

        [TestMethod]
        public void DivRem()
        {
            var x = new PolyL(2, 1);
            var y = new PolyL(1, -5, 3);
            var z = new PolyL(7);
            var p = x * y + z;  // (2 + n)(1 - 5n + 3n^2) + 7 = 9 - 9n + n^2 + 3n^3
            var d = p.DivRem(x, out var r);
            Assert.AreEqual(y, d);
            Assert.AreEqual(z, r);
            d = p.DivRem(y, out r);
            Assert.AreEqual(x, d);
            Assert.AreEqual(z, r);
        }
    }

    [TestClass]
    public class PolyBTests
    {
        [TestMethod]
        public void Multiply()
        {
            var a = new PolyB(3, 1);
            var b = new PolyB(-1, 1);
            var c = new PolyB(4, 1);
            var d = new PolyB(-5, 1);
            var p = a * b * c * d;
            Assert.AreEqual("60 − 37𝑛 − 25𝑛² + 𝑛³ + 𝑛⁴", p.ToString());
        }

        [TestMethod]
        public void Subtract()
        {
            var a = new PolyB(3, 1);
            var b = new PolyB(-1, 1);
            var c = new PolyB(4, 1);
            var d = new PolyB(-5, 1);
            var p = a * b - c * d;
            Assert.AreEqual("17 + 3𝑛", p.ToString());
        }

        [TestMethod]
        public void Evaluate()
        {
            var p = new PolyB(60, -37, -25, 1, 1);
            var x = p.At(-7);
            Assert.AreEqual(60 - 7 * (-37 - 7 * (-25 - 7 * (1 - 7))), x);
        }

        [TestMethod]
        public void Pow()
        {
            var p = new PolyB(-3, 1) ^ 7;
            Assert.AreEqual(new PolyB(-2187, 5103, -5103, 2835, -945, 189, -21, 1), p);
        }

        [TestMethod]
        public void Derivative()
        {
            var p = new PolyB(5, -4, 3, -2, 1);
            var d = p.Derivative();
            Assert.AreEqual(new PolyB(-4, 6, -6, 4), d);
        }

        [TestMethod]
        public void ContentAllPositive()
        {
            var p = new PolyB(3, 9, 6);
            var n = p.Content();
            Assert.AreEqual(3, n);
        }

        [TestMethod]
        public void ContentMixed()
        {
            var p = new PolyB(3, -9, 6);
            var n = p.Content();
            Assert.AreEqual(3, n);
        }

        [TestMethod]
        public void ContentAllNegative()
        {
            var p = new PolyB(-3, -6, -6);
            var n = p.Content();
            Assert.AreEqual(-3, n);
        }

        [TestMethod]
        public void ContentWithZeros()
        {
            var p = new PolyB(2, 0, 4, 0, 2);
            var n = p.Content();
            Assert.AreEqual(2, n);
        }

        [TestMethod]
        public void PrimitivePart()
        {
            var p = new PolyB(3, 0, -6, 6).PrimitivePart();
            Assert.AreEqual(new PolyB(1, 0, -2, 2), p);
        }

        [TestMethod]
        public void FactorsNoContent()
        {
            var p = new PolyB(2, 3).Factors().ToList();
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual(new PolyB(2, 3), p[0]);
        }
        
        [TestMethod]
        public void FactorWithContent()
        {
            var p = new PolyB(2, 4).Factors().ToList();
            Assert.AreEqual(2, p.Count);
            Assert.AreEqual(new PolyB(2), p[0]);
            Assert.AreEqual(new PolyB(1, 2), p[1]);
        }
        
        [TestMethod]
        public void FactorWithMonomial()
        {
            var p = new PolyB(0, 2, 4).Factors().ToList();
            Assert.AreEqual(2, p.Count);
            Assert.AreEqual(new PolyB(0, 2), p[0]);
            Assert.AreEqual(new PolyB(1, 2), p[1]);
        }

        [TestMethod]
        public void DivNoRem()
        {
            var x = new PolyB(2, 1);
            var y = new PolyB(1, -5, 3);
            var p = x * y;  // (2 + n)(1 - 5n + 3n^2) = 2 - 9n + n^2 + 3n^3
            var d = p.DivRem(x, out var r);
            Assert.AreEqual(y, d);
            Assert.AreEqual(PolyB.Zero, r);
            d = p.DivRem(y, out r);
            Assert.AreEqual(x, d);
            Assert.AreEqual(PolyB.Zero, r);
        }

        [TestMethod]
        public void DivRem()
        {
            var x = new PolyB(2, 1);
            var y = new PolyB(1, -5, 3);
            var z = new PolyB(7);
            var p = x * y + z;  // (2 + n)(1 - 5n + 3n^2) + 7 = 9 - 9n + n^2 + 3n^3
            var d = p.DivRem(x, out var r);
            Assert.AreEqual(y, d);
            Assert.AreEqual(z, r);
            d = p.DivRem(y, out r);
            Assert.AreEqual(x, d);
            Assert.AreEqual(z, r);
        }
    }

}
