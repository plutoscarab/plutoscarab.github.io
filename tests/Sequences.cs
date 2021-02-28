using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlutoScarab;

namespace tests
{
    [TestClass]
    public class Sequences
    {
        [TestMethod]
        public void Ruler()
        {
            var x = Seq.Ruler().Take(10);
            Assert.IsTrue(Enumerable.SequenceEqual(new[] { 0, 1, 0, 2, 0, 1, 0, 3, 0, 1 }, x));
        }

        [TestMethod]
        public void Fusc()
        {
            var x = Seq.Fusc().Take(10);
            Assert.IsTrue(Enumerable.SequenceEqual(new[] { 0, 1, 1, 2, 1, 3, 2, 3, 1, 4 }, x));
        }

        [TestMethod]
        public void Rationals()
        {
            var x = Seq.Rationals().Take(5);
            System.Diagnostics.Trace.WriteLine(x.First().Item2);
            Assert.IsTrue(Enumerable.SequenceEqual(new[] { (1, 1), (1, 2), (2, 1), (1, 3), (3, 2) }, x));
        }

        [TestMethod]
        public void Sobol()
        {
            var x = Seq.Sobol().Take(10);
            Assert.IsTrue(Enumerable.SequenceEqual(new[] { 0.5, 0.75, 0.25, 0.375, 0.875, 0.625, 0.125, 0.1875, 0.6875, 0.9375 }, x));
        }
    }
}