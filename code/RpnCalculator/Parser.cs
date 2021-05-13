/*
 * Parser.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This class uses the C# compiler to create an executable class
 * at run-time, derived from commands and constants defined in
 * keyboard and mouse mapping files.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Mulvey.RpnCalculator
{
	internal class Parser
	{
		static CSharpCodeProvider provider;
		static ICodeCompiler compiler;
		static CompilerParameters parameters;

		static Parser()
		{
			provider = new CSharpCodeProvider();
			compiler = provider.CreateCompiler();
			parameters = new CompilerParameters();
			parameters.GenerateInMemory = true;
			parameters.GenerateExecutable = false;
			parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
		}

		public static object CreateObject(string source)
		{
			const string className = "DummyCreateObjectClass";
			source =
				"using Mulvey.RpnCalculator;\r\n" +
				"public class " + className + "\r\n" +
				"{\r\n" +
					source + "\r\n" +
				"}";

			CompilerResults results = compiler.CompileAssemblyFromSource(parameters, source);
			if (results.Errors.Count > 0)
				return null;
			if (results.CompiledAssembly == null)
				return null;

			Type type = results.CompiledAssembly.GetType(className);
			if (type == null)
				return null;

			return Activator.CreateInstance(type);
		}
	}
}
