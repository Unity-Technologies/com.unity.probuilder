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

	public static class pb_IconUtility
	{
		private static Dictionary<string, Texture2D> m_Icons = new Dictionary<string, Texture2D>();
		private static string m_IconFolderPath = "Icons/";

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
					icon = pb_FileUtil.LoadRelative<Texture2D>(fullPath);
				} while (!isDarkSkin && ++i < 2 && icon == null);

				m_Icons.Add(name, icon);
			}

			return icon;
		}
	}
}
