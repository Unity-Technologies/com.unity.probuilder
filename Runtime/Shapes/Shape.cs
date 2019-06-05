namespace UnityEngine.ProBuilder
{
    [RequireComponent(typeof(ProBuilderMesh))]
    public abstract class Shape : MonoBehaviour
    {
        [SerializeField]
        ProBuilderMesh m_Mesh;

        [SerializeField]
        Vector3 m_Size;

        public Vector3 size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        public ProBuilderMesh mesh
        {
            get { return m_Mesh == null ? m_Mesh = GetComponent<ProBuilderMesh>() : m_Mesh; }
        }

        public abstract void Rebuild();
    }
}
