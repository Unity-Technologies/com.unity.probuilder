using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Debug
{
    class FaceView : MeshDebugView
    {
        List<string> m_Content = new List<string>();
        List<Vector3> m_Position = new List<Vector3>();

        protected override void AnythingChanged()
        {
            if (mesh == null)
                return;

            var lookup = mesh.sharedVertexLookup;
            var display = viewState == MeshViewState.Selected ? mesh.selectedFaceIndexes.Select(x => mesh.facesInternal[x]) : mesh.faces;
            m_Content.Clear();
            var positions = mesh.positionsInternal;

            foreach (var face in display)
            {
                var center = Vector3.zero;
                foreach (var index in face.distinctIndexesInternal)
                    center += positions[index];
                center /= face.distinctIndexesInternal.Length;

                var content = "<b>indexes:</b> " + face.ToString() + "\n" +
                    "<b>submesh:</b> " + face.submeshIndex + "\n" +
                    "<b>manual uv:</b> " + face.manualUV + "\n" +
                    "<b>smoothing:</b> " + face.smoothingGroup + "\n" +
                    "<b>texture:</b> " + face.textureGroup;

                m_Content.Add(content);
                m_Position.Add(center);
            }
        }

        public override void OnGUI()
        {
            GUI.skin.label.richText = true;

            foreach (var str in m_Content)
                GUILayout.Label(str);
        }

        public override void Draw(SceneView view)
        {
            var positions = mesh.positionsInternal;
            var trs = mesh.transform;
            var len = m_Content.Count;

            for (int i = 0; i < len; i++)
            {
                var point = trs.TransformPoint(m_Position[i]);

                if (!UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(view.camera, mesh, point))
                    DrawSceneLabel(point, m_Content[i]);
            }
        }
    }
}
