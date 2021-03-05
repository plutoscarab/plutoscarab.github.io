using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlutoScarab;

namespace tests
{
    [TestClass]
    public class Polynomials
    {
        [TestMethod]
        public void All()
        {
            var x = Poly.All().Skip(1000).First();
            var s = Poly.ToString(x);
            Assert.AreEqual("−1 + 2𝑛 + 𝑛⁴", s);
        }
    }
}