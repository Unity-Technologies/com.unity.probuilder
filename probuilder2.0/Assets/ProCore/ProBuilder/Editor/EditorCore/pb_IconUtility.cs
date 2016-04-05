using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	public enum IconSkin
	{
		Default,
		Light,
		Pro
	};

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

		public static Texture2D GetIcon(string iconName, IconSkin skin = IconSkin.Default)
		{
			int ext = iconName.LastIndexOf('.');
			string nameWithoutExtension = ext < 0 ? iconName : iconName.Substring(0, ext);
			Texture2D icon = null;

			if( !nameWithoutExtension.EndsWith("_disabled") )
			{
				switch(skin)
				{
					case IconSkin.Default:

						if( !EditorGUIUtility.isProSkin && !nameWithoutExtension.EndsWith("_Light") )
						{
							icon = GetIcon(string.Format("{0}_Light", nameWithoutExtension));

							if(icon != null)
								return icon;
						}
						break;

					case IconSkin.Pro:
						if(nameWithoutExtension.EndsWith("_Light"))
							nameWithoutExtension = nameWithoutExtension.Replace("_Light", "");
						break;

					case IconSkin.Light:
						if(!nameWithoutExtension.EndsWith("_Light"))
							nameWithoutExtension = string.Format("{0}_Light", nameWithoutExtension);
						break;
				}

				if(icon != null)
					return icon;
			}
			else if(nameWithoutExtension.EndsWith("_Light_disabled"))
			{
				nameWithoutExtension = nameWithoutExtension.Replace("_Light_disabled", "_disabled");
			}

			if(!m_icons.TryGetValue(nameWithoutExtension, out icon))
			{
				string fullPath = iconFolderPath + nameWithoutExtension;

				if(!fullPath.EndsWith(".png"))
					fullPath += ".png";

				icon = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
				
				if(icon == null)
				{
					Debug.LogWarning("failed to find icon: " + fullPath);
					m_icons.Add(nameWithoutExtension, null);
					return null;
				}

				m_icons.Add(nameWithoutExtension, icon);
			}

			return icon;
		}

	}
}
