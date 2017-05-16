using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder.BuildSystem
{
	public static class ReferenceUtility
	{
		// Unity & mono directories are included by default.
		public static readonly List<string> DefaultReferenceSearchPaths = new List<string>()
		{
			"$UNITY/Editor/Data/Mono/lib/mono/2.0",
			"$UNITY/Editor/Data/Managed",
			"$UNITY/Contents/Mono/lib/mono/2.0",
			"$UNITY/Contents/Managed",
			"$UNITY/Contents/Frameworks/Mono/lib/mono/2.0",
			"$UNITY/Contents/Frameworks/Managed"
		};

		// UnityEngine & mono assemblies are included by default.
		public static readonly List<string> DefaultReferencedAssemblies = new List<string>()
		{
			"mscorlib.dll",
			"System.dll",
			"System.Core.dll",
			"UnityEngine.dll"
		};

		/**
		 *	Scan searchDirectories for fileName.
		 */
		public static string ResolveFile(string fileName, List<string> searchDirectories)
		{
			if( File.Exists(fileName) )
				return fileName;

			foreach(string dir in searchDirectories)
			{
				if( Directory.Exists(dir) )
				{
					string path = string.Format("{0}/{1}", dir, fileName);

					if(File.Exists(path))
						return path;
				}
			}

			return null;
		}

		public static string ResolveDirectory(List<string> possiblePaths)
		{
			foreach(string path in possiblePaths)
				if( Directory.Exists(path) )
					return path;

			return null;
		}
	}
}
