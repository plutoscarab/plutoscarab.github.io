/*
 * Program.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Mulvey.RpnCalculator
{
	[CommandContainer]
	public class Program
	{
		static MainForm form;

		/// <summary>
		/// The main entry point for the application. Pass the name of the skin
		/// on the command line. Uses default skin if no skin is specified.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			form = new MainForm(args.Length == 0 ? "default" : args[0]);
			form.Show();
			Commands.Initialize();
			form.DelayedInitialization();
			Application.Run(form);
		}

		public Program(CalculatorEngine engine)
		{
		}

		[Command]
		public void Close()
		{
			form.Close();
		}

		[Command]
		public void Minimize()
		{
			form.WindowState = FormWindowState.Minimized;
		}
	}
}