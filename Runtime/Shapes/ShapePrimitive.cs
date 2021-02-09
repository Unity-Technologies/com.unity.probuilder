namespace UnityEngine.ProBuilder.Shapes
{
    [System.Serializable]
    public abstract class ShapePrimitive
    {
        public virtual Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
        {
            return mesh.mesh.bounds;
        }

        public abstract Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation);

        public abstract void CopyShape(ShapePrimitive shapePrimitive);
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ShapePrimitiveAttribute : System.Attribute
    {
        public string name;

        public ShapePrimitiveAttribute(string n)
        {
            name = n;
        }
    }
}
