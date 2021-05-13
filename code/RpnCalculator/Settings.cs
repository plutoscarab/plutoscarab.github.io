/*
 * Settings.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This class parses simple INI files and makes the contents
 * available as an object model. It is used for skin.ini and
 * the mouse mapping file.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace Mulvey.RpnCalculator
{
	public class Settings : IEnumerable
	{
		private Dictionary<string, Dictionary<string, string>> settings;

		public Settings(string filename)
		{
			settings = new Dictionary<string, Dictionary<string, string>>();
			string section = null;
			Dictionary<string, string> values = null;
			using (StreamReader r = File.OpenText(filename))
			{
				while (!r.EndOfStream)
				{
					string s = r.ReadLine().Trim();
					if (s.StartsWith("[") && s.EndsWith("]"))
					{
						if (values != null)
						{
							settings[section] = values;
						}
						section = s.Substring(1, s.Length - 2).ToLowerInvariant();
						values = new Dictionary<string, string>();
						continue;
					}
					int e = s.IndexOf('=');
					if (e == -1) continue;
					string n = s.Substring(0, e).Trim().ToLowerInvariant();
					string v = s.Substring(e + 1).Trim();
					values[n] = v;
				}
			}
			if (values != null)
			{
				settings[section] = values;
			}
		}

		public Dictionary<string, string> this[string key]
		{
			get
			{
				Dictionary<string, string> values;
				if (!settings.TryGetValue(key, out values))
					return null;
				return values;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return settings.GetEnumerator();
		}
	}
}
