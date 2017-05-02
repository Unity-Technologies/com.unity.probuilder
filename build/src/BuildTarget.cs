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
	public class BuildTarget
	{
		// Name of this build target (ex, ProBuilder Unity 5.5).
		public string Name;

		// Path to Unity directory that contains Mono and Managed folders. Mac path.
		// See also: UnityDataPath
		public string UnityContentsPath;

		// Path to Unity directory that contains Mono and Managed folders. Windows path.
		// See also: UnityContentsPath
		public string UnityDataPath;

		// Replace key w/ value for all strings.
		public Dictionary<string, string> Macros;

		// Assemblies to be built as part of this target.
		public List<AssemblyTarget> Assemblies;

		// Commands to be executed prior to compiling DLLs.
		public List<BuildCommand> OnPreBuild;

		// Commands to be executed after DLLs are built.
		public List<BuildCommand> OnPostBuild;

		/**
		 *	Get the path to Unity contents folder (resolves UnityContentsPath or UnityDataPath).
		 */
		public string GetUnityPath()
		{
			if(Directory.Exists(UnityContentsPath))
				return UnityContentsPath;
			else if(Directory.Exists(UnityDataPath))
				return UnityDataPath;
			return null;
		}

		public void ExpandMacros()
		{
			foreach(var macro in Macros)
			{
				UnityContentsPath = UnityContentsPath.Replace(macro.Key, macro.Value);

				UnityDataPath = UnityDataPath.Replace(macro.Key, macro.Value);

				if(Assemblies != null)
				{
					foreach(AssemblyTarget target in Assemblies)
						target.Replace(macro.Key, macro.Value);
				}

				if(OnPreBuild != null)
				{
					foreach(BuildCommand bc in OnPreBuild)
						bc.Replace(macro.Key, macro.Value);
				}

				if(OnPostBuild != null)
				{
					foreach(BuildCommand bc in OnPostBuild)
						bc.Replace(macro.Key, macro.Value);
				}
			}
		}
	}
}
