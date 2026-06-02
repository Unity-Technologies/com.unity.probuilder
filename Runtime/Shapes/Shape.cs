namespace UnityEngine.ProBuilder.Shapes
{
    /// <summary>
    /// Base class for all Shape types that represent a primitive [shape](../manual/shape-tool.html).
    /// </summary>
    [System.Serializable]
    public abstract class Shape
    {
        /// <summary>
        /// Allows the user to redefine the default bounding box for this shape.
        /// </summary>
        /// <param name="mesh">The mesh to find the bounds for.</param>
        /// <param name="size">The desired size for the shape defined when using the [Shape Tool](../manual/shape-tool.html).</param>
        /// <param name="rotation">The rotation (orientation) to use for this mesh.</param>
        /// <param name="bounds">The default bounds computed for the shape.</param>
        /// <returns>The bounds from this shape's <see cref="Mesh.bounds" /> property.</returns>
        public virtual Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
        {
            return mesh.mesh.bounds;
        }

        /// <summary>
        /// Rebuilds the specified mesh using the existing property values for this shape. This includes
        /// building a list of vertices and normals for each face, applying smoothing to the faces if
        /// required, and calculating the bounds of the mesh.
        /// </summary>
        /// <param name="mesh">The mesh to rebuild.</param>
        /// <param name="size">The position of the opposite corner of the bounding box for this shape.</param>
        /// <param name="rotation">The rotation (orientation) to use for this mesh.</param>
        /// <returns>The bounds calculated for this shape after rebuilding it.</returns>
        public abstract Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation);

        /// <summary>
        /// Overwrites this shape's property values by copying them from the specified Shape object.
        /// </summary>
        /// <param name="shape">The <see cref="Shape" /> to copy property values from.</param>
        public abstract void CopyShape(Shape shape);

        internal abstract void SetParametersToBuiltInShape();
    }

    /// <summary>
    /// Represents an attribute for a Shape type.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ShapeAttribute : System.Attribute
    {
        /// <summary>Name of the attribute</summary>
        public string name;

        /// <summary>
        /// Creates a ShapeAttribute with the specified name.
        /// </summary>
        /// <param name="n">The name of the new ShapeAttribute.</param>
        public ShapeAttribute(string n)
        {
            name = n;
        }
    }
}
