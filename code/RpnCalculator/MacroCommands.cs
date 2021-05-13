/*
 * MacroCommands.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This class implements the macro functionality of the calculator,
 * including storing and retreiving numbers from memory, and recording,
 * storing, and replaying command sequences.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Mulvey.RpnCalculator
{
	[CommandContainer]
	public class MacroCommands
	{
		CalculatorEngine engine;

		Number[] fn = new Number[24];
		List<EventArgs> macro;
		List<EventArgs>[] fnMacro = new List<EventArgs>[24];

		public MacroCommands(CalculatorEngine engine)
		{
			this.engine = engine;
			engine.EventSubmitted += new EventHandler(engine_EventSubmitted);
		}

		void engine_EventSubmitted(object sender, EventArgs e)
		{
			if (macro != null)
				macro.Add(e);
		}

		[Command]
		public void Recall(int n)
		{
			if (n < 0 || n >= fn.Length)
				return;

			if (fn[n] != null)
			{
				engine.Push(fn[n]);
			}
			else if (fnMacro[n] != null)
			{
				engine.StealthMode(delegate()
				{
					engine.StopEditing();
					foreach (EventArgs e in fnMacro[n])
					{
						engine.EmitEvent(e);
					}
				});
			}
			else
			{
				engine.Push(0);
			}
		}

		[Command]
		public void Store(int n)
		{
			if (n < 0 || n >= fn.Length)
				return;

			fn[n] = engine.X;
			fnMacro[n] = null;
		}

		[Command]
		public void Assign(int n)
		{
			if (n < 0 || n >= fn.Length)
				return;

			fnMacro[n] = macro;
			fn[n] = null;
			macro = null;
		}

		[Command]
		public void Exchange(int n)
		{
			if (n < 0 || n >= fn.Length)
				return;

			Number x = engine.Pop();
			if (fn[n] != null)
				engine.Push(fn[n]);
			else
				engine.Push(0);
			fn[n] = x;
		}

		[Command]
		public void Record()
		{
			Command.CancelSubmit = true;
			if (macro == null)
				macro = new List<EventArgs>();
			else
				macro = null;
		}

		[Command]
		public void ClearStored()
		{
			fn = new Number[fn.Length];
			macro = null;
			fnMacro = new List<EventArgs>[fnMacro.Length];
		}
	}
}
