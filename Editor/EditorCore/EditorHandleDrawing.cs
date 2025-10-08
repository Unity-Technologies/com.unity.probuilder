using System;
using UnityEngine;
using UnityEngine.ProBuilder;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine.Rendering;
using UnityEditor.SettingsManagement;
using UnityObject = UnityEngine.Object;
using Edge = UnityEngine.ProBuilder.Edge;

namespace UnityEditor.ProBuilder
{
    static partial class EditorHandleDrawing
    {
        const HideFlags k_ResourceHideFlags = HideFlags.HideAndDontSave;
        const float k_MinLineWidthForGeometryShader = .01f;
        static readonly Color k_OccludedTint = new Color(.75f, .75f, .75f, 1f);

        static bool s_Initialized;

        static ObjectPool<Mesh> s_MeshPool2;

        static Dictionary<ProBuilderMesh, MeshHandle> s_WireHandles;
        static Dictionary<ProBuilderMesh, MeshHandle> s_VertexHandles;
        static Dictionary<ProBuilderMesh, MeshHandle> s_SelectedFaceHandles;
        static Dictionary<ProBuilderMesh, MeshHandle> s_SelectedVertexHandles;
        static Dictionary<ProBuilderMesh, MeshHandle> s_SelectedEdgeHandles;

        // Edge, vert, wire, and line materials Can be either point to a geometry shader or an alternative for devices
        // without geometry shader support
        static Material s_EdgeMaterial;
        static Material s_VertMaterial;
        static Material s_WireMaterial;
        static Material s_LineMaterial;
        static Material s_FaceMaterial;
        static Material s_GlWireMaterial;

        static Material edgeMaterial { get { Init(); return s_EdgeMaterial; } }
        static Material vertMaterial { get { Init(); return s_VertMaterial; } }
        static Material wireMaterial { get { Init(); return s_WireMaterial; } }
        static Material lineMaterial { get { Init(); return s_LineMaterial; } }
        static Material faceMaterial { get { Init(); return s_FaceMaterial; } }
        static Material glWireMaterial { get { Init(); return s_GlWireMaterial; } }

        static ObjectPool<Mesh> meshPool { get { Init(); return s_MeshPool2; } }
        static Dictionary<ProBuilderMesh, MeshHandle> wireHandles { get { Init(); return s_WireHandles; } }
        static Dictionary<ProBuilderMesh, MeshHandle> vertexHandles { get { Init(); return s_VertexHandles; } }
        static Dictionary<ProBuilderMesh, MeshHandle> selectedFaceHandles { get { Init(); return s_SelectedFaceHandles; } }
        static Dictionary<ProBuilderMesh, MeshHandle> selectedVertexHandles { get { Init(); return s_SelectedVertexHandles; } }
        static Dictionary<ProBuilderMesh, MeshHandle> selectedEdgeHandles { get { Init(); return s_SelectedEdgeHandles; } }

        static Color wireframeColor { get { return s_UseUnityColors ? k_WireframeDefault : s_WireframeColorPref; } }
        internal static Color faceSelectedColor { get { return s_UseUnityColors ? Handles.selectedColor : s_SelectedFaceColorPref; } }
        internal static Color preselectionColor { get { return s_UseUnityColors ? Handles.preselectionColor : s_PreselectionColorPref; } }
        internal static Color edgeSelectedColor { get { return s_UseUnityColors ? Handles.selectedColor : s_SelectedEdgeColorPref; } }
        static Color edgeUnselectedColor { get { return s_UseUnityColors ? k_WireframeDefault : s_UnselectedEdgeColorPref; } }
        internal static Color vertexSelectedColor { get { return s_UseUnityColors ? Handles.selectedColor : s_SelectedVertexColorPref; } }
        static Color vertexUnselectedColor { get { return s_UseUnityColors ? k_VertexUnselectedDefault : s_UnselectedVertexColorPref; } }

        // Force line rendering to use GL.LINE without geometry shader billboards. This is set by the
        // EnsureResourcesLoaded function based on available graphics API
        static bool m_ForceEdgeLinesGL;
        static bool m_ForceWireframeLinesGL;


        static Dictionary<ProBuilderMesh, MeshHandle> s_TemporaryHandles;

