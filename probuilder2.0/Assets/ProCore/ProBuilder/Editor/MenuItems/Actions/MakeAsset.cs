/**
 *	To Install:
 *	Create a C# file named MakeAsset in 6by7/ProBuilder/Editor/Actions folder.
 *	Copy this text into the file you just made.
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{
	public class MakeAsset : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Make Asset", true, pb_Constant.MENU_ACTIONS + 1)]
		public static bool VerifyMakeAsset()
		{
			return pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Make Asset", false, pb_Constant.MENU_ACTIONS + 1)]
		public static void MenuMakeAsset()
		{
			string path = "Assets";

			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				path = AssetDatabase.GetAssetPath(Selection.activeObject);
				if(path == "" || path == string.Empty) path = "Assets";
				AssetDatabase.CreateAsset(pb.msh, AssetDatabase.GenerateUniqueAssetPath(path + "/" + pb.name + ".asset"));
			}

			AssetDatabase.Refresh();

			Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(Mesh));
		}
	}
}