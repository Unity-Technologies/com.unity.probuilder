using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{
    [AddComponentMenu(""), DisallowMultipleComponent]
    sealed class ProBuilderShape : MonoBehaviour
    {
        [SerializeReference]
        Shape m_Shape = new Cube();

        [SerializeField]
        Vector3 m_Size = Vector3.one;

        [SerializeField]
        Quaternion m_Rotation = Quaternion.identity;

        ProBuilderMesh m_Mesh;

        [SerializeField]
        internal ushort m_UnmodifiedMeshVersion;

        public Shape shape => m_Shape;

        public Vector3 size
        {
            get => m_Size;
            set
            {
                m_Size.x = System.Math.Abs(value.x) == 0 ? Mathf.Sign(m_Size.x) * 0.001f: value.x;
                m_Size.y = value.y;
                m_Size.z = System.Math.Abs(value.z) == 0 ? Mathf.Sign(m_Size.z) * 0.001f: value.z;
            }
        }

        public Quaternion rotation
        {
            get => m_Rotation;
            set => m_Rotation = value;
        }

        Bounds m_EditionBounds;
        public Bounds editionBounds
        {
            get
            {
                m_EditionBounds.center = m_ShapeBox.center;
                m_EditionBounds.size = m_Size;
                if(Mathf.Abs(m_ShapeBox.size.y) < Mathf.Epsilon)
                    m_EditionBounds.size = new Vector3(m_Size.x, 0f, m_Size.z);

                return m_EditionBounds;
            }
        }

        [SerializeField]
        Bounds m_ShapeBox;
        public Bounds shapeBox => m_ShapeBox;

        public bool isEditable => m_UnmodifiedMeshVersion == mesh.versionIndex;

        /// <summary>
        /// Reference to the <see cref="ProBuilderMesh"/> that this component is creating.
        /// </summary>
        public ProBuilderMesh mesh
        {
            get
            {
                if(m_Mesh == null)
                    m_Mesh = GetComponent<ProBuilderMesh>();

                if(m_Mesh == null)
                    m_Mesh = gameObject.AddComponent<ProBuilderMesh>();

                return m_Mesh;
            }
        }

        void OnValidate()
        {
            //Ensure the size in X and Z is not set to 0 otherwise PhysX
            //is throwing errors as it cannot create a collider
            m_Size.x = System.Math.Abs(m_Size.x) == 0 ? 0.001f: m_Size.x;
            m_Size.z = System.Math.Abs(m_Size.z) == 0 ? 0.001f: m_Size.z;
        }

        internal void UpdateComponent() => Rebuild();

        internal void UpdateBounds(Bounds bounds)
        {
            var centerLocalPos = mesh.transform.InverseTransformPoint(bounds.center);
            Bounds shapeBB = m_ShapeBox;
            shapeBB.center = centerLocalPos;
            m_ShapeBox = shapeBB;

            size = bounds.size;
            Rebuild();
        }

        internal void Rebuild(Bounds bounds, Quaternion rotation)
        {
            var trs = transform;
            trs.position = bounds.center;
            trs.rotation = rotation;
            size = bounds.size;
            Rebuild();
        }

        void Rebuild()
        {
            if(gameObject == null || gameObject.hideFlags == HideFlags.HideAndDontSave)
                return;

            m_ShapeBox = m_Shape.RebuildMesh(mesh, size, rotation);

            Bounds bounds = m_ShapeBox;
            bounds.size = Math.Abs(m_ShapeBox.size);
            MeshUtility.FitToSize(mesh, bounds, size);

            m_UnmodifiedMeshVersion = mesh.versionIndex;
        }

        internal void SetShape(Shape shape)
        {
            m_Shape = shape;
            if(m_Shape is Plane || m_Shape is Sprite)
            {
                Bounds bounds = m_ShapeBox;
                var newCenter = bounds.center;
                var newSize = bounds.size;
                newCenter.y = 0;
                newSize.y = 0;
                bounds.center = newCenter;
                bounds.size = newSize;
                m_ShapeBox = bounds;
                m_Size.y = 0;
            }
            Rebuild();
        }

        /// <summary>
        /// Rotates the Shape by a given quaternion while respecting the bounds
        /// </summary>
        internal void RotateInsideBounds(Quaternion deltaRotation)
        {
            rotation = deltaRotation * rotation;
            Rebuild();
        }
    }
}
