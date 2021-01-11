using UnityEditor;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{
    [Shape("Sprite")]
    public class Sprite : Shape
    {
        public override void UpdatePivot(ProBuilderMesh mesh, PivotLocation pivotLocation)
        {
            mesh.SetPivot(PivotLocation.Center);
        }

        public override void RebuildMesh(ProBuilderMesh mesh, Vector3 meshSize, Quaternion rotation)
        {
            meshSize = Math.Abs(meshSize);

            if(meshSize.x < float.Epsilon || meshSize.z < float.Epsilon)
            {
                mesh.Clear();
                if(mesh.mesh != null)
                    mesh.mesh.Clear();
                return;
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
                v[i] = rotation * new Vector3(p[i].y, 0, p[i].x);

            mesh.RebuildWithPositionsAndFaces(v, f);

            m_ShapeBox = mesh.mesh.bounds;
        }
    }

    [CustomPropertyDrawer(typeof(Sprite))]
    public class SpriteDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }
    }

}
