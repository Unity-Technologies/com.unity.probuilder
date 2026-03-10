using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

        static string s_IconFolderPath = "Packages/com.unity.probuilder/Editor Default Resources/Icons/";

        /// <summary>
        /// Load an icon from icons folder located in the package's 'Editor Default Resources'.
        /// Naming convention is: "path/to/iconName" (without extension). Use the 'd_' prefix for dark skin  icons.
        /// No prefix or suffit for light skin icons. This method assumes the file is .png.
        /// </summary>
        /// <param name="iconName">Relative path to the icon without the file extension in the filename.</param>
        /// <returns>A valid icon texture. Otherwise null is returned.</returns>
        public static Texture2D GetIcon(string iconName)
        {
            Texture2D icon = null;

            if (!s_Icons.TryGetValue(iconName, out icon))
            {
                string fullPath = Path.Combine(s_IconFolderPath, iconName + (Path.HasExtension(iconName)? string.Empty: ".png"));

                icon = EditorGUIUtility.LoadIcon(fullPath);
                s_Icons.Add(iconName, icon);
            }
            return icon;
        }
    }
}
