using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;

namespace ProBuilder.BuildSystem
{
	/**
	 *	Describes a build target for an assembly.
	 */
	public class AssemblyTarget
	{
		// Path to source code root directory. Files will be added recursively.
		public string SourceDirectory;

		// Path to build assembly to.
		public string OutputAssembly;

		// Additional assemblies to reference.
		public List<string> ReferencedAssemblies;

		// Additional scripting defines.
		public List<string> Defines;

		/**
		 *	Get the source files for this build target.
		 */
		public string[] GetSourceFiles()
		{
			return Directory.GetFiles(SourceDirectory, "*.cs", SearchOption.AllDirectories);
		}

		public void Replace(string key, string value)
		{
			SourceDirectory = SourceDirectory.Replace(key, value);
			OutputAssembly = OutputAssembly.Replace(key, value);
			for(int i = 0; i < (ReferencedAssemblies != null ? ReferencedAssemblies.Count : 0); i++)
				ReferencedAssemblies[i] = ReferencedAssemblies[i].Replace(key, value);
		}
	}
}
