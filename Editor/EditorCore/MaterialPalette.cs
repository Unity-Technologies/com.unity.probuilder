using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// A serializable object that stores an array of materials. Used by MaterialEditor.
    /// </summary>
    /// <inheritdoc cref="UnityEngine.ScriptableObject"/>
    /// <inheritdoc cref="IHasDefault"/>
    sealed class MaterialPalette : ScriptableObject, IHasDefault
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
            string path = FileUtility.GetSelectedDirectory() + "/Material Palette.asset";

            // Only generate unique path if it already exists - otherwise GenerateAssetUniquePath can return empty string
            // in event of path existing in a directory that is not yet created.
            if (FileUtility.Exists(path))
                path = AssetDatabase.GenerateUniqueAssetPath(path);

            MaterialPalette newPalette = FileUtility.LoadRequired<MaterialPalette>(path);
            newPalette.array = Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets).Cast<Material>().ToArray();
            UnityEditor.EditorUtility.SetDirty(newPalette);
            EditorGUIUtility.PingObject(newPalette);
        }

        public Material[] array;

        public static implicit operator Material[](MaterialPalette materialArray)
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
            array = new Material[10]
            {
                EditorMaterialUtility.GetUserMaterial(),
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
