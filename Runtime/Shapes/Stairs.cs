namespace UnityEngine.ProBuilder
{
    public class Stairs : Shape
    {
        [SerializeField]
        int m_Steps = 10;

        [SerializeField]
        bool m_Sides = true;

        public int steps
        {
            get { return m_Steps; }
            set { m_Steps = value; }
        }

        public bool sides
        {
            get { return m_Sides; }
            set { m_Sides = value; }
        }

        public float stepHeight
        {
            get { return (1f / m_Steps) * size.y; }
        }

        protected override void RebuildMesh()
        {
            // 4 vertices per quad, 2 quads per step.
            Vector3[] vertices = new Vector3[4 * steps * 2];
            Face[] faces = new Face[steps * 2];
            Vector3 extents = size * .5f;

            // vertex index, face index
            int v = 0, t = 0;

            for (int i = 0; i < steps; i++)
            {
                float inc0 = i / (float)steps;
                float inc1 = (i + 1) / (float)steps;

                float x0 = size.x - extents.x;
                float x1 = 0 - extents.x;
                float y0 = size.y * inc0 - extents.y;
                float y1 = size.y * inc1 - extents.y;
                float z0 = size.z * inc0 - extents.z;
                float z1 = size.z * inc1 - extents.z;

                vertices[v + 0] = new Vector3(x0, y0, z0);
                vertices[v + 1] = new Vector3(x1, y0, z0);
                vertices[v + 2] = new Vector3(x0, y1, z0);
                vertices[v + 3] = new Vector3(x1, y1, z0);

                vertices[v + 4] = new Vector3(x0, y1, z0);
                vertices[v + 5] = new Vector3(x1, y1, z0);
                vertices[v + 6] = new Vector3(x0, y1, z1);
                vertices[v + 7] = new Vector3(x1, y1, z1);

                faces[t + 0] = new Face(new int[] {  v + 0,
                                                     v + 1,
                                                     v + 2,
                                                     v + 1,
                                                     v + 3,
                                                     v + 2 });

                faces[t + 1] = new Face(new int[] {  v + 4,
                                                     v + 5,
                                                     v + 6,
                                                     v + 5,
                                                     v + 7,
                                                     v + 6 });

                v += 8;
                t += 2;
            }

            // sides
            if (sides)
            {
                // first step is special case - only needs a quad, but all other steps need
                // a quad and tri.
                float x = 0f;

                for (int side = 0; side < 2; side++)
                {
                    Vector3[] sides_v = new Vector3[steps * 4 + (steps - 1) * 3];
                    Face[] sides_f = new Face[steps + steps - 1];

                    int sv = 0, st = 0;

                    for (int i = 0; i < steps; i++)
                    {
                        float y0 = (Mathf.Max(i, 1) / (float)steps) * size.y;
                        float y1 = ((i + 1) / (float)steps) * size.y;

                        float z0 = (i / (float)steps) * size.z;
                        float z1 = ((i + 1) / (float)steps) * size.z;

                        sides_v[sv + 0] = new Vector3(x, 0f, z0) - extents;
                        sides_v[sv + 1] = new Vector3(x, 0f, z1) - extents;
                        sides_v[sv + 2] = new Vector3(x, y0, z0) - extents;
                        sides_v[sv + 3] = new Vector3(x, y1, z1) - extents;

                        sides_f[st++] = new Face(side % 2 == 0 ?
                                new int[] { v + 0, v + 1, v + 2, v + 1, v + 3, v + 2 } :
                                new int[] { v + 2, v + 1, v + 0, v + 2, v + 3, v + 1 });

                        sides_f[st - 1].textureGroup = side + 1;

                        v += 4;
                        sv += 4;

                        // that connecting triangle
                        if (i > 0)
                        {
                            sides_v[sv + 0] = new Vector3(x, y0, z0) - extents;
                            sides_v[sv + 1] = new Vector3(x, y1, z0) - extents;
                            sides_v[sv + 2] = new Vector3(x, y1, z1) - extents;

                            sides_f[st++] = new Face(side % 2 == 0 ?
                                    new int[] { v + 2, v + 1, v + 0 } :
                                    new int[] { v + 0, v + 1, v + 2 });

                            sides_f[st - 1].textureGroup = side + 1;

                            v += 3;
                            sv += 3;
                        }
                    }

                    vertices = vertices.Concat(sides_v);
                    faces = faces.Concat(sides_f);

                    x += size.x;
                }

                // add that last back face
                vertices = vertices.Concat(new Vector3[] {
                    new Vector3(0f, 0f, size.z) - extents,
                    new Vector3(size.x, 0f, size.z) - extents,
                    new Vector3(0f, size.y, size.z) - extents,
                    new Vector3(size.x, size.y, size.z) - extents
                });

                faces = faces.Add(new Face(new int[] {v + 0, v + 1, v + 2, v + 1, v + 3, v + 2}));
            }

            mesh.RebuildWithPositionsAndFaces(vertices, faces);
        }
    }
}
