// #define PB_DEBUG

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
				string folder = pb_FileUtil.FindFolder(ICON_FOLDER_PATH);

				if(string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
					Debug.LogError("Could not locate ProBuilder/Icons folder.  The ProBuilder folder may be moved, but the contents of this folder must remain unmodified relative to ProBuilder root.");
				else
					iconFolderPath = folder;
			}
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

				icon = (Texture2D) AssetDatabase.LoadAssetAtPath(fullPath, typeof(Texture2D));
				
				if(icon == null)
				{
#if PB_DEBUG
					Debug.LogWarning("Failed to find icon: " + fullPath);
#endif
					m_icons.Add(nameWithoutExtension, null);
					return null;
				}

				m_icons.Add(nameWithoutExtension, icon);
			}

			return icon;
		}

	}
}