        static readonly Color k_VertexUnselectedDefault = new Color(.7f, .7f, .7f, 1f);
        static readonly Color k_WireframeDefault = new Color(94.0f / 255.0f, 119.0f / 255.0f, 155.0f / 255.0f, 1f);

        [UserSetting] static Pref<bool> s_UseUnityColors =
            new Pref<bool>("graphics.handlesUseUnityColors", true, SettingsScope.User);

        [UserSetting] static Pref<bool> s_DitherFaceHandle =
            new Pref<bool>("graphics.ditherFaceHandles", true, SettingsScope.User);

        [UserSetting] static Pref<Color> s_SelectedFaceColorPref = new Pref<Color>("graphics.userSelectedFaceColor",
            new Color(0f, 210f / 255f, 239f / 255f, 1f), SettingsScope.User);

        [UserSetting] static Pref<Color> s_WireframeColorPref = new Pref<Color>("graphics.userWireframeColor",
            new Color(125f / 255f, 155f / 255f, 185f / 255f, 1f), SettingsScope.User);

        [UserSetting] static Pref<Color> s_UnselectedEdgeColorPref = new Pref<Color>("graphics.userUnselectedEdgeColor",
            new Color(44f / 255f, 44f / 255f, 44f / 255f, 1f), SettingsScope.User);

        [UserSetting] static Pref<Color> s_SelectedEdgeColorPref = new Pref<Color>("graphics.userSelectedEdgeColor",
            new Color(0f, 210f / 255f, 239f / 255f, 1f), SettingsScope.User);

        [UserSetting] static Pref<Color> s_UnselectedVertexColorPref = new Pref<Color>(
            "graphics.userUnselectedVertexColor", new Color(44f / 255f, 44f / 255f, 44f / 255f, 1f),
            SettingsScope.User);

        [UserSetting] static Pref<Color> s_SelectedVertexColorPref = new Pref<Color>("graphics.userSelectedVertexColor",
            new Color(0f, 210f / 255f, 239f / 255f, 1f), SettingsScope.User);

        [UserSetting] static Pref<Color> s_PreselectionColorPref = new Pref<Color>("graphics.userPreselectionColor",
            new Color(179f / 255f, 246f / 255f, 255f / 255f, 1f), SettingsScope.User);

        [UserSetting] static Pref<float> s_WireframeLineSize =
            new Pref<float>("graphics.wireframeLineSize", .5f, SettingsScope.User);

        [UserSetting]
        static Pref<float> s_EdgeLineSize = new Pref<float>("graphics.edgeLineSize", 1f, SettingsScope.User);

        [UserSetting]
        static Pref<float> s_VertexPointSize = new Pref<float>("graphics.vertexPointSize", 3f, SettingsScope.User);

        [UserSetting]
        static Pref<bool> s_XRayView = new Pref<bool>("graphics.xRayView", true, SettingsScope.User);

        [Obsolete]
        static Pref<bool> s_DepthTestHandles = new Pref<bool>("graphics.handleZTest", true, SettingsScope.User);

        static readonly GUIContent k_XRaySetting = new GUIContent("Selection X-Ray", "When enabled, selected mesh elements that are occluded by geometry will be rendered with a faded appearance.");

