namespace UnityEngine.ProBuilder.Shapes
{
    [System.Serializable]
    public abstract class Shape
    {
        [SerializeField]
        Vector3 m_Size = Vector3.one;

        [SerializeField]
        Quaternion m_Rotation = Quaternion.identity;

        [SerializeField]
        protected Bounds m_ShapeBox = new Bounds();

        public Vector3 size
        {
            get => m_Size;
            set => m_Size = value;
        }

        public Quaternion rotation
        {
            get => m_Rotation;
            set => m_Rotation = value;
        }

        public Bounds shapeBox
        {
            get => m_ShapeBox;
            set => m_ShapeBox = value;
        }

        public abstract void RebuildMesh(ProBuilderMesh mesh, Vector3 size);
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ShapeAttribute : System.Attribute
    {
        public string name;

        public ShapeAttribute(string n)
        {
            name = n;
        }
    }
}
