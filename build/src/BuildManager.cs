using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using SimpleJson;

/**
 *	Core build system.
 */
namespace ProBuilder.BuildSystem
{
	/**
	 * Build system for ProBuilder, capable of creating Unity projects & UnityPackage files.
	 */
	public static class BuildManager
	{
		const string m_VersionInfo = "1.1.0f0";

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

			// Iterate through args once looking for swithes
			foreach(string arg in args)
			{
				if(arg.StartsWith("-debug"))
				{
					m_IsDebug = true;
				}
				else if(arg.StartsWith("-version"))
				{
					Log.Print(m_VersionInfo);
					return 1;
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
						m_UnityPathOverride = m_UnityPathOverride.Substring(0, m_UnityPathOverride.Length - 1);
				}
			}

			// Then read in build targets
			foreach(string arg in args)
			{
				// If no valid argument prefix, treat this input as a build target
				if(arg.StartsWith("-debug") || arg.StartsWith("-version") || arg.StartsWith("-silent") || arg.StartsWith("-verbose") || arg.StartsWith("-unity="))
				{
					continue;
				}
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

				target.Macros.Add("$USER", Environment.UserName);

				string m_UnityPath = m_UnityPathOverride;

				if(string.IsNullOrEmpty(m_UnityPath) || !Directory.Exists(m_UnityPath))
				{
					foreach(var kvp in target.Macros)
						for(int i = 0; i < target.UnityPath.Count; i++)
							target.UnityPath[i] = target.UnityPath[i].Replace(kvp.Key, kvp.Value);

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

			    // if debugging manually add the DEBUG define
			    if(m_IsDebug && !target.Defines.Contains("DEBUG"))
			    	target.Defines.Add("DEBUG");

			    Log.Info("Defines:");

			    foreach(var d in target.Defines)
			    	Log.Info(string.Format("  {0}", d));

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

		private static BuildTarget TryReadBuildTarget(string path, bool allowRecurse = true)
		{
			try
			{
				StringBuilder sb = new StringBuilder();

				foreach(string line in File.ReadAllLines(path))
				{
					string trim = line.Trim();

					if( !trim.StartsWith("//") && !trim.StartsWith("#") )
						sb.AppendLine(line);
				}

				BuildTarget t = SimpleJson.SimpleJson.DeserializeObject<BuildTarget>(sb.ToString());

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

				if(!string.IsNullOrEmpty(t.Base))
				{
					if(!allowRecurse)
					{
						Log.Critical("Base build target base recursion > 2. Currently this is not allowed.");
						return null;
					}

					FileInfo fi = new FileInfo(path);
					BuildTarget b = null;

					if(fi != null)
					{
						string base_path = Path.Combine(fi.Directory.FullName, t.Base);
						b = TryReadBuildTarget(base_path, false);
						Log.Info(string.Format("{0} setting base target to {1}", Path.GetFileNameWithoutExtension(path), base_path));
					}
					else
					{
						b = TryReadBuildTarget(t.Base, false);
						Log.Info(string.Format("{0} setting base target to {1}", Path.GetFileNameWithoutExtension(path), t.Base));
					}

					if(b != null)
					{
						b.OverwriteWith(t);
						t = b;
					}
				}


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
