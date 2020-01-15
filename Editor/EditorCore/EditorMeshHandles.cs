using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEditor.SettingsManagement;
#if !UNITY_2019_1_OR_NEWER
using System.Reflection;
#endif

namespace UnityEditor.ProBuilder
{
    partial class EditorMeshHandles : IHasPreferences
    {
        const HideFlags k_MeshHideFlags = (HideFlags)(1 | 2 | 4 | 8);
        const float k_MinLineWidthForGeometryShader = .01f;

        static EditorMeshHandles s_Instance;
        bool m_Initialized;
        ObjectPool<Mesh> m_MeshPool;

        Dictionary<ProBuilderMesh, MeshHandle> m_WireHandles;
        Dictionary<ProBuilderMesh, MeshHandle> m_VertexHandles;

        Dictionary<ProBuilderMesh, MeshHandle> m_SelectedFaceHandles;
        Dictionary<ProBuilderMesh, MeshHandle> m_SelectedVertexHandles;
        Dictionary<ProBuilderMesh, MeshHandle> m_SelectedEdgeHandles;

        static readonly Color k_VertexUnselectedDefault = new Color(.7f, .7f, .7f, 1f);
        static readonly Color k_WireframeDefault = new Color(94.0f / 255.0f, 119.0f / 255.0f, 155.0f / 255.0f, 1f);

        [UserSetting]
        static Pref<bool> s_UseUnityColors = new Pref<bool>("graphics.handlesUseUnityColors", true, SettingsScope.User);
        [UserSetting]
        static Pref<bool> s_DitherFaceHandle = new Pref<bool>("graphics.ditherFaceHandles", true, SettingsScope.User);
        [UserSetting]
        static Pref<Color> s_SelectedFaceColorPref = new Pref<Color>("graphics.userSelectedFaceColor", new Color(0f, 210f / 255f, 239f / 255f, 1f), SettingsScope.User);
        [UserSetting]
        static Pref<Color> s_WireframeColorPref = new Pref<Color>("graphics.userWireframeColor", new Color(125f / 255f, 155f / 255f, 185f / 255f, 1f), SettingsScope.User);
        [UserSetting]
        static Pref<Color> s_UnselectedEdgeColorPref = new Pref<Color>("graphics.userUnselectedEdgeColor", new Color(44f / 255f, 44f / 255f, 44f / 255f, 1f), SettingsScope.User);
        [UserSetting]
        static Pref<Color> s_SelectedEdgeColorPref = new Pref<Color>("graphics.userSelectedEdgeColor", new Color(0f, 210f / 255f, 239f / 255f, 1f), SettingsScope.User);
        [UserSetting]
        static Pref<Color> s_UnselectedVertexColorPref = new Pref<Color>("graphics.userUnselectedVertexColor", new Color(44f / 255f, 44f / 255f, 44f / 255f, 1f), SettingsScope.User);
        [UserSetting]
        static Pref<Color> s_SelectedVertexColorPref = new Pref<Color>("graphics.userSelectedVertexColor", new Color(0f, 210f / 255f, 239f / 255f, 1f), SettingsScope.User);
        [UserSetting]
        static Pref<Color> s_PreselectionColorPref = new Pref<Color>("graphics.userPreselectionColor", new Color(179f / 255f, 246f / 255f, 255f / 255f, 1f), SettingsScope.User);

        [UserSetting]
        static Pref<float> s_WireframeLineSize = new Pref<float>("graphics.wireframeLineSize", .5f, SettingsScope.User);
        [UserSetting]
        static Pref<float> s_EdgeLineSize = new Pref<float>("graphics.edgeLineSize", 1f, SettingsScope.User);
        [UserSetting]
        static Pref<float> s_VertexPointSize = new Pref<float>("graphics.vertexPointSize", 3f, SettingsScope.User);

        [UserSetting]
        static Pref<bool> s_DepthTestHandles = new Pref<bool>("graphics.handleZTest", true, SettingsScope.User);

