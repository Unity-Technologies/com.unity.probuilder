using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{
    [Shape("Sprite")]
    public class Sprite : Shape
    {
        public override void RebuildMesh(ProBuilderMesh mesh, Vector3 size)
        {
            if(size.y < float.Epsilon)
            {
                mesh.Clear();
                if(mesh.mesh != null)
                    mesh.mesh.Clear();
                return;
            }

            var width = size.x;
            var height = size.y;
            int w = 1;
            int h = 1;

            Vector2[] p = new Vector2[(w * h) * 4];
            Vector3[] v = new Vector3[(w * h) * 4];
            Face[] f = new Face[w * h];

            int i = 0, j = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float x0 = x * (width / w) - (width / 2f);
                    float x1 = (x + 1) * (width / w) - (width / 2f);

                    float y0 = y * (height / h) - (height / 2f);
                    float y1 = (y + 1) * (height / h) - (height / 2f);

                    p[i + 0] = new Vector2(x0, y0);
                    p[i + 1] = new Vector2(x1, y0);
                    p[i + 2] = new Vector2(x0, y1);
                    p[i + 3] = new Vector2(x1, y1);

                    f[j++] = new Face(new int[6]
                    {
                            i + 0,
                            i + 1,
                            i + 2,
                            i + 1,
                            i + 3,
                            i + 2
                    });

                    i += 4;
                }
            }

            for (i = 0; i < v.Length; i++)
                v[i] = new Vector3(p[i].y, 0f, p[i].x);

            mesh.RebuildWithPositionsAndFaces(v, f);
            mesh.SetPivot(PivotLocation.Center);
        }
    }
}
