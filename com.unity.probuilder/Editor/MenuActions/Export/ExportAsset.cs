using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using System.Collections.Generic;
using Parabox.STL;
using System.IO;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using EditorUtility = UnityEditor.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ExportAsset : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Export; } }
		public override Texture2D icon { get { return null; } }
		public override TooltipContent tooltip { get { return _tooltip; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Export Asset",
			"Export a Unity mesh asset file."
		);

		public override bool IsEnabled()
		{
			return MeshSelection.count > 0;
		}

		public override bool IsHidden() { return true; }

		public override ActionResult DoAction()
		{
			ExportWithFileDialog( MeshSelection.Top() );
			return new ActionResult(ActionResult.Status.Success, "Make Asset & Prefab");
		}

		/// <summary>
		/// Export meshes to a Unity asset.
		/// </summary>
		/// <param name="meshes"></param>
		/// <returns></returns>
		public static string ExportWithFileDialog(IEnumerable<ProBuilderMesh> meshes)
		{
			if(meshes == null || !meshes.Any())
				return "";

			string res = null;

			if(meshes.Count() < 2)
			{
				ProBuilderMesh first = meshes.FirstOrDefault();

				if(first == null)
					return res;

				string name = first != null ? first.name : "Mesh";
				string path = UnityEditor.EditorUtility.SaveFilePanel("Export to Asset", "Assets", name, "prefab");

				if(string.IsNullOrEmpty(path))
					return null;

				string directory = Path.GetDirectoryName(path);
				name = Path.GetFileNameWithoutExtension(path);
				string meshPath = string.Format("{0}/{1}.asset", directory, first.mesh.name);
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
				string path = UnityEditor.EditorUtility.SaveFolderPanel("Export to Asset", "Assets", "");

				if(string.IsNullOrEmpty(path) || !Directory.Exists(path))
					return null;

				foreach(ProBuilderMesh pb in meshes)
					res = DoExport(string.Format("{0}/{1}.asset", path, pb.name), pb);
			}

			AssetDatabase.Refresh();

			return res;
		}

		private static string DoExport(string path, ProBuilderMesh pb)
		{
			string directory = Path.GetDirectoryName(path);
			string name = Path.GetFileNameWithoutExtension(path);
			string relativeDirectory = string.Format("Assets{0}", directory.Replace(Application.dataPath, ""));

			pb.ToMesh();
			pb.Refresh();
			pb.Optimize(true);

			string meshPath = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", relativeDirectory, pb.mesh.name));

			AssetDatabase.CreateAsset(pb.mesh, meshPath);

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
