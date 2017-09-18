using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Parabox.STL;
using System.Reflection;

namespace ProBuilder2.Actions
{
	public class ExportFbx : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Export; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Export FBX",
			"Export an Autodesk FBX file."
		);

		public override bool IsHidden() { return true; }

		public override bool IsEnabled()
		{
			return pb_Fbx.FbxEnabled && Selection.gameObjects != null && Selection.gameObjects.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			bool asGroup = pb_PreferencesInternal.GetBool("pbExportAsGroup", true);
			pb_FbxOptions options = new pb_FbxOptions() { quads = pb_PreferencesInternal.GetBool("Export::m_FbxQuads", true) };

			string res = ExportWithFileDialog(Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable | SelectionMode.TopLevel).Cast<GameObject>(), asGroup, options);

			if( string.IsNullOrEmpty(res) )
				return new pb_ActionResult(Status.Canceled, "User Canceled");
			else
				return new pb_ActionResult(Status.Success, "Export FBX");
		}

		/**
		 *	Prompt user for a save file location and export meshes as Fbx.
		 */
		public static string ExportWithFileDialog(IEnumerable<GameObject> meshes, bool asGroup = true, pb_FbxOptions options = null)
		{
			if(meshes == null || meshes.Count() < 1)
				return null;

			string path = null, res = null;

			if(asGroup || meshes.Count() < 2)
			{
				GameObject first = meshes.FirstOrDefault();
				string name = first != null ? first.name : "ProBuilderModel";
				path = EditorUtility.SaveFilePanel("Export as FBX", "Assets", name, "fbx");

				if(string.IsNullOrEmpty(path))
					return null;

				res = DoExport(path, meshes, options);
			}
			else
			{
				// instead of spamming FilePanel just ask for a folder and auto-name the meshes.
				path = EditorUtility.SaveFolderPanel("Export to FBX", "Assets", "");

				if(string.IsNullOrEmpty(path) || !Directory.Exists(path))
					return null;

				foreach(GameObject model in meshes)
					res = DoExport(string.Format("{0}/{1}.fbx", path, model.name), new List<GameObject>() { model }, options);
			}

			return res;
		}

		private static string DoExport(string path, IEnumerable<GameObject> models, pb_FbxOptions options)
		{
			Type modelExporterType = pb_Reflection.GetType("FbxExporters.Editor.ModelExporter");

			if(modelExporterType != null)
			{
				MethodInfo exportObjectsMethod = modelExporterType.GetMethod("ExportObjects");

            	if(exportObjectsMethod != null)
            	{
            		object res = exportObjectsMethod.Invoke(null, new object[] { path, models.ToArray() });
            		return res as string;
            	}
			}

			pb_Log.Error("FbxExporter is not loaded in this project! Please import the FbxExporter package to use this feature.");

			return null;
		}
	}
}