        [UserSettingBlock("Graphics")]
        static void HandleColorPreferences(string searchContext)
        {
            EditorGUI.BeginChangeCheck();

            s_XRayView.value = SettingsGUILayout.SettingsToggle(k_XRaySetting, s_XRayView, searchContext);

            s_UseUnityColors.value = SettingsGUILayout.SettingsToggle("Use Unity Colors", s_UseUnityColors, searchContext);

            if(!s_UseUnityColors.value)
            {
                using(new SettingsGUILayout.IndentedGroup())
                {
                    s_DitherFaceHandle.value =
                        SettingsGUILayout.SettingsToggle("Dither Face Overlay", s_DitherFaceHandle, searchContext);
                    s_WireframeColorPref.value =
                        SettingsGUILayout.SettingsColorField("Wireframe", s_WireframeColorPref, searchContext);
                    s_PreselectionColorPref.value =
                        SettingsGUILayout.SettingsColorField("Preselection", s_PreselectionColorPref, searchContext);
                    s_SelectedFaceColorPref.value = SettingsGUILayout.SettingsColorField("Selected Face Color",
                        s_SelectedFaceColorPref, searchContext);
                    s_UnselectedEdgeColorPref.value = SettingsGUILayout.SettingsColorField("Unselected Edge Color",
                        s_UnselectedEdgeColorPref, searchContext);
                    s_SelectedEdgeColorPref.value = SettingsGUILayout.SettingsColorField("Selected Edge Color",
                        s_SelectedEdgeColorPref, searchContext);
                    s_UnselectedVertexColorPref.value = SettingsGUILayout.SettingsColorField("Unselected Vertex Color",
                        s_UnselectedVertexColorPref, searchContext);
                    s_SelectedVertexColorPref.value = SettingsGUILayout.SettingsColorField("Selected Vertex Color",
                        s_SelectedVertexColorPref, searchContext);
                }
            }

            s_VertexPointSize.value = SettingsGUILayout.SettingsSlider("Vertex Size", s_VertexPointSize, 1f, 10f, searchContext);
            s_EdgeLineSize.value = SettingsGUILayout.SettingsSlider("Line Size", s_EdgeLineSize, 0f, 10f, searchContext);
            s_WireframeLineSize.value = SettingsGUILayout.SettingsSlider("Wireframe Size", s_WireframeLineSize, 0f, 10f, searchContext);

            if(EditorGUI.EndChangeCheck())
                ProBuilderEditor.UpdateMeshHandles();
        }

        internal static float dotCapSize
        {
            get { return s_VertexPointSize * .0125f; }
        }

        internal static bool xRay
        {
            get => s_XRayView;
            set => s_XRayView.value = value;
        }

        static void Init()
        {
            if (s_Initialized)
                return;

            s_Initialized = true;

            ReleaseResources();

            s_MeshPool2 = new ObjectPool<Mesh>(0, 8, CreateMesh, DestroyMesh);
            s_WireHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
            s_VertexHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
            s_SelectedFaceHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
            s_SelectedEdgeHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
            s_SelectedVertexHandles = new Dictionary<ProBuilderMesh, MeshHandle>();

            s_TemporaryHandles = new Dictionary<ProBuilderMesh, MeshHandle>();

            var lineShader = BuiltinMaterials.geometryShadersSupported
                ? BuiltinMaterials.lineShader
                : BuiltinMaterials.lineShaderMetal;
            var vertShader = BuiltinMaterials.geometryShadersSupported
                ? BuiltinMaterials.pointShader
                : BuiltinMaterials.dotShader;

            s_EdgeMaterial = CreateMaterial(Shader.Find(lineShader), "ProBuilder::LineMaterial");
            s_WireMaterial = CreateMaterial(Shader.Find(lineShader), "ProBuilder::WireMaterial");
            s_LineMaterial = CreateMaterial(Shader.Find(lineShader), "ProBuilder::GeneralUseLineMaterial");
            s_VertMaterial = CreateMaterial(Shader.Find(vertShader), "ProBuilder::VertexMaterial");
            s_GlWireMaterial = CreateMaterial(Shader.Find(BuiltinMaterials.faceShader), "ProBuilder::GLWire");
            s_FaceMaterial = CreateMaterial(Shader.Find(BuiltinMaterials.faceShader), "ProBuilder::FaceMaterial");

            ResetPreferences();
        }

        internal static void ReleaseResources()
        {
            ClearHandles();
            if(s_MeshPool2 != null)
                s_MeshPool2.Dispose();
            if(s_EdgeMaterial != null) UnityObject.DestroyImmediate(s_EdgeMaterial);
            if(s_WireMaterial != null) UnityObject.DestroyImmediate(s_WireMaterial);
            if(s_LineMaterial != null) UnityObject.DestroyImmediate(s_LineMaterial);
            if(s_VertMaterial != null) UnityObject.DestroyImmediate(s_VertMaterial);
            if(s_GlWireMaterial != null) UnityObject.DestroyImmediate(s_GlWireMaterial);
            if(s_FaceMaterial != null) UnityObject.DestroyImmediate(s_FaceMaterial);
        }

