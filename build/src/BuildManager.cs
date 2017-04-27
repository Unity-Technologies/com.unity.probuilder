using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJson;

namespace ProBuilder.BuildSystem
{
	public static class BuildManager
	{
		static int Main(string[] args)
		{
			List<BuildTarget> m_Targets = new List<BuildTarget>();

			bool m_IsDebug = false;

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
						m_Targets.Add(t);
					}
					catch
					{
						Console.WriteLine("Failed adding built target: " + arg);
					}
				}
			}

			foreach(BuildTarget target in m_Targets)
			{
			    string m_UnityPath = target.GetUnityPath();
			    Dictionary<string, string> m_ReferenceMacros = new Dictionary<string, string>();

			    if(string.IsNullOrEmpty(m_UnityPath))
			    {
			    	Console.WriteLine(string.Format("Build target {0} has invalid Unity path. Skipping.\nMac: {1}\nWindows: {2}",
			    		target.Name,
			    		target.UnityContentsPath,
			    		target.UnityDataPath ));

			    	continue;
			    }

			    m_ReferenceMacros.Add("$UNITY", m_UnityPath);

				foreach(AssemblyTarget at in target.Assemblies)
				{
					if(!Compiler.CompileDLL(at, m_ReferenceMacros, m_IsDebug))
					{
						Console.WriteLine(string.Format("Assembly {0} failed compilation. Stopping build.", at.OutputAssembly));
						// return 1;
					}
				}
			}

			return 0;
		}
	}
}
