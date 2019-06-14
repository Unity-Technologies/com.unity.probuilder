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
            Rebuild();
        }

        public void Rebuild()
        {
            RebuildMesh();
        }

        public abstract void RebuildMesh();
    }
}
