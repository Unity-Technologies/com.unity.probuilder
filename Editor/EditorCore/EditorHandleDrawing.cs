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
using UnityObject = UnityEngine.Object;

namespace UnityEditor.ProBuilder
{
    static partial class EditorHandleDrawing
    {
        const HideFlags k_ResourceHideFlags = HideFlags.HideAndDontSave;
        const float k_MinLineWidthForGeometryShader = .01f;

        static bool s_Initialized;

        static ObjectPool<Mesh> m_MeshPool2;

        static Dictionary<ProBuilderMesh, MeshHandle> m_WireHandles;
        static Dictionary<ProBuilderMesh, MeshHandle> m_VertexHandles;
        static Dictionary<ProBuilderMesh, MeshHandle> m_SelectedFaceHandles;
        static Dictionary<ProBuilderMesh, MeshHandle> m_SelectedVertexHandles;
        static Dictionary<ProBuilderMesh, MeshHandle> m_SelectedEdgeHandles;

        // Edge, vert, wire, and line materials Can be either point to a geometry shader or an alternative for devices
        // without geometry shader support
        static Material m_EdgeMaterial;
        static Material m_VertMaterial;
        static Material m_WireMaterial;
        static Material m_LineMaterial;
        static Material m_FaceMaterial;
        static Material m_GlWireMaterial;

        static Material edgeMaterial { get { Init(); return m_EdgeMaterial; } }
        static Material vertMaterial { get { Init(); return m_VertMaterial; } }
        static Material wireMaterial { get { Init(); return m_WireMaterial; } }
        static Material lineMaterial { get { Init(); return m_LineMaterial; } }
        static Material faceMaterial { get { Init(); return m_FaceMaterial; } }
        static Material glWireMaterial { get { Init(); return m_GlWireMaterial; } }

        static ObjectPool<Mesh> meshPool { get { Init(); return m_MeshPool2; } }
        static Dictionary<ProBuilderMesh, MeshHandle> wireHandles { get { Init(); return m_WireHandles; } }
        static Dictionary<ProBuilderMesh, MeshHandle> vertexHandles { get { Init(); return m_VertexHandles; } }
        static Dictionary<ProBuilderMesh, MeshHandle> selectedFaceHandles { get { Init(); return m_SelectedFaceHandles; } }
        static Dictionary<ProBuilderMesh, MeshHandle> selectedVertexHandles { get { Init(); return m_SelectedVertexHandles; } }
        static Dictionary<ProBuilderMesh, MeshHandle> selectedEdgeHandles { get { Init(); return m_SelectedEdgeHandles; } }

        static Color wireframeColor { get { return s_UseUnityColors ? k_WireframeDefault : s_WireframeColorPref; } }
        static Color faceSelectedColor { get { return s_UseUnityColors ? Handles.selectedColor : s_SelectedFaceColorPref; } }
        static Color preselectionColor { get { return s_UseUnityColors ? Handles.preselectionColor : s_PreselectionColorPref; } }
        static Color edgeSelectedColor { get { return s_UseUnityColors ? Handles.selectedColor : s_SelectedEdgeColorPref; } }
        static Color edgeUnselectedColor { get { return s_UseUnityColors ? k_WireframeDefault : s_UnselectedEdgeColorPref; } }
        static Color vertexSelectedColor { get { return s_UseUnityColors ? Handles.selectedColor : s_SelectedVertexColorPref; } }
        static Color vertexUnselectedColor { get { return s_UseUnityColors ? k_VertexUnselectedDefault : s_UnselectedVertexColorPref; } }

        // Force line rendering to use GL.LINE without geometry shader billboards. This is set by the
        // EnsureResourcesLoaded function based on available graphics API
        static bool m_ForceEdgeLinesGL;
        static bool m_ForceWireframeLinesGL;

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

        internal static float dotCapSize
        {
            get { return s_VertexPointSize * .0125f; }
        }

        static void Init()
        {
            if (s_Initialized)
                return;

            s_Initialized = true;

            ReleaseResources();

            m_MeshPool2 = new ObjectPool<Mesh>(0, 8, CreateMesh, DestroyMesh);
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

            ResetPreferences();
        }

