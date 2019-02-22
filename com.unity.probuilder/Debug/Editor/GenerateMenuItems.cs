#define PROBUILDER_DEBUG

#if PROBUILDER_DEBUG

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Editor utility to generate the pb_EditorToolbarMenuItems class from the classes present in ProBuilder/Editor/MenuActions folder.
    /// </summary>
    /// <inheritdoc />
    sealed class GenerateMenuItems : Editor
    {
        const string k_GeneratedFilePath = "Packages/com.unity.probuilder/Editor/EditorCore/EditorToolbarMenuItems.cs";

        const string k_MenuActionsFolder = "Packages/com.unity.probuilder/Editor/MenuActions/";

        class MenuActionData
        {
            public bool valid { get; private set; }

            // False to not create menu entry
            public bool visibleInMenu { get; private set; }

            // type = "NewBezierShape"
            public string type { get; private set; }

            // path = "Editors/New Bezier Shape"
            public string path { get; private set; }

            // shortcut = "#%d"
            public string shortcut { get; private set; }

            public MenuActionData(string scriptPath)
            {
                var rawPath = scriptPath.Replace("\\", "/").Replace(k_MenuActionsFolder, "").Replace(".cs", "");
                type = GetClassName(rawPath);
                path = Regex.Replace(rawPath, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $0");
                MenuAction instance = null;

                try
                {
                    var t = ReflectionUtility.GetType("UnityEditor.ProBuilder.Actions." + type);
                    instance = System.Activator.CreateInstance(t) as MenuAction;
                    valid = true;
                }
                catch
                {
                    Log.Warning($"Failed generating menu item for {scriptPath}. File names must match class names.", "scriptPath");
                    valid = false;
                }

                if (valid)
                {
                    PropertyInfo hasMenuEntryProperty = typeof(MenuAction).GetProperty("hasFileMenuEntry", BindingFlags.NonPublic | BindingFlags.Instance);
                    visibleInMenu = hasMenuEntryProperty != null && (bool)hasMenuEntryProperty.GetValue(instance, null);

                    PropertyInfo tooltipProperty = typeof(MenuAction).GetProperty("tooltip");
                    shortcut = tooltipProperty == null ? "" : ((TooltipContent)tooltipProperty.GetValue(instance, null)).shortcut;
                }
            }
        }

        static readonly HashSet<string> IgnoreActions = new HashSet<string>()
        {
            "SetEntityType"
        };

        static readonly Dictionary<string, string> MenuPriorityLookup = new Dictionary<string, string>()
        {
            { "Editors", "PreferenceKeys.menuEditor + 1" },
            { "Object", "PreferenceKeys.menuGeometry + 2" },
            { "Geometry", "PreferenceKeys.menuGeometry + 3" },
            { "Interaction", "PreferenceKeys.menuSelection + 1" },
            { "Selection", "PreferenceKeys.menuSelection + 0" },
            { "Export", "PreferenceKeys.menuExport + 0" }
        };

        [MenuItem("Tools/Debug/ProBuilder/Rebuild Menu Items &d", false, 800)]
        static void GenerateMenuItemsForActions()
        {
            if (File.Exists(k_GeneratedFilePath))
                File.Delete(k_GeneratedFilePath);

            StringBuilder sb = new StringBuilder();

            AppendHeader(sb);

            IEnumerable<string> actions = Directory.GetFiles(k_MenuActionsFolder, "*.cs", SearchOption.AllDirectories)
                .Select(x => x.Replace("\\", "/"))
                .Where(y => !IgnoreActions.Contains(GetClassName(y)));

            foreach (string action in actions)
            {
                var data = new MenuActionData(action);
                UnityEngine.Debug.Log($"{data.visibleInMenu}\n{data.type}\n{data.path}\n{data.shortcut}");

                if (data.valid)
                {
                    sb.AppendLine();
                    AppendMenuItem(sb, data);
                }
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            File.WriteAllText(k_GeneratedFilePath, sb.ToString().Replace("\r\n", "\n"));
            EditorUtility.ShowNotification("Successfully Generated\nMenu Items");

            AssetDatabase.Refresh();
        }

        static void AppendHeader(StringBuilder sb)
        {
            sb.AppendLine(@"/**
 *  IMPORTANT
 *
 *  This is a generated file. Any changes will be overwritten.
 *  See Debug/GenerateMenuItems to make modifications.
 */

using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.Actions;

namespace UnityEditor.ProBuilder
{
    static class EditorToolbarMenuItem
    {
        const string k_MenuPrefix = ""Tools/ProBuilder/"";");
        }

        static void AppendMenuItem(StringBuilder sb, MenuActionData data)
        {
            // Verify
            sb.AppendLine($"\t\t[MenuItem(k_MenuPrefix + \"{data.path} \", true)]");
            sb.AppendLine($"\t\tstatic bool MenuVerify_{data.type}()");
            sb.AppendLine( "\t\t{");
            sb.AppendLine($"\t\t\tvar instance = EditorToolbarLoader.GetInstance<{data.type}>();");
            sb.AppendLine( "\t\t\treturn instance != null && instance.enabled;");
            sb.AppendLine( "\t\t}");

            var category = GetActionCategory(data.path);
            var priority = GetMenuPriority(category);
            var shortcut = GetMenuFormattedShortcut(data.shortcut);

            sb.AppendLine();

            // Action
            sb.AppendLine($"\t\t[MenuItem(k_MenuPrefix + \"{data.path}{shortcut}\", false, {priority})]");
            sb.AppendLine($"\t\tstatic void MenuPerform_{data.type}()");
            sb.AppendLine( "\t\t{");
            sb.AppendLine($"\t\t\tvar instance = EditorToolbarLoader.GetInstance<{data.type}>();");
            sb.AppendLine( "\t\t\tif(instance != null && instance.enabled)");
            sb.AppendLine( "\t\t\t\tEditorUtility.ShowNotification(instance.DoAction().notification);");
            sb.AppendLine( "\t\t}");
        }

        static string GetClassName(string scriptPath)
        {
            string file = Path.GetFileName(scriptPath);
            if (file != null)
                return file.Replace(".cs", "");
            return null;
        }

        static string GetActionCategory(string scriptPath)
        {
            string[] split = scriptPath.Split('/');
            return split[0];
        }

        static string GetMenuPriority(string category)
        {
            string menu_priority;

            if (!MenuPriorityLookup.TryGetValue(category, out menu_priority))
                menu_priority = "0";

            return menu_priority;
        }

        static string GetMenuFormattedShortcut(string shortcut)
        {
            string res = "";
            string[] keys = shortcut.Split('+');
            bool inSceneShortcut = true;

            foreach (string s in keys)
            {
                if (s.Contains(PreferenceKeys.CMD_SUPER) || s.Contains("Control"))
                {
                    res += "%";
                    inSceneShortcut = false;
                }
                else if (s.Contains(PreferenceKeys.CMD_OPTION) || s.Contains(PreferenceKeys.CMD_ALT) || s.Contains("Alt") ||
                         s.Contains("Option"))
                {
                    res += "&";
                    inSceneShortcut = false;
                }
                else if (s.Contains(PreferenceKeys.CMD_SHIFT) || s.Contains("Shift"))
                {
                    res += "#";
                    inSceneShortcut = false;
                }
                else
                    res += s.Trim().ToLower();
            }

            if (!string.IsNullOrEmpty(res))
            {
                // Show single-key context shortcuts by invalidating the MenuItem syntax
                if (inSceneShortcut)
                    res = $" [{res}]";
                else
                    res = $" {res}";
            }

            return res;
        }
    }
}

#endif
