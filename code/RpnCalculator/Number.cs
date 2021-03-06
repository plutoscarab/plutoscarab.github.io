/*
 * Number.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This is the complex number class. It's just called Number because
 * the calculator uses this class for all operations even when it's
 * in Real or Binary mode.
 * 
 * Real and imaginary components are represented using quad-precision
 * floating-point numbers implemented in Real.cs.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using Mulvey.QuadPrecision;

namespace Mulvey.RpnCalculator
{
	/// <summary>
	/// A quad-precision complex number.
	/// </summary>
	public class Number
	{
		public Real Re;
		public Real Im;

		public static Number Zero = new Number(0);
		public static Number One = new Number(1);
		public static Number OneHalf = new Number(0.5);

		/// <summary>
		/// Create a number equal to zero.
		/// </summary>
		public Number()
			: this((Real) 0)
		{
		}

		/// <summary>
		/// Create a number with an imaginary part of zero.
		/// </summary>
		/// <param name="d">The real part.</param>
		public Number(Real d)
			: this(d, 0.0)
		{
		}
		
		/// <summary>
		/// Create a number with specified real and imaginary parts.
		/// </summary>
		public Number(Real re, Real im)
		{
			Re = re;
			Im = im;
		}

		/// <summary>
		/// Create a copy of another number.
		/// </summary>
		public Number(Number c)
		{
			Re = c.Re;
			Im = c.Im;
		}

		/// <summary>
		/// Create a real number based on a base-10 string representation
		/// of the number. "^" character is used for the power-of-10 exponent.
		/// </summary>
		public Number(string s)
		{
			Re = new Real(s);
			Im = 0;
		}

		/// <summary>
		/// Not a number.
		/// </summary>
		public static Number NaN
		{
			get
			{
				return new Number(Real.NaN, Real.NaN);
			}
		}

		/// <summary>
		/// Square root of -1
		/// </summary>
		public static Number I
		{
			get
			{
				return new Number(0, 1);
			}
		}

		/// <summary>
		/// Covert from a UInt64 value.
		/// </summary>
		public static implicit operator Number(ulong u)
		{
			if (u == 0)
				return Number.Zero;

			return new Real(false, 64, u, 0);
		}

		/// <summary>
		/// Convert from a double-precision floating-point value.
		/// </summary>
		public static implicit operator Number(double d)
		{
			return new Number(d);
		}

		/// <summary>
		/// Convert from a quad-precision floating-point value.
		/// </summary>
		public static implicit operator Number(Real r)
		{
			return new Number(r);
		}

		/// <summary>
		/// Check if a number is not-a-number. Returns true if
		/// either the real or imaginary parts are NaN.
		/// </summary>
		public static bool IsNaN(Number n)
		{
			return Real.IsNaN(n.Re) || Real.IsNaN(n.Im);
		}

		/// <summary>
		/// Check if either the real or imaginary part is
		/// infinite.
		/// </summary>
		public static bool IsInfinity(Number n)
		{
			return Real.IsInfinity(n.Re) || Real.IsInfinity(n.Im);
		}

		private static ulong NegateInt(ulong i)
		{
			return unchecked((i ^ 0xFFFFFFFFFFFFFFFF) + 1L);
		}

		/// <summary>
		/// Change the sign of the real and imaginary parts of the number.
		/// </summary>
		public void Negate()
		{
			Re *= -1;
			Im *= -1;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Number)) return false;
			return this == (Number)obj;
		}

		public override int GetHashCode()
		{
			return Re.GetHashCode() * Im.GetHashCode();
		}

		public static bool operator ==(Number a, Number b)
		{
			if ((object)a == null && (object)b == null) return true;
			if ((object)a == null || (object)b == null) return false;
			return a.Re == b.Re && a.Im == b.Im;
		}

		public static bool operator !=(Number a, Number b)
		{
			if ((object)a == null && (object)b == null) return false;
			if ((object)a == null || (object)b == null) return true;
			return a.Re != b.Re && a.Im != b.Im;
		}

		public static bool operator >(Number a, Number b)
		{
			if ((object)a == null || (object)b == null) return false;

			if (a.Im != Real.Zero || b.Im != Real.Zero)
				return false;

			return a.Re > b.Re;
		}

		public static bool operator <(Number a, Number b)
		{
			if ((object)a == null || (object)b == null) return false;

			if (a.Im != Real.Zero || b.Im != Real.Zero)
				return false;

			return a.Re < b.Re;
		}

		public static bool operator >=(Number a, Number b)
		{
			if ((object)a == null || (object)b == null) return false;
			return !(a < b);
		}

		public static bool operator <=(Number a, Number b)
		{
			if ((object)a == null || (object)b == null) return false;
			return !(a > b);
		}

		public static Number operator +(Number a, Number b)
		{
			return new Number(a.Re + b.Re, a.Im + b.Im);
		}

		public static Number operator -(Number a, Number b)
		{
			return new Number(a.Re - b.Re, a.Im - b.Im);
		}

		public static Number operator -(Number x)
		{
			return new Number(-x.Re, -x.Im);
		}

		public static Number operator *(Number a, Number b)
		{
			return new Number(
				a.Re * b.Re - a.Im * b.Im, 
				a.Re * b.Im + a.Im * b.Re);
		}

		public static Number operator /(Number a, Number b)
		{
			if (b.Re.Abs() >= b.Im.Abs())
			{
				Real dc = b.Im / b.Re;
				Real r = b.Re + b.Im * dc;
				return new Number(
					(a.Re + a.Im * dc) / r,
					(a.Im - a.Re * dc) / r);
			}
			else
			{
				Real cd = b.Re / b.Im;
				Real r = b.Re * cd + b.Im;
				return new Number(
					(a.Re * cd + a.Im) / r,
					(a.Im * cd - a.Re) / r);
			}
		}

		/// <summary>
		/// Calculate the square root of the number.
		/// </summary>
		public Number Sqrt()
		{
			Real d = Real.Sqrt(Re * Re + Im * Im);
			Real re, im;
			if (Re >= Real.Zero)
			{
				re = Real.Sqrt((d + Re) / 2);
				im = Im == Real.Zero ? Real.Zero : Im / Real.Two * Real.Sqrt(Real.Two / (d + Re));
			}
			else if (Im >= Real.Zero)
			{
				re = Real.Abs(Im) / Real.Two * Real.Sqrt(Real.Two / (d - Re));
				im = Real.Sqrt((d - Re) / Real.Two);
			}
			else
			{
				re = Real.Abs(Im) / Real.Two * Real.Sqrt(Real.Two / (d - Re));
				im = -Real.Sqrt((d - Re) / Real.Two);
			}
			return new Number(re, im);
		}

		/// <summary>
		/// Calculate the square root of a number.
		/// </summary>
		public static Number Sqrt(Number x)
		{
			return x.Sqrt();
		}

		/// <summary>
		/// Calculate the arithmetic-geometric mean of the number and 1.
		/// See http://mathworld.wolfram.com/Arithmetic-GeometricMean.html
		/// </summary>
		/// <returns></returns>
		public Number AGM()
		{
			if (IsNaN(this))
				return NaN;

			if (this == Number.Zero)
				return Number.Zero;

			Number a = Number.One;
			Number b = new Number(this);

			Real e = new Real("1^-36");
			int i = 20;
			while (i-- > 0)
			{
				Number c = (a + b) / Real.Two;
				if ((Abs(a - c) / Abs(c)).Re < e) break;
				b = Sqrt(a * b);
				a = c;
			}

			return a;
		}

		/// <summary>
		/// Calculate the arithmetic-geometric mean of a number and 1.
		/// See http://mathworld.wolfram.com/Arithmetic-GeometricMean.html
		/// </summary>
		/// <returns></returns>
		public static Number AGM(Number x)
		{
			return x.AGM();
		}

		/// <summary>
		/// Calculate e raised to the power of a number.
		/// </summary>
		public static Number Exp(Number x)
		{
			if (x.Im == Real.Zero)
				return Real.Exp(x.Re);

			return Real.Exp(x.Re) * Cis(x.Im);
		}

		/// <summary>
		/// Calculate the logarithm of a number using a
		/// specified number base.
		/// </summary>
		public Number Log(Number logarithmBase)
		{
			return Ln(this) / Ln(logarithmBase);
		}

		/// <summary>
		/// Calculate the logarithm of the number using
		/// a specified real-valued base. 
		/// </summary>
		public Number Log(Real logarithmBase)
		{
			logarithmBase = Real.Log(logarithmBase);
			Real rr = Re * Re;
			Real ii = Im * Im;
			Real re;
			if (rr > 512 * ii)
			{
				re = Real.Log(Real.Abs(Re)) + Real.Log(1 + ii / rr) / Real.Two;
			}
			else
			{
				re = Real.Log(Real.Abs(Im)) + Real.Log(1 + rr / ii) / Real.Two;
			}
			Real im = Real.Atan2(Im, Re);
			return new Number(re / logarithmBase, im / logarithmBase);
		}

		/// <summary>
		/// Calculate the natural logarithm of the number.
		/// </summary>
		public Number Ln()        
		{
			return Log(Real.E);
		}

		/// <summary>
		/// Calculate the natural logarithm of a number.
		/// </summary>
		public static Number Ln(Number x)
		{
			return x.Log(Real.E);
		}

		/// <summary>
		/// Calculate the sine of a number.
		/// </summary>
		public static Number Sin(Number x)
		{
			return x.Sin();
		}

		/// <summary>
		/// Calculate the sine of the number.
		/// </summary>
		public Number Sin()
		{
			if (Im == Real.Zero)
				return Real.Sin(Re);
			Real eb = Real.Exp(Im);
			Real emb = Real.One / eb;
			Real re = (eb + emb) / Real.Two * Real.Sin(Re);
			Real im = (eb - emb) / Real.Two * Real.Cos(Re);
			return new Number(re, im);
		}

		/// <summary>
		/// Calculate the inverse sine of a number.
		/// </summary>
		public static Number Asin(Number x)
		{
			return x.Asin();
		}

		/// <summary>
		/// Calculate the inverse sine of the number.
		/// </summary>
		public Number Asin()
		{
			if (Im == Real.Zero)
				return Real.Asin(Re);
			return -I * Ln(I * Re - Im + Sqrt(Real.One - Re * Re + Im * Im - Real.Two * I * Re * Im));
		}

		/// <summary>
		/// Calculate the hyperbolic sine of a number.
		/// </summary>
		public static Number Sinh(Number x)
		{
			return x.Sinh();
		}

		/// <summary>
		/// Calculate the hyperbolic sine of the number.
		/// </summary>
		public Number Sinh()
		{
			if (Im == Real.Zero)
				return Real.Sinh(Re);
			return I * Sin(Im - I * Re);
		}

		/// <summary>
		/// Calculate the inverse hyperbolic sine of a number.
		/// </summary>
		public static Number Asinh(Number x)
		{
			return x.Asinh();
		}

		/// <summary>
		/// Calculate the inverse hyperbolic sine of the number.
		/// </summary>
		public Number Asinh()
		{
			if (Im == Real.Zero)
				return Real.Log(Re + Real.Sqrt(Real.One + Re * Re));
			return Ln(Re + I * Im + Sqrt(Real.One + Re * Re - Im * Im + Real.Two * I * Re * Im));
		}

		/// <summary>
		/// Calculate the cosine of a number.
		/// </summary>
		public static Number Cos(Number x)
		{
			return x.Cos();
		}

		/// <summary>
		/// Calculate the cosine of the number.
		/// </summary>
		public Number Cos()
		{
			if (Im == Real.Zero)
				return Real.Cos(Re);
			Real eb = Real.Exp(Im);
			Real emb = Real.One / eb;
			Real re = (emb + eb) / Real.Two * Real.Cos(Re);
			Real im = (emb - eb) / Real.Two * Real.Sin(Re);
			return new Number(re, im);
		}

		/// <summary>
		/// Calculate the inverse cosine of a number.
		/// </summary>
		public static Number Acos(Number x)
		{
			return x.Acos();
		}

		/// <summary>
		/// Calculate the inverse cosine of the number.
		/// </summary>
		public Number Acos()
		{
			if (Im == Real.Zero)
				return Real.Acos(Re);
			return -I * Ln(Re + I * Im + Sqrt(Re * Re - Im * Im - Real.One + Real.Two * I * Re * Im));
		}

		/// <summary>
		/// Calculate the hyperbolic cosine of a number.
		/// </summary>
		public static Number Cosh(Number x)
		{
			return x.Cosh();
		}

		/// <summary>
		/// Calculate the hyperbolic cosine of the number.
		/// </summary>
		public Number Cosh()
		{
			if (Im == Real.Zero)
				return Real.Cosh(Re);
			return Cos(Im - I * Re);
		}

		/// <summary>
		/// Calculate the inverse hyperbolic cosine of a number.
		/// </summary>
		public static Number Acosh(Number x)
		{
			return x.Acosh();
		}

		/// <summary>
		/// Calculate the inverse hyperbolic cosine of the number.
		/// </summary>
		public Number Acosh()
		{
			if (Im == Real.Zero)
				return Real.Log(Re + Real.Sqrt(Re * Re - 1));
			return Ln(Re + I * Im + Sqrt(Re * Re - Im * Im - Real.One + Real.Two * I * Re * Im));
		}

		/// <summary>
		/// Calculate the tangent of a number.
		/// </summary>
		public static Number Tan(Number x)
		{
			return x.Tan();
		}

		/// <summary>
		/// Calculate the tangent of the number.
		/// </summary>
		public Number Tan()
		{
			if (Im == Real.Zero)
				return Real.Tan(Re);
			return Sin() / Cos();
		}

		/// <summary>
		/// Calculate the inverse tangent of a number.
		/// </summary>
		public static Number Atan(Number x)
		{
			return x.Atan();
		}

		/// <summary>
		/// Calculate the inverse tangent of the number.
		/// </summary>
		public Number Atan()
		{
			if (Im == Real.Zero)
				return Real.Atan(Re);
			Real a2 = Re * Re;
			Real b2 = Im * Im;
			return -I / Real.Two * Ln((Real.One - a2 - b2 + Real.Two * I * Re) / (Real.One + a2 + b2 + Real.Two * Im));
		}

		/// <summary>
		/// Calculate the hyperbolic tangent of a number.
		/// </summary>
		public static Number Tanh(Number x)
		{
			return x.Tanh();
		}

		/// <summary>
		/// Calculate the hyperbolic tangent of the number.
		/// </summary>
		public Number Tanh()
		{
			if (Im == Real.Zero)
				return Real.Tanh(Re);
			return Sinh() / Cosh();
		}

		/// <summary>
		/// Calculate the inverse hyperbolic tangent of a number.
		/// </summary>
		public static Number Atanh(Number x)
		{
			return x.Atanh();
		}

		/// <summary>
		/// Calculate the inverse hyperbolic tangent of the number.
		/// </summary>
		public Number Atanh()
		{
			if (Im == Real.Zero)
				return Real.Log((Real.One + Re) / (Real.One - Re)) / Real.Two;
			Real a2 = Re * Re;
			Real b2 = Im * Im;
			return -Real.OneHalf * Ln((Real.OneHalf - a2 - b2 - Real.Two * I * Im) / (Real.One + a2 + b2 + Real.Two * Re));
		}

		/// <summary>
		/// Raise the number to a power.
		/// </summary>
		public Number Pow(Number exponent)
		{
			Number x = exponent;
			if (x == Zero)
				return One;
			if (this == Zero)
				return Zero;

			Real re, im;
			if (Im == Real.Zero && x.Im == Real.Zero && (Re >= Real.Zero || x.Re == Real.Round(x.Re)))
			{
				re = Real.Pow(Re, x.Re);
				im = Real.Zero;
			}
			else 
			{
				Number n = Exp(x * Ln());
				re = n.Re;
				im = n.Im;
			}
			return new Number(re, im);
		}

		/// <summary>
		/// Calculate a number raised to the power of another number.
		/// </summary>
		public static Number Pow(Number @base, Number exponent)
		{
			return @base.Pow(exponent);
		}

		/// <summary>
		/// Return a number with real and imaginary parts equal to the
		/// sign (+1, 0, or -1) of the corresponding parts of this number.
		/// </summary>
		public static Number Sign(Number x)
		{
			return x.Sign();
		}

		/// <summary>
		/// Return a number with real and imaginary parts equal to the
		/// sign (+1, 0, or -1) of the corresponding parts of this number.
		/// </summary>
		public Number Sign()
		{
			Number n = new Number(this);
			n.Re = Real.Sign(n.Re);
			n.Im = Real.Sign(n.Im);
			return n;
		}

		/// <summary>
		/// Calculate the absolute magnitude of a number.
		/// </summary>
		public static Number Abs(Number n)
		{
			return n.Abs();
		}

		/// <summary>
		/// Calculate the absolute magnitude of the number.
		/// </summary>
		public Number Abs()
		{
			// n.Re = Real.Sqrt(n.Re * n.Re + n.Im * n.Im);

			Number n = new Number(this);
			if (Im == Real.Zero)
				n.Re = Real.Abs(n.Re);
			else
			{
				Real A = n.Re.Abs();
				Real B = n.Im.Abs();
				if (A >= B)
				{
					Real d = B / A;
					n.Re = A * Real.Sqrt(Real.One + d * d);
				}
				else
				{
					Real d = A / B;
					n.Re = B * Real.Sqrt(Real.One + d * d);
				}
			}
			n.Im = Real.Zero;
			return n;
		}

		/// <summary>
		/// Calculate the fractional part of a number. The real
		/// and imaginary parts are considered separately.
		/// </summary>
		/// <returns>
		/// Returns values in the range [0,1) for non-negative values
		/// and (-1,0] for negative values.
		/// </returns>
		public static Number Frac(Number x)
		{
			return x.Frac();
		}

		/// <summary>
		/// Calculate the fractional part of the number. The real
		/// and imaginary parts are considered separately.
		/// </summary>
		/// <returns>
		/// Returns values in the range [0,1) for non-negative values
		/// and (-1,0] for negative values.
		/// </returns>
		public Number Frac()
		{
			Number n = new Number(this);
			n.Re -= Real.Truncate(n.Re);
			n.Im -= Real.Truncate(n.Im);
			return n;
		}

		/// <summary>
		/// Calculates the angle in the complex plane between the
		/// positive real axis and the ray from the origin to the
		/// specified number.
		/// </summary>
		public static Number Arg(Number x)
		{
			return x.Arg();
		}

		/// <summary>
		/// Calculates the angle in the complex plane between the
		/// positive real axis and the ray from the origin to the
		/// number.
		/// </summary>
		public Number Arg()
		{
			Number n = new Number(this);
			n.Re = Real.Atan2(n.Im, n.Re);
			n.Im = Real.Zero;
			return n;
		}

		/// <summary>
		/// Calculates the complex exponential of a number.
		/// </summary>
		public static Number Cis(Number x)
		{
			return x.Cis();
		}

		/// <summary>
		/// Calculates the complex exponential of the number.
		/// </summary>
		public Number Cis()
		{
			return Cos(this) + I * Sin(this);
		}

		///// <summary>
		///// Calculate the binary OR of two numbers. The binary
		///// representation of the real and imaginary components
		///// are considered separately, and the OR operation is
		///// performed after the radix points are aligned.
		///// The result is negative if either number is negative.
		///// </summary>
		//public static Number operator |(Number a, Number b)
		//{
		//    return new Number(a.Re | b.Re, a.Im | b.Im);
		//}

		///// <summary>
		///// Calculate the binary AND of two numbers. The binary
		///// representation of the real and imaginary components
		///// are considered separately, and the AND operation is
		///// performed after the radix points are aligned.
		///// The result is negative if both numbers are negative.
		///// </summary>
		//public static Number operator &(Number a, Number b)
		//{
		//    return new Number(a.Re & b.Re, a.Im & b.Im);
		//}

		///// <summary>
		///// Calculate the binary XOR of two numbers. The binary
		///// representation of the real and imaginary components
		///// are considered separately, and the XOR operation is
		///// performed after the radix points are aligned.
		///// The result is negative if the two values have
		///// opposite signs.
		///// </summary>
		//public static Number operator ^(Number a, Number b)
		//{
		//    return new Number(a.Re ^ b.Re, a.Im ^ b.Im);
		//}

		/// <summary>
		/// Calculate the greatest integer that is less than
		/// or equal to a given number. Real and imaginary components
		/// are considered separately.
		/// </summary>
		public static Number Floor(Number x)
		{
			return x.Floor();
		}

		/// <summary>
		/// Calculate the greatest integer that is less than
		/// or equal to the number. Real and imaginary components
		/// are considered separately.
		/// </summary>
		public Number Floor()
		{
			return new Number(Real.Floor(Re), Real.Floor(Im));
		}

		/// <summary>
		/// Calculate the smallest integer that is greater than
		/// or equal to a given number. Real and imaginary components
		/// are considered separately.
		/// </summary>
		public static Number Ceiling(Number x)
		{
			return x.Ceiling();
		}

		/// <summary>
		/// Calculate the smallest integer that is greater than
		/// or equal to the number. Real and imaginary components
		/// are considered separately.
		/// </summary>
		public Number Ceiling()
		{
			return new Number(Real.Ceiling(Re), Real.Ceiling(Im));
		}

		/// <summary>
		/// Calculate the integer (non-fractional) portion of a
		/// number. Real and imaginary components are considered
		/// separately.
		/// </summary>
		public static Number Truncate(Number x)
		{
			return x.Truncate();
		}

		/// <summary>
		/// Calculate the integer (non-fractional) portion of the
		/// number. Real and imaginary components are considered
		/// separately.
		/// </summary>
		public Number Truncate()
		{
			return new Number(Real.Truncate(Re), Real.Truncate(Im));
		}

		/// <summary>
		/// Round a number to the nearest integer. Numbers half-way
		/// between integers are rounded to the nearest even integer.
		/// Real and imaginary components are considered separately.
		/// </summary>
		public static Number Round(Number x)
		{
			return x.Round();
		}

		/// <summary>
		/// Round the number to the nearest integer. Numbers half-way
		/// between integers are rounded to the nearest even integer.
		/// Real and imaginary components are considered separately.
		/// </summary>
		public Number Round()
		{
			return new Number(Real.Round(Re), Real.Round(Im));
		}

		/// <summary>
		/// Calculate the remainder when one number is divided by
		/// another number.
		/// </summary>
		/// <returns>Returns a - b * Truncate(a / b)</returns>
		public static Number operator %(Number a, Number b)
		{
			return a - b * (a / b).Truncate();
		}

		public static Real E = Real.E;
		public static Real Pi = Real.Pi;
		public static Real TwoPi = Real.TwoPi;

		const int factorialMax = 3209;
		Number[] factorialTable;

		/// <summary>
		/// Return the factorial of a real integer. For complex or
		/// non-integer arguments, use Gamma(n + 1) instead.
		/// </summary>
		public Number Factorial()
		{
			if (Im != Real.Zero)
				return NaN;

			if (Re < Real.Zero)
				return NaN;

			Real r = Real.Round(Re);
			if (Re != r)
				return NaN;

			if (Re > factorialMax + Real.OneHalf)
				return NaN;

			if (factorialTable == null)
			{
				factorialTable = new Number[factorialMax + 1];
				Number f = factorialTable[0] = Real.One;
				for (int i = 1; i <= factorialMax; i++)
					f = factorialTable[i] = i * f;
			}

			return new Number(factorialTable[(int) r]);
		}

		/// <summary>
		/// Return the factorial of a real integer. For complex or
		/// non-integer arguments, use Gamma(n + 1) instead.
		/// </summary>
		public static Number Factorial(Number n)
		{
			return n.Factorial();
		}

		const int doubleFactorialMax = 5909;
		Number[] doubleFactorialTable;

		/// <summary>
		/// Return the double factorial of a real integer. For complex 
		/// or non-integer arguments, use Gamma(n + 1) instead.
		/// </summary>
		public Number DoubleFactorial()
		{
			if (Im != Real.Zero)
				return NaN;

			if (Re < Real.Zero)
				return NaN;

			Real r = Real.Round(Re);
			if (Re != r)
				return NaN;

			if (Re > doubleFactorialMax + Real.OneHalf)
				return NaN;

			if (doubleFactorialTable == null)
			{
				doubleFactorialTable = new Number[doubleFactorialMax + 1];
				Number even = doubleFactorialTable[0] = Real.One;
				Number odd = doubleFactorialTable[1] = Real.One;
				for (int i = 2; i <= doubleFactorialMax; i += 2)
				{
					even = doubleFactorialTable[i] = i * even;
					if (i < doubleFactorialMax)
						odd = doubleFactorialTable[i + 1] = (i + 1) * odd;
				}
			}

			return new Number(doubleFactorialTable[(int)r]);
		}

		/// <summary>
		/// Return the double factorial of a real integer. For complex 
		/// or non-integer arguments, use Gamma(n + 1) instead.
		/// </summary>
		public static Number DoubleFactorial(Number n)
		{
			return n.DoubleFactorial();
		}

		// Lanczos coefficients
		static Real p0 = new Real(false, 2, 0xA06C98FFB1382CB2, 0xBE520FD70E4D7564);
		static Real p1 = new Real(false, 30, 0xA326DDA46DCFE4A1, 0xE76554111473B031);
		static Real p2 = new Real(true, 33, 0x8281BBD0EA668635, 0x3CE767DB00F34B54);
		static Real p3 = new Real(false, 34, 0xB9C9DEA0D9B863C2, 0x8DAE04BA924CC52F);
		static Real p4 = new Real(true, 35, 0x9B399D8491486539, 0x33980B70DEE1BCEA);
		static Real p5 = new Real(false, 35, 0xA91BCCCF30BEE1C2, 0x516030844F14CFE8);
		static Real p6 = new Real(true, 34, 0xFC705AF24827B24C, 0xE08CF667D4813249);
		static Real p7 = new Real(false, 34, 0x8410838F64C1DB62, 0xFB4EC8547570E4E0);
		static Real p8 = new Real(true, 32, 0xC2EAA1E4695D4B22, 0xC1CD5F633E6A108C);
		static Real p9 = new Real(false, 30, 0xC99151248C6E1118, 0xF57EFFE1A2AB4C16);
		static Real p10 = new Real(true, 28, 0x8F59967D396ADE32, 0x22CA4662E0E811AB);
		static Real p11 = new Real(false, 25, 0x87C97C5DCCF36DB0, 0xEC607F0FF2E178EE);
		static Real p12 = new Real(true, 21, 0xA314E0FAE7573522, 0x9AEA5042F4B27016);
		static Real p13 = new Real(false, 16, 0xE6BB3310312DEF36, 0x498E0D0ADB3F704F);
		static Real p14 = new Real(true, 11, 0xAC1305F2DC28DF93, 0x421DF7D7A272F6AC);
		static Real p15 = new Real(false, 4, 0xE343649A4D050E85, 0x1A8391AB2B909A0B);
		static Real p16 = new Real(true, -4, 0xC622C284BE256785, 0xBA66AAB948534DC2);
		static Real p17 = new Real(false, -14, 0x8344CEE40816BD2E, 0xAE7BD8F4F19FD00F);
		static Real p18 = new Real(true, -29, 0x96F623F1C11A5BE8, 0xDEA18EB7256AA8BF);
		static Real p19 = new Real(false, -52, 0xF3FC8CBF561AC665, 0x747563D22D23EC75); 
		const double g = 19.375;

		/// <summary>
		/// Calculate the natural logarithm of the gamma function.
		/// </summary>
		public static Number LogGamma(Number z)
		{
			if (z.Re > 1e15)
				return NaN;

			if (IsInteger(z) && z.Re < factorialMax + 0.5)
				return Ln(Gamma(z));

			if (z.Re < Real.Zero)
				return Ln(Pi / Sin(Pi * z)) - LogGamma(Real.One - z);

			if (z.Re < Real.One)
				return LogGamma(z + Real.One) - Ln(z);

			Number s = p0 + p1 / z + p2 / (z + 1) + p3 / (z + 2) + p4 / (z + 3) +
				p5 / (z + 4) + p6 / (z + 5) + p7 / (z + 6) + p8 / (z + 7) + p9 / (z + 8) +
				p10 / (z + 9) + p11 / (z + 10) + p12 / (z + 11) + p13 / (z + 12) +
				p14 / (z + 13) + p15 / (z + 14) + p16 / (z + 15) + p17 / (z + 16) +
				p18 / (z + 17) + p19 / (z + 18);
			Number r = z + (g - 0.5);
			r =  Ln(s) + (z - 0.5) * Ln(r) - r;
			r.Re.SignificantBits = 106;
			r.Im.SignificantBits = 106;
			return r;
		}

		/// <summary>
		/// Calculate the natural logarithm of the gamma function.
		/// </summary>
		public Number LogGamma()
		{
			return LogGamma(this);
		}

		/// <summary>
		/// Calculate the gamma function.
		/// </summary>
		public static Number Gamma(Number z)
		{
			return z.Gamma();
		}

		/// <summary>
		/// Calculate the gamma function.
		/// </summary>
		public Number Gamma()
		{
			Real rr = Real.Round(Re);
			if (Im == Real.Zero && rr <= Real.Zero && Re == rr)
			{
				return NaN;
			}

			if (rr > Real.OneHalf && Re == rr)
			{
				return Factorial(Re - 1);
			}

			Number z = new Number(this);
			if (z.Re < Real.OneHalf)
			{
				return Pi / (Sin(Pi * z) * Gamma(Real.One - z));
			}

			Number s = p0 + p1 / z + p2 / (z + 1) + p3 / (z + 2) + p4 / (z + 3) +
				p5 / (z + 4) + p6 / (z + 5) + p7 / (z + 6) + p8 / (z + 7) + p9 / (z + 8) +
				p10 / (z + 9) + p11 / (z + 10) + p12 / (z + 11) + p13 / (z + 12) +
				p14 / (z + 13) + p15 / (z + 14) + p16 / (z + 15) + p17 / (z + 16) +
				p18 / (z + 17) + p19 / (z + 18);
			Number r = z + (g - 0.5);
			r = s * Pow(r, z - 0.5) / Exp(r);
			r.Re.SignificantBits = 106;
			r.Im.SignificantBits = 106;
			return r;
		}

		private static Number ErfSmallZ(Number z)
		{
			Number sum = z;
			Number term = z;
			Number zz = z * z;
			bool odd = true;
			int n = 0;
			while (true)
			{
				term *= zz / ++n;
				Number old = new Number(sum);
				if (odd)
					sum -= term / (2 * n + 1);
				else
					sum += term / (2 * n + 1);
				odd = !odd;
				if (sum == old)
					break;
			}
			return sum * 2 / Sqrt(Pi);
		}

		private static Number ErfLargeZ(Number x)
		{
			Number xx = x * x;
			Number t = Number.Zero;
			for (int k = 60; k >= 1; k--)
			{
				t = (k - Number.OneHalf) / (1 + k / (xx + t));
			}
			return (1 - Exp(-xx + Ln(x)) / (xx + t) / Sqrt(Pi)) * x.Sign();
		}

		/// <summary>
		/// Calculate the error function.
		/// </summary>
		public static Number Erf(Number z)
		{
			if (z == Number.Zero)
				return Number.Zero;

			if (z.Abs().Re < 6.5)
				return ErfSmallZ(z);

			return ErfLargeZ(z);
		}

		/// <summary>
		/// Calculate the error function.
		/// </summary>
		public Number Erf()
		{
			return Erf(this);
		}

		/// <summary>
		/// Determine whether a number is a real-valued integer.
		/// </summary>
		public static bool IsInteger(Number n)
		{
			return n.Im.IsZero() && n.Re == Real.Round(n.Re);
		}

		/// <summary>
		/// Determine whether the number is a real-valued integer.
		/// </summary>
		public bool IsInteger()
		{
			return IsInteger(this);
		}

		/// <summary>
		/// Determine whether a number is real-valued (has no imaginary component).
		/// </summary>
		public static bool IsReal(Number n)
		{
			return n.Im.IsZero();
		}

		/// <summary>
		/// Determine whether the number is real-valued (has no imaginary component).
		/// </summary>
		public bool IsReal()
		{
			return this.Im.IsZero();
		}

		/// <summary>
		/// Calculate the binomial coefficient.
		/// </summary>
		public static Number Binomial(Number n, Number r)
		{
			Number c = Exp(LogGamma(n + 1) - LogGamma(r + 1) - LogGamma(n - r + 1));
			if (IsInteger(n) && IsInteger(r))
				c = c.Round();
			return c;
		}

		internal Number Mask(int wordSize)
		{
			return new Number(this.Re.Mask(wordSize));
		}
	}
}
