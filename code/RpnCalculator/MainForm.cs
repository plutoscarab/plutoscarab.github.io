/*
 * MainForm.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This is the main form of the application. It handles rendering
 * and animation of the calculator, and passing keyboard and mouse
 * events to the appropriate command processors.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Mulvey.QuadPrecision;
using System.Reflection;

namespace Mulvey.RpnCalculator
{
	internal partial class MainForm : PerPixelAlphaForm
	{
		CalculatorEngine engine;
		Bitmap keysUpImage;
		Bitmap keysDownImage;
		Bitmap activeImage;
		Bitmap digitsImage;

		public MainForm(string skinName)
		{
			engine = CalculatorEngine.Instance;
			engine.Changed += new EventHandler(engine_Changed);
			engine.EventEmitted += new EventHandler(engine_EventEmitted);
			LoadSkin(skinName);
		}

		private string name;
		private Dictionary<string, string> files;

		private void LoadSkin(string name)
		{
			this.name = name;
			string filename = name + "\\skin.ini";
			Settings settings = new Settings(filename);
			files = settings["files"];

			filename = Path.Combine(name, files["keysupimage"]);
			keysUpImage = (Bitmap)Bitmap.FromFile(filename);
			activeImage = new Bitmap(keysUpImage);
			base.SetBitmap(keysUpImage);

			this.Width = keysUpImage.Width;
			this.Height = keysUpImage.Height;
			this.Location = new Point(
				(Screen.PrimaryScreen.WorkingArea.Width - Size.Width) / 2,
				(Screen.PrimaryScreen.WorkingArea.Height - Size.Height) / 2);

			InitializeComponent();
		}

		public void DelayedInitialization()
		{
			string filename;

			filename = Path.Combine(name, files["keysdownimage"]);
			keysDownImage = (Bitmap)Bitmap.FromFile(filename);

			filename = Path.Combine(name, files["digitsimage"]);
			digitsImage = (Bitmap)Bitmap.FromFile(filename);

			filename = Path.Combine(name, files["keyboardmap"]);
			Keyboard.Initialize(filename);

			filename = Path.Combine(name, files["mouselayout"]);
			Mouse.Initialize(filename);

			Commands.DoneParsing();
			RestoreNormalImage();
		}

		private void LoadMouseLayout(string filename)
		{
		}

		void engine_EventEmitted(object sender, EventArgs e)
		{
			if (e is KeyEventArgs)
				this.Form1_KeyDown(sender, (KeyEventArgs) e);
			else if (e is KeyPressEventArgs)
				this.Form1_KeyPress(sender, (KeyPressEventArgs) e);
		}

		void engine_Changed(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			int mode = engine.NumberSystem == NumberSystem.Binary ? 1 : 0;
			Rectangle rect = Keyboard.ProcessKeyDown(e, mode);
			if (this.IsDisposed) return;
			DrawPressedRect(rect);
			BeginKeyboardRectTimer();
		}

		Timer timer;

		private void BeginKeyboardRectTimer()
		{
			if (timer != null) return;
			timer = new Timer();
			timer.Interval = 100;
			timer.Enabled = true;
			timer.Tick += new EventHandler(timer_Tick);
		}

		void timer_Tick(object sender, EventArgs e)
		{
			timer.Enabled = false;
			timer = null;
			RestoreNormalImage();
		}

		private void Form1_KeyPress(object sender, KeyPressEventArgs e)
		{
			int mode = engine.NumberSystem == NumberSystem.Binary ? 1 : 0;
			Rectangle rect = Keyboard.ProcessKeyPress(e, mode);
			if (this.IsDisposed) return;
			DrawPressedRect(rect);
			BeginKeyboardRectTimer();
		}

		const string digitsPattern = "+- .0123456789ABCDEF^i";

		private string NixieReal(Real d)
		{
			if (Real.IsNaN(d) || Real.IsInfinity(d))
				return "...";

			string s = engine.Format(d);
			foreach (char ch in s)
				if (digitsPattern.IndexOf(ch) == -1)
					return "...";

			return s;
		}

		string displayText = "0";

		private void DrawDisplayText(Graphics g)
		{
			Rectangle r = Mouse.NumericDisplay;
			if (r.IsEmpty) return;
			g.DrawImage(keysUpImage, r, r, GraphicsUnit.Pixel);
			int x = r.Right;
			int h = digitsImage.Height;
			for (int i = displayText.Length - 1; i >= 0; i--)
			{
				char ch = displayText[i];
				int j = digitsPattern.IndexOf(ch);
				if (j == -1)
					continue;
				x -= 11;
				Rectangle src = new Rectangle(11 * j, 0, 11, h);
				Rectangle dst = new Rectangle(x, r.Top, 11, h);
				g.DrawImage(digitsImage, dst, src, GraphicsUnit.Pixel);
			}
		}

		private void UpdateDisplay()
		{
			if (engine.IsEditing)
				if (engine.IsEditingImaginary)
					displayText = engine.RealText + " " + (engine.ImaginaryText[0] == '-' ? "" : "+") + engine.ImaginaryText + "i";
				else
					displayText = engine.RealText + (engine.ImaginaryText == "0" ? "" : " " + (engine.ImaginaryText[0] == '-' ? "" : "+") + engine.ImaginaryText + "i");
			else if (engine.NumberSystem == NumberSystem.Binary)
				displayText = NixieReal(engine.X.Re.Truncate());
			else if (engine.X.Im == Real.Zero)
				displayText = NixieReal(engine.X.Re);
			else if (engine.X.Re == Real.Zero)
				displayText = NixieReal(engine.X.Im) + "i";
			else
			{
				string s1 = NixieReal(engine.X.Re);
				StringBuilder b1 = new StringBuilder(s1);
				int p1 = s1.IndexOf('^');
				if (p1 == -1) p1 = s1.Length;

				string s2 = (engine.X.Im < 0 ? "" : "+") + NixieReal(engine.X.Im);
				StringBuilder b2 = new StringBuilder(s2);
				int p2 = s2.IndexOf('^');
				if (p2 == -1) p2 = s2.Length;

				while (b1.Length + b2.Length > 44)
				{
					if (b1.Length > b2.Length)
					{
						b1.Remove(--p1, 1);
					}
					else
					{
						b2.Remove(--p2, 1);
					}
				}
				displayText = b1.ToString() + " " + b2.ToString() + "i";
			}
			using (Graphics g = Graphics.FromImage(activeImage))
			{
				DrawDisplayText(g);
			}
			UpdateImage();
		}

		private void Form1_Click(object sender, EventArgs e)
		{
		}

		private void UpdateImage()
		{
			this.SetBitmap(activeImage);
		}

		private void RecopyNormalImage()
		{
			Rectangle all = new Rectangle(0, 0, keysUpImage.Width, keysUpImage.Height);
			BitmapData dst = activeImage.LockBits(all, ImageLockMode.WriteOnly, activeImage.PixelFormat);
			byte[] row = new byte[Math.Abs(dst.Stride)];
			for (int y = 0; y < all.Height; y++)
			{
				IntPtr dstPtr = (IntPtr)(dst.Scan0.ToInt32() + y * dst.Stride);
				Marshal.Copy(row, 0, dstPtr, row.Length);
			}
			activeImage.UnlockBits(dst);

			List<Rectangle> indicatorRects = Mouse.GetIndicatorRects();
			using (Graphics g = Graphics.FromImage(activeImage))
			{
				g.DrawImage(keysUpImage, all, all, GraphicsUnit.Pixel);
				DrawDisplayText(g);
				if (indicatorRects.Count > 0)
				{
					foreach (Rectangle r in indicatorRects)
						g.DrawImage(keysDownImage, r, r, GraphicsUnit.Pixel);
				}
			}
		}

		private void RestoreNormalImage()
		{
			RecopyNormalImage();
			UpdateImage();
		}

		private void DrawPressedRect(Rectangle r)
		{
			RecopyNormalImage();
			using (Graphics g = Graphics.FromImage(activeImage))
			{
				g.DrawImage(keysDownImage, r, r, GraphicsUnit.Pixel);
			}
			UpdateImage();
		}

		private void Form1_MouseDown(object sender, MouseEventArgs e)
		{
			Rectangle rect;

			switch (e.Button)
			{
				case MouseButtons.Left:
					if (Mouse.HandleMouseDown(e.Location, out rect))
					{
						if (!this.IsDisposed)
							DrawPressedRect(rect);
					}
					else
					{
						User32.ReleaseCapture();
						User32.SendMessage(this.Handle, User32.WM_NCLBUTTONDOWN, User32.HT_CAPTION, 0);
					}
					break;
			}
		}

		private void Form1_MouseMove(object sender, MouseEventArgs e)
		{
		}

		private void Form1_MouseUp(object sender, MouseEventArgs e)
		{
			RestoreNormalImage();
		}
	}
}