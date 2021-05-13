/*
 * CoreCommands.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This class implements basic mathematical functionality of the
 * calculator. Operations include addition, subtraction,
 * multiplication, division, roots and powers, trigonometric
 * functions including inverse and hyperbolic, various forms
 * of rounding, manipulation of real and imaginary parts of
 * complex numbers, factorial and double factorial, logarithms
 * and exponents, and some special functions including Gamma and AGM.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Mulvey.RpnCalculator
{
	[CommandContainer]
	public class CoreCommands
	{
		CalculatorEngine engine;

		public CoreCommands(CalculatorEngine engine)
		{
			this.engine = engine;
		}

		[Command]
		public void Add()
		{
			engine.BinaryOperation(delegate(Number a, Number b)
			{
				return a + b;
			}
			);
		}

		[Command]
		public void Subtract()
		{
			engine.BinaryOperation(delegate(Number a, Number b)
			{
				return a - b;
			}
			);
		}

		[Command]
		public void Multiply()
		{
			engine.BinaryOperation(delegate(Number a, Number b)
			{
				return a * b;
			}
			);
		}

		[Command]
		public void Divide()
		{
			engine.BinaryOperation(delegate(Number a, Number b)
			{
				return a / b;
			}
			);
		}

		[Command]
		public void SquareRoot()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Sqrt();
			}
			);
		}

		[Command]
		public void AGM()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.AGM();
			}
			);
		}

		[Command]
		public void Square()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x * x;
			}
			);
		}

		[Command]
		public void Exp(Number b)
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return Number.Pow(b, x);
			}
			);
		}

		[Command]
		public void Log(Number b)
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Log(b);
			}
			);
		}

		[Command]
		public void LogX()
		{
			engine.BinaryOperation(delegate(Number a, Number b)
			{
				return a.Log(b);
			}
			);
		}

		[Command]
		public void Sin()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Sin();
			}
			);
		}

		[Command]
		public void Sinh()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Sinh();
			}
			);
		}

		[Command]
		public void Asin()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Asin();
			}
			);
		}

		[Command]
		public void Asinh()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Asinh();
			}
			);
		}

		[Command]
		public void Cos()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Cos();
			}
			);
		}

		[Command]
		public void Cosh()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Cosh();
			}
			);
		}

		[Command]
		public void Acos()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Acos();
			}
			);
		}

		[Command]
		public void Acosh()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Acosh();
			}
			);
		}

		[Command]
		public void Tan()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Tan();
			}
			);
		}

		[Command]
		public void Tanh()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Tanh();
			}
			);
		}

		[Command]
		public void Atan()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Atan();
			}
			);
		}

		[Command]
		public void Atanh()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Atanh();
			}
			);
		}

		[Command]
		public void Power()
		{
			engine.BinaryOperation(delegate(Number a, Number b)
			{
				return a.Pow(b);
			}
			);
		}

		[Command]
		public void Root()
		{
			engine.BinaryOperation(delegate(Number a, Number b)
			{
				return a.Pow(1 / b);
			}
			);
		}

		[Command]
		public void Reciprocal()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return 1 / x;
			}
			);
		}

		[Command]
		public void Sign()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Sign();
			}
			);
		}

		[Command]
		public void Abs()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Abs();
			}
			);
		}

		[Command]
		public void RealPart()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Re;
			}
			);
		}

		[Command]
		public void ImaginaryPart()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Im;
			}
			);
		}

		[Command]
		public void Frac()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Frac();
			}
			);
		}

		[Command]
		public void Arg()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Arg();
			}
			);
		}

		[Command]
		public void Cis()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Cis();
			}
			);
		}

		[Command]
		public void Floor()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Floor();
			}
			);
		}

		[Command]
		public void Ceiling()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Ceiling();
			}
			);
		}

		[Command]
		public void Truncate()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Truncate();
			}
			);
		}

		[Command]
		public void Round()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Round();
			}
			);
		}

		[Command]
		public void Mod()
		{
			engine.BinaryOperation(delegate(Number a, Number b)
			{
				return a % b;
			}
			);
		}

		[Command]
		public void Gamma()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Gamma();
			}
			);
		}

		[Command]
		public void LogGamma()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.LogGamma();
			}
			);
		}

		[Command]
		public void Factorial()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.Factorial();
			}
			);
		}

		[Command]
		public void DoubleFactorial()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				return x.DoubleFactorial();
			}
			);
		}

		[Command]
		public void Binomial()
		{
			engine.BinaryOperation(delegate(Number n, Number r)
			{
				return Number.Binomial(n, r);
			}
			);
		}

		[Command]
		public void Permutations()
		{
			engine.BinaryOperation(delegate(Number n, Number r)
			{
				return Number.Binomial(n, r) * Number.Gamma(r + 1);
			}
			);
		}
	}
}
