using System;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace ProBuilder.BuildSystem
{
	public static class Compiler
	{
		/**
		 *	Build a DLL with BuildAssemblyTarget. Returns true if compilation succeeded, false if not.
		 */
		public static bool CompileDLL(AssemblyTarget target, bool isDebug = false)
		{
			Log.Status(string.Format(" Compiling {0}", Path.GetFileName(target.OutputAssembly)));
			Log.Info(string.Format("  Reference search paths:\n\t{0}", string.Join("\n\t", target.ReferenceSearchPaths)));
			Log.Info(string.Format("  Reference assemblies:\n\t{0}", string.Join("\n\t", target.ReferencedAssemblies)));

			CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string ,string>() {
					{ "CompilerVersion", "v3.5" }
				});

			CompilerParameters parameters = new CompilerParameters();
			parameters.GenerateExecutable = false;
			parameters.OutputAssembly = target.OutputAssembly;
			parameters.GenerateInMemory = false;
			parameters.IncludeDebugInformation = isDebug;
			parameters.TreatWarningsAsErrors = !isDebug;
			// We're targeting .NET 3.5 framework - the mscorlib, System, and System.Core libs
			// should be included in the referenced assemblies list.
			if(target.Defines != null && target.Defines.Count > 0)
				parameters.CompilerOptions = string.Format("/nostdlib /define:{0}", string.Join(";", target.Defines.ToArray()));
			else
				parameters.CompilerOptions = "/nostdlib";

		    // Console.WriteLine("CompilerOptions: " + parameters.CompilerOptions);

			foreach(string assembly in target.ReferencedAssemblies)
			{
				string assemblyPath = ReferenceUtility.ResolveFile(assembly, target.ReferenceSearchPaths);

				if(string.IsNullOrEmpty(assemblyPath))
				{
					Log.Critical(string.Format("  Could not find referenced assembly: {0}", assembly));
					return false;
				}

				Log.Info("  Adding reference: " + assemblyPath);
				parameters.ReferencedAssemblies.Add(assemblyPath);
			}

			CompilerResults res = provider.CompileAssemblyFromFile(parameters, target.GetSourceFiles());

			if(res.Errors.HasWarnings)
			{
				Log.Warning("  Warnings:");

				foreach(CompilerError ce in res.Errors)
				{
					if(ce.IsWarning)
						Log.Warning(string.Format("\t{0}", ce.ToString()));
				}
			}

			if(res.Errors.HasErrors)
			{
				Log.Critical("  Errors:");

				foreach(CompilerError ce in res.Errors)
				{
					if(!ce.IsWarning)
						Log.Critical(string.Format("\t{0}", ce.ToString()));
				}
			}
			else
			{
				Log.Info(string.Format("  Success: {0}", target.OutputAssembly));
			}

			return !res.Errors.HasErrors;
		}
	}
}
