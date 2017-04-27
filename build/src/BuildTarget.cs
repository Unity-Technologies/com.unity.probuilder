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

		// Assemblies to be built as part of this target.
		public List<AssemblyTarget> Assemblies;

		// Copy commands to execute following a build.
		public List<CopyTarget> CopyTargets;

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
	}
}
