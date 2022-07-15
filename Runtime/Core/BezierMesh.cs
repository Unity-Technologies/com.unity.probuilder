using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Splines;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.ProBuilder
{
    [RequireComponent(typeof(ProBuilderMesh))]
    [ExecuteInEditMode]
    public sealed class BezierMesh : MonoBehaviour, ISplineContainer
    {
        [SerializeField] private Splines.Spline m_Spline;

        // support multiple splines in the container
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

        public const float k_RadiusMin = 0.01f;
        public const float k_RadiusMax = 128f;
        public const int k_FacesMin = 3;
        public const int k_FacesMax = 32;
        public const int k_SegmentsMin = 1;
        public const int k_SegmentsMax = 32;

        [SerializeField] [Range(k_SegmentsMin, k_SegmentsMax)]
        public int m_SegmentsPerUnit = 2;

        [SerializeField] [Range(k_RadiusMin, k_RadiusMax)]
        public float m_Radius = 0.5f;

        [SerializeField] [Range(k_FacesMin, k_FacesMax)]
        public int m_FaceCountPerSegment = 8;

        public static Action BezierMeshModified;

        List<Vector3> m_VertexPositions = new List<Vector3>();

        List<Face> m_Faces = new List<Face>();

        void OnValidate()
        {
            ExtrudeMesh();
        }

        void OnEnable()
        {
            UnityEngine.Splines.Spline.Changed += UpdateMesh;
        }

        void OnDisable()
        {
            UnityEngine.Splines.Spline.Changed -= UpdateMesh;
        }

        public void UpdateMesh(Splines.Spline spline, int index, SplineModification mod)
        {
            if (spline == m_Spline)
            {
                ExtrudeMesh();
            }
        }

        public void Init()
        {
            float3 tan = new float3(0f, 0f, 2f);
            float3 p1 = new float3(3f, 0f, 0f);

            Splines = new Splines.Spline[] { new Splines.Spline() };
            m_Spline.Add(new BezierKnot(float3.zero, -tan, tan, Quaternion.identity));
            m_Spline.Add(new BezierKnot(p1, p1 + tan, p1 + -tan, Quaternion.identity));
        }

        // move the undo, dont mix editor side stuff in runtime
        // use properties instead of fields
        // add documentation
        public void SetParameters(float radius, int faceCount, int segmentCount)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Bezier Mesh Creation");
#endif
            m_FaceCountPerSegment = faceCount;
            m_SegmentsPerUnit = segmentCount;
            m_Radius = radius;

            ExtrudeMesh();
        }

        public void ExtrudeMesh()
        {
            if (m_Spline == null) return;

            m_VertexPositions.Clear();
            m_Faces.Clear();

            var t = 0f;
            var vertexIndex = 0;
            var segmentsCount = (int) m_Spline.GetLength() * m_SegmentsPerUnit;

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
                            vertexIndex + (j + m_FaceCountPerSegment - 1) % m_FaceCountPerSegment + m_FaceCountPerSegment,
                            vertexIndex + j,
                            vertexIndex + (j + m_FaceCountPerSegment - 1) % m_FaceCountPerSegment + m_FaceCountPerSegment,
                            vertexIndex + j + m_FaceCountPerSegment
                        };

                        m_Faces.Add(new Face(face));
                    }

                    vertexIndex += m_FaceCountPerSegment;
                }
            }

            mesh.RebuildWithPositionsAndFaces(m_VertexPositions, m_Faces);
            BezierMeshModified?.Invoke();
        }
    }
}
