using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

/**
 *	Editor utility to generate the pb_EditorToolbarMenuItems class from
 *	the classes present in ProBuilder/Editor/MenuActions folder.
 */
class pb_GenerateMenuItems : Editor
{
	const string GENERATED_FILE_PATH = "Assets/ProCore/ProBuilder/Editor/EditorCore/pb_EditorToolbarMenuItems.cs";
	const string MENU_ACTIONS_PATH = "Assets/ProCore/ProBuilder/Editor/EditorCore/MenuActions/";
	const string PB_MENU_PREFIX = "Tools/ProBuilder/";

	static readonly HashSet<string> IgnoreActions = new HashSet<string>()
	{
		"SetEntityType"
	};

	static readonly Dictionary<string, string> MenuPriorityLookup = new Dictionary<string, string>()
	{
		{ "Editors",		"pb_Constant.MENU_EDITOR + 1" },
		{ "Object", 		"pb_Constant.MENU_GEOMETRY + 2" },
		{ "Geometry", 		"pb_Constant.MENU_GEOMETRY + 3" },
		{ "Interaction", 	"pb_Constant.MENU_SELECTION + 1" },
		{ "Selection", 		"pb_Constant.MENU_SELECTION + 0" }
	};

	[MenuItem("Tools/Do the thing &d")]
	static void doit()
	{
		if( File.Exists(GENERATED_FILE_PATH) )
			File.Delete(GENERATED_FILE_PATH);

		StringBuilder sb = new StringBuilder();

		IEnumerable<string> actions = Directory.GetFiles(MENU_ACTIONS_PATH, "*.cs", SearchOption.AllDirectories)
			.Select(x => x.Replace("\\", "/"))
				.Where(y => !IgnoreActions.Contains(GetClassName(y)));

		sb.AppendLine(@"/**
 *	IMPORTANT
 *
 *	This is a generated file. Any changes will be overwritten.
 *	See pb_GenerateMenuItems.
 */
using UnityEngine;
using UnityEditor;
using ProBuilder2.Actions;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	public class pb_EditorToolbarMenuItems : Editor
	{
");
		foreach(string action in actions)
		{
			sb.AppendLine( GenerateMenuItemFunctions(action) );
		}

		sb.AppendLine("\t}");
		sb.AppendLine("}");

		File.WriteAllText(GENERATED_FILE_PATH, sb.ToString());

		// Debug.Log(sb.ToString());
	}

	/**
	 *	Generate the [MenuItem()] body for a pb_MenuAction from it's script path.
	 */
	static string GenerateMenuItemFunctions(string scriptPath)
	{
		string action = scriptPath.Replace("\\", "/").Replace(MENU_ACTIONS_PATH, "").Replace(".cs", "");
		string category = GetActionCategory(action);
		string menu_priority = GetMenuPriority(category);
		string class_name = GetClassName(action);
		string pretty_path = Regex.Replace(action, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $0");

		StringBuilder sb = new StringBuilder();

		/// VERIFY
		sb.Append("\t\t[MenuItem(\"");
		sb.Append(PB_MENU_PREFIX);
		sb.Append(pretty_path);
		sb.Append("\", true)]");
		sb.AppendLine("");

		sb.Append("\t\tstatic bool MenuVerify");
		sb.Append(class_name);
		sb.AppendLine("()");

		sb.AppendLine("\t\t{");

		sb.Append("\t\t\t");
		sb.Append(class_name);
		sb.Append(" instance = pb_EditorToolbarLoader.GetInstance<");
		sb.Append(class_name);
		sb.AppendLine(">();");
		sb.AppendLine("\t\t\treturn instance != null && instance.IsEnabled();");
		sb.AppendLine("\t\t}");

		sb.AppendLine("");

		/// PERFORM
		sb.Append("\t\t[MenuItem(\"");
		sb.Append(PB_MENU_PREFIX);
		sb.Append(pretty_path);
		sb.Append("\", false, ");
		sb.Append(menu_priority);
		// sb.Append(" < < " + category + " > > ");
		sb.Append(")]");
		sb.AppendLine("");

		sb.Append("\t\tstatic void MenuDo");
		sb.Append(class_name);
		sb.AppendLine("()");

		sb.AppendLine("\t\t{");

		sb.Append("\t\t\t");
		sb.Append(class_name);
		sb.Append(" instance = pb_EditorToolbarLoader.GetInstance<");
		sb.Append(class_name);
		sb.AppendLine(">();");
		sb.AppendLine("\t\t\tif(instance != null)");
		sb.AppendLine("\t\t\t\tpb_Editor_Utility.ShowNotification(instance.DoAction().notification);");
		sb.AppendLine("\t\t}");

		return sb.ToString();
	}

	static string GetClassName(string scriptPath)
	{
		string file = Path.GetFileName(scriptPath);
		if(file != null)
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

		if( !MenuPriorityLookup.TryGetValue(category, out menu_priority) )
			menu_priority = "0";

		return menu_priority;
	}
}
