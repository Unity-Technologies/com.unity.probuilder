using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Splines;

namespace UnityEngine.ProBuilder
{
    [RequireComponent(typeof(ProBuilderMesh))]
    [RequireComponent(typeof(SplineContainer))]
    [ExecuteInEditMode]
    sealed class BezierMesh : MonoBehaviour
    {
        SplineContainer m_SplineContainer;
        public SplineContainer splineContainer
        {
            get => m_SplineContainer;
            set
            {
                if (value == null)
                {
                    m_SplineContainer = GetComponent<SplineContainer>();
                    return;
                }

                m_SplineContainer = value;
            }
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

        public int segmentsPerUnit
        {
            get => m_SegmentsPerUnit;
            set => m_SegmentsPerUnit = Math.Clamp(value, k_SegmentsMin, k_SegmentsMax);
        }

        public float radius
        {
            get => m_Radius;
            set => m_Radius = Mathf.Clamp(value, k_RadiusMin, k_RadiusMax);
        }

        public int faceCountPerSegment
        {
            get => m_FaceCountPerSegment;
            set => m_FaceCountPerSegment = Mathf.Clamp(value, k_FacesMin, k_FacesMax);
        }

        public static Action BezierMeshModified;

        /// <summary>
        /// Extrudes a <see cref="ProBuilderMesh"/> along all the splines in the Bezier Meshes' <see cref="SplineContainer"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void ExtrudeMesh()
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            if (splineContainer == null)
                splineContainer = GetComponent<SplineContainer>();

            List<Vector3> vertexPositions = new List<Vector3>();
            List<Face> faces = new List<Face>();
            var vertexIndex = 0;

            foreach (var spline in splineContainer.Splines)
            {
                if (spline.Knots.Count() < 2)
                    continue;

                if (vertexIndex > 0)
                    vertexIndex += faceCountPerSegment;

                var t = 0f;
                var segmentsCount = (int)spline.GetLength() * m_SegmentsPerUnit;

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
            Splines.Spline.Changed += UpdateMesh;
        }

        void OnDisable()
        {
            Splines.Spline.Changed -= UpdateMesh;
        }

        public void UpdateMesh(Splines.Spline spline, int index, SplineModification mod)
        {
            ExtrudeMesh();
        }
    }
}
