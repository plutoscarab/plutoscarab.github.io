/*
 * EditCommands.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This class implements data entry operations such as typing
 * a digit, or backspace.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Mulvey.RpnCalculator
{
	[CommandContainer]
	public class EditCommands
	{
		CalculatorEngine engine;

		public EditCommands(CalculatorEngine engine)
		{
			this.engine = engine;
		}

		[Command]
		public void Digit(char ch)
		{
			const string digits = "0123456789ABCDEF";

			int d = digits.IndexOf(ch);
			if (d == -1 || d >= engine.NumberBase)
				return;

			string entry = engine.EditText;
			if (entry == "0")
			{
				entry = string.Empty;
			}
			else if (entry == "-0")
			{
				entry = "-";
			}
			else if (entry.EndsWith("^+0") || entry.EndsWith("^-0"))
			{
				entry = entry.Substring(0, entry.Length - 1);
			}

			entry += ch;
			engine.UpdateEditText(entry);
		}

		[Command]
		public void ChangeSign()
		{
			if (engine.IsEditing)
			{
				string entry = engine.EditText;
				int E = entry.IndexOf('^');
				if (E == -1)
				{
					if (entry[0] == '-')
						entry = entry.Substring(1);
					else
						entry = "-" + entry;
				}
				else
				{
					if (entry[E + 1] == '-')
						entry = entry.Substring(0, E + 1) + "+" + entry.Substring(E + 2);
					else
						entry = entry.Substring(0, E + 1) + "-" + entry.Substring(E + 2);
				}
				engine.UpdateEditText(entry);
			}
			else
			{
				Number x = engine.X;
				x.Negate();
				engine.X = x;
			}
		}

		[Command]
		public void Backspace()
		{
			string entry = engine.EditText;
			entry = entry.Substring(0, entry.Length - 1);
			if (entry.Length == 0)
				entry = "0";
			else if (entry == "-")
				entry = "-0";
			else if (entry.EndsWith("^+") || entry.EndsWith("^-"))
				entry = entry.Substring(0, entry.Length - 2);
			engine.UpdateEditText(entry);
		}

		[Command]
		public void Decimal()
		{
			if (engine.NumberSystem == NumberSystem.Binary)
				return;

			string entry = engine.EditText;
			if (entry.IndexOf('.') != -1)
				return;
			if (entry.IndexOf('^') != -1)
				return;
			entry += '.';
			engine.UpdateEditText(entry);
		}

		[Command]
		public void EnterKey()
		{
			if (!engine.StopEditing())
				engine.Push(new Number(engine.X));
		}

		[Command]
		public void ToggleImaginary()
		{
			engine.ToggleImaginary();
		}

		[Command]
		public void EnterExponent()
		{
			if (engine.NumberSystem == NumberSystem.Binary)
				return;
			string entry = engine.EditText;
			if (entry.IndexOf('^') == -1)
			{
				entry += "^+0";
				engine.UpdateEditText(entry);
			}
		}

		[Command]
		public void LastX()
		{
			engine.Push(engine.LastX);
		}
	}
}