        internal static void ResetPreferences()
        {
            faceMaterial.SetFloat("_Dither", (s_UseUnityColors || s_DitherFaceHandle) ? 1f : 0f);

            m_ForceEdgeLinesGL = s_EdgeLineSize.value < k_MinLineWidthForGeometryShader;
            m_ForceWireframeLinesGL = s_WireframeLineSize.value < k_MinLineWidthForGeometryShader;

            wireMaterial.SetColor("_Color", wireframeColor);
            wireMaterial.SetFloat("_HandleZTest", (int)CompareFunction.LessEqual);

            SetMaterialsScaleAttribute();
        }

        static Material CreateMaterial(Shader shader, string materialName)
        {
            if(shader == null)
                shader = BuiltinMaterials.defaultMaterial.shader;

            Material mat = new Material(shader);
            mat.name = materialName;
            mat.hideFlags = k_ResourceHideFlags;
            return mat;
        }

        static Mesh CreateMesh()
        {
            var mesh = new Mesh();
            mesh.name = "EditorMeshHandles.MeshHandle" + mesh.GetObjectId();
            mesh.hideFlags = HideFlags.HideAndDontSave;
            return mesh;
        }

        static void DestroyMesh(Mesh mesh)
        {
            if(mesh == null)
                throw new ArgumentNullException("mesh");

            UnityObject.DestroyImmediate(mesh);
        }

