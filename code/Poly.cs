using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PlutoScarab
{

    public struct PolyI : IEquatable<PolyI>
    {
        public static readonly PolyI Zero = new PolyI(new int[0], true);

        private readonly int[] coeffs;

        public PolyI(params int[] coeffs)
        : this((int[])coeffs.Clone(), true)
        {
        }

        private PolyI(int[] coeffs, bool noCopy)
        {
            var i = coeffs.Length - 1;

            while (i >= 0 && coeffs[i] == 0)
            {
                i--;
            }

            if (i < coeffs.Length - 1)
            {
                Array.Resize(ref coeffs, i + 1);
            }

            this.coeffs = coeffs;
        }

        private int Length => coeffs?.Length ?? 0;
        
        private int this[int index]
        {
            get { return coeffs[index]; }
            set { coeffs[index] = value; }
        }

        private int[] Copy() => coeffs == null ? new int[0] : (int[])coeffs.Clone();

        public static PolyI operator +(PolyI p) => p;

        public static PolyI operator -(PolyI p)
        {
            var q = p.Copy();
            for (var i = 0; i < q.Length; i++) q[i] = -q[i];
            return new PolyI(q, true);
        }

        public static PolyI operator +(PolyI p, PolyI q)
        {
            int[] r;

            if (p.Length >= q.Length)
            {
                r = p.Copy();
                for (var i = 0; i < q.Length; i++) r[i] += q[i];
            }
            else
            {
                r = q.Copy();
                for (var i = 0; i < p.Length; i++) r[i] += p[i];
            }

            return new PolyI(r, true);
        }

        public static PolyI operator -(PolyI p, PolyI q)
        {
            var r = p.Copy();
            if (p.Length < q.Length) Array.Resize(ref r, q.Length);
            for (var i = 0; i < q.Length; i++) r[i] -= q[i];
            return new PolyI(r, true);
        }

        public static PolyI operator *(PolyI p, PolyI q)
        {
            var r = new int[p.Length + q.Length - 1 ];

            for (var i = 0; i < p.Length; i++)
            {
                if (p[i] == 0)
                    continue;

                for (var j = 0; j < q.Length; j++)
                {
                    r[i + j] += p[i] * q[j];
                }
            }

            return new PolyI(r, true);
        }

        public static PolyI operator +(PolyI p, int q)
        {
            var r = p.Copy();
            r[0] += q;
            return new PolyI(r, true);
        }

        public static PolyI operator +(int p,  PolyI q) =>
            q + p;

        public static PolyI operator -(PolyI p, int q)
        {
            var r = p.Copy();
            r[0] -= q;
            return new PolyI(r, true);
        }

        public static PolyI operator -(int p, PolyI q) =>
            (-q) + p;

        public static PolyI operator *(int p, PolyI q)
        {
            var r = q.Copy();
            for (var i = 0; i < r.Length; i++) r[i] *= p;
            return new PolyI(r, true);
        }

        public static PolyI operator /(PolyI p, int q)
        {
            var r = p.Copy();
            for (var i = 0; i < r.Length; i++) r[i] /= q;
            return new PolyI(r, true);
        }

        public static PolyI operator *(PolyI p, int q) =>
            q * p;

        public override string ToString()
        {
            if (coeffs is null)
            {
                return "0";
            }

            var s = new StringBuilder();

            for (var power = 0; power < coeffs.Length; power++)
            {
                var coeff = coeffs[power];

                if (coeff != 0)
                {
                    if (s.Length > 0)
                    {
                        s.Append(" ");
                    }

                    if (power == 0)
                    {
                        s.Append(coeff.ToString().Replace("-", "−"));
                    }
                    else
                    {
                        if (coeff < 0)
                        {
                            s.Append("−");

                            if (s.Length > 1)
                            {
                                s.Append(" ");
                            }
                        }
                        else if (s.Length > 0)
                        {
                            s.Append("+ ");
                        }

                        var abs = Math.Abs(coeff);

                        if (abs != 1)
                        {
                            s.Append(abs);
                        }

                        s.Append("𝑛");

                        if (power > 1)
                        {
                            s.Append(new string(power.ToString().Select(c => "⁰¹²³⁴⁵⁶⁷⁸⁹"[c - '0']).ToArray()));
                        }
                    }
                }
            }

            return s.ToString();
        }

        public override bool Equals(object other) => other is PolyI val && Equals(val);

        public bool Equals(PolyI other) => 
            Length == 0 ? other.Length == 0 : 
            other.Length == 0 ? Length == 0 :
            Enumerable.SequenceEqual(coeffs, other.coeffs);

        public override int GetHashCode() => coeffs?.GetHashCode() ?? 0;

        public static bool operator ==(PolyI p, PolyI q) => p.Equals(q);

        public static bool operator !=(PolyI p, PolyI q) => !p.Equals(q);

        public int At(int x)
        {
            int sum = 0;

            if (coeffs is null)
            {
                return 0;
            }

            for (var i = coeffs.Length - 1; i >= 0; i--)
            {
                sum = sum * x + coeffs[i];
            }

            return sum;
        }

        public PolyI Squared()
        {
            if (Length == 0) return Zero;
            var r = new int[2 * Length - 1];
            
            for (var i = 0; i < Length; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    r[i + j] += 2 * coeffs[i] * coeffs[j];
                }

                r[i + i] = coeffs[i] * coeffs[i];
            }

            return new PolyI(r, true);
        }

        private static int Pow(int k, int n)
        {
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n));

            if (n == 0)
                return 1;

            if (k == 0)
                return 0;

            if (n == 1)
                return k;

            if ((n % 2) == 0)
            {
                var s = Pow(k, n / 2);
                return s * s;
            }

            return k * Pow(k, n / 2);
        }

        public PolyI Pow(int n)
        {
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n));

            if (n == 0)
                return new PolyI(1);

            if (Length == 0)
                return Zero;

            if (n == 1)
                return this;

            if ((n % 2) == 0)
                return Pow(n / 2).Squared();

            return this * Pow(n - 1);
        }

        public static PolyI operator ^(PolyI p, int n) =>
            p.Pow(n);

        public PolyI Derivative()
        {
            if (Length == 0) return Zero;
            var r = new int[Length - 1];
            for (var i = 0; i < r.Length; i++) r[i] = coeffs[i + 1] * (i + 1);
            return new PolyI(r, true);
        }

        private static int GCD(int p, int q)
        {
            while (q != 0)
            {
                (p, q) = (q, p % q);
            }

            return p;
        }

        public int Content()
        {
            int gcd = 0;

            for (var i = 0; i < Length; i++)
            {
                if (coeffs[i] != 0) gcd = GCD(coeffs[i], gcd);
            }

            return gcd == 0 ? 1 : gcd;
        }

        public PolyI PrimitivePart() => PrimitivePart(out _);

        public PolyI PrimitivePart(out int content)
        {
            content = Content();
            if (content == 0) return this;
            var r = Copy();
            for (var i = 0; i < r.Length; i++) r[i] /= content;
            return new PolyI(r, true);
        }

        public IEnumerable<PolyI> Factors()
        {
            var p = PrimitivePart(out var c);
            var n = p.coeffs.TakeWhile(_ => _ == 0).Count();

            if (c != 1 || n != 0)
            {
                var lead = new int[n + 1];
                lead[n] = c;
                yield return new PolyI(lead, true);
            }

            if (n > 0)
            {
                var arr = new int[Length - n];
                Array.Copy(p.coeffs, n, arr, 0, arr.Length);
                p = new PolyI(arr, true);
            }

            yield return p;
        }

        public PolyI DivRem(PolyI divisor, out PolyI remainder)
        {
            var r = Copy();
            var offset = Length - divisor.Length;

            if (offset < 0) 
            {
                remainder = this;
                return PolyI.Zero;
            }

            var result = new int[offset + 1];

            while (offset >= 0)
            {
                var k = r[divisor.Length - 1 + offset] / divisor.coeffs[divisor.Length - 1];

                if (k != 0)
                {
                    result[offset] = k;

                    for (var i = 0; i < divisor.Length; i++)
                    {
                        r[i + offset] -= divisor[i] * k;
                    }
                }

                offset--;
            }

            remainder = new PolyI(r, true);
            return new PolyI(result, true);
        }

        public static PolyI operator /(PolyI p, PolyI q) =>
            p.DivRem(q, out _);

        public static PolyI operator %(PolyI p, PolyI q)
        {
            _ = p.DivRem(q, out var result);
            return result;
        }
    }

    public struct PolyL : IEquatable<PolyL>
    {
        public static readonly PolyL Zero = new PolyL(new long[0], true);

        private readonly long[] coeffs;

        public PolyL(params long[] coeffs)
        : this((long[])coeffs.Clone(), true)
        {
        }

        private PolyL(long[] coeffs, bool noCopy)
        {
            var i = coeffs.Length - 1;

            while (i >= 0 && coeffs[i] == 0)
            {
                i--;
            }

            if (i < coeffs.Length - 1)
            {
                Array.Resize(ref coeffs, i + 1);
            }

            this.coeffs = coeffs;
        }

        private int Length => coeffs?.Length ?? 0;
        
        private long this[int index]
        {
            get { return coeffs[index]; }
            set { coeffs[index] = value; }
        }

        private long[] Copy() => coeffs == null ? new long[0] : (long[])coeffs.Clone();

        public static PolyL operator +(PolyL p) => p;

        public static PolyL operator -(PolyL p)
        {
            var q = p.Copy();
            for (var i = 0; i < q.Length; i++) q[i] = -q[i];
            return new PolyL(q, true);
        }

        public static PolyL operator +(PolyL p, PolyL q)
        {
            long[] r;

            if (p.Length >= q.Length)
            {
                r = p.Copy();
                for (var i = 0; i < q.Length; i++) r[i] += q[i];
            }
            else
            {
                r = q.Copy();
                for (var i = 0; i < p.Length; i++) r[i] += p[i];
            }

            return new PolyL(r, true);
        }

        public static PolyL operator -(PolyL p, PolyL q)
        {
            var r = p.Copy();
            if (p.Length < q.Length) Array.Resize(ref r, q.Length);
            for (var i = 0; i < q.Length; i++) r[i] -= q[i];
            return new PolyL(r, true);
        }

        public static PolyL operator *(PolyL p, PolyL q)
        {
            var r = new long[p.Length + q.Length - 1 ];

            for (var i = 0; i < p.Length; i++)
            {
                if (p[i] == 0)
                    continue;

                for (var j = 0; j < q.Length; j++)
                {
                    r[i + j] += p[i] * q[j];
                }
            }

            return new PolyL(r, true);
        }

        public static PolyL operator +(PolyL p, long q)
        {
            var r = p.Copy();
            r[0] += q;
            return new PolyL(r, true);
        }

        public static PolyL operator +(long p,  PolyL q) =>
            q + p;

        public static PolyL operator -(PolyL p, long q)
        {
            var r = p.Copy();
            r[0] -= q;
            return new PolyL(r, true);
        }

        public static PolyL operator -(long p, PolyL q) =>
            (-q) + p;

        public static PolyL operator *(long p, PolyL q)
        {
            var r = q.Copy();
            for (var i = 0; i < r.Length; i++) r[i] *= p;
            return new PolyL(r, true);
        }

        public static PolyL operator /(PolyL p, long q)
        {
            var r = p.Copy();
            for (var i = 0; i < r.Length; i++) r[i] /= q;
            return new PolyL(r, true);
        }

        public static PolyL operator *(PolyL p, long q) =>
            q * p;

        public override string ToString()
        {
            if (coeffs is null)
            {
                return "0";
            }

            var s = new StringBuilder();

            for (var power = 0; power < coeffs.Length; power++)
            {
                var coeff = coeffs[power];

                if (coeff != 0)
                {
                    if (s.Length > 0)
                    {
                        s.Append(" ");
                    }

                    if (power == 0)
                    {
                        s.Append(coeff.ToString().Replace("-", "−"));
                    }
                    else
                    {
                        if (coeff < 0)
                        {
                            s.Append("−");

                            if (s.Length > 1)
                            {
                                s.Append(" ");
                            }
                        }
                        else if (s.Length > 0)
                        {
                            s.Append("+ ");
                        }

                        var abs = Math.Abs(coeff);

                        if (abs != 1)
                        {
                            s.Append(abs);
                        }

                        s.Append("𝑛");

                        if (power > 1)
                        {
                            s.Append(new string(power.ToString().Select(c => "⁰¹²³⁴⁵⁶⁷⁸⁹"[c - '0']).ToArray()));
                        }
                    }
                }
            }

            return s.ToString();
        }

        public override bool Equals(object other) => other is PolyL val && Equals(val);

        public bool Equals(PolyL other) => 
            Length == 0 ? other.Length == 0 : 
            other.Length == 0 ? Length == 0 :
            Enumerable.SequenceEqual(coeffs, other.coeffs);

        public override int GetHashCode() => coeffs?.GetHashCode() ?? 0;

        public static bool operator ==(PolyL p, PolyL q) => p.Equals(q);

        public static bool operator !=(PolyL p, PolyL q) => !p.Equals(q);

        public long At(long x)
        {
            long sum = 0;

            if (coeffs is null)
            {
                return 0;
            }

            for (var i = coeffs.Length - 1; i >= 0; i--)
            {
                sum = sum * x + coeffs[i];
            }

            return sum;
        }

        public PolyL Squared()
        {
            if (Length == 0) return Zero;
            var r = new long[2 * Length - 1];
            
            for (var i = 0; i < Length; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    r[i + j] += 2 * coeffs[i] * coeffs[j];
                }

                r[i + i] = coeffs[i] * coeffs[i];
            }

            return new PolyL(r, true);
        }

        private static long Pow(long k, int n)
        {
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n));

            if (n == 0)
                return 1;

            if (k == 0)
                return 0;

            if (n == 1)
                return k;

            if ((n % 2) == 0)
            {
                var s = Pow(k, n / 2);
                return s * s;
            }

            return k * Pow(k, n / 2);
        }

        public PolyL Pow(int n)
        {
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n));

            if (n == 0)
                return new PolyL(1);

            if (Length == 0)
                return Zero;

            if (n == 1)
                return this;

            if ((n % 2) == 0)
                return Pow(n / 2).Squared();

            return this * Pow(n - 1);
        }

        public static PolyL operator ^(PolyL p, int n) =>
            p.Pow(n);

        public PolyL Derivative()
        {
            if (Length == 0) return Zero;
            var r = new long[Length - 1];
            for (var i = 0; i < r.Length; i++) r[i] = coeffs[i + 1] * (i + 1);
            return new PolyL(r, true);
        }

        private static long GCD(long p, long q)
        {
            while (q != 0)
            {
                (p, q) = (q, p % q);
            }

            return p;
        }

        public long Content()
        {
            long gcd = 0;

            for (var i = 0; i < Length; i++)
            {
                if (coeffs[i] != 0) gcd = GCD(coeffs[i], gcd);
            }

            return gcd == 0 ? 1 : gcd;
        }

        public PolyL PrimitivePart() => PrimitivePart(out _);

        public PolyL PrimitivePart(out long content)
        {
            content = Content();
            if (content == 0) return this;
            var r = Copy();
            for (var i = 0; i < r.Length; i++) r[i] /= content;
            return new PolyL(r, true);
        }

        public IEnumerable<PolyL> Factors()
        {
            var p = PrimitivePart(out var c);
            var n = p.coeffs.TakeWhile(_ => _ == 0).Count();

            if (c != 1 || n != 0)
            {
                var lead = new long[n + 1];
                lead[n] = c;
                yield return new PolyL(lead, true);
            }

            if (n > 0)
            {
                var arr = new long[Length - n];
                Array.Copy(p.coeffs, n, arr, 0, arr.Length);
                p = new PolyL(arr, true);
            }

            yield return p;
        }

        public PolyL DivRem(PolyL divisor, out PolyL remainder)
        {
            var r = Copy();
            var offset = Length - divisor.Length;

            if (offset < 0) 
            {
                remainder = this;
                return PolyL.Zero;
            }

            var result = new long[offset + 1];

            while (offset >= 0)
            {
                var k = r[divisor.Length - 1 + offset] / divisor.coeffs[divisor.Length - 1];

                if (k != 0)
                {
                    result[offset] = k;

                    for (var i = 0; i < divisor.Length; i++)
                    {
                        r[i + offset] -= divisor[i] * k;
                    }
                }

                offset--;
            }

            remainder = new PolyL(r, true);
            return new PolyL(result, true);
        }

        public static PolyL operator /(PolyL p, PolyL q) =>
            p.DivRem(q, out _);

        public static PolyL operator %(PolyL p, PolyL q)
        {
            _ = p.DivRem(q, out var result);
            return result;
        }
    }

    public struct PolyB : IEquatable<PolyB>
    {
        public static readonly PolyB Zero = new PolyB(new BigInteger[0], true);

        private readonly BigInteger[] coeffs;

        public PolyB(params BigInteger[] coeffs)
        : this((BigInteger[])coeffs.Clone(), true)
        {
        }

        private PolyB(BigInteger[] coeffs, bool noCopy)
        {
            var i = coeffs.Length - 1;

            while (i >= 0 && coeffs[i] == 0)
            {
                i--;
            }

            if (i < coeffs.Length - 1)
            {
                Array.Resize(ref coeffs, i + 1);
            }

            this.coeffs = coeffs;
        }

        private int Length => coeffs?.Length ?? 0;
        
        private BigInteger this[int index]
        {
            get { return coeffs[index]; }
            set { coeffs[index] = value; }
        }

        private BigInteger[] Copy() => coeffs == null ? new BigInteger[0] : (BigInteger[])coeffs.Clone();

        public static PolyB operator +(PolyB p) => p;

        public static PolyB operator -(PolyB p)
        {
            var q = p.Copy();
            for (var i = 0; i < q.Length; i++) q[i] = -q[i];
            return new PolyB(q, true);
        }

        public static PolyB operator +(PolyB p, PolyB q)
        {
            BigInteger[] r;

            if (p.Length >= q.Length)
            {
                r = p.Copy();
                for (var i = 0; i < q.Length; i++) r[i] += q[i];
            }
            else
            {
                r = q.Copy();
                for (var i = 0; i < p.Length; i++) r[i] += p[i];
            }

            return new PolyB(r, true);
        }

        public static PolyB operator -(PolyB p, PolyB q)
        {
            var r = p.Copy();
            if (p.Length < q.Length) Array.Resize(ref r, q.Length);
            for (var i = 0; i < q.Length; i++) r[i] -= q[i];
            return new PolyB(r, true);
        }

        public static PolyB operator *(PolyB p, PolyB q)
        {
            var r = new BigInteger[p.Length + q.Length - 1 ];

            for (var i = 0; i < p.Length; i++)
            {
                if (p[i] == 0)
                    continue;

                for (var j = 0; j < q.Length; j++)
                {
                    r[i + j] += p[i] * q[j];
                }
            }

            return new PolyB(r, true);
        }

        public static PolyB operator +(PolyB p, BigInteger q)
        {
            var r = p.Copy();
            r[0] += q;
            return new PolyB(r, true);
        }

        public static PolyB operator +(BigInteger p,  PolyB q) =>
            q + p;

        public static PolyB operator -(PolyB p, BigInteger q)
        {
            var r = p.Copy();
            r[0] -= q;
            return new PolyB(r, true);
        }

        public static PolyB operator -(BigInteger p, PolyB q) =>
            (-q) + p;

        public static PolyB operator *(BigInteger p, PolyB q)
        {
            var r = q.Copy();
            for (var i = 0; i < r.Length; i++) r[i] *= p;
            return new PolyB(r, true);
        }

        public static PolyB operator /(PolyB p, BigInteger q)
        {
            var r = p.Copy();
            for (var i = 0; i < r.Length; i++) r[i] /= q;
            return new PolyB(r, true);
        }

        public static PolyB operator *(PolyB p, BigInteger q) =>
            q * p;

        public override string ToString()
        {
            if (coeffs is null)
            {
                return "0";
            }

            var s = new StringBuilder();

            for (var power = 0; power < coeffs.Length; power++)
            {
                var coeff = coeffs[power];

                if (coeff != 0)
                {
                    if (s.Length > 0)
                    {
                        s.Append(" ");
                    }

                    if (power == 0)
                    {
                        s.Append(coeff.ToString().Replace("-", "−"));
                    }
                    else
                    {
                        if (coeff < 0)
                        {
                            s.Append("−");

                            if (s.Length > 1)
                            {
                                s.Append(" ");
                            }
                        }
                        else if (s.Length > 0)
                        {
                            s.Append("+ ");
                        }

                        var abs = BigInteger.Abs(coeff);

                        if (abs != 1)
                        {
                            s.Append(abs);
                        }

                        s.Append("𝑛");

                        if (power > 1)
                        {
                            s.Append(new string(power.ToString().Select(c => "⁰¹²³⁴⁵⁶⁷⁸⁹"[c - '0']).ToArray()));
                        }
                    }
                }
            }

            return s.ToString();
        }

        public override bool Equals(object other) => other is PolyB val && Equals(val);

        public bool Equals(PolyB other) => 
            Length == 0 ? other.Length == 0 : 
            other.Length == 0 ? Length == 0 :
            Enumerable.SequenceEqual(coeffs, other.coeffs);

        public override int GetHashCode() => coeffs?.GetHashCode() ?? 0;

        public static bool operator ==(PolyB p, PolyB q) => p.Equals(q);

        public static bool operator !=(PolyB p, PolyB q) => !p.Equals(q);

        public BigInteger At(BigInteger x)
        {
            BigInteger sum = 0;

            if (coeffs is null)
            {
                return 0;
            }

            for (var i = coeffs.Length - 1; i >= 0; i--)
            {
                sum = sum * x + coeffs[i];
            }

            return sum;
        }

        public PolyB Squared()
        {
            if (Length == 0) return Zero;
            var r = new BigInteger[2 * Length - 1];
            
            for (var i = 0; i < Length; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    r[i + j] += 2 * coeffs[i] * coeffs[j];
                }

                r[i + i] = coeffs[i] * coeffs[i];
            }

            return new PolyB(r, true);
        }

        private static BigInteger Pow(BigInteger k, int n)
        {
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n));

            if (n == 0)
                return 1;

            if (k == 0)
                return 0;

            if (n == 1)
                return k;

            if ((n % 2) == 0)
            {
                var s = Pow(k, n / 2);
                return s * s;
            }

            return k * Pow(k, n / 2);
        }

        public PolyB Pow(int n)
        {
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n));

            if (n == 0)
                return new PolyB(1);

            if (Length == 0)
                return Zero;

            if (n == 1)
                return this;

            if ((n % 2) == 0)
                return Pow(n / 2).Squared();

            return this * Pow(n - 1);
        }

        public static PolyB operator ^(PolyB p, int n) =>
            p.Pow(n);

        public PolyB Derivative()
        {
            if (Length == 0) return Zero;
            var r = new BigInteger[Length - 1];
            for (var i = 0; i < r.Length; i++) r[i] = coeffs[i + 1] * (i + 1);
            return new PolyB(r, true);
        }

        private static BigInteger GCD(BigInteger p, BigInteger q)
        {
            while (q != 0)
            {
                (p, q) = (q, p % q);
            }

            return p;
        }

        public BigInteger Content()
        {
            BigInteger gcd = 0;

            for (var i = 0; i < Length; i++)
            {
                if (coeffs[i] != 0) gcd = GCD(coeffs[i], gcd);
            }

            return gcd == 0 ? 1 : gcd;
        }

        public PolyB PrimitivePart() => PrimitivePart(out _);

        public PolyB PrimitivePart(out BigInteger content)
        {
            content = Content();
            if (content == 0) return this;
            var r = Copy();
            for (var i = 0; i < r.Length; i++) r[i] /= content;
            return new PolyB(r, true);
        }

        public IEnumerable<PolyB> Factors()
        {
            var p = PrimitivePart(out var c);
            var n = p.coeffs.TakeWhile(_ => _ == 0).Count();

            if (c != 1 || n != 0)
            {
                var lead = new BigInteger[n + 1];
                lead[n] = c;
                yield return new PolyB(lead, true);
            }

            if (n > 0)
            {
                var arr = new BigInteger[Length - n];
                Array.Copy(p.coeffs, n, arr, 0, arr.Length);
                p = new PolyB(arr, true);
            }

            yield return p;
        }

        public PolyB DivRem(PolyB divisor, out PolyB remainder)
        {
            var r = Copy();
            var offset = Length - divisor.Length;

            if (offset < 0) 
            {
                remainder = this;
                return PolyB.Zero;
            }

            var result = new BigInteger[offset + 1];

            while (offset >= 0)
            {
                var k = r[divisor.Length - 1 + offset] / divisor.coeffs[divisor.Length - 1];

                if (k != 0)
                {
                    result[offset] = k;

                    for (var i = 0; i < divisor.Length; i++)
                    {
                        r[i + offset] -= divisor[i] * k;
                    }
                }

                offset--;
            }

            remainder = new PolyB(r, true);
            return new PolyB(result, true);
        }

        public static PolyB operator /(PolyB p, PolyB q) =>
            p.DivRem(q, out _);

        public static PolyB operator %(PolyB p, PolyB q)
        {
            _ = p.DivRem(q, out var result);
            return result;
        }
    }
}
