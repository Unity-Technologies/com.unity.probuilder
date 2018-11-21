using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Debug
{
    class EdgeView : MeshDebugView
    {
        static GUIContent s_TempContent = new GUIContent();
        Dictionary<EdgeLookup, string> m_Content = new Dictionary<EdgeLookup, string>();

        protected override void AnythingChanged()
        {
            if (mesh == null)
                return;

            var edges = viewState == MeshViewState.All ? mesh.faces.SelectMany(x => x.edgesInternal) : mesh.selectedEdgesInternal;
            var common = EdgeLookup.GetEdgeLookup(edges, mesh.sharedVertexLookup);

            foreach (var edge in common)
            {
                if (m_Content.ContainsKey(edge))
                    m_Content[edge] += ", " + edge.local;
                else
                    m_Content.Add(edge, string.Format("<b>{0}</b>: {1}", edge.common, edge.local));
            }
        }

        public override void Draw(SceneView view)
        {
            var positions = mesh.positionsInternal;
            var center = HandleUtility.WorldToGUIPoint(mesh.GetComponent<Renderer>().bounds.center);
            var trs = mesh.transform;

            foreach (var kvp in m_Content)
            {
                var a = positions[kvp.Key.local.a];
                var b = positions[kvp.Key.local.b];

                var point = trs.TransformPoint((a + b) * .5f);

                if (!UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(view.camera, mesh, point))
                {
                    s_TempContent.text = kvp.Value;
                    var rect = HandleUtility.WorldPointToSizedRect(point, s_TempContent, UI.EditorStyles.sceneTextBox);
                    var offset = new Vector2(rect.x - center.x, rect.y - center.y);
                    offset.Normalize();
                    rect.x += offset.x * (40f * EditorGUIUtility.pixelsPerPoint);
                    rect.y += offset.y * (40f * EditorGUIUtility.pixelsPerPoint);
                    rect.x = Mathf.Clamp(rect.x, 0f, Screen.width - rect.width);

                    GUI.Label(rect, s_TempContent, UI.EditorStyles.sceneTextBox);
                }
            }
        }
    }
}
