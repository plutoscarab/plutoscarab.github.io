using System;
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

        public static string Digits(IEnumerable<BigInteger> terms, int places)
        {
            using (var writer = new StringWriter())
            {
                Write(terms, places, writer);
                return writer.ToString();
            }
        }

        public static void Write(IEnumerable<int> terms, int places, TextWriter writer)
        {
            Write(terms.Select(n => (BigInteger)n), places, writer);
        }

        public enum NegativeTermBehavior
        {
            Throw,
            InvalidDigit,
        }

        public const char InvalidDigit = '!';

        public static void Write(IEnumerable<BigInteger> terms, int places,
            TextWriter writer) => Write(terms, places, writer, NegativeTermBehavior.InvalidDigit);

        public static void Write(IEnumerable<BigInteger> terms, int places,
            TextWriter writer, NegativeTermBehavior negativeTermBehavior)
        {
            using (var te = terms.GetEnumerator())
            {
                if (!te.MoveNext())
                {
                    writer.Write('∞');
                    return;
                }

                var first = te.Current;

                if (first == Signal)
                {
                    if (negativeTermBehavior == NegativeTermBehavior.Throw)
                        throw new InvalidOperationException();

                    writer.Write(InvalidDigit);
                    return;
                }

                if (first.Sign < 0)
                {
                    writer.Write('-');
                    Write(Negate(terms), places, writer);
                    return;
                }

                // Write the integer portion.
                writer.Write(first);
                var nonZeroDigit = false;

                if (first != 0)
                {
                    places -= first.ToString().Length;
                    nonZeroDigit = true;
                }

                // We're done if there is no fractional part.
                if (!te.MoveNext())
                {
                    return;
                }

                // Now we're ready to do the decimal fraction part.
                writer.Write('.');
                BigInteger a = 10, b = 0, c = 0, d = 1;
                var loops = 0;

                while (places > 0)
                {
                    var term = te.Current;

                    if (term == Signal || ++loops >= 100)
                    {
                        if (negativeTermBehavior == NegativeTermBehavior.Throw)
                            throw new InvalidOperationException();

                        writer.Write(InvalidDigit);
                        return;
                    }

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

                        if (n.Sign < 0 || n > 9)
                        {
                            if (negativeTermBehavior == NegativeTermBehavior.Throw)
                                throw new InvalidOperationException();

                            writer.Write(InvalidDigit);
                            return;
                        }

                        // Write the digit. 
                        writer.Write((int)n);
                        nonZeroDigit |= n != 0;
                        
                        if (nonZeroDigit)
                        {
                            places--;
                        }
                        
                        loops = 0;

                        // Take the remainder and multiply by 10 again to set up for next digit.
                        a = r * 10;
                        b = s * 10;
                    }

                    if (!te.MoveNext())
                    {
                        break;
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

        public enum TimeoutBehavior
        {
            Throw,
            Signal,
        }

        public static readonly BigInteger Signal = BigInteger.Parse("-9999999999");

        public static IEnumerable<BigInteger> Simplify(
            IEnumerable<BigInteger> ts,
            IEnumerable<BigInteger> us) => Simplify(ts, us, 400, TimeoutBehavior.Signal);

        public static IEnumerable<BigInteger> Simplify(
            IEnumerable<BigInteger> ts,
            IEnumerable<BigInteger> us,
            int timeoutLoops,
            TimeoutBehavior timeoutBehavior)
        {
            BigInteger a = 0, b = 1, c = 1, d = 0;
            var loops = 0;

            using (var e = us.GetEnumerator())
            {
                foreach (var t in ts)
                {
                    if (++loops > timeoutLoops)
                    {
                        if (timeoutBehavior == TimeoutBehavior.Throw)
                            throw new InvalidOperationException();

                        yield return Signal;
                        yield break;
                    }

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
                        loops = 0;
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
            var loops = 0;

            foreach (var term in terms)
            {
                if (++loops >= 100)
                {
                    yield return Signal;
                    yield break;
                }

                (a, b) = (b, a + term * b);
                (c, d) = (d, c + term * d);

                while (!c.IsZero && !d.IsZero)
                {
                    var m = BigInteger.DivRem(a, c, out var r);
                    var n = BigInteger.DivRem(b, d, out var s);

                    if (m != n)
                        break;

                    yield return n;
                    loops = 0;
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

        public static IEnumerable<BigInteger> FromRatio(BigInteger p, BigInteger q)
        {
            if (p.Sign * q.Sign == -1)
            {
                foreach (var d in Negate(FromRatio(BigInteger.Abs(p), BigInteger.Abs(q))))
                {
                    yield return d;
                }

                yield break;
            }

            while (true)
            {
                var d = BigInteger.DivRem(p, q, out var r);
                yield return d;

                if (r.IsZero)
                {
                    yield break;
                }

                (p, q) = (q, r);
            }
        }

        public static (BigInteger, BigInteger) ToRatio(IEnumerable<BigInteger> terms)
        {
            BigInteger a = 0, b = 1, c = 1, d = 0;

            foreach (var term in terms)
            {
                (a, b) = (b, a + b * term);
                (c, d) = (d, c + d * term);
            }

            var g = BigInteger.GreatestCommonDivisor(b, d);
            return (b / g, d / g);
        }

        public static IList<BigInteger> Normalize(IList<BigInteger> terms)
        {
            var result = new List<BigInteger>();
            result.AddRange(terms);
            var i = 1;

            while (i < result.Count)
            {
                switch (result[i].Sign)
                {
                    case +1:
                        i++;
                        break;

                    case 0:
                        if (i == result.Count - 1)
                        {
                            result.RemoveAt(i);
                            result.RemoveAt(i - 1);
                            i = result.Count;
                            break;
                        }

                        result[i - 1] += result[i + 1];
                        result.RemoveAt(i);
                        result.RemoveAt(i);
                        if (i > 1) i--;
                        break;

                    case -1:
                        result[i - 1]--;
                        result[i] = -result[i] - 1;
                        result.Insert(i, BigInteger.One);

                        for (var j = i + 2; j < result.Count; j++)
                        {
                            result[j] = -result[j];
                        }

                        if (i > 1) i--;
                        break;

                    default:
                        throw new InvalidOperationException($"{nameof(BigInteger)}.Sign returned something other that -1, 0, or 1.");
                }
            }

            if (result.Count > 1 && result[^1] == BigInteger.One)
            {
                result.RemoveAt(result.Count - 1);
                result[^1]++;
            }

            return result;
        }
    }
}