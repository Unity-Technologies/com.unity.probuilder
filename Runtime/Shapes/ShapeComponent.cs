namespace UnityEngine.ProBuilder.Shapes
{
    [RequireComponent(typeof(ProBuilderMesh))]
    public sealed class ShapeComponent : MonoBehaviour
    {
        [SerializeReference]
        Shape m_Shape = new Cube();

        ProBuilderMesh m_Mesh;

        [SerializeField]
        Vector3 m_Size;

        [SerializeField]
        Quaternion m_Rotation = Quaternion.identity;

        [SerializeField]
        Quaternion m_BasisRotation = Quaternion.identity;

        [SerializeField] bool m_Flipped = false;

        public Shape shape
        {
            get { return m_Shape; }
            set { m_Shape = value; }
        }

        public Quaternion rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }

        public Vector3 size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        /// <summary>
        /// Reference to the <see cref="ProBuilderMesh"/> that this component is creating.
        /// </summary>
        public ProBuilderMesh mesh
        {
            get { return m_Mesh == null ? m_Mesh = GetComponent<ProBuilderMesh>() : m_Mesh; }
        }

        // Bounds where center is in world space, size is mesh.bounds.size
        internal Bounds meshFilterBounds
        {
            get
            {
                var mb = mesh.mesh.bounds;
                return new Bounds(transform.TransformPoint(mb.center), mb.size);
            }
        }

        public void Rebuild(Bounds bounds, Quaternion rotation)
        {
            size = Math.Abs(bounds.size);
            transform.position = bounds.center;
            m_BasisRotation = rotation;
            Rebuild();
        }

        public void Rebuild()
        {
            m_Shape.RebuildMesh(mesh, size);
            ApplyRotation(rotation);
            MeshUtility.FitToSize(mesh, size);
        }

        public void SetShape(Shape shape)
        {
            m_Shape = shape;
            Rebuild();
        }

        /// <summary>
        /// Set the rotation of the Shape to a given quaternion, then rotates it while respecting the bounds
        /// </summary>
        /// <param name="angles">The angles to rotate by</param>
        public void SetRotation(Quaternion angles)
        {
            rotation = angles;
            ApplyRotation(rotation);
            MeshUtility.FitToSize(mesh, size);
        }

        /// <summary>
        /// Rotates the Shape by a given quaternion while respecting the bounds
        /// </summary>
        /// <param name="rotation">The angles to rotate by</param>
        public void Rotate(Quaternion rotation)
        {
            if (rotation == Quaternion.identity)
                return;
            this.rotation = rotation * this.rotation;
            ApplyRotation(this.rotation);
            MeshUtility.FitToSize(mesh, size);
        }

        void ApplyRotation(Quaternion rotation)
        {
            if (rotation == Quaternion.identity
            && m_BasisRotation == Quaternion.identity)
            {
                return;
            }
            m_Shape.RebuildMesh(mesh, size);

            var origVerts = mesh.positionsInternal;

            for (int i = 0; i < origVerts.Length; ++i)
            {
                origVerts[i] = (rotation * m_BasisRotation) * origVerts[i];
            }
            mesh.mesh.vertices = origVerts;
            mesh.ReplaceVertices(origVerts);
        }
    }
}
