/*
 * BinaryCommands.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This class implements binary operations including AND, OR,
 * XOR, and NOT, and left- and right-shifts and rotations.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Mulvey.RpnCalculator
{
	[CommandContainer]
	public class BinaryCommands
	{
		CalculatorEngine engine;

		private struct SignedBinary
		{
			public bool Negative;
			public ulong Bits;

			public SignedBinary(bool negative, ulong bits)
			{
				Negative = negative;
				Bits = bits;
			}

			public SignedBinary(Number a)
			{
				Negative = a.Re.IsNegative();
				Bits = (ulong)a.Re.Abs();
			}
		}

		public BinaryCommands(CalculatorEngine engine)
		{
			this.engine = engine;
		}

		private delegate SignedBinary SignedBinaryOp(SignedBinary a, SignedBinary b);

		private void BinaryOperation(SignedBinaryOp op)
		{
			engine.BinaryOperation(delegate(Number a, Number b)
			{
				if (engine.WordSize == 0)
					return Number.NaN;

				SignedBinary s = op(new SignedBinary(a), new SignedBinary(b));
				Number n = new Number(s.Bits);
				if (s.Negative)
					n.Negate();
				return n;
			}
			);
		}

		[Command]
		public void Or()
		{
			BinaryOperation(delegate(SignedBinary a, SignedBinary b)
			{
				return new SignedBinary(a.Negative || b.Negative, a.Bits | b.Bits);
			}
			);
		}

		[Command]
		public void And()
		{
			BinaryOperation(delegate(SignedBinary a, SignedBinary b)
			{
				return new SignedBinary(a.Negative && b.Negative, a.Bits & b.Bits);
			}
			);
		}

		[Command]
		public void Xor()
		{
			BinaryOperation(delegate(SignedBinary a, SignedBinary b)
			{
				return new SignedBinary(a.Negative ^ b.Negative, a.Bits ^ b.Bits);
			}
			);
		}

		[Command]
		public void Not()
		{
			if (engine.WordSize == 0)
			{
				engine.Push(Number.NaN);
				return;
			}

			engine.UnaryOperation(delegate(Number x)
			{
				ulong xi = (ulong)x.Re.Abs();
				return (new Number(~xi)) * x.Sign();
			}
			);
		}

		[Command]
		public void TwosComplement()
		{
			if (engine.WordSize == 0)
			{
				engine.Push(Number.NaN);
				return;
			}

			engine.UnaryOperation(delegate(Number x)
			{
				ulong xi = (ulong)x.Re.Abs();
				return (new Number((~xi) + 1)) * x.Sign();
			}
			);
		}

		[Command]
		public void ShiftLeft()
		{
			BinaryOperation(delegate(SignedBinary a, SignedBinary b)
			{
				return new SignedBinary(a.Negative, a.Bits << (int)(b.Bits & 63) * (b.Negative ? -1 : 1));
			}
			);
		}

		[Command]
		public void ShiftRight()
		{
			BinaryOperation(delegate(SignedBinary a, SignedBinary b)
			{
				return new SignedBinary(a.Negative, a.Bits >> (int)(b.Bits & 63) * (b.Negative ? -1 : 1));
			}
			);
		}

		private ulong RotateLeft(ulong a, int b)
		{
			return (a << b) | (a >> (engine.WordSize - b));
		}

		private ulong RotateRight(ulong a, int b)
		{
			return (a >> b) | (a << (engine.WordSize - b));
		}

		[Command]
		public void RotateLeft()
		{
			BinaryOperation(delegate(SignedBinary a, SignedBinary b)
			{
				int shift = (int)(b.Bits & 63) * (b.Negative ? -1 : 1);
				if (shift == 0)
					return a;
				if (shift > 0)
					return new SignedBinary(a.Negative, RotateLeft(a.Bits, shift));
				return new SignedBinary(a.Negative, RotateRight(a.Bits, -shift));
			}
			);
		}

		[Command]
		public void RotateRight()
		{
			BinaryOperation(delegate(SignedBinary a, SignedBinary b)
			{
				int shift = (int)(b.Bits & 63) * (b.Negative ? -1 : 1);
				if (shift == 0)
					return a;
				if (shift > 0)
					return new SignedBinary(a.Negative, RotateRight(a.Bits, shift));
				return new SignedBinary(a.Negative, RotateLeft(a.Bits, -shift));
			}
			);
		}
	}
}
