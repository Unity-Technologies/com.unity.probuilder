using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProBuilderMesh))]
    sealed class BezierShape : MonoBehaviour
    {
        public List<BezierPoint> points = new List<BezierPoint>();
        public bool closeLoop = false;
        public float radius = .5f;
        public int rows = 8;
        public int columns = 16;
        public bool smooth = true;
        [SerializeField]
        bool m_IsEditing;
        public bool isEditing
        {
            get { return m_IsEditing; }
            set { m_IsEditing = value; }
        }

        ProBuilderMesh m_Mesh;

        public ProBuilderMesh mesh
        {
            get
            {
                if (m_Mesh == null)
                    m_Mesh = GetComponent<ProBuilderMesh>();

                return m_Mesh;
            }

            set
            {
                m_Mesh = value;
            }
        }

        /// <summary>
        /// Initialize the points list with a default set.
        /// </summary>
        public void Init()
        {
            Vector3 tan = new Vector3(0f, 0f, 2f);
            Vector3 p1 = new Vector3(3f, 0f, 0f);
            points.Add(new BezierPoint(Vector3.zero, -tan, tan, Quaternion.identity));
            points.Add(new BezierPoint(p1, p1 + tan, p1 + -tan, Quaternion.identity));
        }

        /// <summary>
        /// Rebuild the ProBuilderMesh with the extruded spline.
        /// </summary>
        public void Refresh()
        {
            if (points.Count < 2)
            {
                mesh.Clear();
                mesh.ToMesh();
                mesh.Refresh();
            }
            else
            {
                ProBuilderMesh m = mesh;
                Spline.Extrude(points, radius, columns, rows, closeLoop, smooth, ref m);
            }
        }
    }
}