        public static void DrawSceneSelection(SceneSelection selection)
        {
            var mesh = selection.mesh;

            if(mesh == null)
                return;

            var positions = mesh.positionsInternal;

            // Draw nearest edge
            using (new TriangleDrawingScope(preselectionColor))
            {
                GL.MultMatrix(mesh.transform.localToWorldMatrix);
                foreach(var face in selection.faces)
                {
                    var ind = face.indexes;

                    for(int i = 0, c = ind.Count; i < c; i += 3)
                    {
                        GL.Vertex(positions[ind[i]]);
                        GL.Vertex(positions[ind[i + 1]]);
                        GL.Vertex(positions[ind[i + 2]]);
                    }
                }
            }

            using (var drawingScope = new LineDrawingScope(preselectionColor, mesh.transform.localToWorldMatrix, -1f, CompareFunction.Always))
            {
                foreach(var edge in selection.edges)
                {
                    drawingScope.DrawLine(positions[edge.a], positions[edge.b]);
                }
            }
            using (var drawingScope = new PointDrawingScope(preselectionColor, CompareFunction.Always) { matrix = mesh.transform.localToWorldMatrix })
            {
                foreach(var vertex in selection.vertexes)
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

            switch(mode)
            {
                case SelectMode.Edge:
                case SelectMode.TextureEdge:
                {
                    // When in Edge mode, use the same material for wireframe
                    Render(wireHandles, m_ForceEdgeLinesGL ? glWireMaterial : edgeMaterial, edgeUnselectedColor, CompareFunction.LessEqual, false);
                    if(xRay) Render(s_SelectedEdgeHandles, m_ForceEdgeLinesGL ? s_GlWireMaterial : s_EdgeMaterial, edgeSelectedColor * k_OccludedTint, CompareFunction.Greater);
                    Render(s_SelectedEdgeHandles, m_ForceEdgeLinesGL ? s_GlWireMaterial : s_EdgeMaterial, edgeSelectedColor, CompareFunction.LessEqual);
                    break;
                }
                case SelectMode.Face:
                case SelectMode.TextureFace:
                {
                    Render(wireHandles, m_ForceWireframeLinesGL ? glWireMaterial : wireMaterial, wireframeColor, CompareFunction.LessEqual, false);
                    if(xRay) Render(s_SelectedFaceHandles, s_FaceMaterial, faceSelectedColor * k_OccludedTint, CompareFunction.Greater);
                    Render(s_SelectedFaceHandles, s_FaceMaterial, faceSelectedColor, CompareFunction.LessEqual);
                    break;
                }
                case SelectMode.Vertex:
                case SelectMode.TextureVertex:
                {
                    Render(s_WireHandles, m_ForceWireframeLinesGL ? s_GlWireMaterial : s_WireMaterial, wireframeColor, CompareFunction.LessEqual, false);
                    Render(s_VertexHandles, s_VertMaterial, vertexUnselectedColor, CompareFunction.LessEqual, false);
                    if(xRay) Render(s_SelectedVertexHandles, s_VertMaterial, vertexSelectedColor * k_OccludedTint, CompareFunction.Greater, false);
                    Render(s_SelectedVertexHandles, s_VertMaterial, vertexSelectedColor, CompareFunction.LessEqual, false);
                    break;
                }
                default:
                {
                    Render(wireHandles, m_ForceWireframeLinesGL ? glWireMaterial : wireMaterial, wireframeColor, CompareFunction.LessEqual, false);
                    break;
                }
            }
        }


        static void Render(Dictionary<ProBuilderMesh, MeshHandle> handles, Material material, Color color, CompareFunction func, bool zWrite = false)
        {
            material.SetFloat("_HandleZTest", (int) func);
            material.SetFloat("_HandleZWrite", zWrite ? 1 : 0);
            material.SetColor("_Color", color);

            if(material.SetPass(0))
            {
                foreach(var kvp in handles)
                    kvp.Value.DrawMeshNow(0);
            }
        }

        public static void ClearHandles()
        {
            if(s_WireHandles != null)
                ClearHandlesInternal(wireHandles);
            if(s_VertexHandles != null)
                ClearHandlesInternal(s_VertexHandles);
            if(selectedFaceHandles != null)
                ClearHandlesInternal(s_SelectedFaceHandles);
            if(s_SelectedEdgeHandles != null)
                ClearHandlesInternal(s_SelectedEdgeHandles);
            if(s_SelectedVertexHandles != null)
                ClearHandlesInternal(s_SelectedVertexHandles);
            if(s_TemporaryHandles != null)
                ClearHandlesInternal(s_TemporaryHandles);
        }

        public static void RebuildSelectedHandles(IEnumerable<ProBuilderMesh> meshes, SelectMode selectionMode)
        {
            ClearHandles();

            foreach(var mesh in meshes)
            {
                switch(selectionMode)
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

            if(!cache.TryGetValue(mesh, out handle))
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

        static void RebuildMeshHandleFromFaces(ProBuilderMesh mesh, IList<Face> faces,
            Dictionary<ProBuilderMesh, MeshHandle> list, Action<ProBuilderMesh, IList<Face>, Mesh> ctor)
        {
            var handle = GetMeshHandle(mesh, list);
            ctor(mesh, faces, handle.mesh);
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

        public static void HighlightFaces(ProBuilderMesh mesh, IList<Face> faces, Color highlightColor)
        {
            RebuildMeshHandleFromFaces(mesh, faces, s_TemporaryHandles, MeshHandles.CreateFaceMeshFromFaces);
            Render(s_TemporaryHandles, s_FaceMaterial, highlightColor,CompareFunction.LessEqual, false);
        }

        public static void HighlightEdges(ProBuilderMesh mesh, IList<Edge> edges, bool highlight = true)
        {
            HighlightEdges(mesh,edges, highlight ? edgeSelectedColor : edgeUnselectedColor);
        }

        public static void HighlightEdges(ProBuilderMesh mesh, IList<Edge> edges, Color highlightColor)
        {
            var handle = GetMeshHandle(mesh, s_TemporaryHandles);

            if(m_ForceEdgeLinesGL || BuiltinMaterials.geometryShadersSupported)
                MeshHandles.CreateEdgeMesh(mesh, handle.mesh, edges.ToArray());
            else
                MeshHandles.CreateEdgeBillboardMesh(mesh, handle.mesh, edges.ToArray());

            Render(s_TemporaryHandles, s_EdgeMaterial, highlightColor, CompareFunction.LessEqual, false);
        }

        public static void HighlightVertices(ProBuilderMesh mesh, IList<int> vertexIndexes, bool highlight = true)
        {
            HighlightVertices(mesh,vertexIndexes, highlight ? vertexSelectedColor : vertexUnselectedColor);
        }

        public static void HighlightVertices(ProBuilderMesh mesh, IList<int> vertexIndexes, Color highlightColor)
        {
            var handle = GetMeshHandle(mesh, s_TemporaryHandles);
            MeshHandles.CreateVertexMesh(mesh, handle.mesh, vertexIndexes);

            Render(s_TemporaryHandles, s_VertMaterial, highlightColor, CompareFunction.LessEqual, false);
        }
    }
}
