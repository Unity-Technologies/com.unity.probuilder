using UnityEngine;
using UnityEditor;
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
		// ProBuilder folder path.
		private static string m_ProBuilderFolderPath = "Assets/ProCore/ProBuilder/";

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
		public static string FindFolder(string folder, bool exactMatch = false)
		{
			string single = folder.Replace("\\", "/").Substring(folder.LastIndexOf('/') + 1);

			string[] matches = Directory.GetDirectories("Assets/", single, SearchOption.AllDirectories);

			foreach(string str in matches)
			{
				string path = str.Replace("\\", "/");

				if( path.Contains(folder) )
				{
					if(exactMatch)
					{
						string found = path.Substring(str.LastIndexOf('/') + 1);

						if(!found.Equals(single))
							continue;
					}

					if(!path.EndsWith("/"))
						path += "/";

					return path;
				}
			}

			return null;
		}

		/**
		 *	Check if a file or folder exists at path.
		 */
		public static bool Exists(string path)
		{
			return Directory.Exists(path) || File.Exists(path);
		}

		/**
		 *	Find root ProBuilder folder.
		 */
		public static string GetRootDir()
		{
			if( !Exists(m_ProBuilderFolderPath) )
				m_ProBuilderFolderPath = FindFolder("ProBuilder", true);

			return m_ProBuilderFolderPath;
		}

		/**
		 *	Load a scriptable object from a path relative to ProBuilder root.
		 */
		public static T LoadRequiredRelative<T>(string path) where T : ScriptableObject, pb_IHasDefault
		{
			string full = string.Format("{0}{1}", GetRootDir(), path);
			return LoadRequired<T>(full);
		}

		/**
		 *	Fetch a default asset from path.  If not found, a new one is created.
		 */
		public static T LoadRequired<T>(string path) where T : ScriptableObject, pb_IHasDefault
		{
#if UNITY_4_7 || UNITY_5_0
			T asset = (T) AssetDatabase.LoadAssetAtPath(path, typeof(T));
#else
			T asset = AssetDatabase.LoadAssetAtPath<T>(path);
#endif

			if(asset == null)
			{
				asset = ScriptableObject.CreateInstance<T>();

				asset.SetDefaultValues();

				EditorUtility.SetDirty(asset);

				string folder = Path.GetDirectoryName(path);

				if(!Directory.Exists(folder))
					Directory.CreateDirectory(folder);

				AssetDatabase.CreateAsset(asset, path);
			}

			return asset;
		}
	}
}
