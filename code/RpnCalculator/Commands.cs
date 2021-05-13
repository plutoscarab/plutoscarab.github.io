/*
 * Commands.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This class handles enumeration of installed plug-ins including
 * built-in commands, parsing commands from the keyboard and mouse
 * mapping files, and passing commands to the appropriate plug-in.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Mulvey.QuadPrecision;

namespace Mulvey.RpnCalculator
{
	public class CommandContainerAttribute : Attribute
	{
	}

	public class CommandAttribute : Attribute
	{
	}

	public class Command : EventArgs
	{
		static Command current;

		string cmd;
		int argsIndex;
		bool cancelSubmit;

		public Command(string cmd, int argsIndex)
		{
			this.cmd = cmd;
			this.argsIndex = argsIndex;
		}

		public void Execute()
		{
			cancelSubmit = false;
			current = this;
			Commands.Execute(cmd, argsIndex);
			if (!cancelSubmit)
				CalculatorEngine.Instance.SubmitEvent(this);
		}

		public override bool Equals(object other)
		{
			return cmd == ((Command)other).cmd && argsIndex == ((Command)other).argsIndex;
		}

		public override int GetHashCode()
		{
			return (cmd + "," + argsIndex.ToString()).GetHashCode();
		}

		public override string ToString()
		{
			if (argsIndex == -1)
				return cmd;
			else
				return cmd + "(" + argsIndex.ToString() + ")";
		}

		public static bool CancelSubmit
		{
			get { return current == null ? false : current.cancelSubmit; }
			set { if (current != null) current.cancelSubmit = value; }
		}
	}

	public class Commands
	{
		static Commands instance = new Commands();

		Dictionary<string, object> commandContainers = new Dictionary<string, object>();
		Dictionary<string, MethodInfo> commandMethods = new Dictionary<string, MethodInfo>();
		int paramIndex = 0;
		StringBuilder paramSource = new StringBuilder();
		object paramLists;
		object[] nullArgs = new object[0];
		Dictionary<string, int> argExpressions = new Dictionary<string, int>();

		internal Commands()
		{
			try
			{
				foreach (string filename in Directory.GetFiles(".", "*.*"))
				{
					try
					{
						string ext = Path.GetExtension(filename).ToLowerInvariant();
						if (ext != ".dll" && ext != ".exe")
							continue;
						string name = Path.GetFileNameWithoutExtension(filename);
						if (name.EndsWith(".vshost")) continue;
						Assembly plugin = Assembly.Load(name);
						Type[] types = plugin.GetExportedTypes();
						foreach (Type type in types)
						{
							object[] attrs = type.GetCustomAttributes(typeof(CommandContainerAttribute), false);
							if (attrs.Length > 0)
							{
								ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(CalculatorEngine) });
								if (constructor == null)
									continue;
								object container = constructor.Invoke(new object[] { CalculatorEngine.Instance });
								if (container == null)
									continue;
								MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
								foreach (MethodInfo method in methods)
								{
									object[] methodAttrs = method.GetCustomAttributes(typeof(CommandAttribute), false);
									if (methodAttrs.Length > 0)
									{
										if (commandContainers.ContainsKey(method.Name))
											continue;
										commandContainers[method.Name] = container;
										commandMethods[method.Name] = method;
										Debug.WriteLine("Exported method: " + method.Name);
									}
								}
							}
						}
					}
					catch
					{
					}
				}
			}
			catch
			{
			}

			CalculatorEngine.Instance.EventEmitted += new EventHandler(Instance_EventEmitted);
		}

		void Instance_EventEmitted(object sender, EventArgs e)
		{
			Command c = e as Command;
			if (c == null) return;
			c.Execute();
		}

		public static void Initialize()
		{
		}

		private static object GetContainer(string command)
		{
			object obj;
			if (!instance.commandContainers.TryGetValue(command, out obj))
				return null;
			return obj;
		}

		private static MethodInfo GetMethod(string command)
		{
			MethodInfo info;
			if (!instance.commandMethods.TryGetValue(command, out info))
				return null;
			return info;
		}

		public static void Execute(string cmd, int argsIndex)
		{
			object[] args = instance.nullArgs;
			if (argsIndex != -1)
			{
				DoneParsing();
				FieldInfo info = instance.paramLists.GetType().GetField("ParamList" + argsIndex.ToString());
				args = (object[])info.GetValue(instance.paramLists);
			}
			object container = Commands.GetContainer(cmd);
			MethodInfo method = Commands.GetMethod(cmd);
			Debug.Write(cmd);
			Debug.Write("(");
			for (int i = 0; i < args.Length; i++)
			{
				if (i > 0) Debug.Write(", ");
				Debug.Write(args[i]);
			}
			Debug.WriteLine(")");
			args = (object[]) args.Clone();
			ParameterInfo[] p = method.GetParameters();
			for (int i = 0; i < args.Length; i++)
				if (p[i].ParameterType == typeof(Number))
				{
					if (args[i] is int)
						args[i] = (Number)(int)args[i];
					else if (args[i] is long)
						args[i] = (Number)(long)args[i];
					else if (args[i] is double)
						args[i] = (Number)(double)args[i];
					else if (args[i] is float)
						args[i] = (Number)(float)args[i];
					else if (args[i] is Real)
						args[i] = (Number)(Real)args[i];
				}
			method.Invoke(container, args);
		}

		public static bool ParseCommand(string expr, out Command command)
		{
			command = null;
			int args = -1;
			string argExpr = null;
			int p = expr.IndexOf('(');
			if (p != -1)
			{
				if (!expr.EndsWith(")"))
				{
					Debug.WriteLine("Syntax error: " + expr);
					return false;
				}
				argExpr = expr.Substring(p + 1, expr.Length - p - 2);
				expr = expr.Substring(0, p);
			}
			if (GetMethod(expr) == null)
			{
				Debug.WriteLine("Not implemented: " + expr);
				return false;
			}
			if (argExpr != null)
			{
				StringBuilder s = new StringBuilder();
				bool q = false;
				char qc = char.MinValue;
				bool r = false;
				bool esc = false;
				for (int i = 0; i < argExpr.Length; i++)
				{
					if (esc)
					{
						esc = false;
					}
					else
					{
						switch (argExpr[i])
						{
							case '\'':
							case '\"':
								if (q)
								{
									if (qc == argExpr[i])
										q = false;
								}
								else
								{
									q = true;
									qc = argExpr[i];
								}
								break;
							case ',':
								if (r)
								{
									s.Append("\")");
									r = false;
								}
								break;
							case 'R':
							case 'r':
								if (!q && !r && (i == 0 || argExpr[i-1] == ',' || argExpr[i-1] == ' '))
								{
									s.Append("new Mulvey.RpnCalculator.Number(\"");
									r = true;
								}
								break;
							case '\\':
								if (q)
								{
									esc = true;
								}
								break;
						}
					}
					if (!r || (argExpr[i] != 'r' && argExpr[i] != 'R'))
						s.Append(argExpr[i]);
				}
				if (r)
					s.Append("\")");
				if (!instance.argExpressions.TryGetValue(argExpr, out args))
				{
					instance.paramSource.Append("public object[] ParamList");
					instance.paramSource.Append(instance.paramIndex);
					instance.paramSource.Append(" = new object[] {");
					instance.paramSource.Append(s.ToString());
					instance.paramSource.Append("};\r\n");
					args = instance.paramIndex++;
					instance.argExpressions[argExpr] = args;
				}
			}
			command = new Command(expr, args);
			return true;
		}

		public static void DoneParsing()
		{
			if (instance.paramLists == null)
			{
				instance.paramLists = Parser.CreateObject(instance.paramSource.ToString());
				instance.paramSource = null;
			}
		}
	}
}
