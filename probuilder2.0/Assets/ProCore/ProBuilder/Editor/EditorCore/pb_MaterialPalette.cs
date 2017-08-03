using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	A serializable object that stores an array of materials. Used by pb_MaterialEditor.
	 */
	public class pb_MaterialPalette : ScriptableObject, pb_IHasDefault
	{
		[MenuItem("Assets/Create/Material Palette", true)]
		static bool VerifyCreateMaterialPalette()
		{
			// This hangs on large projects
			// Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets).Length > 0;
			return true;
		}

		[MenuItem("Assets/Create/Material Palette")]
		static void CreateMaterialPalette()
		{
			string path = AssetDatabase.GenerateUniqueAssetPath(pb_FileUtil.PathFromRelative("Data/Material Palette.asset"));
			pb_MaterialPalette newPalette = pb_FileUtil.LoadRequired<pb_MaterialPalette>(path);
			newPalette.array = Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets).Cast<Material>().ToArray();
			EditorUtility.SetDirty(newPalette);
			EditorGUIUtility.PingObject(newPalette);
		}

		public Material[] array;

		public static implicit operator Material[](pb_MaterialPalette materialArray)
		{
			return materialArray.array;
		}

		public Material this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public void SetDefaultValues()
		{
			array = new Material[10] {
				pb_Constant.DefaultMaterial,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			 };
		}
	}
}
