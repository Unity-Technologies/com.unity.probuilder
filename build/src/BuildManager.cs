using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJson;

/**
 *	Core build system.
 */
namespace ProBuilder.BuildSystem
{
	/**
	 *	Build system for ProBuilder, capable of creating Unity projects & UnityPackage files.
	 */
	public static class BuildManager
	{
		static int Main(string[] args)
		{
			List<BuildTarget> m_Targets = new List<BuildTarget>();

			bool m_IsDebug = false;

			// Read in build targets
			foreach(string arg in args)
			{
				if(arg.StartsWith("-debug"))
				{
					m_IsDebug = true;
				}
				// No valid argument prefix, treat this input as a build target
				else
				{
					try
					{
						BuildTarget t = SimpleJson.SimpleJson.DeserializeObject<BuildTarget>(File.ReadAllText(arg));
							
						if(t.Macros == null)
							t.Macros = new Dictionary<string, string>();

						t.Macros.Add("$TARGET_DIR", new FileInfo(arg).Directory.FullName.Replace("\\", "/"));

						m_Targets.Add(t);
					}
					catch (System.Exception e)
					{
						Console.WriteLine(string.Format("Failed adding built target: {0}\n{1}", arg, e.ToString()));
					}
				}
			}

			// Execute build targets
			foreach(BuildTarget target in m_Targets)
			{
			    string m_UnityPath = target.GetUnityPath();

			    if(string.IsNullOrEmpty(m_UnityPath))
			    {
			    	Console.WriteLine(string.Format("Build target {0} has invalid Unity path. Skipping.\nMac: {1}\nWindows: {2}",
			    		target.Name,
			    		target.UnityContentsPath,
			    		target.UnityDataPath ));

			    	continue;
			    }

			    // Define Unity contents path macro based on GetUnityPath (can be different on Mac/Windows)
				target.Macros.Add("$UNITY", m_UnityPath);

				foreach(var kvp in target.Macros)
					Console.WriteLine(kvp.Key + " : " + kvp.Value);

				// Find/Replace macros in build target strings
			    target.ExpandMacros();

			    if(target.OnPreBuild != null)
			    {
				    foreach(BuildCommand command in target.OnPreBuild)
				    	BuildCommandEvaluator.Execute(command);
			    }

				foreach(AssemblyTarget at in target.Assemblies)
				{
					if(!Compiler.CompileDLL(at, m_IsDebug))
					{
						// If `Release` build do not continue when compiler throws any wornings or errors.
						if(!m_IsDebug)
						{
							Console.WriteLine(string.Format("Assembly {0} failed compilation. Stopping build.", at.OutputAssembly));
							return 1;
						}
						else
						{
							Console.WriteLine(string.Format("Assembly {0} failed compilation.", at.OutputAssembly));
						}
					}
				}

				if(target.OnPostBuild != null)
				{
					foreach(BuildCommand command in target.OnPostBuild)
						BuildCommandEvaluator.Execute(command);
				}
			}

			return 0;
		}
	}
}
