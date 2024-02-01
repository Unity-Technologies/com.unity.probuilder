using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// A simple line-item editor for vertex positions.
    /// </summary>
    sealed class VertexPositionEditor : ConfigurableWindow
    {
        const int k_MaxSelectableVertices = 100;
        Dictionary<ProBuilderMesh, VertexEditorSelection> m_Selection = new Dictionary<ProBuilderMesh, VertexEditorSelection>();
        static Color s_EvenColor;
        static Color s_OddColor;

        Vector2 m_Scroll = Vector2.zero;
        bool m_IsActive;

        public bool m_WorldSpace = true;
        static readonly string[] k_Spaces    = new string[2] { "World Space", "Model Space" };

        class VertexEditorSelection
        {
            public bool isVisible;
            public IEnumerable<int> common;

            public VertexEditorSelection(ProBuilderMesh mesh, bool visible, IEnumerable<int> indexes)
            {
                isVisible = visible;
                common = mesh.GetSharedVertexHandles(indexes);
            }
        }

        public static void MenuOpenVertexEditor()
        {
            EditorWindow.GetWindow<VertexPositionEditor>(true, "Positions Editor", true);
        }

        void OnEnable()
        {
            s_EvenColor = EditorGUIUtility.isProSkin ? new Color(.18f, .18f, .18f, 1f) : new Color(.85f, .85f, .85f, 1f);
            s_OddColor = EditorGUIUtility.isProSkin ? new Color(.15f, .15f, .15f, 1f) : new Color(.80f, .80f, .80f, 1f);

            ProBuilderEditor.selectionUpdated += OnSelectionUpdate;
            SceneView.duringSceneGui += OnSceneGUI;

            if (ProBuilderEditor.instance != null)
                OnSelectionUpdate(ProBuilderEditor.instance.selection);
        }

        void OnDisable()
        {
            ProBuilderEditor.selectionUpdated -= OnSelectionUpdate;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSelectionUpdate(IEnumerable<ProBuilderMesh> newSelection)
        {
            if (newSelection == null)
            {
                if (m_Selection != null)
                    m_Selection.Clear();

                return;
            }

            var res = new Dictionary<ProBuilderMesh, VertexEditorSelection>();

            foreach (var mesh in newSelection)
            {
                VertexEditorSelection sel;

                if (m_Selection.TryGetValue(mesh, out sel))
                {
                    sel.common = mesh.GetSharedVertexHandles(mesh.selectedIndexesInternal);
                    res.Add(mesh, sel);
                }
                else
                {
                    res.Add(mesh, new VertexEditorSelection(mesh, true, mesh.selectedIndexesInternal));
                }
            }

            m_Selection = res;

            this.Repaint();
        }

        void OnVertexMovementBegin(ProBuilderMesh pb)
        {
            m_IsActive = true;
            pb.ToMesh();
            pb.Refresh();
        }

        void OnVertexMovementFinish()
        {
            m_IsActive = false;

            foreach (var kvp in m_Selection)
            {
                kvp.Key.ToMesh();
                kvp.Key.Refresh();
                kvp.Key.Optimize();
            }
        }

        void OnGUI()
        {
            DoContextMenu();

            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.FlexibleSpace();

            //GUIStyle style = m_WorldSpace ? EditorStyles.toolbarButton : UI.EditorGUIUtility.GetOnStyle(EditorStyles.toolbarButton);
            //if (GUILayout.Button(m_WorldSpace ? "Switch to Model Space" : "Switch to World Space", style))
            //    m_WorldSpace = !m_WorldSpace;

            EditorGUI.BeginChangeCheck();
            var newSpace = EditorGUILayout.Popup(m_WorldSpace ? 0 : 1, k_Spaces, EditorStyles.toolbarButton, GUILayout.MaxWidth(150));
            if (EditorGUI.EndChangeCheck())
                m_WorldSpace = newSpace == 0;

            GUILayout.EndHorizontal();

            if (m_Selection == null || m_Selection.Count < 1 || !m_Selection.Any(x => x.Key.selectedVertexCount > 0))
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Select a ProBuilder Mesh", UI.EditorGUIUtility.CenteredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                return;
            }

            Event e = Event.current;

            if (m_IsActive)
            {
                if (e.type == EventType.Ignore ||
                    e.type == EventType.MouseUp)
                    OnVertexMovementFinish();
            }

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

            foreach (var kvp in m_Selection)
            {
                ProBuilderMesh mesh = kvp.Key;
                VertexEditorSelection sel = kvp.Value;

                bool open = sel.isVisible;

                EditorGUI.BeginChangeCheck();
                open = EditorGUILayout.Foldout(open, mesh.name);
                if (EditorGUI.EndChangeCheck())
                    sel.isVisible = open;

                if (open)
                {
                    int index = 0;

                    bool wasWideMode = EditorGUIUtility.wideMode;
                    EditorGUIUtility.wideMode = true;
                    Color background = GUI.backgroundColor;
                    Transform transform = mesh.transform;

                    foreach (int u in sel.common)
                    {
                        GUI.backgroundColor = index % 2 == 0 ? s_EvenColor : s_OddColor;
                        GUILayout.BeginHorizontal(UI.EditorGUIUtility.solidBackgroundStyle);
                        GUI.backgroundColor = background;

                        GUILayout.Label(u.ToString(), GUILayout.MinWidth(32), GUILayout.MaxWidth(32));

                        Vector3 v = mesh.positionsInternal[mesh.sharedVerticesInternal[u][0]];

                        if (m_WorldSpace) v = transform.TransformPoint(v);

                        EditorGUI.BeginChangeCheck();

                        v = EditorGUILayout.Vector3Field("", v);

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (!m_IsActive)
                                OnVertexMovementBegin(mesh);

                            UndoUtility.RecordObject(mesh, "Set Vertex Postion");

                            mesh.SetSharedVertexPosition(u, m_WorldSpace ? transform.InverseTransformPoint(v) : v);

                            if (ProBuilderEditor.instance != null)
                            {
                                mesh.RefreshUV(MeshSelection.selectedFacesInEditZone[mesh]);
                                mesh.Refresh(RefreshMask.Normals);
                                mesh.mesh.RecalculateBounds();
                                ProBuilderEditor.Refresh();
                            }
                        }
                        index++;
                        GUILayout.EndHorizontal();
                    }

                    GUI.backgroundColor = background;
                    EditorGUIUtility.wideMode = wasWideMode;
                }
            }

            EditorGUILayout.EndScrollView();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (m_Selection == null)
                return;

            int labelCount = 0;

            Handles.BeginGUI();

            // Only show dropped down probuilder objects.
            foreach (KeyValuePair<ProBuilderMesh, VertexEditorSelection> selected in m_Selection)
            {
                ProBuilderMesh mesh = selected.Key;
                VertexEditorSelection sel = selected.Value;

                if (!sel.isVisible)
                    continue;

                Vector3[] positions = mesh.positionsInternal;

                foreach (int i in sel.common)
                {
                    var indexes = mesh.sharedVerticesInternal[i];

                    Vector3 point = mesh.transform.TransformPoint(positions[indexes[0]]);

                    Vector2 cen = HandleUtility.WorldToGUIPoint(point);

                    UI.EditorGUIUtility.SceneLabel(i.ToString(), cen, false);

                    if (++labelCount > k_MaxSelectableVertices) break;
                }
            }
            Handles.EndGUI();
        }
    }
}
