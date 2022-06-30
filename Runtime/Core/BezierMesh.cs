using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Splines;

namespace UnityEngine.ProBuilder
{
    [DisallowMultipleComponent, ExcludeFromPreset, ExcludeFromObjectFactory]
    [RequireComponent(typeof(ProBuilderMesh))]
    sealed class BezierMesh : MonoBehaviour, ISplineContainer
    {
        [SerializeField] private Splines.Spline m_Spline;

        public IReadOnlyList<Splines.Spline> Splines
        {
            get => new Splines.Spline[] { m_Spline };
            set
            {
                if (value == null)
                {
                    m_Spline = new Splines.Spline();
                    return;
                }

                m_Spline = value[0];
            }
        }

        [SerializeField] private KnotLinkCollection m_Knots = new KnotLinkCollection();

        public KnotLinkCollection KnotLinkCollection => m_Knots;

        ProBuilderMesh m_Mesh;

        public ProBuilderMesh mesh
        {
            get
            {
                if (m_Mesh == null)
                    m_Mesh = GetComponent<ProBuilderMesh>();

                return m_Mesh;
            }

            set { m_Mesh = value; }
        }

        [SerializeField] private float m_Radius = 0.5f; // min 0.01f

        [SerializeField] private int m_SegmentsPerUnit = 1; // min 1

        [SerializeField] private int m_FaceCount = 10; // min 3

        public void Init()
        {
            float3 tan = new float3(0f, 0f, 2f);
            float3 p1 = new float3(20f, 0f, 0f);

            Splines = new Splines.Spline[] { new Splines.Spline() };
            m_Spline.Add(new BezierKnot(float3.zero, -tan, tan, Quaternion.identity));
            m_Spline.Add(new BezierKnot(p1, p1 + tan, p1 + -tan, Quaternion.identity));
        }

        private List<Vector3> vertexPositions;

        public void Extrude3DMesh()
        {
            int segmentsCount = (int) m_Spline.GetLength() * m_SegmentsPerUnit;
            vertexPositions = new List<Vector3>(segmentsCount * m_FaceCount);
            List<Face> faces = new List<Face>(segmentsCount * m_FaceCount);

            float t = 0f;
            int vertexIndex = 0;

            // define the positions of each segment, and the vertex positions at each segment
            for (int i = 0; i < segmentsCount + 1; i++)
            {
                SplineUtility.Evaluate(m_Spline, t, out var position, out var tangent, out var up);
                var right = Vector3.Cross(tangent, up).normalized;
                t += 1f / segmentsCount;

                // define the vertex positions around the spline at each segmentPosition along the spline
                for (int j = 0; j < m_FaceCount; j++)
                {
                    var angleInRadians = 2 * Mathf.PI / m_FaceCount * j;
                    var verticalPos = Mathf.Sin(angleInRadians);
                    var horizontalPos = Mathf.Cos(angleInRadians);
                    var vertexDirection = horizontalPos * right + verticalPos * (Vector3)up;
                    var vertexPosition = (Vector3)position + vertexDirection * m_Radius;
                    vertexPositions.Add(vertexPosition);
                }

                // define faces
                if (i > 0)
                {
                    for (int j = 0; j < m_FaceCount; j++)
                    {
                        int[] face = new int[]
                        {
                            vertexIndex + j,
                            vertexIndex + (j + m_FaceCount - 1) % m_FaceCount,
                            vertexIndex + (j + m_FaceCount - 1) % m_FaceCount + m_FaceCount,
                            vertexIndex + j,
                            vertexIndex + (j + m_FaceCount - 1) % m_FaceCount + m_FaceCount,
                            vertexIndex + j + m_FaceCount
                        };

                        faces.Add(new Face(face));
                    }
                    vertexIndex += m_FaceCount;
                }
            }
            mesh.RebuildWithPositionsAndFaces(vertexPositions, faces);
        }

        public void Extrude2DMesh()
        {
            List<Vector3> segmentPositions = new List<Vector3>(m_SegmentsPerUnit + 1);
            int verticesAtSegment = 3;
            vertexPositions = new List<Vector3>(segmentPositions.Count * verticesAtSegment);
            List<Face> faces = new List<Face>(m_SegmentsPerUnit * verticesAtSegment);

            float t = 0f;

            // define the positions of each segment of the spline
            for (int i = 0; i < m_SegmentsPerUnit + 1; i++)
            {
                segmentPositions.Add(SplineUtility.EvaluatePosition(m_Spline, t));
                t += 1f / m_SegmentsPerUnit;
            }

            t = 0f;

            // define the vertex positions around the spline at each segmentPosition along the spline
            foreach (var position in segmentPositions)
            {
                Vector3 normal = SplineUtility.EvaluateUpVector(m_Spline, t);
                var pos1 = position + normal.normalized * m_Radius;
                var pos2 = position;
                var pos3 = position - normal.normalized * m_Radius;

                vertexPositions.Add(pos1);
                vertexPositions.Add(pos2);
                vertexPositions.Add(pos3);

                t += 1f / m_SegmentsPerUnit;
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

            mesh.RebuildWithPositionsAndFaces(vertexPositions, faces);
        }

        public void Extrude2DMeshOptimized()
        {
            int verticesAtSegment = 3;
            vertexPositions = new List<Vector3>(m_SegmentsPerUnit * verticesAtSegment);
            List<Face> faces = new List<Face>(m_SegmentsPerUnit * verticesAtSegment);

            float t = 0f;
            int vertexIndex = 0;

            // define the positions of each segment, and the vertex positions at each segment
            for (int i = 0; i < m_SegmentsPerUnit + 1; i++)
            {
                SplineUtility.Evaluate(m_Spline, t, out var position, out var tangent, out var normal);

                t += 1f / m_SegmentsPerUnit;

                var pos1 = position + math.normalize(normal) * m_Radius;
                var pos2 = position;
                var pos3 = position - math.normalize(normal) * m_Radius;

                vertexPositions.Add(pos1);
                vertexPositions.Add(pos2);
                vertexPositions.Add(pos3);

                // define faces
                if (i > 0)
                {
                    int[] face1 = new int[6]
                    {
                        vertexIndex + 1, vertexIndex, vertexIndex + 3,
                        vertexIndex + 1, vertexIndex + 3, vertexIndex + 4
                    };

                    int[] face2 = new int[6]
                    {
                        vertexIndex + 1, vertexIndex + 4, vertexIndex + 2,
                        vertexIndex + 2, vertexIndex + 4, vertexIndex + 5
                    };

                    faces.Add(new Face(face1));
                    faces.Add(new Face(face2));

                    vertexIndex += verticesAtSegment;
                }
            }

            mesh.RebuildWithPositionsAndFaces(vertexPositions, faces);
        }

        // private void OnDrawGizmos()
        // {
        //     int i = 0;
        //     foreach (var pos in vertexPositions)
        //     {
        //         Gizmos.DrawSphere(pos, 0.05f);
        //         Handles.Label(pos, $"{i}");
        //         i++;
        //     }
        // }
    }
}
