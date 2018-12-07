using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    sealed class DimensionsEditor : ScriptableObject
    {
        static DimensionsEditor s_Instance;

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Dimensions Overlay/Hide", true, PreferenceKeys.menuEditor + 30)]
        static bool HideVerify()
        {
            return s_Instance != null;
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Dimensions Overlay/Hide", false, PreferenceKeys.menuEditor + 30)]
        static void Hide()
        {
            if (s_Instance != null)
                Object.DestroyImmediate(s_Instance);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Dimensions Overlay/Show", true, PreferenceKeys.menuEditor + 30)]
        static bool InitVerify()
        {
            return s_Instance == null;
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Dimensions Overlay/Show", false, PreferenceKeys.menuEditor + 30)]
        static void Init()
        {
            CreateInstance<DimensionsEditor>();
        }

        void OnEnable()
        {
            s_Instance = this;
            mesh = new Mesh();
            material = new Material(Shader.Find("ProBuilder/UnlitVertexColor"));
            mesh.hideFlags = HideFlags.DontSave;
            material.hideFlags = HideFlags.DontSave;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
            DestroyImmediate(mesh);
            DestroyImmediate(material);
        }

        bool GetSelectedBounds(out Bounds bounds)
        {
            IEnumerable<MeshRenderer> renderers = Selection.transforms.Where(x => x.GetComponent<MeshRenderer>() != null).Select(x => x.GetComponent<MeshRenderer>());

            if (!renderers.Any())
            {
                bounds = new Bounds();
                return false;
            }

            if (ProBuilderEditor.instance != null)
            {
                Vector3[] positions = MeshSelection.topInternal.SelectMany(x =>
                    {
                        var p = x.positions.ToArray();

                        return x.selectedVertices.Select(y =>
                        {
                            return x.transform.TransformPoint(p[y]);
                        });
                    }).ToArray();

                if (positions.Length > 0)
                {
                    bounds = Math.GetBounds(positions);
                    return true;
                }
            }

            bounds = renderers.First().bounds;

            foreach (var ren in renderers)
                bounds.Encapsulate(ren.bounds);

            return true;
        }

        void OnSceneGUI(SceneView scnview)
        {
            Bounds bounds;
            if (GetSelectedBounds(out bounds))
                RenderBounds(bounds);
        }

        Mesh mesh;
        Material material;

        // readonly Color wirecolor = new Color(.9f, .9f, .9f, .6f);
        readonly Color LightWhite = new Color(.6f, .6f, .6f, .5f);

        /// <summary>
        /// Render an axis aligned bounding box in world space.
        /// </summary>
        /// <param name="bounds">aabb</param>
        void RenderBounds(Bounds bounds)
        {
            if (!mesh)
                return;

            // show labels
            DrawHeight(bounds.center, bounds.extents);
            DrawWidth(bounds.center, bounds.extents);
            DrawDepth(bounds.center, bounds.extents);
        }

        const float DISTANCE_LINE_OFFSET = .2f;

        float LineDistance()
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

            Handles.color = LightWhite;
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

            Handles.color = LightWhite;
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

            Handles.color = LightWhite;
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
