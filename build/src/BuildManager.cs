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
			string m_UnityPath = null;

			// Read in build targets
			foreach(string arg in args)
			{
				if(arg.StartsWith("-debug"))
				{
					m_IsDebug = true;
					Log.Level = Log.Level | LogLevel.Info;
				}
				else if(arg.StartsWith("-unity="))
				{
					m_UnityPath = arg.Replace("-unity=", "").Trim().Replace("\\", "/");

					if(m_UnityPath.EndsWith("/"))
						m_UnityPath = m_UnityPath.Substring(m_UnityPath.Length - 1);
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
				Log.Info("Build: " + target.Name);

				if(string.IsNullOrEmpty(m_UnityPath) || !Directory.Exists(m_UnityPath))
					m_UnityPath = ReferenceUtility.ResolveDirectory(target.UnityPath);

			    if(string.IsNullOrEmpty(m_UnityPath))
			    {
			    	Console.WriteLine(string.Format("Build target {0} has invalid Unity path. ({1})\n",
			    		target.Name,
			    		m_UnityPath ?? "null"));
			    	continue;
			    }

			    // Define Unity contents path macro based on GetUnityPath (can be different on Mac/Windows)
				target.Macros.Add("$UNITY", m_UnityPath);

				Log.Info("Macros:");

				// Find/Replace macros in build target strings
				foreach(var kvp in target.Macros)
				{
					Log.Info(string.Format("  {0} = {1}", kvp.Key, kvp.Value));
			    	target.Replace(kvp.Key, kvp.Value);
			    }

			    Log.Info("OnPreBuild");

			    if(target.OnPreBuild != null && target.OnPreBuild.Count > 0)
			    {
				    foreach(BuildCommand command in target.OnPreBuild)
				    {
			    		Log.Info("  " + command.ToString());
				    	BuildCommandEvaluator.Execute(command);
				    }
			    }
			    else
			    {
			    	Log.Info("  (no commands)");
			    }

				foreach(AssemblyTarget at in target.Assemblies)
				{
					if(at.ReferenceSearchPaths == null)
						at.ReferenceSearchPaths = new List<string>(target.ReferenceSearchPaths);
					else
						at.ReferenceSearchPaths.AddRange(target.ReferenceSearchPaths);

					if(at.ReferencedAssemblies == null)
						at.ReferencedAssemblies = new List<string>(target.ReferencedAssemblies);
					else
						at.ReferencedAssemblies.AddRange(target.ReferencedAssemblies);

					if(at.Defines == null)
						at.Defines = new List<string>(target.Defines);
					else
						at.Defines.AddRange(target.Defines);

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

			    Log.Info("OnPostBuild");

				if(target.OnPostBuild != null && target.OnPostBuild.Count > 0)
				{
					foreach(BuildCommand command in target.OnPostBuild)
					{
			    		Log.Info("  " + command.ToString());
						BuildCommandEvaluator.Execute(command);
					}
				}
				else
				{
			    	Log.Info("  (no commands)");
				}
			}

			return 0;
		}
	}
}
