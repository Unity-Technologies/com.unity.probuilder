using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using Parabox.STL;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class ExportStlAscii : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Export; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Export Stl",
			@"Export an Stl model file."
		);

		public override bool IsHidden() { return true; }

		public override bool IsEnabled()
		{
			return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			if(!string.IsNullOrEmpty(ExportWithFileDialog(Selection.gameObjects, FileType.Ascii)))
				return new pb_ActionResult(Status.Success, "Export STL");
			else
				return new pb_ActionResult(Status.Canceled, "User Canceled");
		}

		public static string ExportWithFileDialog(GameObject[] gameObjects, FileType type)
		{
			GameObject first = gameObjects.FirstOrDefault(x => x.GetComponent<pb_Object>() != null);
			string name = first != null ? first.name : "Mesh";
			string path = EditorUtility.SaveFilePanel("Save Mesh to STL", "", name, "stl");

			if( pb_Stl_Exporter.Export(path, gameObjects, type) )
			{
				string full = path.Replace("\\", "/");

				// if the file was saved in project, ping it
				if(full.Contains(Application.dataPath))
				{
					string relative = full.Replace(Application.dataPath, "Assets");
					Object o = AssetDatabase.LoadAssetAtPath(relative, typeof(Object));
					if(o != null)
						EditorGUIUtility.PingObject(o);
					AssetDatabase.Refresh();

				}
				return path;
			}

			return null;
		}
	}
}
