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
	/**
	 *	Helper functions for working with files and directories.
	 */
	public static class pb_FileUtil
	{
		// ProBuilder folder path.
		private static string m_ProBuilderFolderPath = "unitypackagemanager/com.unity.probuilder/ProCore/ProBuilder/";

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
		public static string GetRootDir()
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

		/**
		 * Find a file in the Assets folder by searching for a partial path.
		 */
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

		/**
		 *	Check if a file or folder exists at path.
		 */
		public static bool Exists(string path)
		{
			return Directory.Exists(path) || File.Exists(path);
		}

		/**
		 *	Returns a new complete path from one relative to the ProBuilder root.
		 */
		public static string PathFromRelative(string relativePath)
		{
			return string.Format("{0}{1}", GetRootDir(), relativePath);
		}

		/**
		 *	Load a scriptable object from a path relative to ProBuilder root. If object is not found
		 *	a new one is created.
		 */
		public static T LoadRequiredRelative<T>(string path) where T : ScriptableObject, pb_IHasDefault
		{
			string full = string.Format("{0}{1}", GetRootDir(), path);
			return LoadRequired<T>(full);
		}

		/**
		 *	Load a scriptable object from a path relative to ProBuilder root. Can return null if asset
		 *	is not found.
		 */
		public static T LoadRelative<T>(string path) where T : Object
		{
			string full = string.Format("{0}{1}", GetRootDir(), path);
			return Load<T>(full);
		}

		/**
		 *	Fetch a default asset from path.  If not found, a new one is created.
		 */
		public static T LoadRequired<T>(string path) where T : ScriptableObject, pb_IHasDefault
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

		/**
		 *	Load an asset from path. Can return null if not found.
		 */
		public static T Load<T>(string path) where T : Object
		{
			return AssetDatabase.LoadAssetAtPath<T>(path);
		}

		/**
		 *	Write contents to a file path.
		 */
		public static void WriteFile(string path, string contents)
		{
			File.WriteAllText(path, contents);
		}
	}
}
