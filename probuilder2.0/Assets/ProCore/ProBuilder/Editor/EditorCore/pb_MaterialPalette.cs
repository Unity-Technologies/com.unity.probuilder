using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// A serializable object that stores an array of materials. Used by pb_MaterialEditor.
	/// </summary>
	class pb_MaterialPalette : ScriptableObject, pb_IHasDefault
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
			string path = pb_FileUtil.GetSelectedDirectory() + "/Material Palette.asset";

			// Only generate unique path if it already exists - otherwise GenerateAssetUniquePath can return empty string
			// in event of path existing in a directory that is not yet created.
			if(pb_FileUtil.Exists(path))
				path = AssetDatabase.GenerateUniqueAssetPath(path);

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
				pb_Material.DefaultMaterial,
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
