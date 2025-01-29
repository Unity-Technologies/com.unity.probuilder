using UnityEditor;

namespace UnityEngine.ProBuilder.Shapes
{
    /// <summary>
    /// Represents a basic [prism](../manual/Prism.html) shape.
    /// </summary>
    [Shape("Prism")]
    public class Prism : Shape
    {
        internal override void SetParametersToBuiltInShape() { }
        
        /// <inheritdoc/>
        public override void CopyShape(Shape shape) {}

        /// <inheritdoc/>
        public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            var meshSize = Math.Abs(size);
            meshSize.y = meshSize.y == 0 ? 2f * Mathf.Epsilon : meshSize.y;
            var baseY = new Vector3(0, meshSize.y / 2f, 0);
            meshSize.y *= 2f;

            Vector3[] template = new Vector3[6]
            {
                Vector3.Scale(new Vector3(-.5f, 0f, -.5f),  meshSize) - baseY,
                Vector3.Scale(new Vector3(.5f, 0f, -.5f),   meshSize) - baseY,
                Vector3.Scale(new Vector3(0f, .5f, -.5f),   meshSize) - baseY,
                Vector3.Scale(new Vector3(-.5f, 0f, .5f),   meshSize) - baseY,
                Vector3.Scale(new Vector3(0.5f, 0f, .5f),   meshSize) - baseY,
                Vector3.Scale(new Vector3(0f, .5f, .5f),    meshSize) - baseY
            };

            Vector3[] v = new Vector3[18]
            {
                template[0],    // 0    front
                template[1],    // 1
                template[2],    // 2

                template[1],    // 3    right side
                template[4],    // 4
                template[2],    // 5
                template[5],    // 6

                template[4],    // 7    back side
                template[3],    // 8
                template[5],    // 9

                template[3],    // 10   left side
                template[0],    // 11
                template[5],    // 12
                template[2],    // 13

                template[0],    // 14   // bottom
                template[1],    // 15
                template[3],    // 16
                template[4]     // 17
            };

            Face[] f = new Face[5]
            {
                new Face(new int[3] {2, 1, 0}),          // x
                new Face(new int[6] {5, 4, 3, 5, 6, 4}), // x
                new Face(new int[3] {9, 8, 7}),
                new Face(new int[6] {12, 11, 10, 12, 13, 11}),
                new Face(new int[6] {14, 15, 16, 15, 17, 16})
            };

            var sizeSigns = Math.Sign(size);
            for(int i = 0; i < v.Length; i++)
                v[i] = Vector3.Scale(rotation * v[i], sizeSigns);

            var sizeSign = Mathf.Sign(size.x) * Mathf.Sign(size.y) * Mathf.Sign(size.z);
            if(sizeSign < 0)
            {
                foreach(var face in f)
                    face.Reverse();
            }

            mesh.RebuildWithPositionsAndFaces(v, f);

            return mesh.mesh.bounds;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Prism))]
    public class PrismDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }
    }
#endif
}
