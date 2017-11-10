using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProBuilder2.Common;
using Object = UnityEngine.Object;

namespace ProBuilder2.EditorCommon
{
	/// <summary>
	/// Helper functions for working with files and directories.
	/// </summary>
	static class pb_FileUtil
	{
		// ProBuilder folder path.
		private static string m_ProBuilderFolderPath = "unitypackagemanager/com.unity.probuilder/ProCore/ProBuilder/";

		private static string m_ProBuilderDataPath = "Assets/ProBuilder Data/";

		// The order is important - always search for the package manager installed version first
		private static string[] k_PossibleInstallDirectories = new string[]
		{
			// 2018.1 package manager
			"packages/",
			// todo remove
			"unitypackagemanager/",
			// installed from asset store or source
			"Assets/",
		};

		/// <summary>
		/// Check that the directory contains a valid ProBuilder install.
		/// </summary>
		/// <param name="dir">Directory to check</param>
		/// <returns></returns>
		private static bool ValidateProBuilderRoot(string dir)
		{
			return Directory.Exists(dir + "/Classes") &&
				Directory.Exists(dir + "/Icons") &&
				Directory.Exists(dir + "/Editor") &&
				Directory.Exists(dir + "/Shader");
		}

		/// <summary>
		/// Return a relative path to the ProCore/ProBuilder directory.
		/// </summary>
		/// <returns></returns>
		private static string GetRootDir()
		{
			if (Directory.Exists(m_ProBuilderFolderPath))
				return m_ProBuilderFolderPath;

			foreach (var install in k_PossibleInstallDirectories)
			{
				m_ProBuilderFolderPath = string.Format("{0}{1}", install, "ProCore/ProBuilder/");

				if (ValidateProBuilderRoot(m_ProBuilderFolderPath))
					return m_ProBuilderFolderPath;
			}

			// It's not in any of the usual haunts, start digging through Assets until we find it
			string[] matches = Directory.GetDirectories("Assets", "ProBuilder", SearchOption.AllDirectories);

			foreach (var match in matches)
			{
				m_ProBuilderFolderPath = match.Replace("\\", "/") +  "/";

				if (m_ProBuilderFolderPath.Contains("ProBuilder") && ValidateProBuilderRoot(m_ProBuilderFolderPath))
					return m_ProBuilderFolderPath;
			}

			// Things are dire. ProBuilder was nowhere to be found in the Assets directory, which means either the user
			// has renamed the folder, or something very spooky is going on.
			// Either way, just create a new ProCore/ProBuilder folder in Assets and return that so at the very least
			// local preferences and the material/color palettes will still work.
			Debug.LogWarning("Creating a new ProBuilder directory. This probably means the ProBuilder folder was renamed. Icons & preferences may not work in this state.");
			m_ProBuilderFolderPath = "Assets/ProCore/ProBuilder";
			Directory.CreateDirectory(m_ProBuilderFolderPath);

			return m_ProBuilderFolderPath;
		}

		/// <summary>
		/// Get the path to the local ProBuilder/Data folder
		/// </summary>
		/// <returns></returns>
		internal static string GetLocalDataDirectory(bool initializeIfMissing = false)
		{
			if (Directory.Exists(m_ProBuilderDataPath))
				return m_ProBuilderDataPath;

			string root = GetRootDir();

			if (root.StartsWith("Assets"))
			{
				// Installed from Asset Store or manual package import
				m_ProBuilderDataPath = root + "Data/";
			}
			else
			{
				// Scan project for ProBuilder Data folder
				// none found? create one at root
				string[] matches = Directory.GetDirectories("Assets", "ProBuilder Data", SearchOption.AllDirectories);
				m_ProBuilderDataPath = matches.Length > 0 ? matches[0] : "Assets/ProBuilder Data/";
			}

			if (!Directory.Exists(m_ProBuilderDataPath) && initializeIfMissing)
				Directory.CreateDirectory(m_ProBuilderDataPath);

			return m_ProBuilderDataPath;
		}

		internal static string[] FindAssets<T>(string pattern) where T : UnityEngine.Object
		{
			return AssetDatabase.FindAssets("t:" + typeof(T).ToString());
		}

