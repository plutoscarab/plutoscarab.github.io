/*
 * Mouse.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This class is responsible for parsing the mouse mapping 
 * file and issuing appropriate commands when mouse actions
 * occur.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace Mulvey.RpnCalculator
{
	public class Mouse
	{
		private static Mouse instance = null;

		Dictionary<string, Rectangle> keyBounds =
			new Dictionary<string, Rectangle>();
		Dictionary<string, Dictionary<int, Command>> keyCommands =
			new Dictionary<string, Dictionary<int, Command>>();
		Dictionary<string, int> indicatorMasks =
			new Dictionary<string, int>();
		Dictionary<Command, Rectangle> commandRects =
			new Dictionary<Command, Rectangle>();
		Dictionary<int, Rectangle> baseRects =
			new Dictionary<int, Rectangle>();
		Dictionary<int, Rectangle> numberSystemRects =
			new Dictionary<int, Rectangle>();
		Dictionary<int, Rectangle> wordSizeRects =
			new Dictionary<int, Rectangle>();

		public static Rectangle NumericDisplay = Rectangle.Empty;

		private Mouse(string filename)
		{
			Settings layout = new Settings(filename);
			foreach (KeyValuePair<string, Dictionary<string, string>> keyConfig in layout)
			{
				string name = keyConfig.Key;
				Dictionary<string, string> info = keyConfig.Value;

				int x = int.Parse(info["left"]);
				int y = int.Parse(info["top"]);
				int w = int.Parse(info["width"]);
				int h = int.Parse(info["height"]);
				Rectangle rect = keyBounds[name] = new Rectangle(x, y, w, h);

				Dictionary<int, Command> commands = new Dictionary<int, Command>();
				foreach (KeyValuePair<string, string> entry in info)
				{
					string n = entry.Key;
					if (n == "command" || n.StartsWith("command("))
					{
						int mask = 0;
						int p = n.IndexOf('(');
						if (p != -1)
						{
							if (!n.EndsWith(")"))
								continue;
							if (!int.TryParse(n.Substring(p + 1, n.Length - p - 2), out mask))
								continue;
							n = n.Substring(0, p);
						}
						Command command;
						if (Commands.ParseCommand(entry.Value, out command))
						{
							commands[mask] = command;
							commandRects[command] = rect;
						}
					}
					else if (n == "mode")
					{
						int mode = 0;
						if (!int.TryParse(entry.Value, out mode))
							continue;
						if (mode == -1)
							NumericDisplay = rect;
					}
					else if (n == "base")
					{
						int b = 0;
						if (!int.TryParse(entry.Value, out b))
							continue;
						baseRects[b] = rect;
					}
					else if (n == "numbersystem")
					{
						int ns = 0;
						if (!int.TryParse(entry.Value, out ns))
							continue;
						numberSystemRects[ns] = rect;
					}
					else if (n == "wordsize")
					{
						int ws = 0;
						if (!int.TryParse(entry.Value, out ws))
							continue;
						wordSizeRects[ws] = rect;
					}
				}
				keyCommands[name] = commands;

				string indicator;
				if (info.TryGetValue("indicator", out indicator))
				{
					indicatorMasks[name] = int.Parse(indicator);
				}
			}
		}

		public static void Initialize(string filename)
		{
			instance = new Mouse(filename);
		}

		public static Mouse Instance
		{
			get { return instance; }
		}

		int shiftMask = 0;

		public static void Shift(int mask)
		{
			instance.shiftMask = mask;
		}

		int toggleMask = 0;

		public static void Toggle(int mask)
		{
			instance.toggleMask ^= mask;
		}

		public static bool HandleMouseDown(Point p, out Rectangle rect)
		{
			rect = Rectangle.Empty;

			foreach (KeyValuePair<string, Rectangle> key in instance.keyBounds)
			{
				if (!key.Value.Contains(p))
					continue;

				int max = int.MinValue;
				Command best = null;
				int keyMask = instance.shiftMask | instance.toggleMask;
				Dictionary<int, Command> commands = instance.keyCommands[key.Key];
				if (commands.Count == 0) continue;

				foreach (KeyValuePair<int, Command> command in commands)
				{
					int match = keyMask & command.Key;
					if (match > max)
					{
						max = match;
						best = command.Value;
					}
				}
				if (best != null)
				{
					int oldMask = instance.shiftMask;
					instance.shiftMask = 0;
					best.Execute();
					if (instance.shiftMask == oldMask)
						instance.shiftMask = 0;
				}
				rect = key.Value;
				return true;
			}

			return false;
		}

		public static List<Rectangle> GetIndicatorRects()
		{
			Rectangle r;
			List<Rectangle> list = new List<Rectangle>();
			int keyMask = instance.shiftMask | instance.toggleMask;
			foreach (KeyValuePair<string, int> ind in instance.indicatorMasks)
			{
				if ((ind.Value & keyMask) != 0)
				{
					r = instance.keyBounds[ind.Key];
					list.Add(r);
				}
			}
			if (instance.baseRects.TryGetValue(CalculatorEngine.Instance.NumberBase, out r))
				list.Add(r);
			if (instance.numberSystemRects.TryGetValue((int)CalculatorEngine.Instance.NumberSystem, out r))
				list.Add(r);
			if (instance.wordSizeRects.TryGetValue((int)CalculatorEngine.Instance.WordSize, out r))
				list.Add(r);
			return list;
		}

		public static Rectangle GetCommandRect(Command command)
		{
			Rectangle rect;
			if (instance.commandRects.TryGetValue(command, out rect))
				return rect;
			return Rectangle.Empty;
		}
	}

	[CommandContainer]
	public class MouseCommands
	{
		public MouseCommands(CalculatorEngine engine)
		{
		}

		[Command]
		public void Shift(int mask)
		{
			Mouse.Shift(mask);
			Command.CancelSubmit = true;
		}

		[Command]
		public void Toggle(int mask)
		{
			Mouse.Toggle(mask);
			Command.CancelSubmit = true;
		}
	}
}
