using UnityEditor;

namespace UnityEngine.ProBuilder.Shapes
{
    /// <summary>
    /// Represents a basic [sprite](../manual/Sprite.html) (a single-unit plane shape).
    /// </summary>
    [Shape("Sprite")]
    public class Sprite : Shape
    {
        /// <inheritdoc/>
        public override void CopyShape(Shape shape) {}

        /// <inheritdoc/>
        public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            var meshSize = Math.Abs(size);

            if(meshSize.x < float.Epsilon || meshSize.z < float.Epsilon)
            {
                mesh.Clear();
                if(mesh.mesh != null)
                    mesh.mesh.Clear();
                return new Bounds();
            }

            var width = meshSize.x;
            var height = meshSize.z;

            Vector2[] p = new Vector2[4];
            Vector3[] v = new Vector3[4];
            Face[] f = new Face[1];

            float x0 = -(width / 2f);
            float x1 = (width / 2f);

            float y0 = -(height / 2f);
            float y1 = (height / 2f);

            p[0] = new Vector2(x0, y0);
            p[1] = new Vector2(x1, y0);
            p[2] = new Vector2(x0, y1);
            p[3] = new Vector2(x1, y1);

            f[0] = new Face(new int[6]
            {
                    0,
                    1,
                    2,
                    1,
                    3,
                    2
            });

            for (int i = 0; i < v.Length; i++)
                v[i] = new Vector3(p[i].y, 0, p[i].x);

            mesh.RebuildWithPositionsAndFaces(v, f);

            return mesh.mesh.bounds;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Sprite))]
    public class SpriteDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }
    }
#endif
}
