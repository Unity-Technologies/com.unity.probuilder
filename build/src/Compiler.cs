using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace ProBuilder.BuildSystem
{
	public static class Compiler
	{
		/**
		 *	Build a DLL with BuildAssemblyTarget.
		 */
		public static CompilerResults CompileDLL(AssemblyTarget target, bool isDebug = false)
		{
			CSharpCodeProvider provider = new CSharpCodeProvider();

			CompilerParameters parameters = new CompilerParameters();
			parameters.GenerateExecutable = false;
			parameters.OutputAssembly = target.OutputAssembly;
			parameters.GenerateInMemory = false;
			parameters.IncludeDebugInformation = isDebug;

			foreach(string assembly in target.ReferencedAssemblies)
				parameters.ReferencedAssemblies.Add(assembly);

			CompilerResults res = provider.CompileAssemblyFromFile(parameters, target.GetSourceFiles());

			Console.WriteLine(string.Format("{0} results:", target.OutputAssembly);

			if(res.Errors.Count > 0)
			{
				Console.WriteLine("Errors:");

				foreach(CompilerError ce in res.Errors)
					Console.WriteLine(string.Format("\t{0}", ce.ToString()));
			}

			Console.WriteLine("Path: " + res.PathToAssembly);

			return res;
		}
	}
}
