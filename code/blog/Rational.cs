using System;
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
            var g = BigInteger.GreatestCommonDivisor(p, q);
            return new Rational(p * sign / g, q / g);
        }
    }
}