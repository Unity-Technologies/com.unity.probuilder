using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;

namespace ProBuilder.BuildSystem
{
	/**
	 *	Describes a ProBuilder version build target.
	 */
	public class BuildTarget : IExpandMacros
	{
		// Name of this build target (ex, ProBuilder Unity 5.5).
		public string Name;

		// Possible paths to Unity directory (`%USER%\Program Files\Unity` or `/Applications/Unity.app`).
		public List<string> UnityPath;

		// Replace key w/ value for all strings.
		// Some pre-defined macros are provided for you:
		// 	- $UNITY = Path to Unity contents folder (resolves UnityContentsPath or UnityDataPath).
		//	- $TARGET_DIR = Directory that this build target json file resides in.
		public Dictionary<string, string> Macros;

		// Directories in which to search for referenced assemblies.
		public List<string> ReferenceSearchPaths;

		// Referenced assemblies to be applied to each AssemblyTarget in this build.
		public List<string> ReferencedAssemblies;

		// Symbols to define for each assembly target.
		public List<string> Defines;

		// Commands to be executed prior to compiling DLLs.
		public List<BuildCommand> OnPreBuild;

		// Commands to be executed after DLLs are built.
		public List<BuildCommand> OnPostBuild;

		// Assemblies to be built as part of this target.
		public List<AssemblyTarget> Assemblies;

		public void Replace(string key, string value)
		{
			for(int i = 0; i < (ReferenceSearchPaths != null ? ReferenceSearchPaths.Count : 0); i++)
				ReferenceSearchPaths[i] = ReferenceSearchPaths[i].Replace(key, value);

			for(int i = 0; i < (ReferencedAssemblies != null ? ReferencedAssemblies.Count : 0); i++)
				ReferencedAssemblies[i] = ReferencedAssemblies[i].Replace(key, value);

			if(Assemblies != null)
			{
				foreach(AssemblyTarget target in Assemblies)
					target.Replace(key, value);
			}

			if(OnPreBuild != null)
			{
				foreach(BuildCommand bc in OnPreBuild)
					bc.Replace(key, value);
			}

			if(OnPostBuild != null)
			{
				foreach(BuildCommand bc in OnPostBuild)
					bc.Replace(key, value);
			}
		}
	}
}