        [UserSettingBlock("Graphics")]
        static void HandleColorPreferences(string searchContext)
        {
            EditorGUI.BeginChangeCheck();

            s_UseUnityColors.value = SettingsGUILayout.SettingsToggle("Use Unity Colors", s_UseUnityColors, searchContext);

            if (!s_UseUnityColors.value)
            {
                using (new SettingsGUILayout.IndentedGroup())
                {
                    s_DitherFaceHandle.value = SettingsGUILayout.SettingsToggle("Dither Face Overlay", s_DitherFaceHandle, searchContext);
                    s_WireframeColorPref.value = SettingsGUILayout.SettingsColorField("Wireframe", s_WireframeColorPref, searchContext);
                    s_PreselectionColorPref.value = SettingsGUILayout.SettingsColorField("Preselection", s_PreselectionColorPref, searchContext);
                    s_SelectedFaceColorPref.value = SettingsGUILayout.SettingsColorField("Selected Face Color", s_SelectedFaceColorPref, searchContext);
                    s_UnselectedEdgeColorPref.value = SettingsGUILayout.SettingsColorField("Unselected Edge Color", s_UnselectedEdgeColorPref, searchContext);
                    s_SelectedEdgeColorPref.value = SettingsGUILayout.SettingsColorField("Selected Edge Color", s_SelectedEdgeColorPref, searchContext);
                    s_UnselectedVertexColorPref.value = SettingsGUILayout.SettingsColorField("Unselected Vertex Color", s_UnselectedVertexColorPref, searchContext);
                    s_SelectedVertexColorPref.value = SettingsGUILayout.SettingsColorField("Selected Vertex Color", s_SelectedVertexColorPref, searchContext);
                }
            }

            s_DepthTestHandles.value = SettingsGUILayout.SettingsToggle("Depth Test", s_DepthTestHandles, searchContext);
            s_VertexPointSize.value = SettingsGUILayout.SettingsSlider("Vertex Size", s_VertexPointSize, 1f, 10f, searchContext);
            s_EdgeLineSize.value = SettingsGUILayout.SettingsSlider("Line Size", s_EdgeLineSize, 0f, 10f, searchContext);
            s_WireframeLineSize.value = SettingsGUILayout.SettingsSlider("Wireframe Size", s_WireframeLineSize, 0f, 10f, searchContext);

            if(EditorGUI.EndChangeCheck())
                ProBuilderEditor.UpdateMeshHandles(true);
        }

        static Color s_FaceSelectedColor;
        static Color s_WireframeColor;
        static Color s_PreselectionColor;
        static Color s_EdgeSelectedColor;
        static Color s_EdgeUnselectedColor;
        static Color s_VertexSelectedColor;
        static Color s_VertexUnselectedColor;

        // Edge, vert, wire, and line materials Can be either point to a geometry shader or an alternative for devices
        // without geometry shader support
        Material m_EdgeMaterial;
        Material m_VertMaterial;
        Material m_WireMaterial;
        Material m_LineMaterial;
        Material m_FaceMaterial;
        Material m_GlWireMaterial;

        // Force line rendering to use GL.LINE without geometry shader billboards
        bool m_ForceEdgeLinesGL;
        bool m_ForceWireframeLinesGL;

        internal static float dotCapSize
        {
            get { return s_VertexPointSize * .0125f; }
        }

        EditorMeshHandles()
        {
            Init();
        }