		internal static T[] FindAndLoadAssets<T>() where T : UnityEngine.Object
		{
			return AssetDatabase.FindAssets("t:" + typeof(T).ToString())
				.Select(x => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(x))).ToArray();
		}

		internal static T FindAssetOfType<T>() where T : UnityEngine.Object
		{
			foreach (var i in AssetDatabase.FindAssets("t:" + typeof(T).ToString()))
			{
				T o = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(i));
				if (o != null)
					return o;
			}

			return null;
		}

		internal static string GetSelectedDirectory()
		{
			Object o = Selection.activeObject;

			if (o != null)
			{
				string path = AssetDatabase.GetAssetPath(o.GetInstanceID());

				if (!string.IsNullOrEmpty(path))
				{
					if (Directory.Exists(path))
						return Path.GetFullPath(path);

					string res = Path.GetDirectoryName(path);

					if (!string.IsNullOrEmpty(res) && System.IO.Directory.Exists(res))
						return Path.GetFullPath(res);
				}
			}

			return Path.GetFullPath("Assets");
		}
		/// <summary>
		/// Get a file or folder path relative to the project directory.
		/// </summary>
		/// <param name="path">File or directory path, either relative or absolute.</param>
		/// <returns>A new path relative to the current project root.</returns>
		public static string GetRelativePath(string path)
		{
			string full = Path.GetFullPath(path).Replace("\\", "/");
			string cur = Directory.GetCurrentDirectory().Replace("\\", "/");
			return full.Replace(cur, "");
		}

		private static float OverlapCoefficient(string left, string right)
		{
			HashSet<char> a = new HashSet<char>(left.Select(x=>x));
			HashSet<char> b = new HashSet<char>(right.Select(x=>x));
			a.IntersectWith(b);
			return (float) a.Count / Mathf.Min(left.Length, right.Length);
		}

		/// <summary>
		/// Find a file in the Assets folder by searching for a partial path.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static string FindFile(string file)
		{
			if (string.IsNullOrEmpty(file))
				return null;
			string nameWithExtension = Path.GetFileName(file);
			string forwardFile = file.Replace("\\", "/");
			var bestMatch = new pb_Tuple<float, string>(0f, null);

			foreach (var dir in k_PossibleInstallDirectories)
			{
				if (!Directory.Exists(dir))
					continue;

				string[] matches = Directory.GetFiles(dir, nameWithExtension, SearchOption.AllDirectories);

				foreach (var str in matches)
				{
					if (!str.Contains(forwardFile))
						continue;

					float oc = OverlapCoefficient(forwardFile, GetRelativePath(str));

					if (oc > bestMatch.Item1)
					{
						bestMatch.Item1 = oc;
						bestMatch.Item2 = str;
					}
				}
			}

			return bestMatch.Item2;
		}

		/// <summary>
		/// Check if a file or folder exists at path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool Exists(string path)
		{
			return Directory.Exists(path) || File.Exists(path);
		}

		/// <summary>
		/// Load an internal asset from the ProBuilder directory.
		/// </summary>
		/// <param name="path"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal static T LoadInternalAssetRequired<T>(string path) where T : ScriptableObject, pb_IHasDefault
		{
			string full = string.Format("{0}{1}", GetRootDir(), path);
			return LoadRequired<T>(full);
		}

		/// <summary>
		/// Load an internal asset relative to the ProBuilder directory.
		/// </summary>
		/// <param name="path"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal static T LoadInternalAsset<T>(string path) where T : Object
		{
			string full = string.Format("{0}{1}", GetRootDir(), path);
			return Load<T>(full);
		}

		/// <summary>
		/// Fetch a default asset from path.  If not found, a new one is created.
		/// </summary>
		/// <param name="path"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal static T LoadRequired<T>(string path) where T : ScriptableObject, pb_IHasDefault
		{
			T asset = Load<T>(path);

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

		private static T Load<T>(string path) where T : Object
		{
			return AssetDatabase.LoadAssetAtPath<T>(path);
		}

		/// <summary>
		/// Write contents to a file path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="contents"></param>
		public static void WriteFile(string path, string contents)
		{
			File.WriteAllText(path, contents);
		}
	}
}
