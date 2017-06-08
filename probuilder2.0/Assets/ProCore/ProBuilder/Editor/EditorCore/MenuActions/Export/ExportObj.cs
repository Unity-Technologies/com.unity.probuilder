using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Parabox.STL;

namespace ProBuilder2.Actions
{
	public class ExportObj : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Export; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Export Obj",
			"Export a Wavefront OBJ file."
		);

		public override bool IsHidden() { return true; }

		public override bool IsEnabled()
		{
			return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			string res = ExportWithFileDialog(pb_Selection.Top());

			if( string.IsNullOrEmpty(res) )
				return new pb_ActionResult(Status.Canceled, "User Canceled");
			else
				return new pb_ActionResult(Status.Success, "Export OBJ");
		}

		/**
		 *	Prompt user for a save file location and export meshes as Obj.
		 */
		public static string ExportWithFileDialog(IEnumerable<pb_Object> meshes, bool asGroup = true, pb_ObjOptions options = null)
		{
			if(meshes == null || meshes.Count() < 1)
				return null;

			IEnumerable<pb_Model> models = meshes.Select(x =>
				new pb_Model(x.gameObject.name,
					x.msh,
					x.GetComponent<MeshRenderer>().sharedMaterials,
					x.transform.localToWorldMatrix));

			string path = null;

			if(asGroup || models.Count() < 2)
			{
				pb_Object first = meshes.FirstOrDefault();
				string name = first != null ? first.name : "ProBuilderModel";
				path = EditorUtility.SaveFilePanel("Export to Obj", "Assets", name, "obj");

				if(string.IsNullOrEmpty(path))
					return null;

				string obj, mat;

				pb_Obj.Export(name, models, out obj, out mat, options);

				try
				{
					path = Path.GetDirectoryName(path);
					pb_FileUtil.WriteFile(string.Format("{0}/{1}.obj", path, name), obj);
					// Spaces are not allowed in mtlib name
					pb_FileUtil.WriteFile(string.Format("{0}/{1}.mtl", path, name.Replace(" ", "_")), mat);
				}
				catch(System.Exception e)
				{
					Debug.LogWarning(string.Format("Failed writing obj to path: {0}\n{1}", string.Format("{0}/{1}.obj", path, name), e.ToString()));
				}
			}
			else
			{
				path = EditorUtility.SaveFolderPanel("Export to Obj", "Assets", "");

				if(string.IsNullOrEmpty(path) || !Directory.Exists(path))
					return null;

				foreach(pb_Model model in models)
				{
					string name = model.name;
					string obj, mat;

					pb_Obj.Export(name, new List<pb_Model>() { model }, out obj, out mat, options);

					try
					{
						pb_FileUtil.WriteFile(string.Format("{0}/{1}.obj", path, name), obj);
						pb_FileUtil.WriteFile(string.Format("{0}/{1}.mtl", path, name.Replace(" ", "_")), mat);
					}
					catch(System.Exception e)
					{
						Debug.LogWarning(string.Format("Failed writing obj to path: {0}\n{1}", string.Format("{0}/{1}.obj", path, name), e.ToString()));
					}
				}
			}

			return path;
		}
	}
}