        void Init()
        {
            if (m_Initialized)
                return;

            m_Initialized = true;

            m_MeshPool = new ObjectPool<Mesh>(0, 8, CreateMesh, DestroyMesh);
            m_WireHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
            m_VertexHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
            m_SelectedFaceHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
            m_SelectedEdgeHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
            m_SelectedVertexHandles = new Dictionary<ProBuilderMesh, MeshHandle>();

            var lineShader = BuiltinMaterials.geometryShadersSupported ? BuiltinMaterials.lineShader : BuiltinMaterials.lineShaderMetal;
            var vertShader = BuiltinMaterials.geometryShadersSupported ? BuiltinMaterials.pointShader : BuiltinMaterials.dotShader;

            m_EdgeMaterial = CreateMaterial(Shader.Find(lineShader), "ProBuilder::LineMaterial");
            m_WireMaterial = CreateMaterial(Shader.Find(lineShader), "ProBuilder::WireMaterial");
            m_LineMaterial = CreateMaterial(Shader.Find(lineShader), "ProBuilder::GeneralUseLineMaterial");
            m_VertMaterial = CreateMaterial(Shader.Find(vertShader), "ProBuilder::VertexMaterial");
            m_GlWireMaterial = CreateMaterial(Shader.Find(BuiltinMaterials.faceShader), "ProBuilder::GLWire");
            m_FaceMaterial = CreateMaterial(Shader.Find(BuiltinMaterials.faceShader), "ProBuilder::FaceMaterial");

            m_FaceMaterial.SetFloat("_Dither", (s_UseUnityColors || s_DitherFaceHandle) ? 1f : 0f);

            ReloadPreferences();
        }

        static EditorMeshHandles Get()
        {
            if(s_Instance == null)
                s_Instance = new EditorMeshHandles();
            return s_Instance;
        }

        void DestroyResources()
        {
            ClearHandles();
            m_MeshPool.Dispose();
            UObject.DestroyImmediate(m_EdgeMaterial);
            UObject.DestroyImmediate(m_WireMaterial);
            UObject.DestroyImmediate(m_VertMaterial);
            UObject.DestroyImmediate(m_FaceMaterial);
            m_Initialized = false;
        }

        internal static void ResetPreferences()
        {
            Get().ReloadPreferences();
        }

        public void ReloadPreferences()
        {
            if (s_UseUnityColors)
            {
                s_WireframeColor = k_WireframeDefault;

                s_FaceSelectedColor = Handles.selectedColor;

                s_EdgeSelectedColor = Handles.selectedColor;
                s_EdgeUnselectedColor = k_WireframeDefault;

                s_VertexSelectedColor = Handles.selectedColor;
                s_VertexUnselectedColor = k_VertexUnselectedDefault;

                s_PreselectionColor = Handles.preselectionColor;
            }
            else
            {
                s_WireframeColor = s_WireframeColorPref;

                s_FaceSelectedColor = s_SelectedFaceColorPref;
                s_PreselectionColor = s_PreselectionColorPref;

                s_EdgeSelectedColor = s_SelectedEdgeColorPref;
                s_EdgeUnselectedColor = s_UnselectedEdgeColorPref;

                s_VertexSelectedColor = s_SelectedVertexColorPref;
                s_VertexUnselectedColor = s_UnselectedVertexColorPref;
            }

            m_ForceEdgeLinesGL = s_EdgeLineSize.value < k_MinLineWidthForGeometryShader;
            m_ForceWireframeLinesGL = s_WireframeLineSize.value < k_MinLineWidthForGeometryShader;

            m_WireMaterial.SetColor("_Color", s_WireframeColor);
            m_WireMaterial.SetInt("_HandleZTest", (int)CompareFunction.LessEqual);

            SetMaterialsScaleAttribute();
        }

        static Material CreateMaterial(Shader shader, string materialName)
        {
            Material mat = new Material(shader);
            mat.name = materialName;
            mat.hideFlags = k_MeshHideFlags;
            return mat;
        }

        static Mesh CreateMesh()
        {
            var mesh = new Mesh();
            mesh.name = "EditorMeshHandles.MeshHandle" + mesh.GetInstanceID();
            mesh.hideFlags = HideFlags.HideAndDontSave;
            return mesh;
        }

        static void DestroyMesh(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            UObject.DestroyImmediate(mesh);
        }

#if !UNITY_2019_1_OR_NEWER
        static MethodInfo s_ApplyWireMaterial = null;

        static object[] s_ApplyWireMaterialArgs = new object[]
        {
            CompareFunction.Always
        };
#endif

