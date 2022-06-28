using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Serialization;
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

        [SerializeField] private int m_SegmentCount = 3; // min 2

        [SerializeField] private int m_FacesAroundRadiusCount = 4; // min 3

        public void Init()
        {
            float3 tan = new float3(0f, 0f, 2f);
            float3 p1 = new float3(3f, 0f, 0f);

            Splines = new Splines.Spline[] { new Splines.Spline() };
            m_Spline.Add(new BezierKnot(float3.zero, -tan, tan, Quaternion.identity));
            m_Spline.Add(new BezierKnot(p1, p1 + tan, p1 + -tan, Quaternion.identity));
        }

        private List<Vector3> vertexPositions;

        public void Extrude2DMesh()
        {
            List<Vector3> segmentPositions = new List<Vector3>(m_SegmentCount + 1);
            int verticesAtSegment = 3;
            vertexPositions = new List<Vector3>(segmentPositions.Count * verticesAtSegment);
            List<Face> faces = new List<Face>(m_SegmentCount * verticesAtSegment);

            float t = 0f;

            // define the positions of each segment of the spline
            for (int i = 0; i < m_SegmentCount + 1; i++)
            {
                segmentPositions.Add(SplineUtility.EvaluatePosition(m_Spline, t));
                t += 1f / m_SegmentCount;
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

                t += 1f / m_SegmentCount;
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
            vertexPositions = new List<Vector3>(m_SegmentCount * verticesAtSegment);
            List<Face> faces = new List<Face>(m_SegmentCount * verticesAtSegment);

            float t = 0f;
            int vertexPosition = 0;

            // define the positions of each segment, and the vertex positions at each segment
            for (int i = 0; i < m_SegmentCount + 1; i++)
            {
                SplineUtility.Evaluate(m_Spline, t, out var position, out var tangent, out var normal);

                t += 1f / m_SegmentCount;

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

            mesh.RebuildWithPositionsAndFaces(vertexPositions, faces);
        }

        public void Extrude3DMesh()
        {
            vertexPositions = new List<Vector3>(m_SegmentCount * m_FacesAroundRadiusCount);
            List<Face> faces = new List<Face>(m_SegmentCount * m_FacesAroundRadiusCount);

            float t = 0f;
            int vertexPositionCount = 0;

            // define the positions of each segment, and the vertex positions at each segment
            for (int i = 0; i < m_SegmentCount + 1; i++)
            {
                SplineUtility.Evaluate(m_Spline, t, out var position, out var tangent, out var up);
                var right = Vector3.Cross(tangent, up).normalized;
                t += 1f / m_SegmentCount;

                // define the vertex positions around the spline at each segmentPosition along the spline
                for (int j = 0; j < m_FacesAroundRadiusCount; j++)
                {
                    var angleInRadians = 2 * Mathf.PI / m_FacesAroundRadiusCount * j;
                    var verticalPos = Mathf.Sin(angleInRadians);
                    var horizontalPos = Mathf.Cos(angleInRadians);
                    var vertexDirection = horizontalPos * right + verticalPos * (Vector3) up;
                    var vertexPosition = (Vector3)position + vertexDirection * m_Radius;
                    vertexPositions.Add(vertexPosition);
                }

                // define faces from vertices
                if (i < m_SegmentCount)
                {
                    for (int j = 0; j < m_FacesAroundRadiusCount; j++)
                    {
                        int[] face1 = new int[]
                        {
                            vertexPositionCount + 1, vertexPositionCount, vertexPositionCount + 3,
                            vertexPositionCount + 1, vertexPositionCount + 3, vertexPositionCount + 4
                        };

                        int[] face2 = new int[]
                        {
                            vertexPositionCount + 1, vertexPositionCount + 4, vertexPositionCount + 2,
                            vertexPositionCount + 2, vertexPositionCount + 4, vertexPositionCount + 5
                        };

                        faces.Add(new Face(face1));
                        faces.Add(new Face(face2));
                
                        vertexPositionCount += m_FacesAroundRadiusCount;
                    }
                }
            }

            mesh.RebuildWithPositionsAndFaces(vertexPositions, faces);
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
