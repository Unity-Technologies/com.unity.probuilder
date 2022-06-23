using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Splines;
using UnityEngine.tvOS;

namespace UnityEngine.ProBuilder
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent, ExcludeFromPreset, ExcludeFromObjectFactory]
    [RequireComponent(typeof(ProBuilderMesh))]
    [RequireComponent(typeof(ISplineContainer))]
    sealed class BezierMesh : MonoBehaviour, ISplineContainer
    {
        [SerializeField] private Splines.Spline m_spline;

        public IReadOnlyList<Splines.Spline> Splines
        {
            get => new Splines.Spline[] { m_spline };
            set
            {
                if (value == null)
                {
                    m_spline = new Splines.Spline();
                    return;
                }

                m_spline = value[0];
            }
        }

        [SerializeField] private KnotLinkCollection m_knots = new KnotLinkCollection();

        public KnotLinkCollection KnotLinkCollection => m_knots;

        ProBuilderMesh m_mesh;

        public ProBuilderMesh mesh
        {
            get
            {
                if (m_mesh == null)
                    m_mesh = GetComponent<ProBuilderMesh>();

                return m_mesh;
            }

            set { m_mesh = value; }
        }

        [SerializeField] bool m_IsEditing;

        public bool isEditing
        {
            get { return m_IsEditing; }
            set { m_IsEditing = value; }
        }

        [SerializeField] private float m_radius = 0.5f; // min 0.01f

        [SerializeField] private int m_segmentCount = 3; // min 2

        [SerializeField] private int m_facesAroundRadiusCount = 3; // min 3

        public void Init()
        {
            float3 tan = new float3(0f, 0f, 2f);
            float3 p1 = new float3(3f, 0f, 0f);

            Splines = new Splines.Spline[] { new Splines.Spline() };
            m_spline.Add(new BezierKnot(new float3(0f, 0f, 0f), -tan, tan, Quaternion.identity));
            m_spline.Add(new BezierKnot(p1, p1 + tan, p1 + -tan, Quaternion.identity));
        }

        private List<Vector3> vertexPositions;

        public void Extrude2DMesh()
        {
            List<Vector3> segmentPositions = new List<Vector3>(m_segmentCount + 1);
            int verticesAtSegment = 3;
            vertexPositions = new List<Vector3>(segmentPositions.Count * verticesAtSegment);
            List<Face> faces = new List<Face>(m_segmentCount * m_facesAroundRadiusCount);

            float t = 0f; // value between 0 and 1 representing the ratio along the curve

            // define the positions of each segment of the spline
            for (int i = 0; i < m_segmentCount + 1; i++)
            {
                segmentPositions.Add(SplineUtility.EvaluatePosition(m_spline, t));
                t += 1f / m_segmentCount;
            }

            t = 0f;

            // define the vertex positions around the spline at each segmentPosition along the spline
            foreach (var position in segmentPositions)
            {
                var normal = SplineUtility.EvaluateUpVector(m_spline, t);
                var pos1 = position + new Vector3(normal.x, normal.y, normal.z).normalized * m_radius;
                var pos2 = position;
                var pos3 = position - new Vector3(normal.x, normal.y, normal.z).normalized * m_radius;

                vertexPositions.Add(pos1);
                vertexPositions.Add(pos2);
                vertexPositions.Add(pos3);

                t += 1f / m_segmentCount;
            }

            // define faces
            for (int i = 0; i < vertexPositions.Count - 6; i+= 3)
            {
                int[] face1 = new int[]
                {
                    i + 1, i, i + 2,
                    i + 1, i + 3, i + 4
                };

                int[] face2 = new int[]
                {
                    i + 1, i + 4, i + 2,
                    i + 2, i + 3, i + 5
                };

                faces.Add(new Face(face1));
                faces.Add(new Face(face2));
            }

            ProBuilderMesh.Create(segmentPositions, faces);
        }

        private void OnDrawGizmos()
        {
            foreach (var pos in vertexPositions)
            {
                Gizmos.DrawSphere(pos, 0.05f);
            }
        }
    }
}
