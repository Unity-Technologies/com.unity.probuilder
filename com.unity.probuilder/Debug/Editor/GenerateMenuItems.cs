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

		static readonly HashSet<string> IgnoreActions = new HashSet<string>()
		{
			"SetEntityType"
		};

		static readonly Dictionary<string, string> MenuPriorityLookup = new Dictionary<string, string>()
		{
			{"Editors", "PreferenceKeys.menuEditor + 1"},
			{"Object", "PreferenceKeys.menuGeometry + 2"},
			{"Geometry", "PreferenceKeys.menuGeometry + 3"},
			{"Interaction", "PreferenceKeys.menuSelection + 1"},
			{"Selection", "PreferenceKeys.menuSelection + 0"},
			{"Export", "PreferenceKeys.menuExport + 0"}
		};

		[MenuItem("Tools/Debug/ProBuilder/Rebuild Menu Items", false, 800)]
		static void doit()
		{
			if (File.Exists(k_GeneratedFilePath))
				File.Delete(k_GeneratedFilePath);

			StringBuilder sb = new StringBuilder();

			IEnumerable<string> actions = Directory.GetFiles(k_MenuActionsFolder, "*.cs", SearchOption.AllDirectories)
				.Select(x => x.Replace("\\", "/"))
				.Where(y => !IgnoreActions.Contains(GetClassName(y)));

			sb.AppendLine(
				@"/**
 *	IMPORTANT
 *
 *	This is a generated file. Any changes will be overwritten.
 *	See Debug/GenerateMenuItems to make modifications.
 */

using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.Actions;
using System.Collections.Generic;

namespace UnityEditor.ProBuilder
{
	static class EditorToolbarMenuItem
	{
		const string k_MenuPrefix = ""Tools/ProBuilder/"";
");
			foreach (string action in actions)
			{
				sb.AppendLine(GenerateMenuItemFunctions(action));
			}

			sb.AppendLine("\t}");
			sb.AppendLine("}");

			File.WriteAllText(k_GeneratedFilePath, sb.ToString().Replace("\r\n", "\n"));
			EditorUtility.ShowNotification("Successfully Generated\nMenu Items");

			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Generate the [MenuItem()] body for a pb_MenuAction from it's script path.
		/// </summary>
		/// <param name="scriptPath"></param>
		/// <returns></returns>
		static string GenerateMenuItemFunctions(string scriptPath)
		{
			string action = scriptPath.Replace("\\", "/").Replace(k_MenuActionsFolder, "").Replace(".cs", "");
			string category = GetActionCategory(action);
			string menu_priority = GetMenuPriority(category);
			string class_name = GetClassName(action);
			string pretty_path = Regex.Replace(action, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $0");

			StringBuilder sb = new StringBuilder();
			object o = null;

			try
			{
				var type = ReflectionUtility.GetType("UnityEditor.ProBuilder.Actions." + class_name);
				o = System.Activator.CreateInstance(type);
			}
			catch
			{
				Debug.LogWarning("Failed generating menu item for class: \"" + class_name +
				                 "\".  File names must match class names.");
				return "";
			}

			PropertyInfo hasMenuEntryProperty = typeof(MenuAction).GetProperty("hasFileMenuEntry", BindingFlags.NonPublic | BindingFlags.Instance);

			if (hasMenuEntryProperty == null || (bool) hasMenuEntryProperty.GetValue(o, null) == false)
				return "";

			PropertyInfo tooltipProperty = typeof(MenuAction).GetProperty("tooltip");
			string shortcut = tooltipProperty == null ? "" : GetMenuFormattedShortcut(((TooltipContent) tooltipProperty.GetValue(o, null)).shortcut);

			// VERIFY
			sb.Append("\t\t[MenuItem(");
			sb.Append("k_MenuPrefix + \"");
			sb.Append(pretty_path);
			sb.Append(" ");
			sb.Append(shortcut);
			sb.Append("\", true)]");
			sb.AppendLine("");

			sb.Append("\t\tstatic bool MenuVerify");
			sb.Append(class_name);
			sb.AppendLine("()");

			sb.AppendLine("\t\t{");

			sb.Append("\t\t\t");
			sb.Append(class_name);
			sb.Append(" instance = EditorToolbarLoader.GetInstance<");
			sb.Append(class_name);
			sb.AppendLine(">();");
			sb.AppendLine(@"
			return instance != null && instance.IsEnabled();
	");
			sb.AppendLine("\t\t}");

			sb.AppendLine("");

			// PERFORM
			sb.Append("\t\t[MenuItem(");
			sb.Append("k_MenuPrefix + \"");
			sb.Append(pretty_path);
			sb.Append(" ");
			sb.Append(shortcut);
			sb.Append("\", false, ");
			sb.Append(menu_priority);
			sb.Append(")]");
			sb.AppendLine("");

			sb.Append("\t\tstatic void MenuDo");
			sb.Append(class_name);
			sb.AppendLine("()");

			sb.AppendLine("\t\t{");

			sb.Append("\t\t\t");
			sb.Append(class_name);
			sb.Append(" instance = EditorToolbarLoader.GetInstance<");
			sb.Append(class_name);
			sb.AppendLine(">();");
			sb.AppendLine("\t\t\tif(instance != null)");
			sb.AppendLine("\t\t\t\tUnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);");
			sb.AppendLine("\t\t}");

			return sb.ToString();
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

			if (res.Length > 0 && inSceneShortcut)
				res = string.Format(" [{0}]", res);

			return res;
		}
	}
}

#endif
