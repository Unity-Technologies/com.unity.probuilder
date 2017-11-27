using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Parabox.STL;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class ExportObj : pb_MenuAction
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
		public static string ExportWithFileDialog(IEnumerable<pb_Object> meshes, bool asGroup = true, bool allowQuads = true, pb_ObjOptions options = null)
		{
			if(meshes == null || meshes.Count() < 1)
				return null;

			IEnumerable<pb_Model> models = allowQuads
				? meshes.Select(x => new pb_Model(x.gameObject.name, x))
				: meshes.Select(x => new pb_Model(x.gameObject.name, x.msh, x.GetComponent<MeshRenderer>().sharedMaterials, x.transform.localToWorldMatrix));

			string path = null, res = null;

			if(asGroup || models.Count() < 2)
			{
				pb_Object first = meshes.FirstOrDefault();
				string name = first != null ? first.name : "ProBuilderModel";
				path = EditorUtility.SaveFilePanel("Export to Obj", "Assets", name, "obj");

				if(string.IsNullOrEmpty(path))
					return null;

				res = DoExport(path, models, options);
			}
			else
			{
				path = EditorUtility.SaveFolderPanel("Export to Obj", "Assets", "");

				if(string.IsNullOrEmpty(path) || !Directory.Exists(path))
					return null;

				foreach(pb_Model model in models)
					res = DoExport(string.Format("{0}/{1}.obj", path, model.name), new List<pb_Model>() { model }, options);
			}

			return res;
		}

		private static string DoExport(string path, IEnumerable<pb_Model> models, pb_ObjOptions options)
		{
			string name = Path.GetFileNameWithoutExtension(path);
			string directory = Path.GetDirectoryName(path);

			List<string> textures = null;
			string obj, mat;

			if( pb_Obj.Export(name, models, out obj, out mat, out textures, options) )
			{
				try
				{
					CopyTextures(textures, directory);
					pb_FileUtil.WriteAllText(string.Format("{0}/{1}.obj", directory, name), obj);
					pb_FileUtil.WriteAllText(string.Format("{0}/{1}.mtl", directory, name.Replace(" ", "_")), mat);
				}
				catch(System.Exception e)
				{
					Debug.LogWarning(string.Format("Failed writing obj to path: {0}\n{1}", string.Format("{0}/{1}.obj", path, name), e.ToString()));
					return null;
				}
			}
			else
			{
				Debug.LogWarning("No meshes selected.");
				return null;
			}

			return path;
		}

		/**
		 *	Copy files from their path to a destination directory.
		 */
		private static void CopyTextures(List<string> textures, string destination)
		{
			foreach(string path in textures)
			{
				string dest = string.Format("{0}/{1}", destination, Path.GetFileName(path));

				if(!File.Exists(dest))
					File.Copy(path, dest);
			}
		}
	}
}
