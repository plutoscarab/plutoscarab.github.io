/*
 * AdminCommands.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This class implements some miscellaneous administrative commands
 * including Nop (no operation), changing the number system (between
 * real and complex), change the number base and precision, basic
 * stack manipulation, and clearing stored information.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Mulvey.QuadPrecision;

namespace Mulvey.RpnCalculator
{
	[CommandContainer]
	public class AdminCommands
	{
		CalculatorEngine engine;

		public AdminCommands(CalculatorEngine engine)
		{
			this.engine = engine;
		}

		[Command]
		public void Nop()
		{
		}

		[Command]
		public void ChangeSystem()
		{
			switch ((int) engine.NumberSystem)
			{
				case 0:
					NumberSystem(1);
					break;
				case 1:
					NumberSystem(0);
					break;
				default:
					NumberSystem(1);
					break;
			}
		}

		[Command]
		public void NumberBase(int b)
		{
			engine.NumberBase = b;
		}

		[Command]
		public void NumberSystem(int n)
		{
			engine.NumberSystem = (NumberSystem)n;
		}

		[Command]
		public void WordSize(int w)
		{
			engine.WordSize = w;
		}

		[Command]
		public void ClearStack()
		{
			engine.ClearStack();
		}

		[Command]
		public void Constant(Number d)
		{
			engine.Push(d);
		}

		[Command]
		public void Pop()
		{
			engine.Pop();
		}

		[Command]
		public void ExchangeXY()
		{
			Number x = engine.Pop();
			Number y = engine.Pop();
			engine.Push(x);
			engine.Push(y);
		}

		[Command]
		public void ClearAll()
		{
			ClearStack();
			Commands.Execute("ClearStored", -1);
			Commands.Execute("ClearStats", -1);
		}
	}
}
