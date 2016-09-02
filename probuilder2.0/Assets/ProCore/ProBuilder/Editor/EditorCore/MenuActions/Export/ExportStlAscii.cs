using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;
using Parabox.STL;

namespace ProBuilder2.Actions
{
	public class ExportStlAscii : pb_MenuAction
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
			if( ExportWithFileDialog(Selection.gameObjects, FileType.Ascii) )
				return new pb_ActionResult(Status.Success, "Export STL");
			else
				return new pb_ActionResult(Status.Canceled, "User Canceled");
		}

		internal static bool ExportWithFileDialog(GameObject[] gameObjects, FileType type)
		{
			string path = EditorUtility.SaveFilePanel("Save Mesh to STL", "", "Mesh", "stl");

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
				return true;
			}

			return false;
		}
	}
}
