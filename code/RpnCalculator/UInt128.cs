/*
 * UInt128.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This file implements an unsigned 128-bit integer data type.
 * It is used to store the mantissa in the quad-precision floating-
 * point data type defined in Real.cs.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Mulvey.QuadPrecision
{
	public struct UInt128
	{
		const ulong msb = 0x8000000000000000;

		ulong hi;
		ulong lo;
		bool overflow;

		public static UInt128 Zero = new UInt128(0);
		public static UInt128 One = new UInt128(1);
		public static UInt128 MinValue = Zero;
		public static UInt128 MaxValue = new UInt128(ulong.MaxValue, ulong.MaxValue);

		public UInt128(ulong hi, ulong lo)
		{
			this.hi = hi;
			this.lo = lo;
			overflow = false;
		}

		public UInt128(ulong u)
			: this(0, u)
		{
		}

		public static implicit operator UInt128(ulong u)
		{
			return new UInt128(u);
		}

		public ulong Hi
		{
			get { return hi; }
		}

		public ulong Lo
		{
			get { return lo; }
		}

		public override bool Equals(object obj)
		{
			if (!(obj is UInt128))
				return false;
			UInt128 o = (UInt128)obj;
			return o.hi == hi && o.lo == lo;
		}

		public override int GetHashCode()
		{
			return lo.GetHashCode();
		}

		public static bool operator ==(UInt128 a, UInt128 b)
		{
			return a.lo == b.lo && a.hi == b.hi;
		}

		public static bool operator !=(UInt128 a, UInt128 b)
		{
			return a.lo != b.lo || a.hi != b.hi;
		}

		public static bool operator >(UInt128 a, UInt128 b)
		{
			return a.hi > b.hi || (a.hi == b.hi && a.lo > b.lo);
		}

		public static bool operator <(UInt128 a, UInt128 b)
		{
			return a.hi < b.hi || (a.hi == b.hi && a.lo < b.lo);
		}

		public static bool operator >=(UInt128 a, UInt128 b)
		{
			return a.hi > b.hi || (a.hi == b.hi && a.lo >= b.lo);
		}

		public static bool operator <=(UInt128 a, UInt128 b)
		{
			return a.hi < b.hi || (a.hi == b.hi && a.lo <= b.lo);
		}

		public bool Overflow
		{
			get { return overflow; }
		}

		public static UInt128 operator +(UInt128 a, UInt128 b)
		{
			UInt128 c;
			c.lo = a.lo + b.lo;
			bool carry = c.lo < a.lo;
			c.hi = a.hi + b.hi;
			c.overflow = c.hi < a.hi;
			if (carry)
			{
				c.hi++;
				if (c.hi == 0) c.overflow = true;
			}
			return c;
		}

		public static UInt128 operator -(UInt128 a, UInt128 b)
		{
			UInt128 c;
			bool borrow = a.lo < b.lo;
			c.lo = a.lo - b.lo;
			c.overflow = a.hi < b.hi;
			c.hi = a.hi - b.hi;
			if (borrow)
			{
				if (c.hi == 0)
					c.overflow = true;
				c.hi--;
			}
			return c;
		}

		public static UInt128 Multiply(ulong x, ulong y)
		{
			UInt128 z;
			z.overflow = false;
			if (x == 0 || y == 0)
			{
				z.hi = z.lo = 0;
				return z;
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
			z.lo = l + (m << 32);
			z.hi = h + (m >> 32);
			if (z.lo < l) z.hi++;
			if (m < m1 || m < m2) z.hi += 0x100000000;
			return z;
		}

		public static void ShiftRight(ref UInt128 a, int b)
		{
			if (b < 0)
			{
				ShiftLeft(ref a, -b);
				return;
			}

			if (b == 0)
				return;

			if (b < 64)
			{
				a.lo = (a.lo >> b) | (a.hi << (64 - b));
				a.hi >>= b;
				return;
			}

			if (b == 64)
			{
				a.lo = a.hi;
				a.hi = 0;
				return;
			}

			if (b < 128)
			{
				a.lo = a.hi >> b - 64;
				a.hi = 0;
				return;
			}

			a.hi = a.lo = 0;
		}

		public static UInt128 operator >>(UInt128 a, int b)
		{
			ShiftRight(ref a, b);
			return a;
		}

		public static void ShiftLeft(ref UInt128 a, int b)
		{
			if (b < 0)
			{
				ShiftRight(ref a, -b);
				return;
			}

			if (b == 0)
				return;

			if (b < 64)
			{
				a.hi = (a.hi << b) | (a.lo >> (64 - b));
				a.lo <<= b;
				return;
			}

			if (b == 64)
			{
				a.hi = a.lo;
				a.lo = 0;
				return;
			}

			if (b < 128)
			{
				a.hi = a.lo << b - 64;
				a.lo = 0;
				return;
			}

			a.hi = a.lo = 0;
		}

		public static UInt128 operator <<(UInt128 a, int b)
		{
			ShiftLeft(ref a, b);
			return a;
		}

		public void ShiftLeft()
		{
			hi = (hi << 1) | (lo >> 63);
			lo <<= 1;
		}

		public void ShiftLeft(int count)
		{
			ShiftLeft(ref this, count);
		}

		public void ShiftRight()
		{
			lo = (lo >> 1) | (hi << 63);
			hi >>= 1;
		}

		public void ShiftRight(int count)
		{
			ShiftRight(ref this, count);
		}

		public void ShiftRightWithMsb()
		{
			lo = (lo >> 1) | (hi << 63);
			hi = (hi >> 1) | msb;
		}

		public bool IsMsbSet()
		{
			return (hi & msb) != 0;
		}

		public bool IsZero()
		{
			return hi == 0 && lo == 0;
		}

		private int FindHighBit(ulong u)
		{
			Debug.Assert(u != 0);

			int count = 0;
			while ((u & msb) == 0)
			{
				count++;
				u <<= 1;
			}
			return count;
		}

		public int Normalize()
		{
			if (hi == 0 && lo == 0)
				throw new ArithmeticException();

			int count;
			if (hi != 0)
				count = FindHighBit(hi);
			else
				count = 64 + FindHighBit(lo);

			ShiftLeft(ref this, count);
			return count;
		}

		public static UInt128 operator &(UInt128 a, UInt128 b)
		{
			a.hi &= b.hi;
			a.lo &= b.lo;
			return a;
		}

		public static UInt128 operator |(UInt128 a, UInt128 b)
		{
			a.hi |= b.hi;
			a.lo |= b.lo;
			return a;
		}

		public static UInt128 operator ^(UInt128 a, UInt128 b)
		{
			a.hi ^= b.hi;
			a.lo ^= b.lo;
			return a;
		}

		public static UInt128 operator ~(UInt128 a)
		{
			a.hi = ~a.hi;
			a.lo = ~a.lo;
			return a;
		}
	}
}
