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
		private const string ICON_FOLDER_PATH = "ProBuilder/Icons";

		static pb_IconUtility()
		{
			if(!Directory.Exists(m_IconFolderPath))
			{
				string folder = pb_FileUtil.FindFolder(ICON_FOLDER_PATH);

				if(string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
					Debug.LogError("Could not locate ProBuilder/Icons folder.  The ProBuilder folder may be moved, but the contents of this folder must remain unmodified relative to ProBuilder root.");
				else
					m_IconFolderPath = folder;
			}
		}

		private static Dictionary<string, Texture2D> m_Icons = new Dictionary<string, Texture2D>();
		private static string m_IconFolderPath = "Assets/ProCore/ProBuilder/GUI/Icons/";

		public static Texture2D GetIcon(string iconName, IconSkin skin = IconSkin.Default)
		{
			int ext = iconName.LastIndexOf('.');
			string nameWithoutExtension = ext < 0 ? iconName : iconName.Substring(0, ext);
			Texture2D icon = null;

			// If icon is disabled there are no hover/normal/pressed states associated.
			if( !nameWithoutExtension.EndsWith("_disabled") )
			{
				switch(skin)
				{
					case IconSkin.Default:
						// If asking for light skin and the name doesn't specify _Light try to load the _Light suffixed
						// version first, and on failure try to return the default icon.
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
			}
			// _Light_disabled is an invalid suffix, but we'll be forgiving and correct the user.
			else if(nameWithoutExtension.EndsWith("_Light_disabled"))
			{
				nameWithoutExtension = nameWithoutExtension.Replace("_Light_disabled", "_disabled");
			}

			if(!m_Icons.TryGetValue(nameWithoutExtension, out icon))
			{
				string fullPath = m_IconFolderPath + nameWithoutExtension;

				if(!fullPath.EndsWith(".png"))
					fullPath += ".png";

				icon = (Texture2D) AssetDatabase.LoadAssetAtPath(fullPath, typeof(Texture2D));

				if(icon == null)
				{
#if PB_DEBUG
					Debug.LogWarning("Failed to find icon: " + fullPath);
#endif
					m_Icons.Add(nameWithoutExtension, null);
					return null;
				}

				m_Icons.Add(nameWithoutExtension, icon);
			}

			return icon;
		}

	}
}
