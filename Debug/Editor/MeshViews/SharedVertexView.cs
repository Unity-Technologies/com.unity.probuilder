using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Debug
{
    class SharedVertexView : MeshDebugView
    {
        int m_VertexCount = 0;
        int[] m_Vertices;
        string[] m_Content;
        HashSet<int> m_Used = new HashSet<int>();

        protected override void AnythingChanged()
        {
            if (mesh == null)
                return;

            var lookup = mesh.sharedVertexLookup;
            var common = mesh.sharedVertices;
            var display = (viewState == MeshViewState.Selected ? mesh.selectedVertices : mesh.sharedVertices.Select(x => x[0])).ToArray();
            m_Used.Clear();

            m_VertexCount = display.Length;
            m_Vertices = new int[m_VertexCount];
            m_Content = new string[m_VertexCount];

            for (int i = 0; i < m_VertexCount; i++)
            {
                int cIndex = lookup[display[i]];
                if (!m_Used.Add(cIndex))
                    continue;
                m_Vertices[i] = display[i];
                m_Content[i] = string.Format("<b>{0}</b>: {1}", cIndex, GetCommonVertexString(common[lookup[display[i]]]));
            }
        }

        public override void OnGUI()
        {
            GUI.skin.label.richText = true;

            foreach (var str in m_Content)
                GUILayout.Label(str);
        }

        static string GetCommonVertexString(SharedVertex shared)
        {
            var str = "";
            var indexes = shared.arrayInternal;
            for (int i = 0, c = indexes.Length - 1; i < c; i++)
                str += indexes[i] + ", ";
            str += indexes[indexes.Length - 1];
            return str;
        }

        public override void Draw(SceneView view)
        {
            var positions = mesh.positionsInternal;
            var trs = mesh.transform;

            for (int i = 0; i < m_VertexCount; i++)
            {
                var point = trs.TransformPoint(positions[m_Vertices[i]]);

                if (!UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(view.camera, mesh, point))
                    DrawSceneLabel(point, m_Content[i]);
            }
        }
    }
}
