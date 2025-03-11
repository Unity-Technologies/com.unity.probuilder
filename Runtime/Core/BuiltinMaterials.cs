using System;
using UnityEngine.Rendering;
using System.Reflection;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Provides access to the built-in materials that ProBuilder uses. In the Editor, see
    /// <see cref="UnityEditor.ProBuilder.EditorMaterialUtility"/> for access to the full array of materials
    /// and shaders.
    /// </summary>
    public static class BuiltinMaterials
    {
        static bool s_IsInitialized;

        /// <summary>
        /// Represents the shader used to highlight <see cref="Face" /> selections.
        /// </summary>
        public const string faceShader = "Hidden/ProBuilder/FaceHighlight";

        /// <summary>
        /// Represents the shader used to highlight <see cref="Edge" /> selections.
        /// </summary>
        /// <remarks>
        /// If the graphics device does not support geometry shaders, this shader doesn't compile.
        /// In that case, use <see cref="wireShader"/>.
        /// </remarks>
        public const string lineShader = "Hidden/ProBuilder/LineBillboard";

        /// <summary>
        /// Represents a line shader for use with `CreateEdgeBillboardMesh` when geometry
        /// shaders are not available. Use <see cref="lineShader"/> where possible.
        /// </summary>
        public const string lineShaderMetal = "Hidden/ProBuilder/LineBillboardMetal";

        /// <summary>
        /// Represents the shader used to draw camera facing billboards from a single vertex.
        /// </summary>
        /// <remarks>
        /// If the graphics device does not support geometry shaders, this shader doesn't compile.
        /// In that case, use <see cref="dotShader"/>.
        /// </remarks>
        public const string pointShader = "Hidden/ProBuilder/PointBillboard";

        /// <summary>
        /// Represents the fallback shader used to draw lines when the graphics device
        /// does not support geometry shaders.
        /// </summary>
        /// <seealso cref="lineShader"/>
        public const string wireShader = "Hidden/ProBuilder/FaceHighlight";

        /// <summary>
        /// Represents a fallback shader used to draw billboards when the graphics
        /// device does not support geometry shaders.
        /// </summary>
        /// <seealso cref="pointShader"/>
        public const string dotShader = "Hidden/ProBuilder/VertexShader";

        internal static readonly Color previewColor = new Color(.5f, .9f, 1f, .56f);

        static Shader s_SelectionPickerShader;

        static bool s_GeometryShadersSupported;

        static Material s_DefaultMaterial;
        static Material s_FacePickerMaterial;
        static Material s_VertexPickerMaterial;
        static Material s_EdgePickerMaterial;
        static Material s_UnityDefaultDiffuse;
        static Material s_ShapePreviewMaterial;

        static string k_EdgePickerMaterial = "Materials/EdgePicker";
        static string k_FacePickerMaterial = "Materials/FacePicker";
        static string k_VertexPickerMaterial = "Materials/VertexPicker";

        static string k_EdgePickerShader = "Hidden/ProBuilder/EdgePicker";
        static string k_FacePickerShader = "Hidden/ProBuilder/FacePicker";
        static string k_VertexPickerShader = "Hidden/ProBuilder/VertexPicker";

        static void Init()
        {
            if (s_IsInitialized)
                return;

            s_IsInitialized = true;

            var geo = Shader.Find(lineShader);
            s_GeometryShadersSupported = geo != null && geo.isSupported;

            // SelectionPicker shader
            s_SelectionPickerShader = (Shader)Shader.Find("Hidden/ProBuilder/SelectionPicker");

            if ((s_FacePickerMaterial = Resources.Load<Material>(k_FacePickerMaterial)) == null)
            {
                Log.Error("FacePicker material not loaded... please re-install ProBuilder to fix this error.");
                s_FacePickerMaterial = new Material(Shader.Find(k_FacePickerShader));
            }

            if ((s_VertexPickerMaterial = Resources.Load<Material>(k_VertexPickerMaterial)) == null)
            {
                Log.Error("VertexPicker material not loaded... please re-install ProBuilder to fix this error.");
                s_VertexPickerMaterial = new Material(Shader.Find(k_VertexPickerShader));
            }

            if ((s_EdgePickerMaterial = Resources.Load<Material>(k_EdgePickerMaterial)) == null)
            {
                Log.Error("EdgePicker material not loaded... please re-install ProBuilder to fix this error.");
                s_EdgePickerMaterial = new Material(Shader.Find(k_EdgePickerShader));
            }
        }

        /// <summary>
        /// Tests whether the current graphics device supports geometry shaders.
        /// </summary>
        /// <value>True if the current graphics device supports geometry shaders, and false if it does not.</value>
        public static bool geometryShadersSupported
        {
            get
            {
                Init();
                return s_GeometryShadersSupported;
            }
        }

        /// <summary>
        /// Represents the default ProBuilder material.
        /// </summary>
        /// <remarks>
        /// When using the Scriptable Render Pipeline this returns the default material for that pipeline.
        /// </remarks>
        public static Material defaultMaterial
        {
            get
            {
                Init();
                if(s_DefaultMaterial == null)
                    s_DefaultMaterial = GetDefaultMaterial();

                return s_DefaultMaterial;
            }
        }

        /// <summary>
        /// Represents the shader used in selection picking functions.
        /// </summary>
        internal static Shader selectionPickerShader
        {
            get
            {
                Init();
                return s_SelectionPickerShader;
            }
        }

        /// <summary>
        /// Represents the material used for face picking functions.
        /// </summary>
        internal static Material facePickerMaterial
        {
            get
            {
                Init();
                return s_FacePickerMaterial;
            }
        }

        /// <summary>
        /// Represents the material used for vertex picking functions.
        /// </summary>
        internal static Material vertexPickerMaterial
        {
            get
            {
                Init();
                return s_VertexPickerMaterial;
            }
        }

        /// <summary>
        /// Represents the material used for edge picking functions.
        /// </summary>
        internal static Material edgePickerMaterial
        {
            get
            {
                Init();
                return s_EdgePickerMaterial;
            }
        }

        /// <summary>
        /// Represents the material used for the ProBuilder <see cref="Entity_Trigger">Trigger</see>
        /// entity type material.
        /// </summary>
        internal static Material triggerMaterial
        {
            get
            {
                Init();
                return (Material)Resources.Load("Materials/Trigger", typeof(Material));
            }
        }

        /// <summary>
        /// Represents the material used for the ProBuilder <see cref="Entity_Trigger">Collider</see>
        /// entity type material.
        /// </summary>
        internal static Material colliderMaterial
        {
            get
            {
                Init();
                return (Material)Resources.Load("Materials/Collider", typeof(Material));
            }
        }

        /// <summary>
        /// Represents the default Unity diffuse material.
        /// </summary>
        internal static Material GetLegacyDiffuse()
        {
            Init();

            if (s_UnityDefaultDiffuse == null)
            {
                var mi = typeof(Material).GetMethod("GetDefaultMaterial",
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

                if (mi != null)
                    s_UnityDefaultDiffuse = mi.Invoke(null, null) as Material;

                if (s_UnityDefaultDiffuse == null)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    s_UnityDefaultDiffuse = go.GetComponent<MeshRenderer>().sharedMaterial;
                    Object.DestroyImmediate(go);
                }
            }

            return s_UnityDefaultDiffuse;
        }

        internal static Material GetDefaultMaterial()
        {
            var material = (Material) Resources.Load("Materials/ProBuilderDefault", typeof(Material));
            material.shader = Shader.Find("ProBuilder6/Standard Vertex Color");

            if (material == null || !material.shader.isSupported)
                material = GetLegacyDiffuse();

            return material;
        }

        static Material GetPreviewMaterial()
        {
            if (defaultMaterial == null)
                return null;

            var material = new Material(defaultMaterial.shader);
            material.hideFlags = HideFlags.HideAndDontSave;

            if (material.HasProperty("_MainTex"))
                material.mainTexture = (Texture2D)Resources.Load("Textures/GridBox_Default");

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", previewColor);

            return material;
        }

        internal static Material ShapePreviewMaterial
        {
            get
            {
                if(s_ShapePreviewMaterial == null)
                    s_ShapePreviewMaterial = GetPreviewMaterial();

                return s_ShapePreviewMaterial;
            }
        }
    }
}
