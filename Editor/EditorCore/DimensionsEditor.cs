using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SettingsManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using Object = UnityEngine.Object;

namespace UnityEditor.ProBuilder
{
    [FilePath("Library/Probuilder/DimensionsOverlay", FilePathAttribute.Location.ProjectFolder)]
    sealed class DimensionsEditor : ScriptableSingleton<DimensionsEditor>
    {
        struct Trs : IEquatable<Trs>
        {
            public Vector3 position { get; set; }
            public Quaternion rotation { get; set; }
            public Vector3 scale { get; set; }

            public Trs(Transform t)
            {
                position = t.position;
                rotation = t.rotation;
                scale = t.localScale;
            }

            public bool Equals(Trs other)
            {
                return position.Equals(other.position) && rotation.Equals(other.rotation) && scale.Equals(other.scale);
            }

            public override bool Equals(object obj)
            {
                return obj is Trs other && Equals(other);
            }

            public static explicit operator Trs(Transform trs)
            {
                return new Trs(trs);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = position.GetHashCode();
                    hashCode = (hashCode * 397) ^ rotation.GetHashCode();
                    hashCode = (hashCode * 397) ^ scale.GetHashCode();
                    return hashCode;
                }
            }
        }
        
        bool m_HasBounds;
        Bounds m_Bounds;
        Dictionary<Transform, Trs> m_Selected = new Dictionary<Transform, Trs>();
        // When ProBuilder is modifying a mesh, it doesn't recalculate the mesh bounds. This lets our bounds encapsulation
        // function know that Renderer.bounds is not to be trusted.
        static bool s_RecalculateMeshBounds;

        [SerializeField]
        bool m_Visible;

