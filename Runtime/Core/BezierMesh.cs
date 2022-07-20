using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Splines;

namespace UnityEngine.ProBuilder
{
    [RequireComponent(typeof(ProBuilderMesh))]
    [ExecuteInEditMode]
    public sealed class BezierMesh : MonoBehaviour, ISplineContainer
    {
        [SerializeField] private List<Splines.Spline> m_Splines;

        // TODO: support multiple splines in the container
        public IReadOnlyList<Splines.Spline> Splines
        {
            get => m_Splines;
            set
            {
                if (value == null)
                {
                    m_Splines = new List<Splines.Spline>() { new Splines.Spline() };
                    return;
                }

                // should I add to m_Spline, or clear previous content ?
                foreach (var spline in value) m_Splines.Add(spline);
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
        int m_SegmentsPerUnit = 2;

        [SerializeField] [Range(k_RadiusMin, k_RadiusMax)]
        float m_Radius = 0.5f;

        [SerializeField] [Range(k_FacesMin, k_FacesMax)]
        int m_FaceCountPerSegment = 8;

        public int SegmentsPerUnit
        {
            get => m_SegmentsPerUnit;
            set => m_SegmentsPerUnit = value;
        }

        public float Radius
        {
            get => m_Radius;
            set => m_Radius = value;
        }

        public int FaceCountPerSegment
        {
            get => m_FaceCountPerSegment;
            set => m_FaceCountPerSegment = value;
        }

        public static Action BezierMeshModified;

        void InitSpline()
        {
            float3 tan = new float3(0f, 0f, 2f);
            float3 p1 = new float3(3f, 0f, 0f);
            float3 p2 = new float3(-3f, 0f, 0f);

            m_Splines = new List<Splines.Spline>() { new Splines.Spline(), new Splines.Spline(), new Splines.Spline()};

            m_Splines[0].Add(new BezierKnot(float3.zero, -tan, tan, Quaternion.identity));
            m_Splines[0].Add(new BezierKnot(p1, p1 + tan, p1 + -tan, Quaternion.identity));

            m_Splines[1].Add(new BezierKnot(float3.zero, -tan, tan, Quaternion.identity));
            m_Splines[1].Add(new BezierKnot(p2, p2 + tan, p2 + -tan, Quaternion.identity));

            m_Splines[2].Add(new BezierKnot(float3.zero, -tan, -tan, Quaternion.identity));
            m_Splines[2].Add(new BezierKnot(-p2, -p2 + -tan, -p2 + tan, Quaternion.identity));
        }

        public void ExtrudeMesh()
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (Splines == null)
                InitSpline();

            List<Vector3> vertexPositions = new List<Vector3>();
            List<Face> faces = new List<Face>();
            var vertexIndex = 0;

            foreach (var spline in m_Splines)
            {
                if(vertexIndex > 0)
                    vertexIndex += FaceCountPerSegment;

                var t = 0f;
                var segmentsCount = (int) spline.GetLength() * m_SegmentsPerUnit;

                // define the positions of each segment, and the vertex positions at each segment
                for (int i = 0; i < segmentsCount + 1; i++)
                {
                    SplineUtility.Evaluate(spline, t, out var position, out var tangent, out var up);
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
                        vertexPositions.Add(vertexPosition);
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

                            faces.Add(new Face(face));
                        }

                        vertexIndex += m_FaceCountPerSegment;
                    }
                }
            }

            mesh.RebuildWithPositionsAndFaces(vertexPositions, faces);
            BezierMeshModified?.Invoke();

            vertexPositions.Clear();
            faces.Clear();
        }

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
            ExtrudeMesh();
        }
    }
}
