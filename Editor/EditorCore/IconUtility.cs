using UnityEngine;
using System;
using System.Collections.Generic;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Describes icon styles for pro and basic skin, or default (whatever the editor is currently using).
    /// </summary>
    enum IconSkin
    {
        Default,
        Light,
        Pro
    };

    static class IconUtility
    {
        static Dictionary<string, Texture2D> s_Icons = new Dictionary<string, Texture2D>();
        static string s_IconFolderPath = "Content/Icons/";

        /// <summary>
        /// Load an icon from the ProBuilder/Icons folder. IconName must *not* include the extension or `_Light` mode suffix.
        /// </summary>
        /// <param name="iconName"></param>
        /// <param name="skin"></param>
        /// <returns></returns>
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

            if (!s_Icons.TryGetValue(name, out icon))
            {
                int i = 0;

                do
                {
                    // if in light mode:
                    // - do one lap searching for light
                    // - if nothing found, next searching for default
                    string fullPath = string.Format("{0}{1}.png", s_IconFolderPath, i == 0 ? name : iconName);
                    icon = FileUtility.LoadInternalAsset<Texture2D>(fullPath);
                }
                while (!isDarkSkin && ++i < 2 && icon == null);

                s_Icons.Add(name, icon);
            }

            return icon;
        }
    }
}
