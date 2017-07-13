using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			string m_UnityPathOverride = null;

			if(args == null || args.Length < 1 || args.Any(x => x.Contains("--help") || x.Contains("-help")))
			{
				Console.WriteLine(HelpText.Contents);
				return 0;
			}

			// Read in build targets
			foreach(string arg in args)
			{
				if(arg.StartsWith("-debug"))
				{
					m_IsDebug = true;
				}
				else if(arg.StartsWith("-silent"))
				{
					Log.Level = LogLevel.None;
				}
				else if(arg.StartsWith("-verbose"))
				{
					Log.Level = LogLevel.All;

				}
				else if(arg.StartsWith("-unity="))
				{
					m_UnityPathOverride = arg.Replace("-unity=", "").Trim().Replace("\\", "/");

					if(m_UnityPathOverride.StartsWith("\"") && m_UnityPathOverride.EndsWith("\""))
						m_UnityPathOverride = m_UnityPathOverride.Substring(1, m_UnityPathOverride.Length - 2);

					if(m_UnityPathOverride.EndsWith("/"))
						m_UnityPathOverride = m_UnityPathOverride.Substring(m_UnityPathOverride.Length - 1);
				}
				// No valid argument prefix, treat this input as a build target
				else
				{
					if( ReferenceUtility.IsDirectory(arg) && Directory.Exists(arg) )
					{
						foreach(string t in Directory.GetFiles(arg, "*.json", SearchOption.TopDirectoryOnly))
						{
							BuildTarget result = TryReadBuildTarget(t);
							if(result != null)
								m_Targets.Add(result);
						}
					}
					else
					{
						BuildTarget result = TryReadBuildTarget(arg);
						if(result != null)
							m_Targets.Add(result);

					}
				}
			}

			int success = 0;

			// Execute build targets
			foreach(BuildTarget target in m_Targets)
			{
				Log.Status("Build: " + target.Name);

				string m_UnityPath = m_UnityPathOverride;

				if(string.IsNullOrEmpty(m_UnityPath) || !Directory.Exists(m_UnityPath))
				{
					m_UnityPath = ReferenceUtility.ResolveDirectory(target.UnityPath);

				    if(string.IsNullOrEmpty(m_UnityPath) || !Directory.Exists(m_UnityPath))
				    {
				    	Console.WriteLine(string.Format("Build target {0} has invalid Unity path: ({1})\n  {2}",
				    		target.Name,
				    		m_UnityPath ?? "null",
				    		target.UnityPath != null ? string.Join("\n  ", target.UnityPath) : "  null"));
				    	continue;
				    }
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

				bool targetBreak = false;

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
						Log.Critical(string.Format("  Assembly {0} failed compilation.", at.OutputAssembly));
						targetBreak = true;
						break;
					}
				}

				if(targetBreak)
					break;

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

				Log.Info("");

				success++;
			}

			int targetCount = m_Targets == null ? 0 : m_Targets.Count;

			Log.Status(string.Format("\n{2}\nBuild Complete: {0} / {1} targets built successfully.\n{2}",
				success,
				targetCount,
				"========================================================"));

			return success - targetCount;
		}

		private static BuildTarget TryReadBuildTarget(string path)
		{
			try
			{
				BuildTarget t = SimpleJson.SimpleJson.DeserializeObject<BuildTarget>(File.ReadAllText(path));

				if(t.Macros == null)
					t.Macros = new Dictionary<string, string>();

				t.Macros.Add("$TARGET_DIR", new FileInfo(path).Directory.FullName.Replace("\\", "/"));

				if(t.ReferenceSearchPaths == null)
					t.ReferenceSearchPaths = new List<string>(ReferenceUtility.DefaultReferenceSearchPaths);
				else
					t.ReferenceSearchPaths.AddRange(ReferenceUtility.DefaultReferenceSearchPaths);

				if(t.ReferencedAssemblies == null)
					t.ReferencedAssemblies = new List<string>(ReferenceUtility.DefaultReferencedAssemblies);
				else
					t.ReferencedAssemblies.AddRange(ReferenceUtility.DefaultReferencedAssemblies);

				return t;
			}
			catch(System.Exception e)
			{
				Log.Critical(string.Format("Failed adding built target: {0}\n{1}", path, e.ToString()));
			}

			return null;
		}
	}
}
