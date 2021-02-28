using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace PlutoScarab
{
    public static class CF
    {
        public static IEnumerable<BigInteger> Negate(IEnumerable<int> terms) =>
            Negate(terms.Select(n => (BigInteger)n));

        public static IEnumerable<BigInteger> Negate(IEnumerable<BigInteger> terms)
        {
            var arr = terms.Take(3).ToArray();

            if (arr.Length == 0)
            {
                return Enumerable.Empty<BigInteger>();
            }

            if (arr.Length == 1)
            {
                return new[] { -arr[0] };
            }

            if (arr[1] == BigInteger.One)
            {
                return new[] { -arr[0] - BigInteger.One, arr[2] + BigInteger.One }
                    .Concat(terms.Skip(3));
            }

            return new[] { -arr[0] - BigInteger.One, BigInteger.One,
            arr[1] - BigInteger.One }.Concat(terms.Skip(2));
        }

        public static void Write(IEnumerable<int> terms, int places, TextWriter writer)
        {
            Write(terms.Select(n => (BigInteger)n), places, writer);
        }

        public static void Write(IEnumerable<BigInteger> terms, int places,
            TextWriter writer)
        {
            if (!terms.Any())
            {
                writer.Write('∞');
                return;
            }

            if (terms.First().Sign < 0)
            {
                writer.Write('-');
                Write(Negate(terms), places, writer);
                return;
            }

            // Write the integer portion.
            writer.Write(terms.First());

            // We're done if there is no fractional part.
            if (terms.Take(2).Count() == 1)
            {
                return;
            }

            // Now we're ready to do the decimal fraction part.
            writer.Write('.');
            BigInteger a = 10, b = 0, c = 0, d = 1;

            foreach (var term in terms.Skip(1).TakeWhile(_ => places > 0))
            {
                (a, b) = (b, a + b * term);
                (c, d) = (d, c + d * term);

                while (!c.IsZero && !d.IsZero && places > 0)
                {
                    // Compute the two quotients and remainders.
                    var m = BigInteger.DivRem(a, c, out var r);
                    var n = BigInteger.DivRem(b, d, out var s);

                    // If the quotients aren't the same we don't know the digit yet.
                    if (m != n)
                        break;

                    // Write the digit. 
                    writer.Write((int)n);
                    places--;

                    // Take the remainder and multiply by 10 again to set up for next digit.
                    a = r * 10;
                    b = s * 10;
                }
            }

            while (!b.IsZero && !d.IsZero && places > 0)
            {
                var n = BigInteger.DivRem(b, d, out var s);
                writer.Write((int)n);
                places--;
                b = s * 10;
            }
        }

        public static string ToString(IEnumerable<int> terms, int places) =>
            ToString(terms.Select(n => (BigInteger)n), places);

        public static string ToString(IEnumerable<BigInteger> terms, int places)
        {
            using (var writer = new StringWriter())
            {
                Write(terms, places, writer);
                return writer.ToString();
            }
        }
        public static IEnumerable<BigInteger> Simplify(
            IEnumerable<BigInteger> ts,
            IEnumerable<BigInteger> us)
        {
            BigInteger a = 0, b = 1, c = 1, d = 0;

            using (var e = us.GetEnumerator())
            {
                foreach (var t in ts)
                {
                    var u = e.MoveNext() ? e.Current : BigInteger.One;
                    (a, b) = (u * b, a + t * b);
                    (c, d) = (u * d, c + t * d);

                    while (!c.IsZero && !d.IsZero)
                    {
                        var m = BigInteger.DivRem(a, c, out var r);
                        var n = BigInteger.DivRem(b, d, out var s);

                        if (m != n)
                            break;

                        yield return n;
                        (a, c) = (c, r);
                        (b, d) = (d, s);
                    }
                }

                while (!b.IsZero && !d.IsZero)
                {
                    var n = BigInteger.DivRem(b, d, out var s);
                    yield return n;
                    (b, d) = (d, s);
                }
            }
        }

        public static IEnumerable<BigInteger> Nats() => Nats(0);

        public static IEnumerable<BigInteger> Nats(int from) =>
            Enumerable.Range(from, int.MaxValue).Select(n => (BigInteger)n);

        public static IEnumerable<BigInteger> Transform(
            IEnumerable<BigInteger> terms,
            BigInteger a, BigInteger b,
            BigInteger c, BigInteger d)
        {
            foreach (var term in terms)
            {
                (a, b) = (b, a + term * b);
                (c, d) = (d, c + term * d);

                while (!c.IsZero && !d.IsZero)
                {
                    var m = BigInteger.DivRem(a, c, out var r);
                    var n = BigInteger.DivRem(b, d, out var s);

                    if (m != n)
                        break;

                    yield return n;
                    (a, c) = (c, r);
                    (b, d) = (d, s);
                }
            }

            while (!b.IsZero && !d.IsZero)
            {
                var n = BigInteger.DivRem(b, d, out var s);
                yield return n;
                (b, d) = (d, s);
            }
        }

        private static readonly double Scale = System.Math.Pow(2, -53);

        public static IEnumerable<BigInteger> Random(int seed)
        {
            var rand = new System.Random(seed);
            var x = rand.NextDouble();
            yield return BigInteger.Zero;

            while (true)
            {
                x = 1 / x;
                var n = (int)x;
                yield return n;
                x -= n;
                x += rand.NextDouble() * Scale;
            }
        }
    }
}