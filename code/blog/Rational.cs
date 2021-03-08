using System;
using System.Linq;
using System.Numerics;

namespace PlutoScarab
{
    public record Rational(BigInteger p, BigInteger q)
    {
        public static explicit operator Rational(double d)
        {
            const int exponentBits = 11;
            const int mantissaBits = 63 - exponentBits;
            const int exponentMask = (1 << exponentBits) - 1;
            const int exponentBias = exponentMask / 2 + mantissaBits;
            const long mantissaMsb = 1L << mantissaBits;
            const long mantissaMask = mantissaMsb - 1;

            var bits = BitConverter.DoubleToInt64Bits(d);
            var sign = bits < 0 ? -1 : +1;
            var exponent = (int)(bits >> mantissaBits) & exponentMask;
            var mantissa = bits & mantissaMask;

            if (exponent == 0)
            {
                exponent = 1;

                if (mantissa == 0)
                {
                    return new Rational(BigInteger.Zero, BigInteger.One);
                }
            }
            else
            {
                mantissa |= mantissaMsb;
            }

            exponent -= exponentBias;

            if (exponent >= 0)
            {
                return new Rational(BigInteger.Pow(2, exponent) * mantissa * sign, BigInteger.One);
            }

            var p = (BigInteger)mantissa;
            var q = BigInteger.Pow(2, -exponent);
            return new Rational(p * sign, q);
        }

        public static explicit operator Rational(float f)
        {
            const int exponentBits = 8;
            const int mantissaBits = 31 - exponentBits;
            const int exponentMask = (1 << exponentBits) - 1;
            const int exponentBias = exponentMask / 2 + mantissaBits;
            const int mantissaMsb = 1 << mantissaBits;
            const int mantissaMask = mantissaMsb - 1;

            var bits = BitConverter.SingleToInt32Bits(f);
            var sign = bits < 0 ? -1 : +1;
            var exponent = (int)(bits >> mantissaBits) & exponentMask;
            var mantissa = bits & mantissaMask;

            if (exponent == 0)
            {
                exponent = 1;

                if (mantissa == 0)
                {
                    return new Rational(BigInteger.Zero, BigInteger.One);
                }
            }
            else
            {
                mantissa |= mantissaMsb;
            }

            exponent -= exponentBias;

            if (exponent >= 0)
            {
                return new Rational(BigInteger.Pow(2, exponent) * mantissa * sign, BigInteger.One);
            }

            var p = (BigInteger)mantissa;
            var q = BigInteger.Pow(2, -exponent);
            return new Rational(p * sign, q);
        }

        public static Rational Best(double d) => Best((Rational)d);

        public static Rational Best(float f) => Best((Rational)f);

        private static Rational Best(Rational r) => Best(new Rational(2 * r.p - 1, 2 * r.q), new Rational(2 * r.p + 1, 2 * r.q));

        public static Rational Best(Rational lo, Rational hi)
        {
            var clo = CF.FromRatio(lo.p, lo.q).ToList();
            var chi = CF.FromRatio(hi.p, hi.q).ToList();
            var matching = clo.Zip(chi).TakeWhile(_ => _.First == _.Second).Count();
            var even = (matching & 1) == 0;
            var tlo = clo[matching];
            var thi = chi[matching];
            var min = BigInteger.Min(tlo, thi);
            var cf = (tlo == min ? clo : chi).Take(matching + 1).ToList();

            if ((even && tlo == min) || (!even && thi == min))
            {
                cf[^1]++;
            }

            var (p, q) = CF.ToRatio(cf);
            return new Rational(p, q);
        }
    }
}