        internal static void ReleaseResources()
        {
            ClearHandles();
            if(m_MeshPool2 != null)
                m_MeshPool2.Dispose();
            if(m_EdgeMaterial != null) UnityObject.DestroyImmediate(m_EdgeMaterial);
            if(m_WireMaterial != null) UnityObject.DestroyImmediate(m_WireMaterial);
            if(m_LineMaterial != null) UnityObject.DestroyImmediate(m_LineMaterial);
            if(m_VertMaterial != null) UnityObject.DestroyImmediate(m_VertMaterial);
            if(m_GlWireMaterial != null) UnityObject.DestroyImmediate(m_GlWireMaterial);
            if(m_FaceMaterial != null) UnityObject.DestroyImmediate(m_FaceMaterial);
        }

        internal static void ResetPreferences()
        {
            faceMaterial.SetFloat("_Dither", (s_UseUnityColors || s_DitherFaceHandle) ? 1f : 0f);

            m_ForceEdgeLinesGL = s_EdgeLineSize.value < k_MinLineWidthForGeometryShader;
            m_ForceWireframeLinesGL = s_WireframeLineSize.value < k_MinLineWidthForGeometryShader;

            wireMaterial.SetColor("_Color", wireframeColor);
            wireMaterial.SetInt("_HandleZTest", (int)CompareFunction.LessEqual);

            SetMaterialsScaleAttribute();
        }

        static Material CreateMaterial(Shader shader, string materialName)
        {
            if (shader == null)
                shader = BuiltinMaterials.defaultMaterial.shader;

            Material mat = new Material(shader);
            mat.name = materialName;
            mat.hideFlags = k_ResourceHideFlags;
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
            using (new TriangleDrawingScope(preselectionColor))
            {
                GL.MultMatrix(mesh.transform.localToWorldMatrix);
                foreach (var face in selection.faces)
                {
                    var ind = face.indexes;

                    for (int i = 0, c = ind.Count; i < c; i += 3)
                    {
                        GL.Vertex(positions[ind[i]]);
                        GL.Vertex(positions[ind[i + 1]]);
                        GL.Vertex(positions[ind[i + 2]]);
                    }
                }
            }
            using (var drawingScope = new LineDrawingScope(preselectionColor, mesh.transform.localToWorldMatrix, -1f, CompareFunction.Always))
            {
                foreach (var edge in selection.edges)
                {
                    drawingScope.DrawLine(positions[edge.a], positions[edge.b]);
                }
            }
            using (var drawingScope = new PointDrawingScope(preselectionColor, CompareFunction.Always) { matrix = mesh.transform.localToWorldMatrix })
            {
                foreach (var vertex in selection.vertexes)
                {
                    drawingScope.Draw(positions[vertex]);
                }
            }
        }

        public static void DrawSceneHandles(SelectMode mode)
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
                    Render(wireHandles, m_ForceEdgeLinesGL ? glWireMaterial : edgeMaterial, edgeUnselectedColor, CompareFunction.LessEqual, false);
                    Render(selectedEdgeHandles, m_ForceEdgeLinesGL ? glWireMaterial : edgeMaterial, edgeSelectedColor, s_DepthTestHandles ? CompareFunction.LessEqual : CompareFunction.Always, true);
                    break;
                }
                case SelectMode.Face:
                case SelectMode.TextureFace:
                {
                    Render(wireHandles, m_ForceWireframeLinesGL ? glWireMaterial : wireMaterial, wireframeColor, CompareFunction.LessEqual, false);
                    Render(selectedFaceHandles, faceMaterial, faceSelectedColor, s_DepthTestHandles);
                    break;
                }
                case SelectMode.Vertex:
                case SelectMode.TextureVertex:
                {
                    Render(wireHandles, m_ForceWireframeLinesGL ? glWireMaterial : wireMaterial, wireframeColor, CompareFunction.LessEqual, false);
                    Render(vertexHandles, vertMaterial, vertexUnselectedColor, CompareFunction.LessEqual, false);
                    Render(selectedVertexHandles, vertMaterial, vertexSelectedColor, s_DepthTestHandles);
                    break;
                }
                default:
                {
                    Render(wireHandles, m_ForceWireframeLinesGL ? glWireMaterial : wireMaterial, wireframeColor, CompareFunction.LessEqual, false);
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
            if(m_WireHandles != null)
                ClearHandlesInternal(wireHandles);
            if(m_VertexHandles != null)
                ClearHandlesInternal(m_VertexHandles);
            if(selectedFaceHandles != null)
                ClearHandlesInternal(m_SelectedFaceHandles);
            if(m_SelectedEdgeHandles != null)
                ClearHandlesInternal(m_SelectedEdgeHandles);
            if(m_SelectedVertexHandles != null)
                ClearHandlesInternal(m_SelectedVertexHandles);
        }

