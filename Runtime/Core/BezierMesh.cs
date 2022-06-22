using System.Collections.Generic;
using UnityEngine.Splines;

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

        public ProBuilderMesh Mesh
        {
            get
            {
                if (m_mesh == null)
                    m_mesh = GetComponent<ProBuilderMesh>();

                return m_mesh;
            }

            set { m_mesh = value; }
        }

        [SerializeField] private float m_radius; // min 0.01f

        [SerializeField] private int m_segmentCount; // min 2

        [SerializeField] private int m_positionCount; // min 3

        public void Init()
        {
            var tan = new Vector3(0f, 0f, 2f);
            var p1 = new Vector3(3f, 0f, 0f);
            var zero = Vector3.zero;

            // m_spline.Add(new BezierKnot(zero, -tan, tan, Quaternion.identity));
            // m_spline.Add(new BezierKnot(p1, p1 + tan, p1 + -tan, Quaternion.identity));
        }

        public void ExtrudeMesh()
        {

        }
    }
}
