using UnityEditor.SettingsManagement;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class EditorMaterialUtility
    {
        static readonly string[] k_StandardRenderPipelineDefaultMaterials = new string[]
        {
            "ProBuilder Default URP",
            "ProBuilder Default LWRP",
            "ProBuilder Default HDRP"
        };

        static bool s_Initialized;

        static Material s_EditorDefaultMaterial;
        static Material s_ShapePreviewMaterial;

        internal static readonly Color previewColor = new Color(.5f, .9f, 1f, .56f);

        [UserSetting("Mesh Settings", "Material", "The default material to be applied to newly created shapes.")]
        static Pref<Material> s_DefaultMaterial = new Pref<Material>("mesh.userMaterial", null);

        static void Init()
        {
            if (s_Initialized)
                return;

            s_Initialized = true;
            s_EditorDefaultMaterial = null;

            for (int i = 0, c = k_StandardRenderPipelineDefaultMaterials.Length; i < c && s_EditorDefaultMaterial == null; i++)
            {
                string search = k_StandardRenderPipelineDefaultMaterials[i] + " t:Material";
                string[] materials = AssetDatabase.FindAssets(search, new[] { "Assets", "Packages" });

                foreach (var asset in materials)
                {
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(asset));

                    if (mat != null && mat.shader != null && mat.shader.isSupported)
                    {
                        s_EditorDefaultMaterial = mat;
                        break;
                    }
                }
            }

            if (s_EditorDefaultMaterial == null)
                s_EditorDefaultMaterial = BuiltinMaterials.defaultMaterial;

            s_ShapePreviewMaterial = new Material(s_EditorDefaultMaterial.shader);
            s_ShapePreviewMaterial.hideFlags = HideFlags.HideAndDontSave;

            if (s_ShapePreviewMaterial.HasProperty("_MainTex"))
                s_ShapePreviewMaterial.mainTexture = (Texture2D)Resources.Load("Textures/GridBox_Default");

            if (s_ShapePreviewMaterial.HasProperty("_Color"))
                s_ShapePreviewMaterial.SetColor("_Color", previewColor);
        }

        internal static Texture2D GetPreviewTexture(Material material)
        {
            if (material == null || material.shader == null)
                return null;

            Texture2D best = null;

            for (int i = 0; i < ShaderUtil.GetPropertyCount(material.shader); i++)
            {
                if (ShaderUtil.GetPropertyType(material.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = ShaderUtil.GetPropertyName(material.shader, i);

                    Texture2D tex = material.GetTexture(propertyName) as Texture2D;

                    if (tex != null)
                    {
                        if (propertyName.Contains("_MainTex") || propertyName.Contains("Albedo"))
                            return tex;
                        else if (best == null)
                            best = tex;
                    }
                }
            }

            return best;
        }

        /// <summary>
        /// Get the material best matching the active mesh selection.
        /// </summary>
        internal static Material GetActiveSelection()
        {
            var mesh = MeshSelection.activeMesh;

            if (mesh != null)
            {
                var face = MeshSelection.activeFace;

                if (face == null)
                    return mesh.renderer.sharedMaterial;

                var sharedMaterials = mesh.renderer.sharedMaterials;

                if (sharedMaterials != null)
                {
                    var length = sharedMaterials.Length;
                    return sharedMaterials[System.Math.Min(face.submeshIndex, length - 1)];
                }
            }

            return null;
        }

        internal static Material GetUserMaterial()
        {
            var mat = (Material)s_DefaultMaterial;
            if (mat != null)
                return mat;
            return GetSrpDefaultMaterial();
        }

        internal static Material GetSrpDefaultMaterial()
        {
            Init();
            return s_EditorDefaultMaterial;
        }

        internal static Material GetShapePreviewMaterial()
        {
            Init();
            return s_ShapePreviewMaterial;
        }
    }
}