        public static void DrawSceneSelection(SceneSelection selection)
        {
            var mesh = selection.mesh;

            if (mesh == null)
                return;

            var positions = mesh.positionsInternal;

            // Draw nearest edge
            if (selection.face != null)
            {
                using (new TriangleDrawingScope(s_PreselectionColor))
                {
                    GL.MultMatrix(mesh.transform.localToWorldMatrix);

                    var face = selection.face;
                    var ind = face.indexes;

                    for (int i = 0, c = ind.Count; i < c; i += 3)
                    {
                        GL.Vertex(positions[ind[i]]);
                        GL.Vertex(positions[ind[i+1]]);
                        GL.Vertex(positions[ind[i+2]]);
                    }
                }
            }
            else if (selection.edge != Edge.Empty)
            {
                using (var drawingScope = new LineDrawingScope(s_PreselectionColor, mesh.transform.localToWorldMatrix, -1f, CompareFunction.Always))
                {
                    drawingScope.DrawLine(positions[selection.edge.a], positions[selection.edge.b]);
                }
            }
            else if (selection.vertex > -1)
            {
                using (var drawingScope = new PointDrawingScope(s_PreselectionColor, CompareFunction.Always) { matrix = mesh.transform.localToWorldMatrix })
                {
                    drawingScope.Draw(positions[selection.vertex]);
                }
            }
        }

        public static void DrawSceneHandles(SelectMode mode)
        {
            Get().DrawSceneHandlesInternal(mode);
        }

        void DrawSceneHandlesInternal(SelectMode mode)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            // Update the scale based on EditorGUIUtility.pixelsPerPoints in case the DPI would have changed.
            SetMaterialsScaleAttribute();

            switch (mode)
            {
                case SelectMode.Edge:
                case SelectMode.TextureEdge:
                {
                    // When in Edge mode, use the same material for wireframe
                    Render(m_WireHandles, m_ForceEdgeLinesGL ? m_GlWireMaterial : m_EdgeMaterial, s_EdgeUnselectedColor, CompareFunction.LessEqual, false);
                    Render(m_SelectedEdgeHandles, m_ForceEdgeLinesGL ? m_GlWireMaterial : m_EdgeMaterial, s_EdgeSelectedColor, s_DepthTestHandles ? CompareFunction.LessEqual : CompareFunction.Always, true);
                    break;
                }
                case SelectMode.Face:
                case SelectMode.TextureFace:
                {
                    Render(m_WireHandles, m_ForceWireframeLinesGL ? m_GlWireMaterial : m_WireMaterial, s_WireframeColor, CompareFunction.LessEqual, false);
                    Render(m_SelectedFaceHandles, m_FaceMaterial, s_FaceSelectedColor, s_DepthTestHandles);
                    break;
                }
                case SelectMode.Vertex:
                case SelectMode.TextureVertex:
                {
                    Render(m_WireHandles, m_ForceWireframeLinesGL ? m_GlWireMaterial : m_WireMaterial, s_WireframeColor, CompareFunction.LessEqual, false);
                    Render(m_VertexHandles, m_VertMaterial, s_VertexUnselectedColor, CompareFunction.LessEqual, false);
                    Render(m_SelectedVertexHandles, m_VertMaterial, s_VertexSelectedColor, s_DepthTestHandles);
                    break;
                }
                default:
                {
                    Render(m_WireHandles, m_ForceWireframeLinesGL ? m_GlWireMaterial : m_WireMaterial, s_WireframeColor, CompareFunction.LessEqual, false);
                    break;
                }
            }
        }

        static void Render(Dictionary<ProBuilderMesh, MeshHandle> handles, Material material, Color color, bool depthTest = true)
        {
            Render(handles, material, color, depthTest ? CompareFunction.LessEqual : CompareFunction.Always, true);
        }

        static void Render(Dictionary<ProBuilderMesh, MeshHandle> handles, Material material, Color color, CompareFunction func, bool zWrite)
        {
            material.SetInt("_HandleZTest", (int) func);
            material.SetInt("_HandleZWrite", zWrite ? 1 : 0);
            material.SetColor("_Color", color);

            if (material.SetPass(0))
            {
                foreach (var kvp in handles)
                    kvp.Value.DrawMeshNow(0);
            }
        }

        public static void ClearHandles()
        {
            Get().ClearHandlesInternal();
        }

