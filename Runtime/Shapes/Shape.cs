namespace UnityEngine.ProBuilder.Shapes
{
    [System.Serializable]
    public abstract class Shape
    {
        public virtual Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
        {
            return mesh.mesh.bounds;
        }

        public abstract Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation);

        public abstract void CopyShape(Shape shape);
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
