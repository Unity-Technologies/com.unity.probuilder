namespace UnityEngine.ProBuilder.Shapes
{
    [Shape("Stairs")]
    public class Stairs : Shape
    {
        [Min(1)]
        [SerializeField]
        int m_Steps = 10;

        [Range(0, 360)]
        [SerializeField]
        float m_Circumference = 0f;

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

        public override void RebuildMesh(ProBuilderMesh mesh, Vector3 size)
        {
            if (m_Circumference > 0)
                BuildCurvedStairs(mesh, size);
            else
                BuildStairs(mesh, size);
        }

        private void BuildStairs(ProBuilderMesh mesh, Vector3 size)
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

                faces = faces.Add(new Face(new int[] { v + 0, v + 1, v + 2, v + 1, v + 3, v + 2 }));
            }

            // if(m_Forward.x < 0)
            // {
            //     for(int i = 0; i < vertices.Length; i++)
            //         vertices[i] = new Vector3(-vertices[i].x, vertices[i].y, -vertices[i].z);
            // }
            // else if(m_Forward.z < 0)
            // {
            //     for(int i = 0; i < vertices.Length; i++)
            //         vertices[i] = new Vector3(vertices[i].z, vertices[i].y, -vertices[i].x);
            // }
            // else if(m_Forward.z > 0)
            // {
                for(int i = 0; i < vertices.Length; i++)
                    vertices[i] = new Vector3(-vertices[i].z, vertices[i].y, vertices[i].x);
            //}

            mesh.RebuildWithPositionsAndFaces(vertices, faces);
        }

        private void BuildCurvedStairs(ProBuilderMesh mesh, Vector3 size)
        {
            var buildSides = m_Sides;
            var innerRadius = size.z;
            var stairWidth = size.x;
            var height = size.y;
            var circumference = m_Circumference;
            bool noInnerSide = innerRadius < Mathf.Epsilon;

            // 4 vertices per quad, vertical step first, then floor step can be 3 or 4 verts depending on
            // if the inner radius is 0 or not.
            Vector3[] positions = new Vector3[(4 * steps) + ((noInnerSide ? 3 : 4) * steps)];
            Face[] faces = new Face[steps * 2];

            // vertex index, face index
            int v = 0, t = 0;

            float cir = Mathf.Abs(circumference) * Mathf.Deg2Rad;
            float outerRadius = innerRadius + stairWidth;

            for (int i = 0; i < steps; i++)
            {
                float inc0 = (i / (float)steps) * cir;
                float inc1 = ((i + 1) / (float)steps) * cir;

                float h0 = ((i / (float)steps) * height);
                float h1 = (((i + 1) / (float)steps) * height);

                Vector3 v0 = new Vector3(-Mathf.Cos(inc0), 0f, Mathf.Sin(inc0));
                Vector3 v1 = new Vector3(-Mathf.Cos(inc1), 0f, Mathf.Sin(inc1));

                /**
                 *
                 *      /6-----/7
                 *     /      /
                 *    /5_____/4
                 *    |3     |2
                 *    |      |
                 *    |1_____|0
                 *
                 */

                positions[v + 0] = v0 * innerRadius;
                positions[v + 1] = v0 * outerRadius;
                positions[v + 2] = v0 * innerRadius;
                positions[v + 3] = v0 * outerRadius;

                positions[v + 0].y = h0;
                positions[v + 1].y = h0;
                positions[v + 2].y = h1;
                positions[v + 3].y = h1;

                positions[v + 4] = positions[v + 2];
                positions[v + 5] = positions[v + 3];

                positions[v + 6] = v1 * outerRadius;
                positions[v + 6].y = h1;

                if (!noInnerSide)
                {
                    positions[v + 7] = v1 * innerRadius;
                    positions[v + 7].y = h1;
                }

                faces[t + 0] = new Face(new int[] {
                    v + 0,
                    v + 1,
                    v + 2,
                    v + 1,
                    v + 3,
                    v + 2
                });

                if (noInnerSide)
                {
                    faces[t + 1] = new Face(new int[] {
                        v + 4,
                        v + 5,
                        v + 6
                    });
                }
                else
                {
                    faces[t + 1] = new Face(new int[] {
                        v + 4,
                        v + 5,
                        v + 6,
                        v + 4,
                        v + 6,
                        v + 7
                    });
                }

                float uvRotation = ((inc1 + inc0) * -.5f) * Mathf.Rad2Deg;
                uvRotation %= 360f;
                if (uvRotation < 0f)
                    uvRotation = 360f + uvRotation;

                var uv = faces[t + 1].uv;
                uv.rotation = uvRotation;
                faces[t + 1].uv = uv;

                v += noInnerSide ? 7 : 8;
                t += 2;
            }

            // sides
            if (buildSides)
            {
                // first step is special case - only needs a quad, but all other steps need
                // a quad and tri.
                float x = noInnerSide ? innerRadius + stairWidth : innerRadius;

                for (int side = (noInnerSide ? 1 : 0); side < 2; side++)
                {
                    Vector3[] sides_v = new Vector3[steps * 4 + (steps - 1) * 3];
                    Face[] sides_f = new Face[steps + steps - 1];

                    int sv = 0, st = 0;

                    for (int i = 0; i < steps; i++)
                    {
                        float inc0 = (i / (float)steps) * cir;
                        float inc1 = ((i + 1) / (float)steps) * cir;

                        float h0 = ((Mathf.Max(i, 1) / (float)steps) * height);
                        float h1 = (((i + 1) / (float)steps) * height);

                        Vector3 v0 = new Vector3(-Mathf.Cos(inc0), 0f, Mathf.Sin(inc0)) * x;
                        Vector3 v1 = new Vector3(-Mathf.Cos(inc1), 0f, Mathf.Sin(inc1)) * x;

                        sides_v[sv + 0] = v0;
                        sides_v[sv + 1] = v1;
                        sides_v[sv + 2] = v0;
                        sides_v[sv + 3] = v1;

                        sides_v[sv + 0].y = 0f;
                        sides_v[sv + 1].y = 0f;
                        sides_v[sv + 2].y = h0;
                        sides_v[sv + 3].y = h1;

                        sides_f[st++] = new Face(side % 2 == 0 ?
                                new int[] { v + 2, v + 1, v + 0, v + 2, v + 3, v + 1 } :
                                new int[] { v + 0, v + 1, v + 2, v + 1, v + 3, v + 2 });
                        sides_f[st - 1].smoothingGroup = side + 1;

                        v += 4;
                        sv += 4;

                        // that connecting triangle
                        if (i > 0)
                        {
                            sides_f[st - 1].textureGroup = (side * steps) + i;

                            sides_v[sv + 0] = v0;
                            sides_v[sv + 1] = v1;
                            sides_v[sv + 2] = v0;
                            sides_v[sv + 0].y = h0;
                            sides_v[sv + 1].y = h1;
                            sides_v[sv + 2].y = h1;

                            sides_f[st++] = new Face(side % 2 == 0 ?
                                    new int[] { v + 2, v + 1, v + 0 } :
                                    new int[] { v + 0, v + 1, v + 2 });

                            sides_f[st - 1].textureGroup = (side * steps) + i;
                            sides_f[st - 1].smoothingGroup = side + 1;

                            v += 3;
                            sv += 3;
                        }
                    }

                    positions = positions.Concat(sides_v);
                    faces = faces.Concat(sides_f);

                    x += stairWidth;
                }

                // // add that last back face
                float cos = -Mathf.Cos(cir), sin = Mathf.Sin(cir);

                positions = positions.Concat(new Vector3[]
                {
                    new Vector3(cos, 0f, sin) * innerRadius,
                    new Vector3(cos, 0f, sin) * outerRadius,
                    new Vector3(cos * innerRadius, height, sin * innerRadius),
                    new Vector3(cos * outerRadius, height, sin * outerRadius)
                });

                faces = faces.Add(new Face(new int[] { v + 2, v + 1, v + 0, v + 2, v + 3, v + 1 }));
            }

            if (circumference < 0f)
            {
                Vector3 flip = new Vector3(-1f, 1f, 1f);

                for (int i = 0; i < positions.Length; i++)
                    positions[i].Scale(flip);

                foreach (Face f in faces)
                    f.Reverse();
            }

            // if(m_Forward.x < 0)
            // {
            //     for(int i = 0; i < positions.Length; i++)
            //         positions[i] = new Vector3(-positions[i].x, positions[i].y, -positions[i].z);
            // }
            // else if(m_Forward.z < 0)
            // {
            //     for(int i = 0; i < positions.Length; i++)
            //         positions[i] = new Vector3(positions[i].z, positions[i].y, -positions[i].x);
            // }
            // else if(m_Forward.z > 0)
            // {
                for(int i = 0; i < positions.Length; i++)
                    positions[i] = new Vector3(-positions[i].z, positions[i].y, positions[i].x);
            // }

            mesh.RebuildWithPositionsAndFaces(positions, faces);
        }
    }
}
