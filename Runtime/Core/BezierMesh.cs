using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Splines;

namespace UnityEngine.ProBuilder
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent, ExcludeFromPreset, ExcludeFromObjectFactory]
    [RequireComponent(typeof(ProBuilderMesh))]
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

        [SerializeField] private int m_segmentCount = 6; // min 2

        [SerializeField] private int m_facesAroundRadiusCount = 4; // min 3

        public void Init()
        {
            float3 tan = new float3(0f, 0f, 2f);
            float3 p1 = new float3(3f, 0f, 0f);

            Splines = new Splines.Spline[] { new Splines.Spline() };
            m_spline.Add(new BezierKnot(float3.zero, -tan, tan, Quaternion.identity));
            m_spline.Add(new BezierKnot(p1, p1 + tan, p1 + -tan, Quaternion.identity));
        }

        private List<Vector3> vertexPositions;

        public void Extrude2DMesh()
        {
            List<Vector3> segmentPositions = new List<Vector3>(m_segmentCount + 1);
            int verticesAtSegment = 3;
            vertexPositions = new List<Vector3>(segmentPositions.Count * verticesAtSegment);
            List<Face> faces = new List<Face>(m_segmentCount * verticesAtSegment);

            float t = 0f;

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
                Vector3 normal = SplineUtility.EvaluateUpVector(m_spline, t);
                var pos1 = position + normal.normalized * m_radius;
                var pos2 = position;
                var pos3 = position - normal.normalized * m_radius;

                vertexPositions.Add(pos1);
                vertexPositions.Add(pos2);
                vertexPositions.Add(pos3);

                t += 1f / m_segmentCount;
            }

            // define faces
            for (int i = 0; i < vertexPositions.Count - 3; i += 3)
            {
                int[] face1 = new int[6]
                {
                    i + 1, i, i + 3,
                    i + 1, i + 3, i + 4
                };

                int[] face2 = new int[6]
                {
                    i + 1, i + 4, i + 2,
                    i + 2, i + 4, i + 5
                };

                faces.Add(new Face(face1));
                faces.Add(new Face(face2));
            }

            ProBuilderMesh.Create(vertexPositions, faces);
        }

        public void Extrude2DMeshOptimized()
        {
            int verticesAtSegment = 3;
            vertexPositions = new List<Vector3>(m_segmentCount * verticesAtSegment);
            List<Face> faces = new List<Face>(m_segmentCount * verticesAtSegment);

            float t = 0f;
            int vertexPosition = 0;

            // define the positions of each segment, and the vertex positions at each segment
            for (int i = 0; i < m_segmentCount + 1; i++)
            {
                Vector3 position = SplineUtility.EvaluatePosition(m_spline, t);
                Vector3 normal = SplineUtility.EvaluateUpVector(m_spline, t);
                t += 1f / m_segmentCount;

                var pos1 = position + normal.normalized * m_radius;
                var pos2 = position;
                var pos3 = position - normal.normalized * m_radius;

                vertexPositions.Add(pos1);
                vertexPositions.Add(pos2);
                vertexPositions.Add(pos3);

                // define faces
                if (i < m_segmentCount)
                {
                    int[] face1 = new int[6]
                    {
                        vertexPosition + 1, vertexPosition, vertexPosition + 3,
                        vertexPosition + 1, vertexPosition + 3, vertexPosition + 4
                    };

                    int[] face2 = new int[6]
                    {
                        vertexPosition + 1, vertexPosition + 4, vertexPosition + 2,
                        vertexPosition + 2, vertexPosition + 4, vertexPosition + 5
                    };

                    faces.Add(new Face(face1));
                    faces.Add(new Face(face2));

                    vertexPosition += 3;
                }
            }

            ProBuilderMesh.Create(vertexPositions, faces);
        }

        public void Extrude3DMesh()
        {
            vertexPositions = new List<Vector3>(m_segmentCount * m_facesAroundRadiusCount);
            List<Face> faces = new List<Face>(m_segmentCount * m_facesAroundRadiusCount);

            float t = 0f;
            int vertexPositionCount = 0;

            // define the positions of each segment, and the vertex positions at each segment
            for (int i = 0; i < m_segmentCount + 1; i++)
            {
                Vector3 position = SplineUtility.EvaluatePosition(m_spline, t);
                Vector3 normal = SplineUtility.EvaluateUpVector(m_spline, t);
                t += 1f / m_segmentCount;

                // define the vertex positions around the spline at each segmentPosition along the spline
                for(int j = 0; j < m_facesAroundRadiusCount; j++)
                {
                    var angleInRadians = 2 * Mathf.PI / m_facesAroundRadiusCount * j;
                    var verticalPos = Mathf.Sin(angleInRadians);
                    var horizontalPos = Mathf.Cos(angleInRadians);
                    var vertexDirection = new Vector3(horizontalPos, verticalPos, 0);
                    var vertexPosition = position + vertexDirection * m_radius;
                    vertexPositions.Add(vertexPosition);
                }

                // define faces from vertices
                // if (i < m_segmentCount)
                // {
                //     int[] face1 = new int[]
                //     {
                //         vertexPositionCount + 1, vertexPositionCount, vertexPositionCount + 3,
                //         vertexPositionCount + 1, vertexPositionCount + 3, vertexPositionCount + 4
                //     };
                //
                //     int[] face2 = new int[]
                //     {
                //         vertexPositionCount + 1, vertexPositionCount + 4, vertexPositionCount + 2,
                //         vertexPositionCount + 2, vertexPositionCount + 4, vertexPositionCount + 5
                //     };
                //
                //     faces.Add(new Face(face1));
                //     faces.Add(new Face(face2));
                //
                //     vertexPositionCount += 3;
                // }
            }

            var mesh = ProBuilderMesh.Create(vertexPositions, faces);
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
