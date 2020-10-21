namespace UnityEngine.ProBuilder.Shapes
{
    public class Plane : Shape
    {
        [Min(0)]
        [SerializeField]
        int m_HeightSegments = 1;

        [Min(0)]
        [SerializeField]
        int m_WidthSegments = 1;

        [SerializeField]
        Axis m_Axis = Axis.Up;

        public override void RebuildMesh(ProBuilderMesh mesh, Vector3 size)
        {
            if(size.y < float.Epsilon && m_Axis != Axis.Up && m_Axis != Axis.Down)
            {
                mesh.Clear();
                if(mesh.mesh != null)
                    mesh.mesh.Clear();
                return;
            }

            int w = m_WidthSegments + 1;
            int h = m_HeightSegments + 1;

            Vector2[] p = new Vector2[(w * h) * 4];
            Vector3[] v = new Vector3[(w * h) * 4];

            float width = 1f, height = 1f;
            int i = 0;
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float x0 = x * (width / w) - (width / 2f);
                        float x1 = (x + 1) * (width / w) - (width / 2f);

                        float y0 = y * (height / h) - (height / 2f);
                        float y1 = (y + 1) * (height / h) - (height / 2f);

                        p[i + 0] = new Vector2(x0,    y0);
                        p[i + 1] = new Vector2(x1,    y0);
                        p[i + 2] = new Vector2(x0,    y1);
                        p[i + 3] = new Vector2(x1,    y1);

                        i += 4;
                    }
                }
            }

            // Axis axis = m_Axis;
            // if(size.y < float.Epsilon && axis != Axis.Up && axis != Axis.Down)
            //     axis = Axis.Up;

            switch (m_Axis)
            {
                case Axis.Right:
                    for (i = 0; i < v.Length; i++)
                        v[i] = new Vector3(0f, p[i].x, p[i].y);
                    break;
                case Axis.Left:
                    for (i = 0; i < v.Length; i++)
                        v[i] = new Vector3(0f, p[i].y, p[i].x);
                    break;
                case Axis.Up:
                    for (i = 0; i < v.Length; i++)
                        v[i] = new Vector3(p[i].y, 0f, p[i].x);
                    break;
                case Axis.Down:
                    for (i = 0; i < v.Length; i++)
                        v[i] = new Vector3(p[i].x, 0f, p[i].y);
                    break;
                case Axis.Forward:
                    for (i = 0; i < v.Length; i++)
                        v[i] = new Vector3(p[i].x, p[i].y, 0f);
                    break;
                case Axis.Backward:
                    for (i = 0; i < v.Length; i++)
                        v[i] = new Vector3(p[i].y, p[i].x, 0f);
                    break;
            }

            mesh.GeometryWithPoints(v);
        }
    }
}

