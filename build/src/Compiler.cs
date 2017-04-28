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
		public static bool CompileDLL(AssemblyTarget target, Dictionary<string, string> referenceMacros = null, bool isDebug = false)
		{
			CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string ,string>() {
					{ "CompilerVersion", "v3.5" }
				});

			CompilerParameters parameters = new CompilerParameters();
			parameters.GenerateExecutable = false;
			parameters.OutputAssembly = target.OutputAssembly;
			parameters.GenerateInMemory = false;
			parameters.IncludeDebugInformation = isDebug;
			parameters.TreatWarningsAsErrors = isDebug;
			// We're targeting .NET 3.5 framework - the mscorlib, System, and System.Core libs
			// should be included in the referenced assemblies list.
			if(target.Defines != null && target.Defines.Count > 0)
				parameters.CompilerOptions = string.Format("/nostdlib /define:{0}", string.Join(";", target.Defines.ToArray()));
			else
				parameters.CompilerOptions = "/nostdlib";

		    Console.WriteLine("CompilerOptions: " + parameters.CompilerOptions);

			foreach(string assembly in target.ReferencedAssemblies)
			{
				string path = assembly;

				if(referenceMacros != null)
				{
					foreach(var kvp in referenceMacros)
						path = path.Replace(kvp.Key, kvp.Value);
				}

				parameters.ReferencedAssemblies.Add(path);
			}

			CompilerResults res = provider.CompileAssemblyFromFile(parameters, target.GetSourceFiles());

			Console.WriteLine(string.Format("{0} results:", target.OutputAssembly));

			if(res.Errors.Count > 0)
			{
				Console.WriteLine("  Errors:");

				foreach(CompilerError ce in res.Errors)
					Console.WriteLine(string.Format("\t{0}", ce.ToString()));
			}

			Console.WriteLine("Path: " + res.PathToAssembly);

			return res.Errors.Count < 1;
		}
	}
}
