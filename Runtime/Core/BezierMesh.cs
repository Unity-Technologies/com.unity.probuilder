using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Splines;

namespace UnityEngine.ProBuilder
{
    [RequireComponent(typeof(ProBuilderMesh))]
    sealed class BezierMesh : MonoBehaviour, ISplineContainer
    {
        [SerializeField] public Splines.Spline m_Spline;

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

        [SerializeField] public KnotLinkCollection m_Knots = new KnotLinkCollection();
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

        [SerializeField] [Range(0.001f, 64f)]
        public float m_Radius = 0.5f;

        [SerializeField] [Range(1, 128)]
        public int m_SegmentsPerUnit = 2;

        [SerializeField] [Range(3, 128)]
        public int m_FaceCountPerSegment = 8;

        private List<Vector3> m_VertexPositions;

        private List<Face> m_Faces;

         void OnValidate()
        {
            Extrude3DMesh();
        }

        //  void OnEnable()
        // {
        //     Debug.Log("enable");
        //     UnityEngine.Splines.Spline.Changed += UpdateMesh;
        // }
        //
        // void OnDisable()
        // {
        //     Debug.Log("disable");
        //     UnityEngine.Splines.Spline.Changed -= UpdateMesh;
        // }
        //
        // void OnDestroy()
        // {
        //     Debug.Log("destroy");
        //     UnityEngine.Splines.Spline.Changed -= UpdateMesh;
        // }

        public void UpdateMesh(Splines.Spline spline, int index, SplineModification mod)
        {
            Debug.Log($"update mesh");
            if(spline == m_Spline)
                Extrude3DMesh();
        }

        public void Init()
        {
            float3 tan = new float3(0f, 0f, 2f);
            float3 p1 = new float3(3f, 0f, 0f);

            Splines = new Splines.Spline[] { new Splines.Spline() };
            m_Spline.Add(new BezierKnot(float3.zero, -tan, tan, Quaternion.identity));
            m_Spline.Add(new BezierKnot(p1, p1 + tan, p1 + -tan, Quaternion.identity));

            UnityEngine.Splines.Spline.Changed += UpdateMesh;
        }

        public void Extrude3DMesh()
        {
            if (m_Spline == null) return;

            mesh.Clear();
            mesh.ToMesh();
            mesh.Refresh();

            var segmentsCount = (int)m_Spline.GetLength() * m_SegmentsPerUnit;
            m_VertexPositions = new List<Vector3>(segmentsCount * m_FaceCountPerSegment);
            m_Faces = new List<Face>(segmentsCount * m_FaceCountPerSegment);

            var t = 0f;
            var vertexIndex = 0;

            // define the positions of each segment, and the vertex positions at each segment
            for (int i = 0; i < segmentsCount + 1; i++)
            {
                SplineUtility.Evaluate(m_Spline, t, out var position, out var tangent, out var up);
                var right = Vector3.Cross(tangent, up).normalized;
                t += 1f / segmentsCount;

                // define the vertex positions
                for (int j = 0; j < m_FaceCountPerSegment; j++)
                {
                    var angleInRadians = 2 * Mathf.PI / m_FaceCountPerSegment * j;
                    var verticalPos = Mathf.Sin(angleInRadians);
                    var horizontalPos = Mathf.Cos(angleInRadians);
                    var vertexDirection = horizontalPos * right + verticalPos * (Vector3)up;
                    var vertexPosition = (Vector3)position + vertexDirection * m_Radius;
                    m_VertexPositions.Add(vertexPosition);
                }

                // define faces
                if (i > 0)
                {
                    for (int j = 0; j < m_FaceCountPerSegment; j++)
                    {
                        int[] face = new int[]
                        {
                            vertexIndex + j,
                            vertexIndex + (j + m_FaceCountPerSegment - 1) % m_FaceCountPerSegment,
                            vertexIndex + (j + m_FaceCountPerSegment - 1) % m_FaceCountPerSegment +
                            m_FaceCountPerSegment,
                            vertexIndex + j,
                            vertexIndex + (j + m_FaceCountPerSegment - 1) % m_FaceCountPerSegment +
                            m_FaceCountPerSegment,
                            vertexIndex + j + m_FaceCountPerSegment
                        };

                        m_Faces.Add(new Face(face));
                    }

                    vertexIndex += m_FaceCountPerSegment;
                }
            }

            mesh.RebuildWithPositionsAndFaces(m_VertexPositions, m_Faces);
        }

        public void Extrude2DMesh()
        {
            List<Vector3> segmentPositions = new List<Vector3>(m_SegmentsPerUnit + 1);
            int verticesAtSegment = 3;
            m_VertexPositions = new List<Vector3>(segmentPositions.Count * verticesAtSegment);
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

                m_VertexPositions.Add(pos1);
                m_VertexPositions.Add(pos2);
                m_VertexPositions.Add(pos3);

                t += 1f / m_SegmentsPerUnit;
            }

            // define faces
            for (int i = 0; i < m_VertexPositions.Count - 3; i += 3)
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

            mesh.RebuildWithPositionsAndFaces(m_VertexPositions, faces);
        }

        public void Extrude2DMeshOptimized()
        {
            int verticesAtSegment = 3;
            m_VertexPositions = new List<Vector3>(m_SegmentsPerUnit * verticesAtSegment);
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

                m_VertexPositions.Add(pos1);
                m_VertexPositions.Add(pos2);
                m_VertexPositions.Add(pos3);

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

            mesh.RebuildWithPositionsAndFaces(m_VertexPositions, faces);
        }
    }
}
