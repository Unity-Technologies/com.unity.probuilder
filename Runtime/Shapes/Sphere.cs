using System;

namespace UnityEngine.ProBuilder.Shapes
{
    [Shape("Sphere")]
    public class Sphere : Shape
    {
        static readonly Vector3[] k_IcosphereVertices = new Vector3[12]
        {
            new Vector3(-1f,  Math.phi,  0f),
            new Vector3(1f,  Math.phi,  0f),
            new Vector3(-1f, -Math.phi,  0f),
            new Vector3(1f, -Math.phi,  0f),

            new Vector3(0f, -1f,  Math.phi),
            new Vector3(0f,  1f,  Math.phi),
            new Vector3(0f, -1f, -Math.phi),
            new Vector3(0f,  1f, -Math.phi),

            new Vector3(Math.phi, 0f, -1f),
            new Vector3(Math.phi, 0f,  1f),
            new Vector3(-Math.phi, 0f, -1f),
            new Vector3(-Math.phi, 0f,  1f)
        };

        static readonly int[] k_IcosphereTriangles = new int[60]
        {
            0, 11, 5,
            0, 5, 1,
            0, 1, 7,
            0, 7, 10,
            0, 10, 11,

            1, 5, 9,
            5, 11, 4,
            11, 10, 2,
            10, 7, 6,
            7, 1, 8,

            3, 9, 4,
            3, 4, 2,
            3, 2, 6,
            3, 6, 8,
            3, 8, 9,

            4, 9, 5,
            2, 4, 11,
            6, 2, 10,
            8, 6, 7,
            9, 8, 1
        };

        [Range(1, 5)]
        [SerializeField]
        int m_Subdivisions = 3;

        public override void RebuildMesh(ProBuilderMesh mesh, Vector3 size)
        {
            var radius = System.Math.Min(System.Math.Min(size.x, size.y), size.z);
            //avoid to create a degenerated sphere with a radius set to 0
            radius = radius < 0.001f ? 0.001f : radius;

            // http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html

            Vector3[] v = new Vector3[k_IcosphereTriangles.Length];

            // Regular Icosahedron - 12 vertices, 20 faces.
            for (int i = 0; i < k_IcosphereTriangles.Length; i += 3)
            {
                v[i + 0] = k_IcosphereVertices[k_IcosphereTriangles[i + 0]].normalized * radius;
                v[i + 1] = k_IcosphereVertices[k_IcosphereTriangles[i + 1]].normalized * radius;
                v[i + 2] = k_IcosphereVertices[k_IcosphereTriangles[i + 2]].normalized * radius;
            }

            for (int i = 0; i < m_Subdivisions; i++)
            {
                v = SubdivideIcosahedron(v, radius);
            }

            Face[] f = new Face[v.Length / 3];

            Vector3 bottomMostVertexPosition = Vector3.positiveInfinity;
            int bottomMostVertexIndex = -1;

            for (int i = 0; i < v.Length; i += 3)
            {
                f[i / 3] = new Face(new int[3] { i, i + 1, i + 2 });
                f[i / 3].manualUV = true;

                // Get the bottom most vertex of the whole shape. We'll use it as a pivot point.
                for (int j = 0; j < f[i / 3].indexes.Count; ++j)
                {
                    int index = f[i / 3].indexes[j];

                    if (v[index].y < bottomMostVertexPosition.y)
                    {
                        bottomMostVertexPosition = v[index];
                        bottomMostVertexIndex = index;
                    }
                }
            }
            mesh.unwrapParameters = new UnwrapParameters()
            {
                packMargin = 30f
            };


            mesh.RebuildWithPositionsAndFaces(v, f);
        }


        // Subdivides a set of vertices (wound as individual triangles) on an icosphere.
        //
        //   /\          /\
        //      /  \    ->      /--\
        // /____\      /_\/_\
        //
        static Vector3[] SubdivideIcosahedron(Vector3[] vertices, float radius)
        {
            Vector3[] v = new Vector3[vertices.Length * 4];

            int index = 0;

            Vector3 p0 = Vector3.zero,  //      5
                    p1 = Vector3.zero,  //    3   4
                    p2 = Vector3.zero,  //  0,  1,  2
                    p3 = Vector3.zero,
                    p4 = Vector3.zero,
                    p5 = Vector3.zero;

            for (int i = 0; i < vertices.Length; i += 3)
            {
                p0 = vertices[i + 0];
                p2 = vertices[i + 1];
                p5 = vertices[i + 2];
                p1 = ((p0 + p2) * .5f).normalized * radius;
                p3 = ((p0 + p5) * .5f).normalized * radius;
                p4 = ((p2 + p5) * .5f).normalized * radius;

                v[index++] = p0;
                v[index++] = p1;
                v[index++] = p3;

                v[index++] = p1;
                v[index++] = p2;
                v[index++] = p4;

                v[index++] = p1;
                v[index++] = p4;
                v[index++] = p3;

                v[index++] = p3;
                v[index++] = p4;
                v[index++] = p5;
            }

            return v;
        }
    }


}