        public bool visible
        {
            get => m_Visible;
            set
            {
                if (m_Visible == value)
                    return;

                m_Visible = value;
                if (m_Visible)
                    OnShowed();
                else
                    OnHid();

                Save(true);
            }
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Dimensions Overlay/Hide", true, PreferenceKeys.menuEditor + 30)]
        static bool HideVerify()
        {
            return instance.visible;
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Dimensions Overlay/Hide", false, PreferenceKeys.menuEditor + 30)]
        static void Hide()
        {
            instance.visible = false;
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Dimensions Overlay/Show", true, PreferenceKeys.menuEditor + 30)]
        static bool ShowVerify()
        {
            return !instance.visible;
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Dimensions Overlay/Show", false, PreferenceKeys.menuEditor + 30)]
        static void Show()
        {
            instance.visible = true;
        }

        enum BoundsDisplay
        {
            Object = 1,
            Element = 2
        }

        [UserSetting("Dimensions Overlay", "Bounds Display", "Sets what content is considered when calculating the selected" +
            "bounds.\nObject displays the world space bounds of the selection.\nElement displays the world space bounds" +
            "of the selected mesh elements (vertices, faces, edges).")]
        static Pref<BoundsDisplay> s_BoundsDisplay = new Pref<BoundsDisplay>("s_BoundsDisplay", BoundsDisplay.Object, SettingsScope.User);

        [Shortcut("ProBuilder/Dimensions Overlay/Toggle Overlay", typeof(SceneView))]
        static void ToggleUseElementBounds()
        {
            // toggle between { Off, Visible Object, Visible Selection }
            if (instance.visible)
            {
                var display = s_BoundsDisplay.value;

                // Visible Object -> Visible Selection
                if (display == BoundsDisplay.Object)
                    s_BoundsDisplay.SetValue(BoundsDisplay.Element, true);
                // Visible Selection -> Off
                else
                    instance.visible = false;
            }
            else
            {
                // Off -> Visible Object
                s_BoundsDisplay.SetValue(BoundsDisplay.Object, true);
                Show();
            }

            if (instance.visible)
            {
                instance.RebuildBounds();
                EditorUtility.ShowNotification("Dimensions Overlay\n" + s_BoundsDisplay.value.ToString());
            }
            else
            {
                EditorUtility.ShowNotification("Dimensions Overlay\nOff");
            }

            SceneView.RepaintAll();
        }

        void OnEnable()
        {
            m_DisplayMesh = new Mesh();
            m_DisplayMaterial = new Material(Shader.Find("ProBuilder/UnlitVertexColor"));
            m_DisplayMesh.hideFlags = HideFlags.DontSave;
            m_DisplayMaterial.hideFlags = HideFlags.DontSave;
            
            if (m_Visible)
                EditorApplication.delayCall += OnShowed; //Rebuild bounds on the first frame after entering play mode returns no bounds (the selection isn't valid yet)
        }

        void OnDisable()
        {
            DestroyImmediate(m_DisplayMesh);
            DestroyImmediate(m_DisplayMaterial);
        }

        void OnShowed()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            MeshSelection.objectSelectionChanged += OnObjectSelectionChanged;
            ProBuilderMesh.elementSelectionChanged += OnElementSelectionChanged;
            ProBuilderEditor.selectionUpdated += OnEditingMeshSelection;
            VertexManipulationTool.beforeMeshModification += OnBeginMeshModification;
            VertexManipulationTool.afterMeshModification += OnFinishMeshModification;

            RebuildBounds();
        }

        void OnHid()
        {
            MeshSelection.objectSelectionChanged -= OnObjectSelectionChanged;
            ProBuilderMesh.elementSelectionChanged -= OnElementSelectionChanged;
            ProBuilderEditor.selectionUpdated -= OnEditingMeshSelection;
            VertexManipulationTool.beforeMeshModification -= OnBeginMeshModification;
            VertexManipulationTool.afterMeshModification -= OnFinishMeshModification;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        static void OnBeginMeshModification(IEnumerable<ProBuilderMesh> meshes)
        {
            s_RecalculateMeshBounds = true;
        }

        static void OnFinishMeshModification(IEnumerable<ProBuilderMesh> meshes)
        {
            s_RecalculateMeshBounds = false;
        }

        static bool GetElementBounds(IEnumerable<ProBuilderMesh> meshes, SelectMode mode, out Bounds bounds)
        {
            bool initialized = false;
            bounds = new Bounds();

            foreach (var mesh in meshes)
            {
                var positions = mesh.positionsInternal;
                var trs = mesh.transform;

                switch (mode)
                {
                    case SelectMode.Face:
                    case SelectMode.TextureFace:
                    {
                        var faces = mesh.facesInternal;

                        foreach (var face in mesh.selectedFaceIndicesInternal)
                        {
                            foreach (var index in faces[face].distinctIndexesInternal)
                            {
                                var position = trs.TransformPoint(positions[index]);

                                if (!initialized)
                                {
                                    bounds = new Bounds(position, Vector3.zero);
                                    initialized = true;
                                }
                                else
                                {
                                    bounds.Encapsulate(position);
                                }
                            }
                        }
                        break;
                    }

                    case SelectMode.Edge:
                    case SelectMode.TextureEdge:
                    {
                        foreach (var edge in mesh.selectedEdgesInternal)
                        {
                            var a = trs.TransformPoint(positions[edge.a]);
                            var b = trs.TransformPoint(positions[edge.b]);

                            if (!initialized)
                            {
                                bounds = new Bounds(a, Vector3.zero);
                                initialized = true;
                            }
                            else
                            {
                                bounds.Encapsulate(a);
                            }

                            bounds.Encapsulate(b);
                        }

                        break;
                    }

                    case SelectMode.Vertex:
                    case SelectMode.TextureVertex:
                    {
                        foreach (var index in mesh.selectedIndexesInternal)
                        {
                            var position = trs.TransformPoint(positions[index]);

                            if (!initialized)
                            {
                                bounds = new Bounds(position, Vector3.zero);
                                initialized = true;
                            }
                            else
                            {
                                bounds.Encapsulate(position);
                            }
                        }
                        break;
                    }
                }
            }

            return initialized;
        }

        bool GetSelectedBounds(out Bounds bounds)
        {
            m_Selected.Clear();

            var selectMode = ProBuilderEditor.selectMode;

            if (s_BoundsDisplay.value == BoundsDisplay.Element && ProBuilderEditor.selectMode.IsMeshElementMode())
            {
                foreach (var m in MeshSelection.topInternal)
                    m_Selected.Add(m.transform, new Trs(m.transform));

                return GetElementBounds(MeshSelection.topInternal, selectMode, out bounds);
            }

            foreach (var m in Selection.transforms)
                m_Selected.Add(m.transform, new Trs(m.transform));

            if (s_RecalculateMeshBounds)
            {
                foreach(var mesh in MeshSelection.topInternal)
                    mesh.mesh.RecalculateBounds();
            }

            var renderers = Selection.transforms
                .Where(x => x.GetComponent<MeshRenderer>() != null)
                .Select(x => x.GetComponent<MeshRenderer>());

            if (!renderers.Any())
            {
                bounds = new Bounds();
                return false;
            }

            bounds = renderers.First().bounds;

            foreach (var ren in renderers)
                bounds.Encapsulate(ren.bounds);

            return true;
        }

        void OnObjectSelectionChanged()
        {
            RebuildBounds();
        }

        void OnElementSelectionChanged(ProBuilderMesh mesh)
        {
            RebuildBounds();
        }

        void OnEditingMeshSelection(IEnumerable<ProBuilderMesh> meshes)
        {
            RebuildBounds();
        }

        void RebuildBounds()
        {
            m_HasBounds = GetSelectedBounds(out m_Bounds);
            SceneView.RepaintAll();
        }

        void OnSceneGUI(SceneView scnview)
        {
            if (Selection.count > 0 && m_HasBounds)
            {
                foreach (var m in m_Selected)
                {
                    if (!((Trs)m.Key).Equals(m.Value))
                    {
                        RebuildBounds();
                        break;
                    }
                }

                RenderBounds(m_Bounds);
            }
        }

        Mesh m_DisplayMesh;
        Material m_DisplayMaterial;

        // readonly Color wirecolor = new Color(.9f, .9f, .9f, .6f);
        readonly Color k_LightWhite = new Color(.6f, .6f, .6f, .5f);

        /// <summary>
        /// Render an axis aligned bounding box in world space.
        /// </summary>
        /// <param name="bounds">aabb</param>
        void RenderBounds(Bounds bounds)
        {
            if (!m_DisplayMesh)
                return;

            // show labels
            DrawHeight(bounds.center, bounds.extents);
            DrawWidth(bounds.center, bounds.extents);
            DrawDepth(bounds.center, bounds.extents);
        }

        const float DISTANCE_LINE_OFFSET = .2f;

        static float LineDistance()
        {
            return HandleUtility.GetHandleSize(Selection.activeTransform.position) * DISTANCE_LINE_OFFSET;
        }

        Transform cam { get { return SceneView.lastActiveSceneView.camera.transform; } }

        void DrawHeight(Vector3 cen, Vector3 ext)
        {
            // positibilities
            Vector3[] edges = new Vector3[8]
            {
                // front left
                new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z),
                new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z),

                // front right
                new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z),
                new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z),

                // back left
                new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z),
                new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z),

