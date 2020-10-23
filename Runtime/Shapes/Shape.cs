namespace UnityEngine.ProBuilder.Shapes
{
    [System.Serializable]
    public abstract class Shape
    {
        protected Vector3 m_Forward;

        public Vector3 Forward
        {
            get => m_Forward;
            set => m_Forward = value;
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
