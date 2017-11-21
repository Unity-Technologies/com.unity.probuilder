using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using System.Collections.Generic;
using Parabox.STL;
using System.IO;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class ExportAsset : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Export; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Export Asset",
			"Export a Unity mesh asset file."
		);

		public override bool IsEnabled()
		{
			return 	selection != null &&
					selection.Length > 0;
		}

		public override bool IsHidden() { return true; }

		public override pb_ActionResult DoAction()
		{
			ExportWithFileDialog( pb_Selection.Top() );
			return new pb_ActionResult(Status.Success, "Make Asset & Prefab");
		}

		/**
		 *	Export meshes to a Unity asset.
		 */
		public static string ExportWithFileDialog(IEnumerable<pb_Object> meshes)
		{
			if(meshes == null || meshes.Count() < 1)
				return "";

			string res = null;

			if(meshes.Count() < 2)
			{
				pb_Object first = meshes.FirstOrDefault();

				if(first == null)
					return res;

				string name = first != null ? first.name : "Mesh";
				string path = EditorUtility.SaveFilePanel("Export to Asset", "Assets", name, "prefab");

				if(string.IsNullOrEmpty(path))
					return null;

				string directory = Path.GetDirectoryName(path);
				name = Path.GetFileNameWithoutExtension(path);
				string meshPath = string.Format("{0}/{1}.asset", directory, first.msh.name);
				string prefabPath = string.Format("{0}/{1}.prefab", directory, first.name);

				// If a file dialog was presented that means the user has already been asked to overwrite.
				if(File.Exists(meshPath))
					AssetDatabase.DeleteAsset(meshPath.Replace(Application.dataPath, "Assets"));

				if(File.Exists(prefabPath))
					AssetDatabase.DeleteAsset(prefabPath.Replace(Application.dataPath, "Assets"));

				res = DoExport(path, first);
			}
			else
			{
				string path = EditorUtility.SaveFolderPanel("Export to Asset", "Assets", "");

				if(string.IsNullOrEmpty(path) || !Directory.Exists(path))
					return null;

				foreach(pb_Object pb in meshes)
					res = DoExport(string.Format("{0}/{1}.asset", path, pb.name), pb);
			}

			AssetDatabase.Refresh();

			return res;
		}

		private static string DoExport(string path, pb_Object pb)
		{
			string directory = Path.GetDirectoryName(path);
			string name = Path.GetFileNameWithoutExtension(path);
			string relativeDirectory = string.Format("Assets{0}", directory.Replace(Application.dataPath, ""));

			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();

			string meshPath = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", relativeDirectory, pb.msh.name));

			AssetDatabase.CreateAsset(pb.msh, meshPath);

			pb.MakeUnique();

			Mesh meshAsset = (Mesh) AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh));

			GameObject go = new GameObject();
			go.AddComponent<MeshFilter>().sharedMesh = meshAsset;
			go.AddComponent<MeshRenderer>().sharedMaterials = pb.gameObject.GetComponent<MeshRenderer>().sharedMaterials;
			string relativePrefabPath = string.Format("{0}/{1}.prefab", relativeDirectory, name);
			string prefabPath = AssetDatabase.GenerateUniqueAssetPath(relativePrefabPath);
			PrefabUtility.CreatePrefab(prefabPath, go, ReplacePrefabOptions.Default);
			GameObject.DestroyImmediate(go);

			return meshPath;
		}
	}
}
