namespace UnityEngine.ProBuilder
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(ProBuilderMesh))]
    public abstract class Shape : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        Vector3 m_Size;

        ProBuilderMesh m_Mesh;

        Transform m_Transform;

        public Vector3 size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        // Bounds where center is in world space, size is mesh.bounds.size
        internal Bounds bounds
        {
            get
            {
                var mb = mesh.mesh.bounds;
                return new Bounds(transform.TransformPoint(mb.center), mb.size);
            }
        }

        public ProBuilderMesh mesh
        {
            get { return m_Mesh == null ? m_Mesh = GetComponent<ProBuilderMesh>() : m_Mesh; }
        }

        public new Transform transform
        {
            get { return m_Transform == null ? m_Transform = GetComponent<Transform>() : m_Transform; }
        }

        public void Rebuild(Bounds bounds, Quaternion rotation)
        {
            size = Math.Abs(bounds.size);
            transform.position = bounds.center;
            transform.rotation = rotation;
            RebuildMesh();
        }

        public void Rebuild()
        {
            RebuildMesh();
        }

        protected abstract void RebuildMesh();
    }
}
