using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{
    [Icon(k_IconPath)]
    [AddComponentMenu(""), DisallowMultipleComponent]
    [HelpURL(k_HelpUrl)]
    sealed class ProBuilderShape : MonoBehaviour
    {
        const string k_HelpUrl = "https://docs.unity3d.com/Packages/com.unity.probuilder@latest";
        const string k_IconPath = "Packages/com.unity.probuilder/Editor Default Resources/Icons/EditableMesh/EditableMesh.png";

        const float k_MinHeight = 0.0001f;

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
                if(Mathf.Abs(m_Size.y) < k_MinHeight)
                    m_EditionBounds.size = new Vector3(m_Size.x, 0f, m_Size.z);

                return m_EditionBounds;
            }
        }

        [SerializeField]
        Vector3 m_LocalCenter;

        // Per axis, how far the pivot sits from the center as a fraction of the half-size: 0 for a
        // Center pivot, 1 for a First Vertex pivot. Captured whenever the shape is fully rebuilt so
        // that resizing (which only knows the new size, not what it was built with) can re-derive the
        // center from the current size instead of keeping the previous size's stale center fixed.
        [SerializeField]
        Vector3 m_PivotRatio;

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

            var newLocalCenter = Vector3.Scale(m_PivotRatio, size * 0.5f);
            var newWorldCenter = mesh.transform.TransformPoint(newLocalCenter);

            Rebuild(mesh.transform.position, mesh.transform.rotation, new Bounds(newWorldCenter, size));
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
            m_PivotRatio = new Vector3(
                RatioOrZero(m_LocalCenter.x, bounds.size.x),
                RatioOrZero(m_LocalCenter.y, bounds.size.y),
                RatioOrZero(m_LocalCenter.z, bounds.size.z));

            m_UnmodifiedMeshVersion = mesh.versionIndex;
        }

        static float RatioOrZero(float localCenterComponent, float sizeComponent)
        {
            if (Mathf.Abs(sizeComponent) < 0.0001f)
                return 0f;
            return Mathf.Clamp(localCenterComponent / (sizeComponent * 0.5f), -1f, 1f);
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
            bool wasFlat = m_Shape is Plane || m_Shape is Sprite;
            bool isFlat = shape is Plane || shape is Sprite;

            m_Shape = shape;

            if(isFlat)
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
            else if(wasFlat && !isFlat)
            {
                // Transitioning FROM a 2D shape TO a 3D shape - restore Y dimension
                if(Mathf.Abs(m_Size.y) < k_MinHeight)
                    m_Size.y = 1f;
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
