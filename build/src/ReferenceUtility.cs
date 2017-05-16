using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder.BuildSystem
{
	public static class ReferenceUtility
	{
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
