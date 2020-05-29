using System;
using UnityEngine.Rendering;
using System.Reflection;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Access the built-in materials that ProBuilder uses. In the editor, see
    /// <see cref="UnityEditor.ProBuilder.EditorMaterialUtility"/> for access to the full array of provided materials
    /// and shaders.
    /// </summary>
    public static class BuiltinMaterials
    {
        static bool s_IsInitialized;

        /// <value>
        /// A shader used to highlight face selections.
        /// </value>
        public const string faceShader = "Hidden/ProBuilder/FaceHighlight";

        /// <value>
        /// A shader used to highlight edge selections.
        /// </value>
        /// <remarks>
        /// If the graphics device does not support geometry shaders, this shader will not be compiled.
        /// Use <see cref="wireShader"/> in that case.
        /// </remarks>
        public const string lineShader = "Hidden/ProBuilder/LineBillboard";

        /// <value>
        /// A line shader for use with CreateEdgeBillboardMesh when geometry shaders are not available. Prefer to use
        /// <see cref="lineShader"/> where possible.
        /// </value>
        public const string lineShaderMetal = "Hidden/ProBuilder/LineBillboardMetal";

        /// <value>
        /// A shader used to draw camera facing billboards from a single vertex.
        /// </value>
        /// <remarks>
        /// If the graphics device does not support geometry shaders, this shader will not be compiled.
        /// Use <see cref="dotShader"/> in that case.
        /// </remarks>
        public const string pointShader = "Hidden/ProBuilder/PointBillboard";

        /// <value>
        /// A fallback shader used to draw lines when the graphics device does not support geometry shaders.
        /// </value>
        /// <seealso cref="lineShader"/>
        public const string wireShader = "Hidden/ProBuilder/FaceHighlight";

        /// <value>
        /// A fallback shader used to draw billboards when the graphics device does not support geometry shaders.
        /// </value>
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
        static Material s_UnlitVertexColorMaterial;
        static Material s_ShapePreviewMaterial;


#if HDRP_7_1_0_OR_NEWER
        static string k_EdgePickerMaterial = "Materials/EdgePickerHDRP";
        static string k_FacePickerMaterial = "Materials/FacePickerHDRP";
        static string k_VertexPickerMaterial = "Materials/VertexPickerHDRP";

        static string k_EdgePickerShader = "Hidden/ProBuilder/EdgePickerHDRP";
        static string k_FacePickerShader = "Hidden/ProBuilder/FacePickerHDRP";
        static string k_VertexPickerShader = "Hidden/ProBuilder/VertexPickerHDRP";
#else
        static string k_EdgePickerMaterial = "Materials/EdgePicker";
        static string k_FacePickerMaterial = "Materials/FacePicker";
        static string k_VertexPickerMaterial = "Materials/VertexPicker";

        static string k_EdgePickerShader = "Hidden/ProBuilder/EdgePicker";
        static string k_FacePickerShader = "Hidden/ProBuilder/FacePicker";
        static string k_VertexPickerShader = "Hidden/ProBuilder/VertexPicker";
#endif

        static void Init()
        {
            if (s_IsInitialized)
                return;

            s_IsInitialized = true;

            var geo = Shader.Find(lineShader);
            s_GeometryShadersSupported = geo != null && geo.isSupported;

            s_DefaultMaterial = GetDefaultMaterial();

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

            s_UnlitVertexColorMaterial = (Material)Resources.Load("Materials/UnlitVertexColor", typeof(Material));

            s_ShapePreviewMaterial = new Material(s_DefaultMaterial.shader);
            s_ShapePreviewMaterial.hideFlags = HideFlags.HideAndDontSave;

            if (s_ShapePreviewMaterial.HasProperty("_MainTex"))
                s_ShapePreviewMaterial.mainTexture = (Texture2D)Resources.Load("Textures/GridBox_Default");

            if (s_ShapePreviewMaterial.HasProperty("_Color"))
                s_ShapePreviewMaterial.SetColor("_Color", previewColor);
        }

        /// <summary>
        /// Return true if the current graphics device supports geometry shaders, and false if it does not.
        /// </summary>
        public static bool geometryShadersSupported
        {
            get
            {
                Init();
                return s_GeometryShadersSupported;
            }
        }

        /// <value>
        /// The default ProBuilder material.
        /// </value>
        /// <remarks>
        /// When using the Scriptable Render Pipeline this returns the default material for that pipeline.
        /// </remarks>
        public static Material defaultMaterial
        {
            get
            {
                Init();
                return s_DefaultMaterial;
            }
        }

        /// <value>
        /// Shader used in selection picking functions.
        /// </value>
        internal static Shader selectionPickerShader
        {
            get
            {
                Init();
                return s_SelectionPickerShader;
            }
        }

        /// <value>
        /// Material used for face picking functions.
        /// </value>
        internal static Material facePickerMaterial
        {
            get
            {
                Init();
                return s_FacePickerMaterial;
            }
        }

        /// <value>
        /// Material used for vertex picking functions.
        /// </value>
        internal static Material vertexPickerMaterial
        {
            get
            {
                Init();
                return s_VertexPickerMaterial;
            }
        }

        /// <value>
        /// Material used for edge picking functions.
        /// </value>
        internal static Material edgePickerMaterial
        {
            get
            {
                Init();
                return s_EdgePickerMaterial;
            }
        }

        /// <value>
        /// The ProBuilder "Trigger" entity type material.
        /// </value>
        internal static Material triggerMaterial
        {
            get
            {
                Init();
                return (Material)Resources.Load("Materials/Trigger", typeof(Material));
            }
        }

        /// <value>
        /// The ProBuilder "Collider" entity type material.
        /// </value>
        internal static Material colliderMaterial
        {
            get
            {
                Init();
                return (Material)Resources.Load("Materials/Collider", typeof(Material));
            }
        }

        /// <summary>
        /// The ProBuilder "NoDraw" material. Faces with this material are hidden when the game is played.
        /// </summary>
        [Obsolete("NoDraw is no longer supported.")]
        internal static Material noDrawMaterial
        {
            get
            {
                Init();
                return (Material)Resources.Load("Materials/NoDraw", typeof(Material));
            }
        }

        /// <summary>
        /// Default Unity diffuse material.
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
            Material material = null;

            if (GraphicsSettings.renderPipelineAsset != null)
            {
#if UNITY_2019_1_OR_NEWER
                    material = GraphicsSettings.renderPipelineAsset.defaultMaterial;
#else
                    material = GraphicsSettings.renderPipelineAsset.GetDefaultMaterial();
#endif
            }

            if (material == null)
            {
                material = (Material) Resources.Load("Materials/ProBuilderDefault", typeof(Material));

                if (material == null || !material.shader.isSupported)
                    material = GetLegacyDiffuse();
            }

            return material;
        }

        /// <summary>
        /// An unlit vertex color material.
        /// </summary>
        internal static Material unlitVertexColor
        {
            get
            {
                Init();
                return s_UnlitVertexColorMaterial;
            }
        }

        internal static Material ShapePreviewMaterial
        {
            get
            {
                Init();
                return s_ShapePreviewMaterial;
            }
        }
    }
}
