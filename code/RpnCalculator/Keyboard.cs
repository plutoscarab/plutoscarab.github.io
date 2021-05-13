/*
 * Keyboard.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This class is responsible for parsing the keyboard mapping 
 * file and issuing appropriate commands when keys are pressed.
 * 
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text;
using System.Drawing;

namespace Mulvey.RpnCalculator
{
	internal class Keyboard
	{
		static Keyboard instance = null;

		Dictionary<int, Command>[] keyDownCommands = new Dictionary<int, Command>[2];
		Dictionary<char, Command>[] keyPressCommands = new Dictionary<char, Command>[2];

		internal Keyboard(string filename)
		{
			for (int k = 0; k < 2; k++)
			{
				keyDownCommands[k] = new Dictionary<int, Command>();
				keyPressCommands[k] = new Dictionary<char, Command>();
			}

			try
			{
				using (StreamReader reader = new StreamReader(filename))
				{
					while (!reader.EndOfStream)
					{
						string entry = reader.ReadLine().Trim();
						if (entry.Length == 0)
							continue;
						if (entry.StartsWith("//"))
							continue;
						string[] fields = entry.Split('\t');
						if (fields.Length != 2 && fields.Length != 3)
							continue;
						string key = fields[0];
						if (key.Length == 0)
							continue;

						int modifiers = 0;
						int i = 0;
						while (i < key.Length - 1 && "+^%".IndexOf(key[i]) != -1)
						{
							switch (key[i])
							{
								case '+':
									modifiers |= (int)Keys.Shift;
									break;
								case '^':
									modifiers |= (int)Keys.Control;
									break;
								case '%':
									modifiers |= (int)Keys.Alt;
									break;
							}
							i++;
						}

						int keyValue = -1;
						char keyChar = key[i];
						if (i == key.Length - 1)	// single-char key
						{
							if (modifiers == 0)
							{
							}
							else if ((int)key[i] < 32)
							{
								keyChar = (char)((int)char.ToUpperInvariant(keyChar) - (int)'A' + 1);
							}
							else
							{
								keyValue = (int)keyChar | modifiers;
							}
						}
						else if (key[i] != '.')
						{
							continue;
						}
						else	// special key
						{
							key = key.Substring(i + 1);
							if (typeof(Keys).GetField(key) == null)
								continue;
							Keys keyEnum = (Keys)Enum.Parse(typeof(Keys), key);
							keyValue = (int)keyEnum | modifiers;
						}

						for (int mode = 0; mode < 2; mode++)
						{
							string cmd = fields[Math.Min(mode + 1, fields.Length - 1)];
							if (cmd.Length == 0)
								continue;
							Command command;
							if (!Commands.ParseCommand(cmd, out command))
								continue;

							if (keyValue == -1)	// single-char key
							{
								keyPressCommands[mode][keyChar] = command;
							}
							else	// special key
							{
								keyDownCommands[mode][keyValue] = command;
							}
						}
					}
				}
			}
			catch
			{
			}
		}
	

		public static void Initialize(string filename)
		{
			instance = new Keyboard(filename);
		}

		public static Rectangle ProcessKeyDown(KeyEventArgs e, int mode)
		{
			Command command;
			if (!instance.keyDownCommands[mode].TryGetValue((int)e.KeyData, out command))
				return Rectangle.Empty;
			e.SuppressKeyPress = true;
			return ProcessKeyCommand(command);
		}

		public static Rectangle ProcessKeyPress(KeyPressEventArgs e, int mode)
		{
			Command command;
			if (!instance.keyPressCommands[mode].TryGetValue(e.KeyChar, out command))
				return Rectangle.Empty;
			return ProcessKeyCommand(command);
		}

		private static Rectangle ProcessKeyCommand(Command command)
		{
			command.Execute();
			return Mouse.GetCommandRect(command);

		}
	}
}