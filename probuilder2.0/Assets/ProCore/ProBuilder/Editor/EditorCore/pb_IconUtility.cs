using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	[InitializeOnLoad]
	public static class pb_IconUtility
	{
		const string ICON_FOLDER_PATH = "ProBuilder/Icons";

		static pb_IconUtility()
		{
			if(!Directory.Exists(iconFolderPath))
			{
				string folder = FindFolder(ICON_FOLDER_PATH);

				if(Directory.Exists(folder))
					iconFolderPath = folder;
			}
		}

		private static string FindFolder(string folder)
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

			Debug.LogError("Could not locate ProBuilder/Icons folder.  The ProBuilder folder may be moved, but the contents of this folder must remain unmodified.");

			return null;
		}

		private static Dictionary<string, Texture2D> m_icons = new Dictionary<string, Texture2D>();
		private static string iconFolderPath = "Assets/ProCore/ProBuilder/GUI/Icons/";

		public static Texture2D GetIcon(string iconName)
		{
			Texture2D icon = null;

			if(!EditorGUIUtility.isProSkin && !(iconName.EndsWith("_disabled") || iconName.EndsWith("_Light")))
			{
				icon = GetIcon(string.Format("{0}_Light", iconName.Replace(".png", "")));

				if(icon != null)
					return icon;
			}

			if(iconName.EndsWith("_Light_disabled"))
				iconName = iconName.Replace("_Light_disabled", "_disabled");

			if(!m_icons.TryGetValue(iconName, out icon))
			{
				string fullPath = iconFolderPath + iconName;

				if(!fullPath.EndsWith(".png"))
					fullPath += ".png";

				icon = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
				
				if(icon == null)
				{
					Debug.LogWarning("failed to find icon: " + fullPath);
					m_icons.Add(iconName, null);
					return null;
				}

				m_icons.Add(iconName, icon);
			}

			return icon;
		}

	}
}
