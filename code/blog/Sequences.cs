using System;
using System.Collections.Generic;
using System.Linq;

namespace PlutoScarab
{
    public static class Seq
    {
        public static IEnumerable<int> Ruler()
        {
            yield return 0;

            foreach (var r in Ruler())
            {
                yield return r + 1;
                yield return 0;
            }
        }

        public static IEnumerable<int> Fusc()
        {
            yield return 0;
            yield return 1;
            yield return 1;
            var n = 1;

            foreach (var f in Fusc().Skip(2))
            {
                yield return n + f;
                yield return f;
                n = f;
            }
        }

        public static IEnumerable<(int, int)> Rationals()
        {
            var n = 1;

            foreach (var f in Fusc().Skip(2))
            {
                yield return (n, f);
                n = f;
            }
        }
        
        public static IEnumerable<double> Sobol()
        {
            var bits = 0ul;
            var scale = Math.Pow(2.0, -64);

            foreach (var r in Ruler())
            {
                bits ^= 1ul << (63 - r);
                yield return bits * scale;
            }
        }
    }
}