        void ClearHandlesInternal()
        {
            ClearHandlesInternal(m_WireHandles);
            ClearHandlesInternal(m_VertexHandles);
            ClearHandlesInternal(m_SelectedFaceHandles);
            ClearHandlesInternal(m_SelectedEdgeHandles);
            ClearHandlesInternal(m_SelectedVertexHandles);
        }

        public static void RebuildSelectedHandles( IEnumerable<ProBuilderMesh> meshes, SelectMode selectionMode)
        {
            Get().RebuildSelectedHandlesInternal(meshes, selectionMode);
        }

        void RebuildSelectedHandlesInternal(IEnumerable<ProBuilderMesh> meshes, SelectMode selectionMode)
        {
            ClearHandles();

            foreach (var mesh in meshes)
            {
                switch (selectionMode)
                {
                    case SelectMode.Vertex:
                    case SelectMode.TextureVertex:
                    {
                        RebuildMeshHandle(mesh, m_VertexHandles, MeshHandles.CreateVertexMesh);
                        var handle = GetMeshHandle(mesh, m_SelectedVertexHandles);
                        MeshHandles.CreateVertexMesh(mesh, handle.mesh, mesh.selectedIndexesInternal);
                        goto default;
                    }

                    case SelectMode.Edge:
                    case SelectMode.TextureEdge:
                    {
                        if(m_ForceEdgeLinesGL || BuiltinMaterials.geometryShadersSupported)
                            RebuildMeshHandle(mesh, m_WireHandles, MeshHandles.CreateEdgeMesh);
                        else
                            RebuildMeshHandle(mesh, m_WireHandles, MeshHandles.CreateEdgeBillboardMesh);

                        var handle = GetMeshHandle(mesh, m_SelectedEdgeHandles);

                        if(m_ForceEdgeLinesGL || BuiltinMaterials.geometryShadersSupported)
                            MeshHandles.CreateEdgeMesh(mesh, handle.mesh, mesh.selectedEdgesInternal);
                        else
                            MeshHandles.CreateEdgeBillboardMesh(mesh, handle.mesh, mesh.selectedEdgesInternal);

                        break;
                    }

                    case SelectMode.Face:
                    case SelectMode.TextureFace:
                    {
                        RebuildMeshHandle(mesh, m_SelectedFaceHandles, MeshHandles.CreateFaceMesh);
                        goto default;
                    }

                    default:
                        if(m_ForceWireframeLinesGL || BuiltinMaterials.geometryShadersSupported)
                            RebuildMeshHandle(mesh, m_WireHandles, MeshHandles.CreateEdgeMesh);
                        else
                            RebuildMeshHandle(mesh, m_WireHandles, MeshHandles.CreateEdgeBillboardMesh);
                        break;
                }
            }
        }

        MeshHandle GetMeshHandle(ProBuilderMesh mesh, Dictionary<ProBuilderMesh, MeshHandle> cache)
        {
            MeshHandle handle;

            if (!cache.TryGetValue(mesh, out handle))
            {
                var m = m_MeshPool.Get();
                handle = new MeshHandle(mesh.transform, m);
                cache.Add(mesh, handle);
            }

            return handle;
        }

        void RebuildMeshHandle(ProBuilderMesh mesh, Dictionary<ProBuilderMesh, MeshHandle> list, Action<ProBuilderMesh, Mesh> ctor)
        {
            var handle = GetMeshHandle(mesh, list);
            ctor(mesh, handle.mesh);
        }

        void ClearHandlesInternal(Dictionary<ProBuilderMesh, MeshHandle> handles)
        {
            foreach (var kvp in handles)
                m_MeshPool.Put(kvp.Value.mesh);
            handles.Clear();
        }

        void SetMaterialsScaleAttribute()
        {
            m_VertMaterial.SetFloat("_Scale", s_VertexPointSize * EditorGUIUtility.pixelsPerPoint);
            m_WireMaterial.SetFloat("_Scale", s_WireframeLineSize * EditorGUIUtility.pixelsPerPoint);
            m_EdgeMaterial.SetFloat("_Scale", s_EdgeLineSize * EditorGUIUtility.pixelsPerPoint);
        }
    }
}
