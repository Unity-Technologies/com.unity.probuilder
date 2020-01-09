using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.UI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Smoothing groups editor window.
    /// </summary>
    sealed class SmoothGroupEditor : ConfigurableWindow
    {
        class SmoothGroupData : IDisposable
        {
            bool m_Disposed;
            public bool isVisible;
            public Dictionary<int, List<Face>> groups;
            public Dictionary<int, Color> groupColors;
            public HashSet<int> selected;
            public Mesh previewMesh;
            public Mesh normalsMesh;

            public SmoothGroupData(ProBuilderMesh pb)
            {
                groups = new Dictionary<int, List<Face>>();
                selected = new HashSet<int>();
                groupColors = new Dictionary<int, Color>();
                isVisible = true;

                previewMesh = new Mesh()
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = pb.name + "_SmoothingPreview"
                };

                normalsMesh = new Mesh()
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = pb.name + "_SmoothingNormals"
                };

                Rebuild(pb);
            }

            public void Rebuild(ProBuilderMesh pb)
            {
                CacheGroups(pb);
                CacheSelected(pb);
                RebuildPreviewMesh(pb);
                RebuildNormalsMesh(pb);
            }

            public void CacheGroups(ProBuilderMesh pb)
            {
                groups.Clear();

                foreach (Face face in pb.facesInternal)
                {
                    List<Face> affected;

                    if (!groups.TryGetValue(face.smoothingGroup, out affected))
                        groups.Add(face.smoothingGroup, new List<Face>() {face});
                    else
                        affected.Add(face);
                }
            }

            public void CacheSelected(ProBuilderMesh pb)
            {
                selected.Clear();

                foreach (Face face in pb.selectedFacesInternal)
                    selected.Add(face.smoothingGroup);
            }

            void RebuildPreviewMesh(ProBuilderMesh pb)
            {
                List<int> indexes = new List<int>();
                Color32[] colors = new Color32[pb.vertexCount];
                groupColors.Clear();

                foreach (KeyValuePair<int, List<Face>> smoothGroup in groups)
                {
                    if (smoothGroup.Key > Smoothing.smoothingGroupNone)
                    {
                        Color32 color = GetDistinctColor(smoothGroup.Key);
                        groupColors.Add(smoothGroup.Key, color);
                        var groupIndexes = smoothGroup.Value.SelectMany(y => y.indexesInternal);
                        indexes.AddRange(groupIndexes);
                        foreach (int i in groupIndexes)
                            colors[i] = color;
                    }
                }

                previewMesh.Clear();
                previewMesh.vertices = pb.positionsInternal;
                previewMesh.colors32 = colors;
                previewMesh.triangles = indexes.ToArray();
            }

            public void RebuildNormalsMesh(ProBuilderMesh pb)
            {
                normalsMesh.Clear();
                Vector3[] srcPositions = pb.mesh.vertices;
                Vector3[] srcNormals = pb.mesh.normals;
                int vertexCount = System.Math.Min(ushort.MaxValue / 2, pb.mesh.vertexCount);
                Vector3[] positions = new Vector3[vertexCount * 2];
                Vector4[] tangents = new Vector4[vertexCount * 2];
                int[] indexes = new int[vertexCount * 2];
                for (int i = 0; i < vertexCount; i++)
                {
                    int a = i * 2, b = i * 2 + 1;

                    positions[a] = srcPositions[i];
                    positions[b] = srcPositions[i];
                    tangents[a] = new Vector4(srcNormals[i].x, srcNormals[i].y, srcNormals[i].z, 0f);
                    tangents[b] = new Vector4(srcNormals[i].x, srcNormals[i].y, srcNormals[i].z, 1f);
                    indexes[a] = a;
                    indexes[b] = b;
                }
                normalsMesh.vertices = positions;
                normalsMesh.tangents = tangents;
                normalsMesh.subMeshCount = 1;
                normalsMesh.SetIndices(indexes, MeshTopology.Lines, 0);
            }

            void Dispose(bool disposing)
            {
                if (!disposing && !m_Disposed)
                {
                    if (previewMesh)
                        DestroyImmediate(previewMesh);

                    if (normalsMesh)
                        DestroyImmediate(normalsMesh);

                    m_Disposed = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }
        }

        static Material s_FaceMaterial = null;
        static Material smoothPreviewMaterial
        {
            get
            {
                if (s_FaceMaterial == null)
                {
                    s_FaceMaterial = new Material(Shader.Find("Hidden/ProBuilder/SmoothingPreview"));
                    s_FaceMaterial.hideFlags = HideFlags.HideAndDontSave;
                }

                return s_FaceMaterial;
            }
        }

        static Material s_NormalPreviewMaterial = null;
        static Material normalPreviewMaterial
        {
            get
            {
                if (s_NormalPreviewMaterial == null)
                    s_NormalPreviewMaterial = new Material(Shader.Find("Hidden/ProBuilder/NormalPreview"));
                return s_NormalPreviewMaterial;
            }
        }

        static GUIStyle s_GroupButtonStyle = null;
        static GUIStyle s_GroupButtonSelectedStyle = null;
        static GUIStyle s_GroupButtonInUseStyle = null;
        static GUIStyle s_GroupButtonMixedSelectionStyle = null;
        static GUIStyle s_ColorKeyStyle = null;
        static GUIStyle s_WordWrappedRichText = null;

        static GUIStyle groupButtonStyle
        {
            get
            {
                if (s_GroupButtonStyle == null)
                {
                    s_GroupButtonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
                    s_GroupButtonStyle.normal.background = IconUtility.GetIcon("Toolbar/Background/RoundedRect_Normal");
                    s_GroupButtonStyle.hover.background = IconUtility.GetIcon("Toolbar/Background/RoundedRect_Hover");
                    s_GroupButtonStyle.active.background = IconUtility.GetIcon("Toolbar/Background/RoundedRect_Pressed");
                    Font asap = FileUtility.LoadInternalAsset<Font>("About/Font/Asap-Regular.otf");
                    if (asap != null)
                    {
                        s_GroupButtonStyle.font = asap;
                        s_GroupButtonStyle.fontSize = 12;
                        s_GroupButtonStyle.padding = new RectOffset(2, 2, 2, 2);
                    }
                    s_GroupButtonStyle.border = new RectOffset(3, 3, 3, 3);
                    s_GroupButtonStyle.margin = new RectOffset(4, 4, 4, 6);
                    s_GroupButtonStyle.alignment = TextAnchor.MiddleCenter;
                    s_GroupButtonStyle.fixedWidth = IconWidth;
                    s_GroupButtonStyle.fixedHeight = IconHeight;

                    // todo Move text & background colors to a global settings file
                    s_GroupButtonStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(.9f, .9f, .9f) : new Color(.3f, .3f, .3f);
                    s_GroupButtonStyle.hover.textColor = EditorGUIUtility.isProSkin ? new Color(.9f, .9f, .9f) : new Color(.3f, .3f, .3f);
                    s_GroupButtonStyle.active.textColor = EditorGUIUtility.isProSkin ? new Color(.9f, .9f, .9f) : new Color(.3f, .3f, .3f);
                }
                return s_GroupButtonStyle;
            }
        }

        static GUIStyle groupButtonSelectedStyle
        {
            get
            {
                if (s_GroupButtonSelectedStyle == null)
                {
                    s_GroupButtonSelectedStyle = new GUIStyle(groupButtonStyle);
                    s_GroupButtonSelectedStyle.normal.background =
                        IconUtility.GetIcon("Toolbar/Background/RoundedRect_Normal_Blue");
                    s_GroupButtonSelectedStyle.hover.background =
                        IconUtility.GetIcon("Toolbar/Background/RoundedRect_Hover_Blue");
                    s_GroupButtonSelectedStyle.active.background =
                        IconUtility.GetIcon("Toolbar/Background/RoundedRect_Pressed_Blue");
                }

                return s_GroupButtonSelectedStyle;
            }
        }

        static GUIStyle groupButtonInUseStyle
        {
            get
            {
                if (s_GroupButtonInUseStyle == null)
                {
                    s_GroupButtonInUseStyle = new GUIStyle(groupButtonStyle);
                    s_GroupButtonInUseStyle.normal.background =
                        IconUtility.GetIcon("Toolbar/Background/RoundedRect_Normal_BlueSteel");
                    s_GroupButtonInUseStyle.hover.background =
                        IconUtility.GetIcon("Toolbar/Background/RoundedRect_Hover_BlueSteel");
                    s_GroupButtonInUseStyle.active.background =
                        IconUtility.GetIcon("Toolbar/Background/RoundedRect_Pressed_BlueSteel");
                }

                return s_GroupButtonInUseStyle;
            }
        }

        static GUIStyle groupButtonMixedSelectionStyle
        {
            get
            {
                if (s_GroupButtonMixedSelectionStyle == null)
                {
                    s_GroupButtonMixedSelectionStyle = new GUIStyle(groupButtonStyle);
                    s_GroupButtonMixedSelectionStyle.normal.background =
                        IconUtility.GetIcon("Toolbar/Background/RoundedRect_Normal_Orange");
                    s_GroupButtonMixedSelectionStyle.hover.background =
                        IconUtility.GetIcon("Toolbar/Background/RoundedRect_Hover_Orange");
                    s_GroupButtonMixedSelectionStyle.active.background =
                        IconUtility.GetIcon("Toolbar/Background/RoundedRect_Pressed_Orange");
                }

                return s_GroupButtonMixedSelectionStyle;
            }
        }

        static GUIStyle colorKeyStyle
        {
            get
            {
                if (s_ColorKeyStyle == null)
                {
                    s_ColorKeyStyle = new GUIStyle(groupButtonStyle);
                    s_ColorKeyStyle.fixedWidth = IconWidth;
                    s_ColorKeyStyle.fixedHeight = 3;
                    s_ColorKeyStyle.padding = new RectOffset(4, 4, 0, 4);
                    s_ColorKeyStyle.normal.background = EditorGUIUtility.whiteTexture;
                }
                return s_ColorKeyStyle;
            }
        }

        static GUIStyle wordWrappedRichText
        {
            get
            {
                if (s_WordWrappedRichText == null)
                {
                    s_WordWrappedRichText = new GUIStyle(EditorStyles.wordWrappedLabel);
                    s_WordWrappedRichText.richText = true;
                    s_WordWrappedRichText.alignment = TextAnchor.LowerLeft;
                }
                return s_WordWrappedRichText;
            }
        }

        const int IconWidth = 24;
        const int IconHeight = 24;

        GUIContent m_GroupKeyContent =
            new GUIContent("21", "Smoothing Group.\n\nRight Click to Select all faces matching this group.");
        Vector2 m_Scroll = Vector2.zero;
        GUIContent m_HelpIcon = null;
        GUIContent m_BreakSmoothingContent = null;
        GUIContent m_SelectFacesWithSmoothGroupSelectionContent = null;
        Dictionary<ProBuilderMesh, SmoothGroupData> m_SmoothGroups = new Dictionary<ProBuilderMesh, SmoothGroupData>();

        static bool s_IsMovingVertices = false;

        static Pref<bool> s_ShowPreview = new Pref<bool>("smoothing.showPreview", false);
        static Pref<bool> s_ShowNormals = new Pref<bool>("smoothing.showNormals", false);
        static Pref<bool> s_ShowHelp = new Pref<bool>("smoothing.showHelp", false);
        static Pref<float> s_NormalsSize = new Pref<float>("smoothing.NormalsSize", 0.1f);
        static Pref<float> s_PreviewOpacity = new Pref<float>("smoothing.PreviewOpacity", .5f);
        static Pref<bool> s_PreviewDither = new Pref<bool>("smoothing.previewDither", false);
        static Pref<bool> s_ShowSettings = new Pref<bool>("smoothing.showSettings", false);

        public static void MenuOpenSmoothGroupEditor()
        {
            GetWindow<SmoothGroupEditor>("Smooth Group Editor");
        }

        void OnEnable()
        {
            ProBuilderEditor.selectMode = SelectMode.Face;

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
            Selection.selectionChanged += OnSelectionChanged;
            Undo.undoRedoPerformed += OnSelectionChanged;
            ProBuilderMesh.elementSelectionChanged += OnElementSelectionChanged;
            VertexManipulationTool.beforeMeshModification += OnBeginVertexMovement;
            VertexManipulationTool.afterMeshModification += OnFinishVertexMovement;
            autoRepaintOnSceneChange = true;
            m_HelpIcon = new GUIContent(IconUtility.GetIcon("Toolbar/Help"), "Open Documentation");
            m_BreakSmoothingContent = new GUIContent(IconUtility.GetIcon("Toolbar/Face_BreakSmoothing"),
                    "Clear the selected faces of their smoothing groups");
            m_SelectFacesWithSmoothGroupSelectionContent = new GUIContent(IconUtility.GetIcon("Toolbar/Selection_SelectBySmoothingGroup"),
                    "Expand the face selection by selecting all faces matching the currently selected face groups");
            MeshSelection.OnObjectSelectionChanged();
            OnSelectionChanged();
        }

        void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
            Selection.selectionChanged -= OnSelectionChanged;
            Undo.undoRedoPerformed -= OnSelectionChanged;
            ProBuilderMesh.elementSelectionChanged -= OnElementSelectionChanged;
            ClearSmoothGroupData();
        }

        void OnDestroy()
        {
            ClearSmoothGroupData();
        }

        void ClearSmoothGroupData()
        {
            foreach (var v in m_SmoothGroups)
                v.Value.Dispose();
            m_SmoothGroups.Clear();
        }

        void OnBeginVertexMovement(IEnumerable<ProBuilderMesh> selection)
        {
            s_IsMovingVertices = true;
        }

        void OnFinishVertexMovement(IEnumerable<ProBuilderMesh> selection)
        {
            s_IsMovingVertices = false;
            OnSelectionChanged();
        }

        void OnSelectionChanged()
        {
            ClearSmoothGroupData();

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
                m_SmoothGroups.Add(pb, new SmoothGroupData(pb));

            this.Repaint();
        }

        void OnElementSelectionChanged(ProBuilderMesh pb)
        {
            SmoothGroupData data;

            if (!m_SmoothGroups.TryGetValue(pb, out data))
                m_SmoothGroups.Add(pb, data = new SmoothGroupData(pb));
            else
                data.CacheSelected(pb);
        }

        void OnGUI()
        {
            DoContextMenu();

            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginChangeCheck();
            s_ShowSettings.value = GUILayout.Toggle(s_ShowSettings.value, "Settings", EditorStyles.toolbarButton);
            s_ShowPreview.value = GUILayout.Toggle(s_ShowPreview.value, "Preview", EditorStyles.toolbarButton);
            s_ShowNormals.value = GUILayout.Toggle(s_ShowNormals.value, "Normals", EditorStyles.toolbarButton);
            if(EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            if (s_ShowNormals)
            {
                EditorGUI.BeginChangeCheck();

                s_NormalsSize.value = GUILayout.HorizontalSlider(
                        s_NormalsSize,
                        .001f,
                        1f,
                        GUILayout.MinWidth(30f),
                        GUILayout.MaxWidth(100f));

                if (EditorGUI.EndChangeCheck())
                {
                    ProBuilderSettings.Save();

                    foreach (var kvp in m_SmoothGroups)
                        kvp.Value.RebuildNormalsMesh(kvp.Key);
                    SceneView.RepaintAll();
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(m_HelpIcon, UI.EditorStyles.toolbarHelpIcon))
                s_ShowHelp.SetValue(!s_ShowHelp, true);
            GUILayout.EndHorizontal();

            if (s_ShowSettings)
            {
                GUILayout.BeginVertical(UI.EditorStyles.settingsGroup);

                EditorGUIUtility.labelWidth = 100;

                EditorGUI.BeginChangeCheck();

                s_PreviewOpacity.value = EditorGUILayout.Slider("Preview Opacity", s_PreviewOpacity, .001f, 1f);
                s_PreviewDither.value = EditorGUILayout.Toggle("Preview Dither", s_PreviewDither);

                if (EditorGUI.EndChangeCheck())
                {
                    ProBuilderSettings.Save();
                    smoothPreviewMaterial.SetFloat("_Opacity", s_PreviewOpacity);
                    smoothPreviewMaterial.SetFloat("_Dither", s_PreviewDither ? 1f : 0f);
                    SceneView.RepaintAll();
                }

                EditorGUIUtility.labelWidth = 0;

                GUILayout.EndVertical();
            }

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

            if (s_ShowHelp)
            {
                GUILayout.BeginVertical(UI.EditorStyles.settingsGroup);

                GUILayout.Label("Create and Clear Smoothing Groups", EditorStyles.boldLabel);

                GUILayout.Label("Adjacent faces with the same smoothing group will appear to have a soft adjoining edge.", wordWrappedRichText);
                GUILayout.Space(2);
                GUILayout.Label("<b>To smooth</b> a selected group of faces, click one of the Smooth Group buttons.", wordWrappedRichText);
                GUILayout.Label("<b>To clear</b> selected faces of their smooth group, click the [Break] icon.", wordWrappedRichText);
                GUILayout.Label("<b>To select</b> all faces in a group, Right+Click or Alt+Click a smooth group button.", wordWrappedRichText);
                GUILayout.Space(2);

                global::UnityEditor.ProBuilder.UI.EditorGUILayout.BeginRow();
                GUILayout.Button("1", groupButtonStyle);
                GUILayout.Label("An unused smooth group", wordWrappedRichText);
                global::UnityEditor.ProBuilder.UI.EditorGUILayout.EndRow();

                global::UnityEditor.ProBuilder.UI.EditorGUILayout.BeginRow();
                GUILayout.Button("1", groupButtonInUseStyle);
                GUILayout.Label("A smooth group that is in use, but not in the current selection", wordWrappedRichText);
                global::UnityEditor.ProBuilder.UI.EditorGUILayout.EndRow();

                global::UnityEditor.ProBuilder.UI.EditorGUILayout.BeginRow();
                GUILayout.Button("1", groupButtonSelectedStyle);
                GUILayout.Label("A smooth group that is currently selected", wordWrappedRichText);
                global::UnityEditor.ProBuilder.UI.EditorGUILayout.EndRow();

                global::UnityEditor.ProBuilder.UI.EditorGUILayout.BeginRow();
                GUILayout.Button("1", groupButtonMixedSelectionStyle);
                GUI.backgroundColor = Color.white;
                GUILayout.Label("A smooth group is selected, but the selection also contains non-grouped faces", wordWrappedRichText);
                global::UnityEditor.ProBuilder.UI.EditorGUILayout.EndRow();

                if (GUILayout.Button("Open Documentation"))
                    Application.OpenURL("https://docs.unity3d.com/Packages/com.unity.probuilder@latest/index.html?subfolder=/manual/workflow-edit-smoothing.html");

                GUILayout.EndVertical();
            }

            // border style is 4 margin, 4 pad, 1px content. inner is accounted for by btn size + btn margin.
            float area = (position.width - 10);
            float margin = Mathf.Max(groupButtonStyle.margin.left, groupButtonStyle.margin.right);
            int columns = (int)(area / (groupButtonStyle.CalcSize(m_GroupKeyContent).x + margin)) - 1;

            if (m_SmoothGroups.Count < 1)
            {
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Select a ProBuilder Mesh", UI.EditorGUIUtility.CenteredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
            }
            else
            {
                foreach (var mesh in m_SmoothGroups)
                {
                    ProBuilderMesh pb = mesh.Key;
                    SmoothGroupData data = mesh.Value;

                    GUILayout.BeginVertical(UI.EditorStyles.settingsGroup);

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button(pb.name, UI.EditorStyles.headerLabel))
                        data.isVisible = !data.isVisible;

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(m_SelectFacesWithSmoothGroupSelectionContent,
                            UI.EditorStyles.buttonStyle))
                        SelectGroups(pb, new HashSet<int>(pb.selectedFacesInternal.Select(x => x.smoothingGroup)));

                    if (GUILayout.Button(m_BreakSmoothingContent,
                            UI.EditorStyles.buttonStyle))
                        SetGroup(pb, Smoothing.smoothingGroupNone);

                    GUILayout.EndHorizontal();

                    bool isMixedSelection = data.selected.Contains(Smoothing.smoothingGroupNone);

                    if (data.isVisible)
                    {
                        int column = 0;
                        bool anySmoothGroups = data.groups.Any(x => x.Key > Smoothing.smoothingGroupNone);

                        GUILayout.BeginHorizontal();

                        for (int i = 1; i < Smoothing.smoothRangeMax; i++)
                        {
                            bool isSelected = data.selected.Contains(i);

                            GUIStyle stateStyle = isSelected ?
                                (isMixedSelection ? groupButtonMixedSelectionStyle : groupButtonSelectedStyle) :
                                data.groups.ContainsKey(i) ? groupButtonInUseStyle : groupButtonStyle;

                            if (s_ShowPreview && anySmoothGroups)
                                GUILayout.BeginVertical(GUILayout.MaxWidth(IconWidth));

                            m_GroupKeyContent.text = i.ToString();

                            if (GUILayout.Button(m_GroupKeyContent, stateStyle))
                            {
                                // if right click or alt click select the faces instead of setting a group
                                if ((Event.current.modifiers & EventModifiers.Alt) == EventModifiers.Alt ||
                                    Event.current.button != 0)
                                    SelectGroups(pb, new HashSet<int>() { i });
                                else
                                    SetGroup(pb, i);
                            }

                            if (s_ShowPreview && anySmoothGroups)
                            {
                                GUI.backgroundColor = data.groupColors.ContainsKey(i) ? data.groupColors[i] : Color.clear;
                                GUILayout.Label("", colorKeyStyle);
                                GUILayout.EndVertical();
                                GUI.backgroundColor = Color.white;
                            }

                            if (++column > columns)
                            {
                                column = 0;
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                            }
                        }

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndScrollView();

            // This isn't great, but we need hover previews to work
            if (mouseOverWindow == this)
                Repaint();
        }

        void OnSceneGUI(SceneView view)
        {
            if (m_SmoothGroups.Count > 1)
            {
                using (new HandleGUI())
                {
                    foreach (var kvp in m_SmoothGroups)
                        Handles.Label(kvp.Key.transform.position, kvp.Key.name, EditorStyles.boldLabel);
                }
            }

            Event evt = Event.current;

            if (!s_IsMovingVertices && evt.type == EventType.Repaint)
            {
                foreach (var kvp in m_SmoothGroups)
                {
                    if (s_ShowPreview)
                    {
                        Mesh m = kvp.Value.previewMesh;

                        if (m != null)
                        {
                            smoothPreviewMaterial.SetPass(0);
                            Graphics.DrawMeshNow(m, kvp.Key.transform.localToWorldMatrix);
                        }
                    }

                    if (s_ShowNormals)
                    {
                        Mesh m = kvp.Value.normalsMesh;

                        if (m != null)
                        {
                            Transform trs = kvp.Key.transform;
                            normalPreviewMaterial.SetFloat("_Scale", s_NormalsSize * HandleUtility.GetHandleSize(trs.GetComponent<MeshRenderer>().bounds.center));
                            normalPreviewMaterial.SetPass(0);
                            Graphics.DrawMeshNow(m, trs.localToWorldMatrix);
                        }
                    }
                }
            }
        }

        static void SelectGroups(ProBuilderMesh pb, HashSet<int> groups)
        {
            UndoUtility.RecordSelection(pb, "Select with Smoothing Group");

            if ((Event.current.modifiers & EventModifiers.Shift) == EventModifiers.Shift ||
                (Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control)
                pb.SetSelectedFaces(pb.facesInternal.Where(x => groups.Contains(x.smoothingGroup) || pb.selectedFacesInternal.Contains(x)));
            else
                pb.SetSelectedFaces(pb.facesInternal.Where(x => groups.Contains(x.smoothingGroup)));
            ProBuilderEditor.Refresh();
        }

        void SetGroup(ProBuilderMesh pb, int index)
        {
            UndoUtility.RecordObject(pb, "Set Smoothing Group");

            foreach (Face face in pb.selectedFaceCount < 1 ? pb.facesInternal : pb.selectedFacesInternal)
                face.smoothingGroup = index;

            // todo pb.Rebuild
            pb.ToMesh();
            pb.Refresh();
            pb.Optimize();

            SmoothGroupData data;

            if (!m_SmoothGroups.TryGetValue(pb, out data))
                m_SmoothGroups.Add(pb, new SmoothGroupData(pb));
            else
                data.Rebuild(pb);

            ProBuilderEditor.Refresh();
        }

        static Color32 GetDistinctColor(int index)
        {
            return s_KellysMaxContrastSet[index % s_KellysMaxContrastSet.Length];
        }

        static readonly Color32[] s_KellysMaxContrastSet = new Color32[]
        {
            new Color32(230, 25, 75, 255),      // Red
            new Color32(60, 180, 75, 255),      // Green
            new Color32(255, 225, 25, 255),     // Yellow
            new Color32(0, 130, 200, 255),      // Blue
            new Color32(245, 130, 48, 255),     // Orange
            new Color32(145, 30, 180, 255),     // Purple
            new Color32(70, 240, 240, 255),     // Cyan
            new Color32(240, 50, 230, 255),     // Magenta
            new Color32(210, 245, 60, 255),     // Lime
            new Color32(250, 190, 190, 255),    // Pink
            new Color32(0, 128, 128, 255),      // Teal
            new Color32(230, 190, 255, 255),    // Lavender
            new Color32(170, 110, 40, 255),     // Brown
            new Color32(255, 250, 200, 255),    // Beige
            new Color32(128, 0, 0, 255),        // Maroon
            new Color32(170, 255, 195, 255),    // Mint
            new Color32(128, 128, 0, 255),      // Olive
            new Color32(255, 215, 180, 255),    // Coral
            new Color32(0, 0, 128, 255),        // Navy
            new Color32(128, 128, 128, 255),    // Grey
            new Color32(255, 255, 255, 255),    // White
            new Color32(0, 0, 0, 255),          // Black
        };
    }
}
