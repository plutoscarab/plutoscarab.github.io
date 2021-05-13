/*
 * Real.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This file implements quad-precision floating point math. No
 * guarantees are made about the performance or accuracy of this
 * code. I wrote it as a challenge for myself, and not for use
 * as a general-purpose library. But it seems to work well enough
 * for the RPN calculator. 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using ulonglong = Mulvey.QuadPrecision.UInt128;

namespace Mulvey.QuadPrecision
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct DoubleBytes
	{
		[FieldOffset(0)]
		public double Double;
		[FieldOffset(0)]
		public ulong Bytes;
		[FieldOffset(0)]
		public byte Byte0;
		[FieldOffset(1)]
		public byte Byte1;
		[FieldOffset(2)]
		public byte Byte2;
		[FieldOffset(3)]
		public byte Byte3;
		[FieldOffset(4)]
		public byte Byte4;
		[FieldOffset(5)]
		public byte Byte5;
		[FieldOffset(6)]
		public byte Byte6;
		[FieldOffset(7)]
		public byte Byte7;
		[FieldOffset(0)]
		public ushort Word0;
		[FieldOffset(2)]
		public ushort Word1;
		[FieldOffset(4)]
		public ushort Word2;
		[FieldOffset(6)]
		public ushort Word3;

		public DoubleBytes(double d)
		{
			Bytes = Word0 = Word1 = Word2 = Word3 = Byte0 = Byte1 = Byte2 = Byte3 = Byte4 = Byte5 = Byte6 = Byte7 = 0;
			Double = d;
		}

		public DoubleBytes(byte[] bytes)
		{
			Double = Bytes = Word0 = Word1 = Word2 = Word3 = 0;
			Byte0 = bytes[0]; Byte1 = bytes[1]; Byte2 = bytes[2]; Byte3 = bytes[3];
			Byte4 = bytes[4]; Byte5 = bytes[5]; Byte6 = bytes[6]; Byte7 = bytes[7];
		}
	}

	/// <summary>
	/// Quadruple-precision floating-point number. 144 bits of precision with one sign
	/// bit, 16 exponent bits, and 127 mantissa bits.
	/// </summary>
	public struct Real
	{
		const ulong msb = 0x8000000000000000;

		bool negative;
		short exponent;
		ulonglong mantissa;
		int signif;	// number of significant bits

		/// <summary>
		/// Represents a value that is not a number (NaN).
		/// </summary>
		public static Real NaN = new Real(double.NaN);

		/// <summary>
		/// Represents positive infinity.
		/// </summary>
		public static Real PositiveInfinity = new Real(double.PositiveInfinity);

		/// <summary>
		/// Represents negative infinity.
		/// </summary>
		public static Real NegativeInfinity = new Real(double.NegativeInfinity);

		/// <summary>
		/// Represents the smallest possible Real greater than zero.
		/// </summary>
		public static Real Epsilon = new Real(false, short.MinValue, msb, 0);

		/// <summary>
		/// Represents the most positive valid value of Real.
		/// </summary>
		public static Real MaxValue = new Real(false, short.MaxValue - 1, 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF);

		/// <summary>
		/// Represents the most negative valid value of Real.
		/// </summary>
		public static Real MinValue = new Real(true, short.MaxValue - 1, 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF);

		/// <summary>
		/// Represents zero.
		/// </summary>
		public static Real Zero = new Real(false, short.MinValue, 0, 0);

		/// <summary>
		/// Represents one.
		/// </summary>
		public static Real One = new Real(1);
		public static Real NegativeOne = new Real(-1);

		/// <summary>
		/// Represents two.
		/// </summary>
		public static Real Two = new Real(2);
		public static Real OneHalf = new Real(0.5);
		public static Real Ln2 = new Real(0x3ffe, 0xb172, 0x17f7, 0xd1cf, 0x79ab, 0xc9e3, 0xb398, 0x03f2, 0xf6af);
		public static Real OneOverLn2 = new Real(false, 1, 0xb8aa3b295c17f0bb, 0xbe87fed0691d3ec6);
		public static Real Sqrt2 = new Real(0x3fff, 0xb504, 0xf333, 0xf9de, 0x6484, 0x597d, 0x89b3, 0x754a, 0xbe9f);

		/// <summary>
		/// Represents the natural logarithmic base.
		/// </summary>
		public static Real E = new Real(0x4000, 0xadf8, 0x5458, 0xa2bb, 0x4a9a, 0xafdc, 0x5620, 0x273d, 0x3cf1);

		/// <summary>
		/// Represents the ratio of a circle's circumference to its diameter in Euclidean geometry.
		/// </summary>
		public static Real Pi = new Real(0x4000, 0xc90f, 0xdaa2, 0x2168, 0xc234, 0xc4c6, 0x628b, 0x80dc, 0x1cd1);
		public static Real PiOver2 = new Real(0x3FFF, 0xc90f, 0xdaa2, 0x2168, 0xc234, 0xc4c6, 0x628b, 0x80dc, 0x1cd1);
		public static Real TwoPi = new Real(0x4001, 0xc90f, 0xdaa2, 0x2168, 0xc234, 0xc4c6, 0x628b, 0x80dc, 0x1cd1);

		public static Real Ten = new Real(10);
		public static Real OneOverLn10 = new Real(false, -1, 0xde5bd8a937287195, 0x355baaafad33dc6e);

		public static Real Omega = new Real(false, 0, 0x91304d7c74b2ba5e, 0xafddaa6286dc28e2);

		private Real(ushort Word8, ushort Word7, ushort Word6, ushort Word5, ushort Word4,
			ushort Word3, ushort Word2, ushort Word1, ushort Word0)
		{
			negative = Word8 > short.MaxValue;
			exponent = (short) ((Word8 & 0x7FFF) - 16382);
			ulong mantHi = (ulong)((ulong)Word4 | ((ulong)Word5 << 16) | ((ulong)Word6 << 32) | ((ulong)Word7 << 48));
			ulong mantLo = (ulong)((ulong)Word0 | ((ulong)Word1 << 16) | ((ulong)Word2 << 32) | ((ulong)Word3 << 48));
			mantissa = new ulonglong(mantHi, mantLo);
			signif = 128;
		}

		public Real(bool negative, short exponent, ulong mantHi, ulong mantLo)
		{
			this.negative = negative;
			this.exponent = exponent;
			this.mantissa = new ulonglong(mantHi, mantLo);
			if (this.mantissa != ulonglong.Zero)
				this.exponent -= (short) this.mantissa.Normalize();
			signif = 128;
		}

		public Real(long i)
		{
			signif = 128;
			if (i == 0)
			{
				negative = false;
				exponent = short.MinValue;
				mantissa = 0;
				return;
			}

			negative = i < 0;
			exponent = 64;
			if (i > long.MinValue)
				mantissa = new ulonglong((ulong)Math.Abs(i), 0);
			else
				mantissa = new ulonglong((ulong)long.MaxValue + 1UL, 0);
			exponent -= (short)mantissa.Normalize();
		}

		public static implicit operator Real(long i)
		{
			return new Real(i);
		}

		public Real(ulong u)
		{
			negative = false;
			signif = 128;
			if (u == 0)
			{
				exponent = short.MinValue;
				mantissa = 0;
				return;
			}

			exponent = 64;
			mantissa = new ulonglong(u, 0);
			exponent -= (short)mantissa.Normalize();
		}

		public Real(double d)
		{
			signif = 128;
			if (Double.IsNaN(d))
			{
				negative = false;
				exponent = short.MaxValue;
				mantissa = ulonglong.MaxValue;
			}
			else if (Double.IsPositiveInfinity(d))
			{
				negative = false;
				exponent = short.MaxValue;
				mantissa = new ulonglong(msb, 0);
			}
			else if (Double.IsNegativeInfinity(d))
			{
				negative = true;
				exponent = short.MaxValue;
				mantissa = new ulonglong(msb, 0);
			}
			else if (d == 0)
			{
				negative = false;
				exponent = short.MinValue;
				mantissa = 0;
			}
			else
			{
				DoubleBytes db = new DoubleBytes(d);
				negative = d < 0;
				exponent = (short)(((db.Word3 >> 4) & 0x7FF) - 0x3FE);
				ulong mantHi;
				if (exponent > -0x3FE)
					mantHi = (db.Bytes << 11) | msb;
				else
				{
					mantHi = db.Bytes << 12;
					if (mantHi != 0)
					{
						while ((mantHi & msb) == 0)
						{
							mantHi <<= 1;
							exponent--;
						}
					}
				}
				mantissa = new ulonglong(mantHi, 0);
			}
		}

		public Real(string s)
			: this(s, 10)
		{
		}

		public Real(string s, int numberBase)
		{
			const string digits = "0123456789ABCDEF";

			if (s == null) throw new ArgumentNullException();
			if (s.Length == 0) throw new ArithmeticException();
			if (numberBase < 2 || numberBase > 16) throw new ArgumentOutOfRangeException();

			int b = numberBase;
			Real n = 0, f = 1;
			int i = 0;
			if (s[i] == '-')
			{
				i++;
			}
			int D = -1, E = -1;
			int exp = 0;
			for (; i < s.Length; i++)
			{
				if (s[i] == '^')
				{
					if (E != -1) throw new FormatException();
					E = i;
					i++;
					if (i >= s.Length) throw new FormatException();
					if (s[i] != '+' && s[i] != '-') throw new FormatException();
				}
				else if (s[i] == '.')
				{
					if (D != -1 || E != -1) throw new FormatException();
					D = i;
				}
				else
				{
					int d = digits.IndexOf(s[i]);
					if (d == -1 || d >= b) throw new FormatException();
					if (E != -1)
					{
						exp = exp * b + d;
					}
					else if (D != -1)
					{
						f /= b;
						n += f * d;
					}
					else
					{
						n = n * b + d;
					}
				}
			}
			if (E != -1)
			{
				if (s[E + 1] == '-')
					exp *= -1;
				n *= Real.Pow(b, exp);
			}
			if (s[0] == '-')
				n *= -1;
			negative = n.negative;
			exponent = n.exponent;
			mantissa = n.mantissa;
			signif = 128;
		}

		public static implicit operator Real(ulong u)
		{
			return new Real(u);
		}

		public static implicit operator Real(int i)
		{
			return new Real((long)i);
		}

		public static explicit operator Real(string s)
		{
			return new Real(s);
		}

		public static implicit operator Real(double d)
		{
			return new Real(d);
		}

		public static explicit operator int(Real r)
		{
			r = Truncate(r);
			if (r < int.MinValue || r > int.MaxValue)
				throw new ArgumentOutOfRangeException();
			return (int)(r.mantissa >> (128 - r.exponent)).Lo * (r.negative ? -1 : 1);
		}

		public static bool IsNaN(Real r)
		{
			return r.exponent == short.MaxValue && !IsInfinity(r);
		}

		public static bool IsInfinity(Real r)
		{
			return IsPositiveInfinity(r) || IsNegativeInfinity(r);
		}

		public static bool IsPositiveInfinity(Real r)
		{
			return r == Real.PositiveInfinity;
		}

		public static bool IsNegativeInfinity(Real r)
		{
			return r == Real.NegativeInfinity;
		}

		public static bool IsZero(Real r)
		{
			return r.exponent == short.MinValue && r.mantissa == UInt128.Zero;
		}

		public bool IsZero()
		{
			return IsZero(this);
		}

		public static bool IsNegative(Real r)
		{
			return r.negative;
		}

		public bool IsNegative()
		{
			return IsNegative(this);
		}

		public static bool operator ==(Real a, Real b)
		{
			if (IsZero(a) && IsZero(b)) return true;	// +0 == -0
			return a.negative == b.negative && a.exponent == b.exponent && a.mantissa == b.mantissa;
		}

		public static bool operator !=(Real a, Real b)
		{
			return !(a == b);
		}

		public override bool Equals(object o)
		{
			if (!(o is Real))
				return false;
			return this == (Real)o;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return ToString(10);
		}

		public string ToString(int numberBase)
		{
			if (numberBase < 2 || numberBase > 26)
				throw new ArgumentOutOfRangeException();

			if (IsNaN(this))
				return double.NaN.ToString();

			if (IsNegativeInfinity(this))
				return double.NegativeInfinity.ToString();

			if (IsPositiveInfinity(this))
				return double.PositiveInfinity.ToString();

			Real nb = numberBase;
			StringBuilder s = new StringBuilder(45);
			if (negative)
				s.Append('-');
			Real abs = Abs(this);
			int exp;
			if (this == Zero)
				exp = 0;
			else
				exp = (int) Ceiling(Log(abs, nb)) - 1;
			Real div = IntPower(nb, exp);
			Real mant = abs / div;
			if ((int) mant >= numberBase)
			{
				exp++;
				mant /= nb;
			}
			int dec = 1;
			if (exp >= 0 && exp <= 20)
			{
				dec += exp;
				exp = 0;
			}
			int trailz = 0;
			bool emitdec = false;
			for (int i = 0; i < 37; i++)
			{
				int digit = (int) mant;
				if (digit < 0 || digit >= numberBase)
					Debugger.Break();
				if (digit == 0 && dec <= 0)
				{
					trailz++;
				}
				else
				{
					if (emitdec)
					{
						s.Append('.');
						emitdec = false;
					}
					while (trailz > 0)
					{
						s.Append('0');
						trailz--;
					}
					s.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[digit]);
					if (--dec == 0)
						emitdec = true;
				}
				mant = nb * Frac(mant);
			}
			if (exp != 0)
			{
				s.Append("E");
				if (exp >= 0) s.Append('+');
				s.Append(exp.ToString());
			}
			return s.ToString();
		}

		public static implicit operator ulong(Real r)
		{
			r = r.Floor();
			if (r < ulong.MinValue || r > ulong.MaxValue)
				throw new ArgumentOutOfRangeException();

			return r.mantissa.Hi >> (64 - r.exponent);
		}

		public static implicit operator double(Real r)
		{
			if (IsNaN(r))
				return double.NaN;

			if (r.exponent < -0x3FE - 56)
				return 0.0;

			if (r.exponent > 0x400)
				return r.negative ? double.NegativeInfinity : double.PositiveInfinity;

			while (r.exponent < -0x3FE)
			{
				r.exponent++;
				r.mantissa.ShiftRight();
			}

			DoubleBytes db = new DoubleBytes();
			if (r.exponent == -0x3FE)
			{
				db.Word3 = (ushort)((r.negative ? 0x8000 : 0) | ((r.exponent + 0x3FE) << 4) | ((byte)(r.mantissa.Hi >> 60) & 0xF));
				db.Word2 = (ushort)(r.mantissa.Hi >> 44);
				db.Word1 = (ushort)(r.mantissa.Hi >> 28);
				db.Word0 = (ushort)(r.mantissa.Hi >> 12);
			}
			else
			{
				db.Word3 = (ushort)((r.negative ? 0x8000 : 0) | ((r.exponent + 0x3FE) << 4) | ((byte)(r.mantissa.Hi >> 59) & 0xF));
				db.Word2 = (ushort)(r.mantissa.Hi >> 43);
				db.Word1 = (ushort)(r.mantissa.Hi >> 27);
				db.Word0 = (ushort)(r.mantissa.Hi >> 11);
			}
			return db.Double;
		}

		public static Real operator -(Real r)
		{
			if (IsNaN(r))
				return NaN;

			Real temp = r;
			temp.negative = !temp.negative;
			return temp;
		}

		public static Real operator +(Real a, Real b)
		{
			if (a.negative != b.negative)
			{
				b.negative = !b.negative;
				return a - b;
			}

			if (IsNaN(a) || IsNaN(b))
				return NaN;

			Real r = a;
			if (b.exponent > a.exponent)
			{
				a = b; b = r; r = a;
			}
			r.signif = Math.Min(a.signif, b.signif);

			int shift = a.exponent - b.exponent;
			if (shift > 127)
				return a;

			if (a.mantissa.IsZero())
				return Zero;

			b.mantissa.ShiftRight(shift);

			r.mantissa = a.mantissa + b.mantissa;
			if (r.mantissa.Overflow)
			{
				r.mantissa.ShiftRightWithMsb();
				if (r.exponent == short.MaxValue)
					return r.negative ? NegativeInfinity : PositiveInfinity;
				r.exponent++;
			}

			return r;
		}

		private void Normalize()
		{
			if (mantissa == ulonglong.Zero)
			{
				negative = false;
				exponent = short.MinValue;
				mantissa = ulonglong.Zero;
				return;
			}

			int exp = exponent - mantissa.Normalize();
			if (exp < short.MinValue)
			{
				negative = false;
				exponent = short.MinValue;
				mantissa = ulonglong.Zero;
				return;
			}

			exponent = (short)exp;
		}

		public static Real operator -(Real a, Real b)
		{
			if (a.negative != b.negative)
				return a + (-b);

			if (IsNaN(a) || IsNaN(b))
				return NaN;

			if (a == b)
				return Zero;

			bool negate = false;
			Real r = a;
			if (b.exponent > a.exponent)
			{
				a = b; b = r; r = a;
				negate = true;
			}

			int shift = a.exponent - b.exponent;
			if (shift > 127)
				return negate ? -a : a;

			if (shift == 0 && b.mantissa > a.mantissa)
			{
				a = b; b = r; r = a;
				negate = true;
			}

			r.signif = Math.Min(a.signif, b.signif);

			if (a.mantissa.IsZero())
				return Zero;

			b.mantissa.ShiftRight(shift);

			r.mantissa = a.mantissa - b.mantissa;
			r.Normalize();
			if (negate)
				r.negative = !r.negative;
			return r;
		}

		public static Real operator *(Real a, Real b)
		{
			if (IsNaN(a) || IsNaN(b))
				return NaN;

			if (a == Zero || b == Zero)
				return Zero;

			if (IsInfinity(a) || IsInfinity(b))
				return a.negative == b.negative ? PositiveInfinity : NegativeInfinity;

			Real r;
			r.signif = Math.Min(a.signif, b.signif);
			r.negative = a.negative != b.negative;
			int exponent = a.exponent + b.exponent;
			r.mantissa = ulonglong.Multiply(a.mantissa.Hi, b.mantissa.Hi);
			r.mantissa += ulonglong.Multiply(a.mantissa.Lo, b.mantissa.Hi) >> 64;
			bool carry = r.mantissa.Overflow;
			r.mantissa += ulonglong.Multiply(a.mantissa.Hi, b.mantissa.Lo) >> 64;
			carry |= r.mantissa.Overflow;
			if (carry)
			{
				exponent++;
				r.mantissa.ShiftRightWithMsb();
			}
			exponent -= (short)r.mantissa.Normalize();
			if (exponent >= short.MaxValue)
				return r.negative ? NegativeInfinity : PositiveInfinity;
			if (exponent <= short.MinValue)
				return Zero;
			r.exponent = (short)exponent;
			return r;
		}

		private static void Multiply128(ulong x, ulong y, out ulong hi, out ulong lo)
		{
			if (x == 0 || y == 0)
			{
				hi = lo = 0;
				return;
			}
			uint a = (uint)(x >> 32);
			uint b = (uint)x;
			uint c = (uint)(y >> 32);
			uint d = (uint)y;
			ulong l = (ulong)b * d;
			ulong m1 = (ulong)a * d;
			ulong m2 = (ulong)b * c;
			ulong m = m1 + m2;
			ulong h = (ulong)a * c;
			lo = l + (m << 32);
			hi = h + (m >> 32);
			if (lo < l) hi++;
			if (m < m1 || m < m2) hi += 0x100000000;
		}

		public static Real operator /(Real a, Real b)
		{
			if (IsNaN(a) || IsNaN(b))
				return NaN;

			if (b == Zero)
				return NaN;

			if (IsInfinity(a) && IsInfinity(b))
				return NaN;

			if (IsInfinity(b))
				return Zero;

			if (IsInfinity(a))
				return a.negative == b.negative ? a : -a;

			if (a == Zero)
				return Zero;

			Real r;
			r.signif = Math.Min(a.signif, b.signif);
			r.negative = a.negative != b.negative;
			int exponent = a.exponent - b.exponent + 1;
			r.mantissa = ulonglong.Zero;
			exponent -= a.mantissa.Normalize();
			exponent -= b.mantissa.Normalize();
			ulonglong mask = new ulonglong(msb, 0);
			for (int i = 0; i < 128; i++)
			{
				if (a.mantissa >= b.mantissa)
				{
					r.mantissa |= mask;
					a.mantissa -= b.mantissa;
				}
				mask.ShiftRight();
				b.mantissa.ShiftRight();
			}
			exponent -= (short)r.mantissa.Normalize();
			if (exponent >= short.MaxValue)
				return r.negative ? NegativeInfinity : PositiveInfinity;
			if (exponent <= short.MinValue)
				return Zero;
			r.exponent = (short)exponent;
			return r;
		}

		public static bool operator <(Real a, Real b)
		{
			if (IsNaN(a) || IsNaN(b))
				return false;

			if (a == b)
				return false;

			if (a == Zero)
				return !b.negative;

			if (b == Zero)
				return a.negative;

			if (a.negative != b.negative)
				return a.negative;

			if (a.exponent > b.exponent)
				return a.negative;

			if (a.exponent < b.exponent)
				return !a.negative;

			return (a.mantissa > b.mantissa) == a.negative;
		}

		public static bool operator >(Real b, Real a)
		{
			if (IsNaN(a) || IsNaN(b))
				return false;

			if (a == b)
				return false;

			if (a == Zero)
				return !b.negative;

			if (b == Zero)
				return a.negative;

			if (a.negative != b.negative)
				return a.negative;

			if (a.exponent > b.exponent)
				return a.negative;

			if (a.exponent < b.exponent)
				return !a.negative;

			return (a.mantissa > b.mantissa) == a.negative;
		}

		public static bool operator >=(Real a, Real b)
		{
			return !(a < b);
		}

		public static bool operator <=(Real a, Real b)
		{
			return !(a > b);
		}

		private void Signif(int n)
		{
			signif = Math.Min(n, signif);
		}

		public int SignificantBits
		{
			get
			{ 
				return signif; 
			}
			set
			{
				Signif(Math.Max(0, value));
			}
		}

		public static Real Sqrt(Real r)
		{
			if (r.negative)
				return NaN;
			if (IsNaN(r))
				return NaN;
			if (IsInfinity(r))
				return r;
			if (IsZero(r))
				return Zero;

			short exponent = r.exponent;
			r.exponent = 0;
			Real x = r;
			while (true)
			{
				Real y = x;
				x += r / x;
				x.exponent--;
				if (x.exponent == y.exponent && x.mantissa.Hi == y.mantissa.Hi)
				{
					long diff = (long)(x.mantissa.Lo - y.mantissa.Lo);
					if (diff != long.MinValue)
						if (Math.Abs(diff) < 5)
							break;
				}
			}
			x.exponent = (short) (exponent >> 1);
			if ((exponent & 1) != 0)
			{
				x *= Sqrt2;
			}
			x.Signif(122);
			return x;
		}

		public Real Sqrt()
		{
			return Sqrt(this);
		}

		public static Real Abs(Real r)
		{
			if (IsNaN(r))
				return NaN;
			r.negative = false;
			return r;
		}

		public Real Abs()
		{
			return Abs(this);
		}

		public static Real Truncate(Real r)
		{
			if (IsNaN(r) || IsInfinity(r))
				return r;

			if (r.exponent <= 0)
				return Zero;

			if (r.exponent >= 128)
				return r;

			ulonglong mask = (ulonglong.One << (128 - r.exponent)) - ulonglong.One;
			r.mantissa &= ~mask;
			return r;
		}

		public Real Truncate()
		{
			return Truncate(this);
		}

		public static Real Ceiling(Real r)
		{
			if (IsNaN(r) || IsInfinity(r))
				return r;

			if (r.negative)
				return Truncate(r);

			if (r.exponent <= 0)
				return r > Zero ? One : Zero;

			if (r.exponent >= 128)
				return r;

			ulonglong mask = (ulonglong.One << (128 - r.exponent)) - ulonglong.One;
			bool frac = (r.mantissa & mask) != 0;
			r.mantissa &= ~mask;
			return frac ? r + One : r;
		}

		public Real Ceiling()
		{
			return Ceiling(this);
		}

		public static Real Floor(Real r)
		{
			if (IsNaN(r) || IsInfinity(r))
				return r;

			if (!r.negative)
				return Truncate(r);

			if (r.exponent <= 0)
				return r > Zero ? Zero : NegativeOne;

			if (r.exponent >= 128)
				return r;

			ulonglong mask = (ulonglong.One << (128 - r.exponent)) - ulonglong.One;
			bool frac = (r.mantissa & mask) != 0;
			r.mantissa &= ~mask;
			return frac ? r - One : r;
		}

		public Real Floor()
		{
			return Floor(this);
		}

		public static Real Frac(Real r)
		{
			return r - Truncate(r);
		}

		public Real Frac()
		{
			return Frac(this);
		}

		public static bool IsInteger(Real r)
		{
			return r == Truncate(r);
		}

		public bool IsInteger()
		{
			return IsInteger(this);
		}

		public static bool IsOdd(Real r)
		{
			if (!IsInteger(r))
				return false;

			if (r.exponent <= 0)
				return true;

			if (r.exponent >= 128)
				return false;

			return (r.mantissa & (ulonglong.One << (128 - r.exponent))) != ulonglong.Zero;
		}

		public bool IsOdd()
		{
			return IsOdd(this);
		}

		public static Real Round(Real r)
		{
			Real f = Frac(r);
			r = Floor(r + OneHalf);
			if (Abs(f) == OneHalf)
				if (IsOdd(r))
					r -= One;
			return r;
		}

		public Real Round()
		{
			return Round(this);
		}

		public static Real Min(Real a, Real b)
		{
			return a < b ? a : b;
		}

		public static Real Max(Real a, Real b)
		{
			return a > b ? a : b;
		}

		public static Real Sign(Real r)
		{
			if (IsNaN(r))
				return Real.NaN;

			if (r == Zero)
				return 0;

			return r.negative ? Real.NegativeOne : Real.One;
		}

		public Real Sign()
		{
			return Sign(this);
		}

		static byte[] randBytes = new byte[16];

		public static Real NextReal(Random rand)
		{
			rand.NextBytes(randBytes);
			Real r;
			r.negative = false;
			r.exponent = 0;
			ulong hi = BitConverter.ToUInt64(randBytes, 0);
			ulong lo = BitConverter.ToUInt64(randBytes, 8);
			if (hi == 0 && lo == 0)
				return Zero;
			r.mantissa = new ulonglong(hi, lo);
			r.signif = 128;
			r.Normalize();
			return r;
		}

		internal static Real Random(Random rand)
		{
			rand.NextBytes(randBytes);
			Real r;
			r.negative = rand.Next(2) == 1;
			r.exponent = (short)rand.Next(short.MinValue + 1, short.MaxValue);
			ulong hi = BitConverter.ToUInt64(randBytes, 0);
			ulong lo = BitConverter.ToUInt64(randBytes, 8);
			r.mantissa = new ulonglong(hi | msb, lo);
			r.signif = 128;
			return r;
		}

		public static Real Log(Real r)
		{
			if (IsNaN(r))
				return NaN;
			if (r.negative)
				return NaN;
			if (IsZero(r))
				return NaN;
			if (IsPositiveInfinity(r))
				return r;
			if (r == One)
				return Zero;

			Real e = r.exponent * Ln2;
			r.exponent = 0;
			Real y = (r - One) / (r + One);
			Real y2 = y * y;
			Real num = One;
			Real den = One;
			Real sum = One;
			while (true)
			{
				num *= y2;
				den += Two;
				Real term = num / den;
				if (term == Zero || term.exponent < sum.exponent - 128)
					break;
				sum += term;
			}
			sum = Two * y * sum + e;
			sum.Signif(122);
			return sum;
		}

		public Real Log()
		{
			return Log(this);
		}

		public static Real Log(Real a, Real b)
		{
			return Log(a) / Log(b);
		}

		public static Real Log10(Real r)
		{
			return Log(r) * OneOverLn10;
		}

		public Real Log10()
		{
			return Log10(this);
		}

		public static Real Log2(Real r)
		{
			return Log(r) * OneOverLn2;
		}

		public Real Log2()
		{
			return Log2(this);
		}

		private static Real IntPower(Real r, Real n)
		{
			if (n == 0)
				return One;
			if (n < 0)
				return One / IntPower(r, -n);
			if (IsOdd(n))
				return r * IntPower(r, n - One);
			n.exponent--;
			r = IntPower(r, n);
			return r * r;
		}

		private static Real expMax = new Real(22712);
		private static Real negExpMax = new Real(-22713);

		public static Real Exp(Real r)
		{
			if (IsNaN(r))
				return NaN;

			if (r > expMax)
				return PositiveInfinity;

			if (r < -expMax)
				return Zero;

			if (r.exponent < -88)
				return One;

			Real z = Truncate(r);
			Real f = Frac(r);
			Real ez = IntPower(E, z);
			Real sum = One;
			Real count = Zero;
			Real fac = One;
			Real power = One;
			while (true)
			{
				power *= f;
				count += One;
				fac *= count;
				Real term = power / fac;
				if (term == Zero || term.exponent < sum.exponent - 128)
					break;
				sum += term;
			}
			sum *= ez;
			sum.Signif(119);
			return sum;
		}

		public Real Exp()
		{
			return Exp(this);
		}

		public static Real Mod(Real a, Real b)
		{
			Real q = a / b;
			q = b * Truncate(q);
			return a - q;
		}

		public Real Mod(Real b)
		{
			return Mod(this, b);
		}

		public static Real Sin(Real r)
		{
			if (r.exponent > 63)
				return r;
			bool neg = r.negative;
			r.negative = false;
			r = Mod(r, TwoPi);
			if (r > Pi)
			{
				neg = !neg;
				r -= Pi;
			}
			Real r2 = -(r * r);
			Real num = r;
			Real den = One;
			Real count = One;
			Real sum = r;
			while (true)
			{
				num *= r2;
				count += One;
				den *= count;
				count += One;
				den *= count;
				Real term = num / den;
				if (term == Zero || term.exponent < sum.exponent - 128)
					break;
				sum += term;
			}
			r = sum;
			if (neg) r.negative = true;
			r.Signif(122);
			return r;
		}

		public Real Sin()
		{
			return Sin(this);
		}

		public static Real Cos(Real r)
		{
			return Sin(PiOver2 - r);
		}

		public Real Cos()
		{
			return Cos(this);
		}

		public static Real Tan(Real r)
		{
			return Sin(r) / Cos(r);
		}

		public Real Tan()
		{
			return Tan(this);
		}

		public static Real Asin(Real r)
		{
			Real a = Abs(r);
			if (a > 1)
				return NaN;
			if (a == 1)
				return r.Sign() * Pi / 2;

			return r.Sign() * Atan(a / Sqrt(One - a * a));
		}

		public Real Asin()
		{
			return Asin(this);
		}

		public static Real Acos(Real r)
		{
			if (Abs(r) > 1)
				return NaN;

			if (Abs(r) == 1)
				return Zero;

			if (r < 0)
				return Pi / 2 + Asin(r);

			return Pi / 2 - Asin(r);
		}

		public Real Acos()
		{
			return Acos(this);
		}

		public static Real Atan(Real r)
		{
			Real a = Abs(r);
			if (a > One)
				return r.Sign() * (Pi / Two - Atan(1 / a));
			if (a == One)
				return r > Zero ? Pi / 4 : -Pi / 4;
			if (a > Sqrt2 - One)
				return r.Sign() * (Pi / 4 - Atan((1 - a) / (1 + a)));
			if (a.exponent < -64)
				return a;

			Real sum = r;
			Real r2 = -r * r;
			Real power = r;
			int n = 1;
			while (true)
			{
				n += 2;
				power *= r2;
				Real term = power / n;
				if (term == Zero || term.exponent < sum.exponent - 128)
					break;
				sum += term;
			}
			return sum;
		}

		public Real Atan()
		{
			return Atan(this);
		}

		public static Real Atan2(Real y, Real x)
		{
			if (IsNaN(y) || IsNaN(x))
				return NaN;

			if (x == Zero)
				if (y == Zero)
					return NaN;
				else
					return y.Sign() * Pi / 2;

			if (y == Zero)
				return x > Zero ? Zero : Pi;

			if (x > 0)
				return Atan(y / x);

			return y.Sign() * (Pi - Abs(Atan(y / x)));
		}

		public static Real Pow(Real a, Real b)
		{
			if (IsZero(b)) return One;
			if (IsZero(a)) return Zero;
			return Exp(b * Log(a));
		}

		public static Real Pow(Real a, int b)
		{
			if (b == 0) return One;
			if (IsZero(a)) return Zero;
			if (b < 0) return Real.One / Pow(a, -b);
			if ((b & 1) == 0)
			{
				Real p = Pow(a, b / 2);
				return p * p;
			}
			return a * Pow(a, b - 1);
		}

		public Real Pow(Real b)
		{
			return Pow(this, b);
		}

		public static Real Sinh(Real r)
		{
			if (r.exponent < -42)
				return r;

			r = Exp(r);
			if (IsInfinity(r))
				return PositiveInfinity;
			if (r == Zero)
				return NegativeInfinity;

			r -= One / r;
			r.exponent--;
			return r;
		}

		public Real Sinh()
		{
			return Sinh(this);
		}

		public static Real Cosh(Real r)
		{
			r = Exp(r);
			if (IsInfinity(r))
				return PositiveInfinity;
			if (r == Zero)
				return NegativeInfinity;

			r += One / r;
			r.exponent--;
			return r;
		}

		public Real Cosh()
		{
			return Cosh(this);
		}

		public static Real Tanh(Real r)
		{
			if (IsNaN(r))
				return NaN;

			if (r.exponent < -42)
				return r;

			r.exponent++;
			r = Exp(r);
			if (IsInfinity(r))
				return One;
			if (r == Zero)
				return NegativeOne;

			r = (r - One) / (r + One);
			return r;
		}

		public Real Tanh()
		{
			return Tanh(this);
		}

		internal Real Mask(int wordSize)
		{
			if (this.exponent <= 0 || this.exponent >= 128 + wordSize)
				return Real.Zero;

			Real r = this.Truncate();
			while (r.exponent > wordSize)
			{
				r.mantissa <<= 1;
				r.exponent--;
			}
			r.Normalize();
			return r;
		}
	}
}
