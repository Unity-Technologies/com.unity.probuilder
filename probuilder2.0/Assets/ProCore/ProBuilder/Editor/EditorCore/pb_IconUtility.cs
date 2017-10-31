// #define PB_DEBUG

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using ProBuilder2.Common;

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
			Debug.Log(System.IO.Directory.GetCurrentDirectory());

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

		/**
		 * Load an icon from the ProBuilder/Icons folder. IconName must *not* include the extension or `_Light` mode
		 * suffix.
		 */
		public static Texture2D GetIcon(string iconName, IconSkin skin = IconSkin.Default)
		{
#if PB_DEBUG
			if (iconName.EndsWith(".png"))
				pb_Log.Error("GetIcon(string) called with .png suffix!");

			if (iconName.EndsWith("_Light"))
				pb_Log.Error("GetIcon(string) called with _Light suffix!");
#endif

			bool isDarkSkin = skin == IconSkin.Default ? EditorGUIUtility.isProSkin : skin == IconSkin.Pro;
			string name = isDarkSkin ? iconName : iconName + "_Light";
			Texture2D icon = null;

			if (!m_Icons.TryGetValue(name, out icon))
			{
				int i = 0;

				do
				{
					// if in light mode:
					// - do one lap searching for light
					// - if nothing found, next searching for default
					string fullPath = string.Format("{0}{1}.png", m_IconFolderPath, i == 0 ? name : iconName);
					icon = (Texture2D) AssetDatabase.LoadAssetAtPath(fullPath, typeof(Texture2D));
				} while (!isDarkSkin && ++i < 2 && icon == null);

				m_Icons.Add(name, icon);
			}

			return icon;
		}
	}
}