        public static void RebuildSelectedHandles(IEnumerable<ProBuilderMesh> meshes, SelectMode selectionMode)
        {
            ClearHandles();

            foreach (var mesh in meshes)
            {
                switch (selectionMode)
                {
                    case SelectMode.Vertex:
                    case SelectMode.TextureVertex:
                    {
                        RebuildMeshHandle(mesh, vertexHandles, MeshHandles.CreateVertexMesh);
                        var handle = GetMeshHandle(mesh, selectedVertexHandles);
                        MeshHandles.CreateVertexMesh(mesh, handle.mesh, mesh.selectedIndexesInternal);
                        goto default;
                    }

                    case SelectMode.Edge:
                    case SelectMode.TextureEdge:
                    {
                        if(m_ForceEdgeLinesGL || BuiltinMaterials.geometryShadersSupported)
                            RebuildMeshHandle(mesh, wireHandles, MeshHandles.CreateEdgeMesh);
                        else
                            RebuildMeshHandle(mesh, wireHandles, MeshHandles.CreateEdgeBillboardMesh);

                        var handle = GetMeshHandle(mesh, selectedEdgeHandles);

                        if(m_ForceEdgeLinesGL || BuiltinMaterials.geometryShadersSupported)
                            MeshHandles.CreateEdgeMesh(mesh, handle.mesh, mesh.selectedEdgesInternal);
                        else
                            MeshHandles.CreateEdgeBillboardMesh(mesh, handle.mesh, mesh.selectedEdgesInternal);

                        break;
                    }

                    case SelectMode.Face:
                    case SelectMode.TextureFace:
                    {
                        RebuildMeshHandle(mesh, selectedFaceHandles, MeshHandles.CreateFaceMesh);
                        goto default;
                    }

                    default:
                        if(m_ForceWireframeLinesGL || BuiltinMaterials.geometryShadersSupported)
                            RebuildMeshHandle(mesh, wireHandles, MeshHandles.CreateEdgeMesh);
                        else
                            RebuildMeshHandle(mesh, wireHandles, MeshHandles.CreateEdgeBillboardMesh);
                        break;
                }
            }
        }

        static MeshHandle GetMeshHandle(ProBuilderMesh mesh, Dictionary<ProBuilderMesh, MeshHandle> cache)
        {
            MeshHandle handle;

            if (!cache.TryGetValue(mesh, out handle))
            {
                var m = meshPool.Dequeue();
                handle = new MeshHandle(mesh.transform, m);
                cache.Add(mesh, handle);
            }

            return handle;
        }

        static void RebuildMeshHandle(ProBuilderMesh mesh, Dictionary<ProBuilderMesh, MeshHandle> list, Action<ProBuilderMesh, Mesh> ctor)
        {
            var handle = GetMeshHandle(mesh, list);
            ctor(mesh, handle.mesh);
        }

        static void ClearHandlesInternal(Dictionary<ProBuilderMesh, MeshHandle> handles)
        {
            foreach (var kvp in handles)
                meshPool.Enqueue(kvp.Value.mesh);
            handles.Clear();
        }

        static void SetMaterialsScaleAttribute()
        {
            vertMaterial.SetFloat("_Scale", s_VertexPointSize * EditorGUIUtility.pixelsPerPoint);
            wireMaterial.SetFloat("_Scale", s_WireframeLineSize * EditorGUIUtility.pixelsPerPoint);
            edgeMaterial.SetFloat("_Scale", s_EdgeLineSize * EditorGUIUtility.pixelsPerPoint);
        }
    }
}
