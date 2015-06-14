using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	public class pb_MakeMeshAsset : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Make Asset", true, pb_Constant.MENU_ACTIONS + 40)]
		public static bool VerifyMakeAsset()
		{
			return pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Make Asset", false, pb_Constant.MENU_ACTIONS + 40)]
		public static void MenuMakeAsset()
		{
			string path = "Assets";
			path = AssetDatabase.GetAssetPath(Selection.activeObject);
			Mesh meshAsset = null;

			if(path == "" || path == string.Empty) 
			{
				path = "Assets/ProBuilder Saved Assets";
			}

			if(!System.IO.Directory.Exists(path + "/ProBuilder Saved Assets"))
			{
				AssetDatabase.CreateFolder("Assets", "ProBuilder Saved Assets");
				AssetDatabase.Refresh();
			}

			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				string meshPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + pb.name + ".asset");

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();

				AssetDatabase.CreateAsset(pb.msh, meshPath);
				
				pb.MakeUnique();

				meshAsset = (Mesh) AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh));

				GameObject go = new GameObject();
				go.AddComponent<MeshFilter>().sharedMesh = meshAsset;
				go.AddComponent<MeshRenderer>().sharedMaterials = pb.gameObject.GetComponent<MeshRenderer>().sharedMaterials;

				PrefabUtility.CreatePrefab(AssetDatabase.GenerateUniqueAssetPath(path + "/" + pb.name + ".prefab"), go, ReplacePrefabOptions.Default);
				DestroyImmediate(go);
			}

			AssetDatabase.Refresh();

			Selection.activeObject = meshAsset;
		}
	}
}