                // back right
                new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z),
                new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z)
            };

            // figure leftmost height boundary
            Vector2 pos = Vector2.right * 20000f;
            Vector3 a = Vector3.zero, b = Vector3.zero;

            for (int i = 0; i < edges.Length; i += 2)
            {
                Vector2 screen = HandleUtility.WorldToGUIPoint((edges[i] + edges[i + 1]) * .5f);

                if (screen.x < pos.x)
                {
                    pos = screen;
                    a = edges[i + 0];
                    b = edges[i + 1];
                }
            }

            float dist = Vector3.Distance(a, b);

            if (dist < Mathf.Epsilon)
                return;

            Vector3 left = Vector3.Cross(cam.forward, Vector3.up).normalized * LineDistance();

            Handles.color = k_LightWhite;
            Handles.DrawLine(a + left * .1f, a + left);
            Handles.DrawLine(b + left * .1f, b + left);
            Handles.color = Color.green;
            Handles.DrawLine(a + left, b + left);

            a += left;
            b += left;

            Handles.BeginGUI();
            pos.x -= UI.EditorStyles.sceneTextBox.CalcSize(gc).x * 2f;
            DrawSceneLabel(dist.ToString("F2"), pos);
            Handles.EndGUI();
        }

        void DrawDepth(Vector3 cen, Vector3 ext)
        {
            // positibilities
            Vector3[] edges = new Vector3[8]
            {
                // bottom right
                new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z),
                new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z),

                // top right
                new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z),
                new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z),

                // bottom left
                new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z),
                new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z),

                // top left
                new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z),
                new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z),
            };

            // figure leftmost height boundary
            Vector2 pos = Vector2.up * -20000f;
            Vector3 a = Vector3.zero, b = Vector3.zero;

            for (int i = 0; i < edges.Length; i += 2)
            {
                Vector2 screen = HandleUtility.WorldToGUIPoint((edges[i] + edges[i + 1]) * .5f);

                if (screen.y > pos.y)
                {
                    pos = screen;
                    a = edges[i + 0];
                    b = edges[i + 1];
                }
            }

            float dist = Vector3.Distance(a, b);

            if (dist < Mathf.Epsilon)
                return;

            float dot = Vector3.Dot(cam.transform.forward, Vector3.right);
            float sign = dot < 0f ? -1f : 1f;
            Vector3 offset = -(Vector3.up + (Vector3.right * sign)).normalized * LineDistance();

            Handles.color = k_LightWhite;
            Handles.DrawLine(a + offset * .1f, a + offset);
            Handles.DrawLine(b + offset * .1f, b + offset);

            a += offset;
            b += offset;

            Handles.color = Color.blue;
            Handles.DrawLine(a, b);

            Handles.BeginGUI();
            pos.y += UI.EditorStyles.sceneTextBox.CalcHeight(gc, 20000);
            DrawSceneLabel(dist.ToString("F2"), pos);

            Handles.EndGUI();
        }

        void DrawWidth(Vector3 cen, Vector3 extents)
        {
            Vector3 ext = extents;// + extents.normalized * .2f;

            // positibilities
            Vector3[] edges = new Vector3[8]
            {
                // bottom front
                new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z),
                new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z),

                // bottom back
                new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z),
                new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z),

                // top front
                new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z),
                new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z),

                // top back
                new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z),
                new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z)
            };

            // figure leftmost height boundary
            Vector2 pos = Vector2.up * -20000f;
            Vector3 a = Vector3.zero, b = Vector3.zero;

            for (int i = 0; i < edges.Length; i += 2)
            {
                Vector2 screen = HandleUtility.WorldToGUIPoint((edges[i] + edges[i + 1]) * .5f);

                if (screen.y > pos.y)
                {
                    pos = screen;
                    a = edges[i + 0];
                    b = edges[i + 1];
                }
            }

            float dist = Vector3.Distance(a, b);

            if (dist < Mathf.Epsilon)
                return;
            // Vector3 offset = -Vector3.up;
            // offset = -Vector3.Cross(Vector3.Cross(cam.forward, Vector3.up), cam.forward).normalized * LineDistance();

            float dot = Vector3.Dot(cam.transform.forward, Vector3.forward);
            float sign = dot < 0f ? -1f : 1f;
            Vector3 offset = -(Vector3.up + (Vector3.forward * sign)).normalized * LineDistance();

            Handles.color = k_LightWhite;
            Handles.DrawLine(a + offset * .1f, a + offset);
            Handles.DrawLine(b + offset * .1f, b + offset);

            a += offset;
            b += offset;

            Handles.color = Color.red;
            Handles.DrawLine(a, b);
            Handles.BeginGUI();
            DrawSceneLabel(dist.ToString("F2"), HandleUtility.WorldToGUIPoint((a + b) * .5f));
            Handles.EndGUI();
        }

        GUIContent gc = new GUIContent("", "");

        void DrawSceneLabel(string content, Vector2 position)
        {
            gc.text = content;
            float width = UI.EditorStyles.sceneTextBox.CalcSize(gc).x;
            float height = UI.EditorStyles.sceneTextBox.CalcHeight(gc, width);
            GUI.Label(new Rect(position.x, position.y, width, height), gc, UI.EditorStyles.sceneTextBox);
        }
    }
}
