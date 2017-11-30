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

		// A path to another build target that should form the base of this one.
		// Any defined properties in this target will override base.
		public string Base;

		// If `Base` is defined any properties listed in the `AppendToBase` will be appended to the base value instead
		// of overwritten (exempting Name, which is always overwritten).
		public List<string> AppendToBase;

		// Possible paths to Unity directory (`%USER%\Program Files\Unity` or `/Applications/Unity.app`).
		public List<string> UnityPath;

		// Replace key w/ value for all strings.
		// Some pre-defined macros are provided for you:
		// 	- $UNITY = Path to Unity contents folder (resolves UnityContentsPath or UnityDataPath).
		//	- $TARGET_DIR = Directory that this build target json file resides in.
		public Dictionary<string, string> Macros;

		// Directories in which to search for referenced assemblies. Unity & mono paths are included by default.
		public List<string> ReferenceSearchPaths;

		// Referenced assemblies to be applied to each AssemblyTarget in this build.
		// Unity & mono paths are included by default.
		public List<string> ReferencedAssemblies;

		// Symbols to define for each assembly target.
		public List<string> Defines;

		// Commands to be executed prior to compile and pre-build.
		public List<BuildCommand> Clean;

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

			if(Clean != null)
			{
				foreach(BuildCommand bc in Clean)
					bc.Replace(key, value);
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

		/*
		 * Overwrites this properties with any target properties that are non-null and non-empty.
		 */
		public void OverwriteWith(BuildTarget target)
		{
			if(!string.IsNullOrEmpty(target.Name))
				this.Name = target.Name;

			if( target.UnityPath != null && target.UnityPath.Count > 0 )
				if(target.AppendToBase != null && target.AppendToBase.Contains("UnityPath"))
					this.UnityPath.AddRange(target.UnityPath);
				else
					this.UnityPath = target.UnityPath;

			if( target.Macros != null && target.Macros.Count > 0 )
			{
				if(target.AppendToBase != null && target.AppendToBase.Contains("Macros"))
				{
					foreach(var kvp in this.Macros)
						if(!target.Macros.ContainsKey(kvp.Key))
							target.Macros.Add(kvp.Key, kvp.Value);
				}

				this.Macros = target.Macros;
			}

			if( target.ReferenceSearchPaths != null && target.ReferenceSearchPaths.Count > 0 )
				if(target.AppendToBase != null && target.AppendToBase.Contains("ReferenceSearchPaths"))
					this.ReferenceSearchPaths.AddRange(target.ReferenceSearchPaths);
				else
					this.ReferenceSearchPaths = target.ReferenceSearchPaths;

			if( target.ReferencedAssemblies != null && target.ReferencedAssemblies.Count > 0 )
				if(target.AppendToBase != null && target.AppendToBase.Contains("ReferencedAssemblies"))
					this.ReferencedAssemblies.AddRange(target.ReferencedAssemblies);
				else
					this.ReferencedAssemblies = target.ReferencedAssemblies;

			if( target.Defines != null && target.Defines.Count > 0 )
				if(target.AppendToBase != null && target.AppendToBase.Contains("Defines"))
					this.Defines.AddRange(target.Defines);
				else
					this.Defines = target.Defines;

			if( target.Clean != null && target.Clean.Count > 0 )
				if(target.AppendToBase != null && target.AppendToBase.Contains("Clean"))
					this.Clean.AddRange(target.Clean);
				else
					this.Clean = target.Clean;

			if( target.OnPreBuild != null && target.OnPreBuild.Count > 0 )
				if(target.AppendToBase != null && target.AppendToBase.Contains("OnPreBuild"))
					this.OnPreBuild.AddRange(target.OnPreBuild);
				else
					this.OnPreBuild = target.OnPreBuild;

			if( target.OnPostBuild != null && target.OnPostBuild.Count > 0 )
				if(target.AppendToBase != null && target.AppendToBase.Contains("OnPostBuild"))
					this.OnPostBuild.AddRange(target.OnPostBuild);
				else
					this.OnPostBuild = target.OnPostBuild;

			if( target.Assemblies != null && target.Assemblies.Count > 0 )
				if(target.AppendToBase != null && target.AppendToBase.Contains("Assemblies"))
					this.Assemblies.AddRange(target.Assemblies);
				else
					this.Assemblies = target.Assemblies;
		}
	}
}
