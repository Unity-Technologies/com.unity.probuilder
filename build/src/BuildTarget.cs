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

		// Assemblies to be built as part of this target.
		public List<AssemblyTarget> Assemblies;
	}
}
