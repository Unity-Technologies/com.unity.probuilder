using System;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace ProBuilder.BuildSystem
{
	public static class Compiler
	{
		/**
		 *	Build a DLL with BuildAssemblyTarget.
		 */
		public static bool CompileDLL(AssemblyTarget target, bool isDebug = false)
		{
			CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string ,string>()
				{
					{ "CompilerVersion", "v3.5" }
				});

			// List<string> m_SystemAssemblies = new List<string>()
			// {
			// 	Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Microsoft.NET/Framework/v2.0.50727/mscorlib.dll"),
			// 	Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Microsoft.NET/Framework/v2.0.50727/System.dll"),
			// 	Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Microsoft.NET/Framework/v2.0.50727/System.Core.dll")
			// };

			CompilerParameters parameters = new CompilerParameters();
			parameters.GenerateExecutable = false;
			parameters.OutputAssembly = target.OutputAssembly;
			parameters.GenerateInMemory = false;
			parameters.IncludeDebugInformation = isDebug;
			// We're targeting .NET 3.5 framework
		    parameters.CompilerOptions = "/nostdlib";              

			foreach(string assembly in target.ReferencedAssemblies)
				parameters.ReferencedAssemblies.Add(assembly);

			CompilerResults res = provider.CompileAssemblyFromFile(parameters, target.GetSourceFiles());

			Console.WriteLine(string.Format("{0} results:", target.OutputAssembly));

			if(res.Errors.Count > 0)
			{
				Console.WriteLine("Errors:");

				foreach(CompilerError ce in res.Errors)
					Console.WriteLine(string.Format("\t{0}", ce.ToString()));
			}

			Console.WriteLine("Path: " + res.PathToAssembly);

			return res.Errors.Count < 1;
		}
	}
}
