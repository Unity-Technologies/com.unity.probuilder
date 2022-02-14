using UnityEditor;

namespace UnityEngine.ProBuilder.Shapes
{
    /// <summary>
    /// Represents a basic [cube](../manual/Cube.html) shape.
    /// </summary>
    [Shape("Cube")]
    public class Cube : Shape
    {
        /// <summary>
        /// Defines a set of 8 vertices that forms the template for a cube mesh.
        /// </summary>
        static readonly Vector3[] k_CubeVertices = new Vector3[]
        {
            // bottom 4 verts
            new Vector3(-.5f, -.5f, .5f),       // 0
            new Vector3(.5f, -.5f, .5f),        // 1
            new Vector3(.5f, -.5f, -.5f),       // 2
            new Vector3(-.5f, -.5f, -.5f),      // 3

            // top 4 verts
            new Vector3(-.5f, .5f, .5f),        // 4
            new Vector3(.5f, .5f, .5f),         // 5
            new Vector3(.5f, .5f, -.5f),        // 6
            new Vector3(-.5f, .5f, -.5f)        // 7
        };

        /// <summary>
        /// Defines a set of triangles forming a cube with reference to the k_CubeVertices array.
        /// </summary>
        static readonly int[] k_CubeTriangles = new int[] {
            0, 1, 4, 5, 1, 2, 5, 6, 2, 3, 6, 7, 3, 0, 7, 4, 4, 5, 7, 6, 3, 2, 0, 1
        };

        /// <inheritdoc/>
        public override void CopyShape(Shape shape) {}

        /// <inheritdoc/>
        public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            mesh.Clear();

            Vector3[] points = new Vector3[k_CubeTriangles.Length];

            for (int i = 0; i < k_CubeTriangles.Length; i++)
                points[i] = rotation * Vector3.Scale(k_CubeVertices[k_CubeTriangles[i]], Math.Abs(size));

            mesh.GeometryWithPoints(points);

            return mesh.mesh.bounds;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Cube))]
    public class CubeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }
    }
#endif
}
