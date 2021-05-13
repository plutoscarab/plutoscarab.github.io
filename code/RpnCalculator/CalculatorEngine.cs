/*
 * CalculatorEngine.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This is the calculator engine, which is an abstract representation
 * of the calculator used by command implementations (plug-ins) to
 * manipulate the state of the calculator.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using Mulvey.QuadPrecision;

namespace Mulvey.RpnCalculator
{
	/// <summary>
	/// Enumeration that specifies the current display and editing format.
	/// </summary>
	public enum NumberSystem
	{
		/// <summary>
		/// Display number with floating-point real and imaginary parts.
		/// </summary>
		Complex,

		/// <summary>
		/// Display floating-point numbers with no imaginary part.
		/// </summary>
		Real,

		/// <summary>
		/// Display number as an integer.
		/// </summary>
		Binary,
	}

	/// <summary>
	/// Provides access to the internal mechanism of the calculator.
	/// Used by plug-in developers to add additional calculator operations.
	/// </summary>
	public class CalculatorEngine
	{
		static CalculatorEngine instance = new CalculatorEngine();

		// maximum length of edit strings
		const int maxDigits = 20;
		const int maxRealDigits = 40;
		const int maxHexDigits = 16;

		// for data entry (editing)
		bool entering = true;
		string entry = "0";
		string complexEntry = "0";
		bool enteringImaginary = false;
		int numberBase = 10;
		int wordSize = 0;
		//Real wordMask = Real.Zero;
		int stealthMode;

		// the RPN stack
		Stack<Number> stack;

		// current display format
		NumberSystem numberSystem = NumberSystem.Complex;

		// dummy event info for Changed event
		EventArgs eventArgs = new EventArgs();

		// last-entered number
		Number lastX;

		/// <summary>
		/// Intialize the stack. The stack always contains at least one item.
		/// If it becomes empty, a zero is pushed.
		/// </summary>
		internal CalculatorEngine()
		{
			stack = new Stack<Number>();
			stack.Push(0);
		}

		/// <summary>
		/// Get the CalculatorEngine singleton instance.
		/// </summary>
		internal static CalculatorEngine Instance
		{
			get { return instance; }
		}

		/// <summary>
		/// This event is fired whenever the engine state has changed in a way
		/// that would effect anyone who cares.
		/// </summary>
		public event EventHandler Changed;

		private void OnChanged()
		{
			if (stealthMode > 0) return;
			if (Changed != null)
				Changed(this, eventArgs);
		}

		/// <summary>
		/// Gets or set the top value on the stack. Changing the retrieve value does not
		/// affect the calculator. Getting this value causes edit mode to be exited. 
		/// Setting the value replaces the top stack value. Setting is ignored if 
		/// in edit mode.
		/// </summary>
		public Number X
		{
			get
			{
				StopEditing();
				return new Number(stack.Peek());
			}
			set
			{
				if (!entering)
				{
					stack.Pop();
					stack.Push(value);
					OnChanged();
				}
			}
		}

		/// <summary>
		/// Check to see if the stack is empty. 
		/// </summary>
		public bool StackIsEmpty
		{
			get
			{
				return stack.Count == 1 && X == Number.Zero;
			}
		}

		/// <summary>
		/// Pop a value from the stack. Exits edit mode.
		/// </summary>
		/// <returns></returns>
		public Number Pop()
		{
			StopEditing();
			Number n = stack.Pop();
			if (stack.Count == 0)
				stack.Push(0);
			OnChanged();
			return n;
		}

		/// <summary>
		/// Push a value to the stack. Exits edit mode.
		/// </summary>
		public void Push(Number n)
		{
			StopEditing();
			if (numberSystem != NumberSystem.Complex && n.Im != Real.Zero)
			{
				n.Re = Real.NaN;
				n.Im = Real.Zero;
			}
			else if (numberSystem == NumberSystem.Binary && !Number.IsInfinity(n) && !Number.IsNaN(n))
			{
				n = n.Re.Mask(wordSize);
			}
			stack.Push(n);
			OnChanged();
		}

		/// <summary>
		/// Clear the stack and reset the edit state.
		/// </summary>
		public void ClearStack()
		{
			stack.Clear();
			stack.Push(0);
			entry = complexEntry = "0";
			entering = enteringImaginary = false;
			OnChanged();
		}

		/// <summary>
		/// Enter editing mode. Does nothing if already editing, otherwise
		/// clears the display to "0".
		/// </summary>
		public void StartEditing()
		{
			if (!entering)
			{
				entry = "0";
				complexEntry = "0";
				entering = true;
				enteringImaginary = false;
			}
		}

		/// <summary>
		/// Exits edit mode and pushes the value to the stack. Does nothing if not
		/// in edit mode already.
		/// </summary>
		/// <returns>Returns true if a value was added to the stack, otherwise
		/// returns false.</returns>
		public bool StopEditing()
		{
			if (!entering)
				return false;

			entering = false;
			Number x = new Number();

			switch (numberSystem)
			{
				case NumberSystem.Complex:
					if (enteringImaginary)
					{
						x.Re = ParseReal(complexEntry);
						x.Im = ParseReal(entry);
					}
					else
					{
						x.Re = ParseReal(entry);
						x.Im = ParseReal(complexEntry);
					}
					break;
				case NumberSystem.Real:
					x.Re = ParseReal(entry);
					x.Im = Real.Zero;
					break;
				case NumberSystem.Binary:
					x.Re = ParseInt(entry);
					x.Im = Real.Zero;
					break;
			}

			lastX = x;
			Push(x);
			return true;
		}

		public Number LastX
		{
			get { return lastX == null ? 0 : lastX; }
		}

		/// <summary>
		/// Determine if the calculator is in edit mode.
		/// </summary>
		public bool IsEditing
		{
			get { return entering; }
		}

		/// <summary>
		/// Determine if the calculator is in edit mode and the imaginary part
		/// of the number is being editing (as opposed to the real part).
		/// </summary>
		public bool IsEditingImaginary
		{
			get { return entering && enteringImaginary; }
		}

		/// <summary>
		/// Gets or sets the current display format. If setting the value,
		/// edit mode is ended before the change occurs.
		/// </summary>
		public NumberSystem NumberSystem
		{
			get 
			{ 
				return numberSystem; 
			}
			set
			{
				StopEditing();
				if (numberSystem != value)
				{
					if (numberSystem == NumberSystem.Complex)
					{
						Stack<Number> temp = new Stack<Number>();
						while (stack.Count > 0)
							temp.Push(stack.Pop().Re);
						while (temp.Count > 0)
							stack.Push(temp.Pop());
					}
					numberSystem = value;
					if (value != NumberSystem.Binary)
						wordSize = 0;
					enteringImaginary = false;
				}
				OnChanged();
			}
		}

		/// <summary>
		/// Returns the string value of the number currently being edited.
		/// Enters edit mode if not currently editing. Call should be followed
		/// by a call to UpdateEditText even if the value isn't modified.
		/// </summary>
		public string EditText
		{
			get 
			{
				StartEditing();
				return entry;
			}
		}

		const string digits = "0123456789ABCDEF";

		private Real ParseInt(string s)
		{
			if (s == null) throw new ArgumentNullException();
			if (s.Length == 0) throw new ArgumentException();

			int b = numberBase;
			ulong n = 0;
			int i = 0;
			bool negative = s[i] == '-';
			if (negative) i++;
			for (; i < s.Length; i++)
			{
				int d = digits.IndexOf(s[i]);
				if (d == -1 || d >= b) throw new FormatException();
				n = n * (ulong) b + (ulong) d;
			}
			return negative ? -(Real)n : (Real)n;
		}

		private Real ParseReal(string s)
		{
			if (s == null) throw new ArgumentNullException();
			if (s.Length == 0) throw new ArgumentException();

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
			return n;
		}

		public string Format(ulong u)
		{
			StringBuilder s = new StringBuilder();
			if (u == 0)
				s.Append('0');
			else
			{
				ulong b = (ulong)numberBase;
				while (u != 0)
				{
					s.Insert(0, digits[(int) (u % b)]);
					u /= b;
				}
			}
			return s.ToString();
		}

		public string Format(Real d)
		{
			StringBuilder s = new StringBuilder();
			if (d < 0)
			{
				s.Append('-');
				d *= -1;
			}
			int exp;
			int nb = numberBase;
			if (d == 0)
				exp = 0;
			else
				exp = (int)Real.Ceiling(Real.Log(d, nb)) - 1;
			Real div = Real.Pow(nb, exp);
			Real mant = d / div;
			if ((int)mant >= nb)
			{
				exp++;
				mant /= nb;
			}

			int length = maxRealDigits - 2;
			int maxSignif = (int)Math.Ceiling(d.SignificantBits * Math.Log(2.0, nb));
			if (length > maxSignif) length = maxSignif;
			int[] dig = new int[length];
			for (int i = 0; i < length; i++)
			{
				int digit = (int)mant;
				if (digit < 0 || digit >= nb)
					Debugger.Break();
				dig[i] = digit;
				mant = nb * (mant - Real.Truncate(mant));
			}

			int r = 2 * dig[length - 1];
			dig[length - 1] = 0;
			if (r > nb || (r == nb && ((dig[length - 2] & 1) != 0)))
			{
				int i = length - 2;
				while (true)
				{
					dig[i]++;
					if (dig[i] < nb) break;
					dig[i] = 0;
					i--;
					if (i < 0) break;
				}
				if (i < 0)
				{
					exp++;
					dig[0] = 1;
					for (i = 1; i < length; i++)
						dig[i] = 0;
				}
			}

			int lz = length - 1;
			while (lz > 1 && dig[lz - 1] == 0)
				lz--;

			int maxExp = 16;
			if (numberSystem == NumberSystem.Binary && numberBase == 2)
				maxExp = maxRealDigits;

			if (exp < -6 || exp > maxExp)
			{
				for (int i = 0; i < lz; i++)
				{
					if (i == 1)
						s.Append('.');
					s.Append(digits[dig[i]]);
				}
				s.Append('^');
				if (exp < 0)
					s.Append('-');
				else
					s.Append('+');
				s.Append(Format((ulong)Real.Abs(exp)));
			}
			else if (exp < 0)
			{
				s.Append("0.");
				s.Append('0', -1 - exp);
				for (int i = 0; i < lz; i++)
					s.Append(digits[dig[i]]);
			}
			else if (exp < lz)
			{
				for (int i = 0; i < lz; i++)
				{
					if (i == exp + 1)
						s.Append('.');
					s.Append(digits[dig[i]]);
				}
			}
			else
			{
				for (int i = 0; i < lz; i++)
					s.Append(digits[dig[i]]);
				s.Append('0', exp - lz + 1);
			}

			//if (numberSystem == NumberSystem.Binary && wordSize > 0)
			//{
			//    for (int i = s.Length - 4; i > 0; i -= 4)
			//        s.Insert(i, ' ');
			//}

			return s.ToString();
		}

		/// <summary>
		/// Validate and update the current edit-mode entry. Should not be called
		/// without first retrieving EditText to get the previous value and ensure
		/// edit mode is entered properly.
		/// </summary>
		/// <returns>Returns true if the text was found to be valid, otherwise
		/// returns false and the value is not updated.</returns>
		public bool UpdateEditText(string text)
		{

			try
			{
				if (text.Length == 0)
					return false;

				if (numberSystem == NumberSystem.Binary)
				{
					if (text.Length > maxHexDigits)
						return false;
					ParseInt(text);
				}
				else if (numberSystem == NumberSystem.Real)
				{
					if (text.Length > maxRealDigits - (text[0] == '-' ? 0 : 1))
						return false;
					ParseReal(text);
				}
				else
				{
					if (text.Length > maxDigits - (text[0] == '-' ? 0 : 1))
						return false;
					ParseReal(text);
				}
				entry = text;
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				OnChanged();
			}
		}

		/// <summary>
		/// Toggles between editing the real part and the imaginary part of
		/// the complex number. If not in edit mode, this call begins editing the
		/// imaginary part.
		/// </summary>
		public void ToggleImaginary()
		{
			if (numberSystem != NumberSystem.Complex)
				return;

			StartEditing();

			string temp = entry;
			entry = complexEntry;
			complexEntry = temp;

			enteringImaginary ^= true;
			OnChanged();
		}

		/// <summary>
		/// Returns the real part of the number being edited, or null if not in edit mode.
		/// </summary>
		public string RealText
		{
			get { return entering ? enteringImaginary ? complexEntry : entry : null; }
		}

		/// <summary>
		/// Returns the imaginary part of the number being edited, or null if not in
		/// edit mode.
		/// </summary>
		public string ImaginaryText
		{
			get { return entering ? enteringImaginary ? entry : complexEntry : null; }
		}

		/// <summary>
		/// Describes a method that takes one Number argument and returns a Number.
		/// </summary>
		public delegate Number UnaryOp(Number x);

		/// <summary>
		/// Helper function for implementing unary (single-argument) operations.
		/// Pops a value from the stack, calls the anonymous method, and then pushes the
		/// result.
		/// </summary>
		public void UnaryOperation(UnaryOp op)
		{
			StealthMode(delegate()
			{
				Push(op(Pop()));
			}
			);
		}

		/// <summary>
		/// Describes a method that takes two Number arguments and returns a Number.
		/// </summary>
		public delegate Number BinaryOp(Number a, Number b);

		/// <summary>
		/// Helper function for implementing binary (two-argument) operations.
		/// Pops two values from the stack, calls the anonymous method, and then pushes
		/// the result.
		/// </summary>
		public void BinaryOperation(BinaryOp op)
		{
			StealthMode(delegate()
			{
				Number x = Pop();
				Number y = Pop();
				Push(op(y, x));
			});
		}

		/// <summary>
		/// This event fires whenever the main calculator form receives a 
		/// KeyDown or KeyPress event. It can be used to record and/or modify keystrokes.
		/// </summary>
		public event EventHandler EventSubmitted;

		/// <summary>
		/// Causes the EventSumitted event to fire.
		/// </summary>
		public void SubmitEvent(EventArgs e)
		{
			if (EventSubmitted != null)
				EventSubmitted(this, e);
		}

		/// <summary>
		/// This event fires whenever anyone calls EmitEvent. 
		/// </summary>
		public event EventHandler EventEmitted;

		/// <summary>
		/// This method simulates KeyDown and KeyPress events on the main form.
		/// It can be used to replay recorded keystrokes.
		/// </summary>
		public void EmitEvent(EventArgs e)
		{
			if (EventEmitted != null)
				EventEmitted(this, e);
		}

		public int NumberBase
		{
			get
			{
				return numberBase;
			}
			set
			{
				StopEditing();
				if (value == numberBase || value < 2 || value > 16)
					return;
				numberBase = value;
				OnChanged();
			}
		}

		public int WordSize
		{
			get
			{
				return wordSize;
			}
			set
			{
				if (value == 0)
				{
					wordSize = 0;
					this.NumberSystem = NumberSystem.Real;
					OnChanged();
				}
				else if (value >= 8 && value <= 64)
				{
					int oldSize = wordSize;
					wordSize = value;
					//wordMask = Real.Truncate(new Real(true, (short) wordSize, ulong.MaxValue, 0));
					this.NumberSystem = NumberSystem.Binary;
					if (numberBase != 2 && numberBase != 4 && numberBase != 8 && numberBase != 16)
						numberBase = 16;
					if (wordSize < oldSize || oldSize == 0)
					{
						Stack<Number> temp = new Stack<Number>();
						while (stack.Count > 0)
							temp.Push(stack.Pop().Mask(wordSize));
						while (temp.Count > 0)
							stack.Push(temp.Pop());
					}
					OnChanged();
				}
			}
		}

		public delegate void StealthMethod();

		public void StealthMode(StealthMethod method)
		{
			stealthMode++;
			method();
			stealthMode--;
			OnChanged();
		}
	}
}
