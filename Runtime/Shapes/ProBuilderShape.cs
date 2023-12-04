using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{
    [Icon(k_IconPath)]
    [AddComponentMenu(""), DisallowMultipleComponent]
    sealed class ProBuilderShape : MonoBehaviour
    {
        const string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/EditableMesh/EditableMesh.png";

        [SerializeReference]
        Shape m_Shape = new Cube();

        [SerializeField]
        Quaternion m_ShapeRotation = Quaternion.identity;

        ProBuilderMesh m_Mesh;

        [SerializeField]
        internal ushort m_UnmodifiedMeshVersion;

        public Shape shape => m_Shape;


        [SerializeField]
        Vector3 m_Size = Vector3.one;
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

        public Quaternion shapeRotation
        {
            get => m_ShapeRotation;
            set => m_ShapeRotation = value;
        }

        public Vector3 shapeWorldCenter
        {
            get
            {
                return transform.TransformPoint(m_LocalCenter);
            }
        }

        Bounds m_EditionBounds;
        public Bounds editionBounds
        {
            get
            {
                m_EditionBounds.center = m_LocalCenter;
                m_EditionBounds.size = m_Size;
                if(Mathf.Abs(m_Size.y) < Mathf.Epsilon)
                    m_EditionBounds.size = new Vector3(m_Size.x, 0f, m_Size.z);

                return m_EditionBounds;
            }
        }

        [SerializeField]
        Vector3 m_LocalCenter;
        public Bounds shapeLocalBounds => new Bounds(m_LocalCenter, size);
        public Bounds shapeWorldBounds => new Bounds(shapeWorldCenter, size);

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

        internal void UpdateShape()
        {
            if(gameObject == null || gameObject.hideFlags == HideFlags.HideAndDontSave)
                return;

            Rebuild(mesh.transform.position, mesh.transform.rotation, new Bounds(shapeWorldCenter, size));
        }

        internal void UpdateBounds(Bounds bounds)
        {
            Rebuild(mesh.transform.position, mesh.transform.rotation, bounds);
        }

        internal void Rebuild(Vector3 pivotPosition, Quaternion rotation, Bounds bounds)
        {
            var trs = transform;
            trs.position = bounds.center;
            trs.rotation = rotation;
            size = bounds.size;
            Rebuild();
            mesh.SetPivot(pivotPosition);
            m_LocalCenter = mesh.transform.InverseTransformPoint(bounds.center);

            m_UnmodifiedMeshVersion = mesh.versionIndex;
        }

        internal void Rebuild(Bounds bounds, Quaternion rotation)
        {
            var trs = transform;
            trs.position = bounds.center;
            trs.rotation = rotation;
            size = bounds.size;
            Rebuild();

            m_UnmodifiedMeshVersion = mesh.versionIndex;
        }

        void Rebuild()
        {
            if(gameObject == null || gameObject.hideFlags == HideFlags.HideAndDontSave)
                return;

            var bbox = m_Shape.RebuildMesh(mesh, size, shapeRotation);
            bbox.size = Math.Abs(bbox.size);
            MeshUtility.FitToSize(mesh, bbox, size);
        }

        internal void SetShape(Shape shape)
        {
            m_Shape = shape;
            if(m_Shape is Plane || m_Shape is Sprite)
            {
                Bounds bounds = new Bounds(m_LocalCenter, size);
                var newCenter = bounds.center;
                var newSize = bounds.size;
                newCenter.y = 0;
                newSize.y = 0;
                m_LocalCenter = newCenter;
                size = newSize;
                m_Size.y = 0;
            }
            UpdateShape();

            m_UnmodifiedMeshVersion = mesh.versionIndex;
        }

        /// <summary>
        /// Rotates the Shape by a given quaternion while respecting the bounds
        /// </summary>
        internal void RotateInsideBounds(Quaternion deltaRotation)
        {
            shapeRotation = deltaRotation * shapeRotation;
            var bounds = new Bounds(mesh.transform.TransformPoint(m_LocalCenter), size);
            Rebuild(mesh.transform.position, mesh.transform.rotation , bounds);
        }
    }
}
