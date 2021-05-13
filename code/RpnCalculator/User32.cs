/*
 * User32.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This file exposes a handful of Win32 API functions.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Mulvey.RpnCalculator
{
	public class User32
	{
		public const int SPI_GETDRAGFULLWINDOWS = 38;
		public const int SPI_SETDRAGFULLWINDOWS = 37;
		public const int SPIF_SENDWININICHANGE = 2;
		public const int SPIF_UPDATEINIFILE = 1;
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;

		[DllImport("user32.dll")]
		public static extern int SystemParametersInfo(int uAction, int uParam, ref int lpvParam, int fuWinIni);
		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[DllImport("user32.dll")]
		public static extern bool ReleaseCapture();
	}
}
