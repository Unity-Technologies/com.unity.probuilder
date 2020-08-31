using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Custom editor for ProBuilderMesh type.
    /// </summary>
    [CustomEditor(typeof(ProBuilderMesh))]
    [CanEditMultipleObjects]
    sealed class ProBuilderMeshEditor : Editor
    {
        static class Styles
        {
            static bool s_Initialized;
            public static GUIStyle miniButton;

            public static readonly GUIContent lightmapStatic = new GUIContent("Lightmap Static", "Controls whether the geometry will be marked as Static for lightmapping purposes. When enabled, this mesh will be present in lightmap calculations.");
            public static readonly GUIContent lightmapUVs = new GUIContent("Generate Lightmap UVs");

            public static void Init()
            {
                if (s_Initialized)
                    return;
                s_Initialized = true;
                miniButton = new GUIStyle(GUI.skin.button);
                miniButton.stretchHeight = false;
                miniButton.stretchWidth = false;
                miniButton.padding = new RectOffset(6, 6, 3, 3);
            }
        }

        internal static event System.Action onGetFrameBoundsEvent;
        ProBuilderMesh m_Mesh;

        SerializedObject m_GameObjectsSerializedObject;
        SerializedProperty m_UnwrapParameters;
        SerializedProperty m_StaticEditorFlags;
        bool m_AnyMissingLightmapUVs;
        bool m_ModifyingMesh;

        ProBuilderEditor editor
        {
            get { return ProBuilderEditor.instance; }
        }

        internal override string targetTitle
        {
            get { return "ProBuilder MeshFilter"; }
        }

        Renderer m_MeshRenderer = null;

        protected override void OnHeaderGUI()
        {
        }

        void OnEnable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            m_Mesh = (ProBuilderMesh)target;

            if (!m_Mesh)
                return;

            m_GameObjectsSerializedObject = new SerializedObject(serializedObject.targetObjects.Select(t => ((Component)t).gameObject).ToArray());

            m_UnwrapParameters = serializedObject.FindProperty("m_UnwrapParameters");
            m_StaticEditorFlags = m_GameObjectsSerializedObject.FindProperty("m_StaticEditorFlags");
            m_MeshRenderer = m_Mesh.gameObject.GetComponent<Renderer>();

            VertexManipulationTool.beforeMeshModification += OnBeginMeshModification;
            VertexManipulationTool.afterMeshModification += OnFinishMeshModification;
        }

        void OnDisable()
        {
            VertexManipulationTool.beforeMeshModification -= OnBeginMeshModification;
            VertexManipulationTool.afterMeshModification -= OnFinishMeshModification;

//            if (m_PreviewUtility != null)
//            {
//                m_PreviewUtility.Cleanup();
//                m_PreviewUtility = null;
//            }
//            if (m_WireMaterial)
//            {
//                DestroyImmediate(m_WireMaterial, true);
//            }
        }

        void OnBeginMeshModification(IEnumerable<ProBuilderMesh> selection)
        {
            m_ModifyingMesh = true;
        }

        void OnFinishMeshModification(IEnumerable<ProBuilderMesh> selection)
        {
            m_ModifyingMesh = false;
        }

        public override void OnInspectorGUI()
        {
            if (m_UnwrapParameters ==  null || m_StaticEditorFlags == null)
                return;

            Styles.Init();

            if (GUILayout.Button("Open ProBuilder"))
                ProBuilderEditor.MenuOpenWindow();

            Vector3 bounds = m_MeshRenderer != null ? m_MeshRenderer.bounds.size : Vector3.zero;
            EditorGUILayout.Vector3Field("Object Size (read only)", bounds);

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            LightmapStaticSettings();
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

#if DEVELOPER_MODE
            GUILayout.Label("Compiled Mesh Information", EditorStyles.boldLabel);

            MeshFilter filter;
            Mesh sharedMesh = null;

            if (m_Mesh.TryGetComponent(out filter) && (sharedMesh = filter.sharedMesh) != null)
            {
                // When inspecting a prefab asset the AssetDatabase continually loops on some very expensive operations
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                LightmapStaticSettings();
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
                GUILayout.Label("Vertex Count: " + sharedMesh.vertexCount);
                GUILayout.Label("Submesh Count: " + sharedMesh.subMeshCount);
            }
            else
            {
                GUILayout.Label("No compiled mesh", EditorStyles.centeredGreyMiniLabel);
            }

            GUILayout.Label("HideFlags & Driven Properties", EditorStyles.boldLabel);

            if(filter)
                GUILayout.Label($"MeshFilter {filter.hideFlags}");
            else
                GUILayout.Label($"MeshFilter (null)");

            MeshCollider collider;

            if (m_Mesh.TryGetComponent(out collider))
                GUILayout.Label($"MeshCollider.m_Mesh {DrivenPropertyManagerInternal.IsDriven(collider, "m_Mesh")}");
#endif
        }

        void LightmapStaticSettings()
        {
            m_GameObjectsSerializedObject.Update();

#if UNITY_2019_2_OR_NEWER
            bool lightmapStatic = (m_StaticEditorFlags.intValue & (int)StaticEditorFlags.ContributeGI) != 0;
#else
            bool lightmapStatic = (m_StaticEditorFlags.intValue & (int)StaticEditorFlags.LightmapStatic) != 0;
#endif

            EditorGUI.BeginChangeCheck();

            lightmapStatic = EditorGUILayout.Toggle(Styles.lightmapStatic, lightmapStatic);

            if (EditorGUI.EndChangeCheck())
            {
#if UNITY_2019_2_OR_NEWER
                SceneModeUtility.SetStaticFlags(m_GameObjectsSerializedObject.targetObjects, (int)StaticEditorFlags.ContributeGI, lightmapStatic);
#else
                SceneModeUtility.SetStaticFlags(m_GameObjectsSerializedObject.targetObjects, (int)StaticEditorFlags.LightmapStatic, lightmapStatic);
#endif
            }

            if (lightmapStatic)
            {
                EditorGUILayout.PropertyField(m_UnwrapParameters, true);

                if (m_UnwrapParameters.isExpanded)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Reset", Styles.miniButton))
                        ResetUnwrapParams(m_UnwrapParameters);

                    if (GUILayout.Button("Apply", Styles.miniButton))
                        RebuildLightmapUVs();

                    GUILayout.EndHorizontal();
                    GUILayout.Space(4);
                }

                if (!m_ModifyingMesh)
                {
                    m_AnyMissingLightmapUVs = targets.Any(x =>
                    {
                        if (x is ProBuilderMesh)
                            return !((ProBuilderMesh)x).HasArrays(MeshArrays.Texture1);

                        return false;
                    });
                }

                if (m_AnyMissingLightmapUVs)
                {
                    EditorGUILayout.HelpBox("Lightmap UVs are missing, please generate Lightmap UVs.", MessageType.Warning);

                    if (GUILayout.Button("Generate Lightmap UVs"))
                        RebuildLightmapUVs();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("To enable generation of lightmap UVs for this Mesh, please enable the 'Lightmap Static' property.", MessageType.Info);
            }

        }

        void RebuildLightmapUVs(bool forceRebuildAll = true)
        {
            foreach (var obj in targets)
            {
                if (obj is ProBuilderMesh)
                {
                    var mesh = (ProBuilderMesh)obj;

#if UNITY_2019_2_OR_NEWER
                    if (!mesh.gameObject.HasStaticFlag(StaticEditorFlags.ContributeGI))
                        continue;
#else
                    if (!mesh.gameObject.HasStaticFlag(StaticEditorFlags.LightmapStatic))
                        continue;
#endif

                    if (forceRebuildAll || !mesh.HasArrays(MeshArrays.Texture1))
                        mesh.Optimize(true);
                }
            }
        }

        void ResetUnwrapParams(SerializedProperty prop)
        {
            var hardAngle = prop.FindPropertyRelative("m_HardAngle");
            var packMargin = prop.FindPropertyRelative("m_PackMargin");
            var angleError = prop.FindPropertyRelative("m_AngleError");
            var areaError = prop.FindPropertyRelative("m_AreaError");

            hardAngle.floatValue = UnwrapParameters.k_HardAngle;
            packMargin.floatValue = UnwrapParameters.k_PackMargin;
            angleError.floatValue = UnwrapParameters.k_AngleError;
            areaError.floatValue = UnwrapParameters.k_AreaError;

            RebuildLightmapUVs();
        }

        bool HasFrameBounds()
        {
            if (m_Mesh == null)
                m_Mesh = (ProBuilderMesh)target;

            return ProBuilderEditor.instance != null &&
                InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms).Sum(x => x.selectedIndexesInternal.Length) > 0;
        }

        Bounds OnGetFrameBounds()
        {
            if (!ProBuilderEditor.selectMode.IsMeshElementMode())
                return m_MeshRenderer != null ? m_MeshRenderer.bounds : default(Bounds);

            if (onGetFrameBoundsEvent != null)
                onGetFrameBoundsEvent();

            Vector3 min = Vector3.zero, max = Vector3.zero;
            bool init = false;

            foreach (ProBuilderMesh mesh in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
            {
                int[] tris = mesh.selectedIndexesInternal;

                if (tris == null || tris.Length < 1)
                    continue;

                Vector3[] verts = mesh.positionsInternal;
                var trs = mesh.transform;

                if (!init)
                {
                    init = true;
                    min = trs.TransformPoint(verts[tris[0]]);
                    max = trs.TransformPoint(verts[tris[0]]);
                }

                for (int i = 0, c = tris.Length; i < c; i++)
                {
                    Vector3 p = trs.TransformPoint(verts[tris[i]]);

                    min.x = Mathf.Min(p.x, min.x);
                    max.x = Mathf.Max(p.x, max.x);

                    min.y = Mathf.Min(p.y, min.y);
                    max.y = Mathf.Max(p.y, max.y);

                    min.z = Mathf.Min(p.z, min.z);
                    max.z = Mathf.Max(p.z, max.z);
                }
            }

            return new Bounds((min + max) / 2f, max != min ? max - min : Vector3.one * .1f);
        }
//
//        private PreviewRenderUtility m_PreviewUtility;
//        private Material m_Material;
//        private Material m_WireMaterial;
//        public Vector2 previewDir = new Vector2(-120, 20);
//
//        void Init()
//        {
//            if (m_PreviewUtility == null)
//            {
//                m_PreviewUtility = new PreviewRenderUtility();
//                m_PreviewUtility.camera.fieldOfView = 30.0f;
//                m_Material = BuiltinMaterials.defaultMaterial;
//                m_WireMaterial = ModelInspector.CreateWireframeMaterial();
//            }
//        }
//
//        public override void OnPreviewSettings()
//        {
//            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
//                return;
//            GUI.enabled = true;
//            Init();
//        }
//
//        private void DoRenderPreview()
//        {
//            var t = target as ProBuilderMesh;
//            Mesh m;
//            if (t.mesh != null)
//                m  = t.mesh;
//            else
//            {
//                m = new Mesh();
//                UnityEngine.ProBuilder.MeshUtility.Compile(t,m);
//            }
//            ModelInspector.RenderMeshPreview(m, m_PreviewUtility, m_Material, m_WireMaterial, previewDir, -1);
//        }
//
//        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
//        {
//            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
//            {
//                Debug.Log("Could not generate static preview. Render texture not supported by hardware.");
//                return null;
//            }
//
//            Init();
//
//            m_PreviewUtility.BeginStaticPreview(new Rect(0, 0, width, height));
//
//            DoRenderPreview();
//
//            return m_PreviewUtility.EndStaticPreview();
//        }
//
//        public override bool HasPreviewGUI()
//        {
//            return (target != null);
//        }
//
//        public override void OnPreviewGUI(Rect r, GUIStyle background)
//        {
//            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
//            {
//                if (Event.current.type == EventType.Repaint)
//                    EditorGUI.DropShadowLabel(new Rect(r.x, r.y, r.width, 40), "Mesh preview requires\nrender texture support");
//                return;
//            }
//
//            Init();
//
//            previewDir = PreviewGUI.Drag2D(previewDir, r);
//
//            if (Event.current.type != EventType.Repaint)
//                return;
//
//            m_PreviewUtility.BeginPreview(r, background);
//
//            DoRenderPreview();
//
//            m_PreviewUtility.EndAndDrawPreview(r);
//        }
//
//        // A minimal list of settings to be shown in the Asset Store preview inspector
//        internal override void OnAssetStoreInspectorGUI()
//        {
//            OnInspectorGUI();
//        }
//
//        public override string GetInfoString()
//        {
//            var t = target as ProBuilderMesh;
//            t.Rebuild();
//            Mesh mesh = t.mesh;
//            string info = mesh.vertexCount + " verts, " + InternalMeshUtil.GetPrimitiveCount(mesh) + " tris";
//            int submeshes = mesh.subMeshCount;
//            if (submeshes > 1)
//                info += ", " + submeshes + " submeshes";
//
//            int blendShapeCount = mesh.blendShapeCount;
//            if (blendShapeCount > 0)
//                info += ", " + blendShapeCount + " blendShapes";
//
//            info += "\n" + InternalMeshUtil.GetVertexFormat(mesh);
//            return info;
//        }

        [MenuItem("CONTEXT/ProBuilderMesh/Open ProBuilder")]
        static void OpenProBuilder()
        {
            ProBuilderEditor.MenuOpenWindow();
        }
    }

    //[CustomPreview(typeof(ProBuilderMesh))]
    public class ProBuilderMeshPreview : ObjectPreview
    {
        static PreviewRenderUtility s_PreviewUtility;
        static Material s_Material;
        static Material s_WireMaterial;
        static Vector2 s_PreviewDir = new Vector2(-120, 20);

        static readonly Dictionary<Object, Mesh> s_Meshes = new Dictionary<Object, Mesh>();


        [InitializeOnLoadMethod]
        static void InitOnLoad()
        {
            ActiveEditorTracker.editorTrackerRebuilt += ActiveEditorTrackerOnEditorTrackerRebuilt;
        }

        static void ActiveEditorTrackerOnEditorTrackerRebuilt()
        {
            s_Meshes.Clear();
            foreach (var allInspectorWindow in InspectorWindow.GetAllInspectorWindows())
            {
                foreach (var activeEditor in allInspectorWindow.tracker.activeEditors)
                {
                    if (activeEditor is ProBuilderMeshEditor)
                    {
                         foreach (var o in activeEditor.targets)
                         {
                             var t = o as ProBuilderMesh;
                             if (!s_Meshes.ContainsKey(t))
                             {
                                 if (t.mesh != null)
                                     s_Meshes[t] = t.mesh;
                                 else
                                 {
                                     var m = new Mesh();
                                     UnityEngine.ProBuilder.MeshUtility.Compile(t,m);
                                     s_Meshes[t] = m;
                                 }
                             }
                         }
                    }
                }
            }
        }

        static void Init()
        {
            if (s_PreviewUtility != null) return;

            s_PreviewUtility = new PreviewRenderUtility();
            s_PreviewUtility.camera.fieldOfView = 30.0f;
            s_Material = BuiltinMaterials.defaultMaterial;
            s_WireMaterial = ModelInspector.CreateWireframeMaterial();
        }

        public override void OnPreviewSettings()
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
                return;
            GUI.enabled = true;
            Init();
        }


        public override bool HasPreviewGUI()
        {
            return (target != null);
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
            {
                if (Event.current.type == EventType.Repaint)
                    EditorGUI.DropShadowLabel(new Rect(r.x, r.y, r.width, 40), "Mesh preview requires\nrender texture support");
                return;
            }

            Init();

            s_PreviewDir = PreviewGUI.Drag2D(s_PreviewDir, r);

            if (Event.current.type != EventType.Repaint)
                return;

            s_PreviewUtility.BeginPreview(r, background);

            var t = target as ProBuilderMesh;
            ModelInspector.RenderMeshPreview(s_Meshes[t], s_PreviewUtility, s_Material, s_WireMaterial, s_PreviewDir, -1);

            s_PreviewUtility.EndAndDrawPreview(r);
        }

        public override string GetInfoString()
        {
            var mesh = s_Meshes[target];
            var info = $"{mesh.vertexCount} verts, {InternalMeshUtil.GetPrimitiveCount(mesh)} tris";
            var subMeshes = mesh.subMeshCount;
            if (subMeshes > 1)
                info += $", {subMeshes} submeshes";

            var blendShapeCount = mesh.blendShapeCount;
            if (blendShapeCount > 0)
                info += $", {blendShapeCount} blendShapes";

            info += $"\n{InternalMeshUtil.GetVertexFormat(mesh)}";
            return info;
        }
    }
}
