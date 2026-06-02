using UnityEditor.SettingsManagement;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class EditorMaterialUtility
    {
        [UserSetting("Mesh Settings", "Material", "The default material to be applied to newly created shapes.")]
        static Pref<Material> s_DefaultMaterial = new Pref<Material>("mesh.userMaterial", null);

        internal static Texture2D GetPreviewTexture(Material material)
        {
            if (material == null || material.shader == null)
                return null;

            Texture2D best = null;

            for (int i = 0; i < material.shader.GetPropertyCount(); i++)
            {
                if (material.shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture)
                {
                    string propertyName = material.shader.GetPropertyName(i);

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
            return BuiltinMaterials.defaultMaterial;
        }
    }
}
