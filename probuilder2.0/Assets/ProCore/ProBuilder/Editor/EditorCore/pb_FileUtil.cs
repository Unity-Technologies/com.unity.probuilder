using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Helper functions for working with files and directories.
	 */
	public static class pb_FileUtil
	{
		/**
		 *	Find a file in the Assets folder by searching for a partial path.
		 */
		public static string FindFile(string file)
		{
			string name = Path.GetFileName(file);
			string[] matches = Directory.GetFiles("Assets/", name, SearchOption.AllDirectories);
			string forward_file = file.Replace("\\", "/");
			return matches.FirstOrDefault(x => x.Replace("\\", "/").Contains(forward_file));
		}


		/**
		 *	Find a directory in the Assets folder by searching for a partial path.
		 */
		public static string FindFolder(string folder)
		{
			string single = folder.Replace("\\", "/").Substring(folder.LastIndexOf('/') + 1);

			string[] matches = Directory.GetDirectories("Assets/", single, SearchOption.AllDirectories);

			foreach(string str in matches)
			{
				string path = str.Replace("\\", "/");

				if(path.Contains(folder))
				{
					if(!path.EndsWith("/"))
						path += "/";

					return path;
				}
			}

			return null;
		}
	}
}
