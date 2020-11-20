namespace UnityEngine.ProBuilder.Shapes
{
    [System.Serializable]
    public abstract class Shape
    {
        [SerializeField]
        protected Bounds m_ShapeBox = new Bounds();

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
