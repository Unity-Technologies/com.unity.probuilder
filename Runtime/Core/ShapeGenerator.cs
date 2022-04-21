using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// This enum describes each primitive shape that ProBuilder can create by default.
    /// To get a primitive with default parameters, use one of these values when calling <see cref="ShapeGenerator.CreateShape"/>.
    /// </summary>
    public enum ShapeType
    {
        /// <summary>
        /// [Cube](../manual/Cube.html) shape.
        /// </summary>
        Cube,
        /// <summary>
        /// Straight [stairs](../manual/Stair.html) shape.
        /// </summary>
        Stair,
        /// <summary>
        /// Curved [stairs](../manual/Stair.html) shape.
        /// </summary>
        CurvedStair,
        /// <summary>
        /// [Prism](../manual/Prism.html) shape.
        /// </summary>
        Prism,
        /// <summary>
        /// [Cylinder](../manual/Cylinder.html) shape.
        /// </summary>
        Cylinder,
        /// <summary>
        /// [Plane](../manual/Plane.html) shape with 2 subdivisions and 10x10 units size.
        /// </summary>
        Plane,
        /// <summary>
        /// [Door](../manual/Door.html) shape.
        /// </summary>
        Door,
        /// <summary>
        /// [Pipe](../manual/Pipe.html) shape.
        /// </summary>
        Pipe,
        /// <summary>
        /// [Cone](../manual/Cone.html) shape.
        /// </summary>
        Cone,
        /// <summary>
        /// [Sprite](../manual/Sprite.html) shape (1x1 quad).
        /// </summary>
        Sprite,
        /// <summary>
        /// [Arch](../manual/Arch.html) shape (180 degrees).
        /// </summary>
        Arch,
        /// <summary>
        /// [Sphere](../manual/Sphere.html) shape. Sometimes called "icosphere", or "icosahedron".
        /// </summary>
        Sphere,
        /// <summary>
        /// [Torus](../manual/Torus.html) shape.
        /// </summary>
        Torus
    }

    /// <summary>
    /// Provides functions for creating ProBuilderMesh primitives.
    /// </summary>
    public static class ShapeGenerator
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

        /// <summary>
        /// A set of 8 vertices forming the template for a cube mesh.
        /// </summary>
        static readonly Vector3[] k_CubeVertices = new Vector3[] {
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
        /// A set of triangles forming a cube with reference to the k_CubeVertices array.
        /// </summary>
        static readonly int[] k_CubeTriangles = new int[] {
            0, 1, 4, 5, 1, 2, 5, 6, 2, 3, 6, 7, 3, 0, 7, 4, 4, 5, 7, 6, 3, 2, 0, 1
        };

        /// <summary>
        /// Creates a shape with default parameters.
        ///
        /// In the Editor, the equivalent is the [Shape tool](../manual/shape-tool.html).
        /// </summary>
        /// <param name="shape">The ShapeType to create.</param>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitve shape.</returns>
        public static ProBuilderMesh CreateShape(ShapeType shape, PivotLocation pivotType = PivotLocation.Center)
        {
            ProBuilderMesh pb = null;

            if (shape == ShapeType.Cube)
                pb = GenerateCube(pivotType, Vector3.one);
            if (shape == ShapeType.Stair)
                pb = GenerateStair(pivotType, new Vector3(2f, 2.5f, 4f), 6, true);
            if (shape == ShapeType.CurvedStair)
                pb = GenerateCurvedStair(pivotType, 2f, 2.5f, 2f, 180f, 8, true);
            if (shape == ShapeType.Prism)
                pb = GeneratePrism(pivotType, Vector3.one);
            if (shape == ShapeType.Cylinder)
                pb = GenerateCylinder(pivotType, 8, 1f, 2f, 2);
            if (shape == ShapeType.Plane)
                pb = GeneratePlane(pivotType, 5f, 5f, 5, 5, Axis.Up);
            if (shape == ShapeType.Door)
                pb = GenerateDoor(pivotType, 3f, 2.5f, .5f, .75f, 1f);
            if (shape == ShapeType.Pipe)
                pb = GeneratePipe(pivotType, 1f, 2f, .25f, 8, 2);
            if (shape == ShapeType.Cone)
                pb = GenerateCone(pivotType, .5f, 1f, 8);
            if (shape == ShapeType.Sprite)
                pb = GeneratePlane(pivotType, 1f, 1f, 0, 0, Axis.Up);
            if (shape == ShapeType.Arch)
                pb = GenerateArch(pivotType, 180f, 2f, 1f, 1f, 9, true, true, true, true, true);
            if (shape == ShapeType.Sphere)
                pb = GenerateIcosahedron(pivotType, .5f, 2, true, false);
            if (shape == ShapeType.Torus)
            {
                pb = GenerateTorus(pivotType, 12, 16, 1f, .3f, true, 360f, 360f, true);
                UVEditing.ProjectFacesBox(pb, pb.facesInternal);
            }

            if (pb == null)
            {
#if DEBUG
                Log.Error(shape.ToString() + " type has no default!");
#endif
                pb = GenerateCube(pivotType, Vector3.one);
            }

            pb.gameObject.name = shape.ToString();
            pb.renderer.sharedMaterial = BuiltinMaterials.defaultMaterial;

            return pb;
        }

        /// <summary>
        /// Creates a set of straight stairs.
        /// </summary>
        /// <param name="pivotType">Set the location of the shape's pivot (center or first corner).</param>
        /// <param name="size">Set the position of the opposite corner of the bounding box for the stairs.</param>
        /// <param name="steps">Set the number of steps to build.</param>
        /// <param name="buildSides">Set this to true to build the side and back walls or false to build only the stair top and connecting planes.</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        /// <seealso cref="GenerateCurvedStair"/>
        public static ProBuilderMesh GenerateStair(PivotLocation pivotType, Vector3 size, int steps, bool buildSides)
        {
            // 4 vertices per quad, 2 quads per step.
            Vector3[] vertices = new Vector3[4 * steps * 2];
            Face[] faces = new Face[steps * 2];

            // vertex index, face index
            int v = 0, t = 0;

            for (int i = 0; i < steps; i++)
            {
                float inc0 = i / (float)steps;
                float inc1 = (i + 1) / (float)steps;

                float x0 = size.x;
                float x1 = 0;
                float y0 = size.y * inc0;
                float y1 = size.y * inc1;
                float z0 = size.z * inc0;
                float z1 = size.z * inc1;

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
            if (buildSides)
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

                        sides_v[sv + 0] = new Vector3(x, 0f, z0);
                        sides_v[sv + 1] = new Vector3(x, 0f, z1);
                        sides_v[sv + 2] = new Vector3(x, y0, z0);
                        sides_v[sv + 3] = new Vector3(x, y1, z1);

                        sides_f[st++] = new Face(side % 2 == 0 ?
                                new int[] { v + 0, v + 1, v + 2, v + 1, v + 3, v + 2 } :
                                new int[] { v + 2, v + 1, v + 0, v + 2, v + 3, v + 1 });

                        sides_f[st - 1].textureGroup = side + 1;

                        v += 4;
                        sv += 4;

                        // that connecting triangle
                        if (i > 0)
                        {
                            sides_v[sv + 0] = new Vector3(x, y0, z0);
                            sides_v[sv + 1] = new Vector3(x, y1, z0);
                            sides_v[sv + 2] = new Vector3(x, y1, z1);

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
                    new Vector3(0f, 0f, size.z),
                    new Vector3(size.x, 0f, size.z),
                    new Vector3(0f, size.y, size.z),
                    new Vector3(size.x, size.y, size.z)
                });

                faces = faces.Add(new Face(new int[] {v + 0, v + 1, v + 2, v + 1, v + 3, v + 2}));
            }

            ProBuilderMesh pb = ProBuilderMesh.Create(vertices, faces);
            pb.gameObject.name = "Stairs";
            pb.SetPivot(pivotType);

            return pb;
        }

        /// <summary>
        /// Creates a set of curved stairs.
        /// </summary>
        /// <param name="pivotType">Set the location of the shape's pivot (center or first corner).</param>
        /// <param name="stairWidth">Set the width of the stairs.</param>
        /// <param name="height">Set the height of the stairs.</param>
        /// <param name="innerRadius">Set the radius from the center of the bounding box to the inner stair bounds.</param>
        /// <param name="circumference">Set the amount of curvature in degrees.</param>
        /// <param name="steps">Set the number of steps to build.</param>
        /// <param name="buildSides">Set this to true to build the side and back walls or false to build only the stair top and connecting planes.</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        /// <seealso cref="GenerateStair"/>
        public static ProBuilderMesh GenerateCurvedStair(PivotLocation pivotType, float stairWidth, float height, float innerRadius, float circumference, int steps, bool buildSides)
        {
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

                faces = faces.Add(new Face(new int[] {v + 2, v + 1, v + 0, v + 2, v + 3, v + 1}));
            }

            if (circumference < 0f)
            {
                Vector3 flip = new Vector3(-1f, 1f, 1f);

                for (int i = 0; i < positions.Length; i++)
                    positions[i].Scale(flip);

                foreach (Face f in faces)
                    f.Reverse();
            }

            ProBuilderMesh pb = ProBuilderMesh.Create(positions, faces);

            pb.gameObject.name = "Stairs";
            pb.SetPivot(pivotType);

            return pb;
        }

        /// <summary>
        /// Creates a stair set with the given parameters.
        /// </summary>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <param name="steps">How many steps should this stairwell have?</param>
        /// <param name="width">How wide (in meters) should this stairset be?</param>
        /// <param name="height">How tall (in meters) should this stairset be?</param>
        /// <param name="depth">How deep (in meters) should this stairset be?</param>
        /// <param name="sidesGoToFloor">If true, stair step sides will extend to the floor.  If false, sides will only extend as low as the stair is high.</param>
        /// <param name="generateBack">If true, a back face to the stairwell will be appended.</param>
        /// <param name="platformsOnly">If true, only the front face and tops of the stairwell will be built.  Nice for when a staircase is embedded between geometry.</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        internal static ProBuilderMesh GenerateStair(PivotLocation pivotType, int steps, float width, float height, float depth, bool sidesGoToFloor, bool generateBack, bool platformsOnly)
        {
            int i = 0;

            List<Vector3> verts = new List<Vector3>();
            Vector3[] v = (platformsOnly) ? new Vector3[8] : new Vector3[16];

            float stepWidth = width;
            float stepHeight = height / steps;
            float stepDepth = depth / steps;
            float yMax = stepHeight; // used when stair sides extend to floor

            // platforms
            for (i = 0; i < steps; i++)
            {
                float x = stepWidth / 2f, y = i * stepHeight, z = i * stepDepth;

                if (sidesGoToFloor)
                    y = 0;

                yMax = i * stepHeight + stepHeight;

                // Front
                v[0]  = new Vector3(x, i * stepHeight,   z);
                v[1]  = new Vector3(-x, i * stepHeight,   z);
                v[2] = new Vector3(x, yMax,            z);
                v[3] = new Vector3(-x, yMax,            z);

                // Platform
                v[4] = new Vector3(x, yMax, z);
                v[5] = new Vector3(-x, yMax, z);
                v[6] = new Vector3(x, yMax, z + stepDepth);
                v[7] = new Vector3(-x, yMax, z + stepDepth);

                if (!platformsOnly)
                {
                    // Left side
                    v[8] = new Vector3(x, y,       z + stepDepth);
                    v[9] = new Vector3(x, y,       z);
                    v[10] = new Vector3(x, yMax,    z + stepDepth);
                    v[11] = new Vector3(x, yMax,    z);

                    // Right side
                    v[12] = new Vector3(-x, y,     z);
                    v[13] = new Vector3(-x, y,     z + stepDepth);
                    v[14] = new Vector3(-x, yMax,  z);
                    v[15] = new Vector3(-x, yMax,  z + stepDepth);
                }

                verts.AddRange(v);
            }

            if (generateBack)
            {
                verts.Add(new Vector3(-stepWidth / 2f, 0f, depth));
                verts.Add(new Vector3(stepWidth / 2f, 0f, depth));
                verts.Add(new Vector3(-stepWidth / 2f, height, depth));
                verts.Add(new Vector3(stepWidth / 2f, height, depth));
            }

            ProBuilderMesh pb = ProBuilderMesh.CreateInstanceWithPoints(verts.ToArray());
            pb.gameObject.name = "Stairs";
            pb.SetPivot(pivotType);

            return pb;
        }

        /// <summary>
        /// Creates a new cube with the specified size as a bounding box. The size is fixed: it is not applied as a scale value in the transform.
        /// </summary>
        /// <param name="pivotType">Set the location of the shape's pivot (center or first corner).</param>
        /// <param name="size">Set the position of the opposite corner of the bounding box for the cube.</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        public static ProBuilderMesh GenerateCube(PivotLocation pivotType, Vector3 size)
        {
            Vector3[] points = new Vector3[k_CubeTriangles.Length];

            for (int i = 0; i < k_CubeTriangles.Length; i++)
                points[i] = Vector3.Scale(k_CubeVertices[k_CubeTriangles[i]], size);

            ProBuilderMesh mesh = ProBuilderMesh.CreateInstanceWithPoints(points);
            mesh.gameObject.name = "Cube";

            if(pivotType == PivotLocation.Center)
                foreach (var face in mesh.facesInternal)
                    face.uv = new AutoUnwrapSettings(face.uv) { offset = new Vector2(-.5f, -.5f) };

            mesh.SetPivot(pivotType);

            return mesh;
        }

        /// <summary>
        /// Creates a cylinder primitive.
        /// </summary>
        /// <param name="pivotType">Set the location of the shape's pivot (center or first corner).</param>
        /// <param name="axisDivisions">Set the number of divisions to create on the vertical axis.  The larger the value, the smoother the surface. </param>
        /// <param name="radius">Set the radius in world units.</param>
        /// <param name="height">Set the height of this object in world units.</param>
        /// <param name="heightCuts">Set the amount of divisions to create on the horizontal axis.</param>
        /// <param name="smoothing">Set the smoothing group ID (index).</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        public static ProBuilderMesh GenerateCylinder(PivotLocation pivotType, int axisDivisions, float radius, float height, int heightCuts, int smoothing = -1)
        {
            if (axisDivisions % 2 != 0)
                axisDivisions++;

            if (axisDivisions > 64)
                axisDivisions = 64;

            float stepAngle = 360f / axisDivisions;
            float heightStep = height / (heightCuts + 1);

            Vector3[] circle = new Vector3[axisDivisions];

            // get a circle
            for (int i = 0; i < axisDivisions; i++)
            {
                float angle0 = stepAngle * i * Mathf.Deg2Rad;

                float x = Mathf.Cos(angle0) * radius;
                float z = Mathf.Sin(angle0) * radius;

                circle[i] = new Vector3(x, 0f, z);
            }

            // add two because end caps
            Vector3[] verts = new Vector3[(axisDivisions * (heightCuts + 1) * 4) + (axisDivisions * 6)];
            Face[] faces = new Face[axisDivisions * (heightCuts + 1)   + (axisDivisions * 2)];

            // build vertex array
            int it = 0;
            // +1 to account for 0 height cuts
            for (int i = 0; i < heightCuts + 1; i++)
            {
                float Y = i * heightStep;
                float Y2 = (i + 1) * heightStep;

                for (int n = 0; n < axisDivisions; n++)
                {
                    verts[it + 0] = new Vector3(circle[n + 0].x, Y, circle[n + 0].z);
                    verts[it + 1] = new Vector3(circle[n + 0].x, Y2, circle[n + 0].z);

                    if (n != axisDivisions - 1)
                    {
                        verts[it + 2] = new Vector3(circle[n + 1].x, Y, circle[n + 1].z);
                        verts[it + 3] = new Vector3(circle[n + 1].x, Y2, circle[n + 1].z);
                    }
                    else
                    {
                        verts[it + 2] = new Vector3(circle[0].x, Y, circle[0].z);
                        verts[it + 3] = new Vector3(circle[0].x, Y2, circle[0].z);
                    }

                    it += 4;
                }
            }

            // wind side faces
            int f = 0;
            for (int i = 0; i < heightCuts + 1; i++)
            {
                for (int n = 0; n < axisDivisions * 4; n += 4)
                {
                    int index = (i * (axisDivisions * 4)) + n;
                    int zero    = index;
                    int one     = index + 1;
                    int two     = index + 2;
                    int three   = index + 3;

                    faces[f++] = new Face(
                            new int[6] { zero, one, two, one, three, two },
                            0,
                            AutoUnwrapSettings.tile,
                            smoothing,
                            -1,
                            -1,
                            false);
                }
            }

            // construct caps seperately, cause they aren't wound the same way
            int ind = (axisDivisions * (heightCuts + 1) * 4);
            int f_ind = axisDivisions * (heightCuts + 1);

            for (int n = 0; n < axisDivisions; n++)
            {
                // bottom faces
                verts[ind + 0] = new Vector3(circle[n].x, 0f, circle[n].z);

                verts[ind + 1] = Vector3.zero;

                if (n != axisDivisions - 1)
                    verts[ind + 2] = new Vector3(circle[n + 1].x, 0f, circle[n + 1].z);
                else
                    verts[ind + 2] = new Vector3(circle[000].x, 0f, circle[000].z);

                faces[f_ind + n] = new Face(new int[3] {ind + 2, ind + 1, ind + 0});

                ind += 3;

                // top faces
                verts[ind + 0]    = new Vector3(circle[n].x, height, circle[n].z);
                verts[ind + 1]    = new Vector3(0f, height, 0f);
                if (n != axisDivisions - 1)
                    verts[ind + 2] = new Vector3(circle[n + 1].x, height, circle[n + 1].z);
                else
                    verts[ind + 2] = new Vector3(circle[000].x, height, circle[000].z);

                faces[f_ind + (n + axisDivisions)] = new Face(new int[3] {ind + 0, ind + 1, ind + 2});

                ind += 3;
            }

            ProBuilderMesh pb = ProBuilderMesh.Create(verts, faces);
            pb.gameObject.name = "Cylinder";
            pb.SetPivot(pivotType);

            return pb;
        }

        /// <summary>
        /// Creates a new prism primitive.
        /// </summary>
        /// <param name="pivotType">Set the location of the shape's pivot (center or first corner).</param>
        /// <param name="size">Set the scale to apply to the shape.</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        public static ProBuilderMesh GeneratePrism(PivotLocation pivotType, Vector3 size)
        {
            size.y *= 2f;

            Vector3[] template = new Vector3[6]
            {
                Vector3.Scale(new Vector3(-.5f, 0f, -.5f),  size),
                Vector3.Scale(new Vector3(.5f, 0f, -.5f),   size),
                Vector3.Scale(new Vector3(0f, .5f, -.5f),   size),
                Vector3.Scale(new Vector3(-.5f, 0f, .5f),   size),
                Vector3.Scale(new Vector3(0.5f, 0f, .5f),   size),
                Vector3.Scale(new Vector3(0f, .5f, .5f),    size)
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

            ProBuilderMesh pb = ProBuilderMesh.Create(v, f);
            pb.gameObject.name = "Prism";
            pb.SetPivot(pivotType);

            return pb;
        }

        /// <summary>
        /// Creates a door shape to place into a wall structure. Only the faces that are visible inside the walls have faces.
        /// </summary>
        /// <param name="pivotType">Set the location of the shape's pivot (center or first corner).</param>
        /// <param name="totalWidth">Set the total width of the door.</param>
        /// <param name="totalHeight">Set the total height of the door.</param>
        /// <param name="ledgeHeight">Set the height between the top of the door frame and the top of the object.</param>
        /// <param name="legWidth">Set the width of each leg on both sides of the door.</param>
        /// <param name="depth">Set the distance between the front and back faces of the door object.</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        public static ProBuilderMesh GenerateDoor(PivotLocation pivotType, float totalWidth, float totalHeight, float ledgeHeight, float legWidth, float depth)
        {
            float xLegCoord = totalWidth / 2f;
            legWidth = xLegCoord - legWidth;
            ledgeHeight = totalHeight - ledgeHeight;

            // 8---9---10--11
            // |           |
            // 4   5---6   7
            // |   |   |   |
            // 0   1   2   3
            Vector3[] template = new Vector3[12]
            {
                new Vector3(-xLegCoord, 0f, depth),           // 0
                new Vector3(-legWidth, 0f, depth),            // 1
                new Vector3(legWidth, 0f, depth),             // 2
                new Vector3(xLegCoord, 0f, depth),            // 3
                new Vector3(-xLegCoord, ledgeHeight, depth),  // 4
                new Vector3(-legWidth, ledgeHeight, depth),   // 5
                new Vector3(legWidth, ledgeHeight, depth),    // 6
                new Vector3(xLegCoord, ledgeHeight, depth),   // 7
                new Vector3(-xLegCoord, totalHeight, depth),  // 8
                new Vector3(-legWidth, totalHeight, depth),   // 9
                new Vector3(legWidth, totalHeight, depth),    // 10
                new Vector3(xLegCoord, totalHeight, depth)    // 11
            };

            List<Vector3> points = new List<Vector3>();

            points.Add(template[0]);
            points.Add(template[1]);
            points.Add(template[4]);
            points.Add(template[5]);

            points.Add(template[2]);
            points.Add(template[3]);
            points.Add(template[6]);
            points.Add(template[7]);

            points.Add(template[4]);
            points.Add(template[5]);
            points.Add(template[8]);
            points.Add(template[9]);

            points.Add(template[6]);
            points.Add(template[7]);
            points.Add(template[10]);
            points.Add(template[11]);

            points.Add(template[5]);
            points.Add(template[6]);
            points.Add(template[9]);
            points.Add(template[10]);

            List<Vector3> reverse = new List<Vector3>();

            for (int i = 0; i < points.Count; i += 4)
            {
                reverse.Add(points[i + 1] - Vector3.forward * depth);
                reverse.Add(points[i + 0] - Vector3.forward * depth);
                reverse.Add(points[i + 3] - Vector3.forward * depth);
                reverse.Add(points[i + 2] - Vector3.forward * depth);
            }

            points.AddRange(reverse);

            points.Add(template[6]);
            points.Add(template[5]);
            points.Add(template[6] - Vector3.forward * depth);
            points.Add(template[5] - Vector3.forward * depth);

            points.Add(template[2] - Vector3.forward * depth);
            points.Add(template[2]);
            points.Add(template[6] - Vector3.forward * depth);
            points.Add(template[6]);

            points.Add(template[1]);
            points.Add(template[1] - Vector3.forward * depth);
            points.Add(template[5]);
            points.Add(template[5] - Vector3.forward * depth);

            ProBuilderMesh pb = ProBuilderMesh.CreateInstanceWithPoints(points.ToArray());
            pb.gameObject.name = "Door";
            pb.SetPivot(pivotType);

            return pb;
        }

        /// <summary>
        /// Creates a new plane shape.
        /// </summary>
        /// <param name="pivotType">Set the location of the shape's pivot (center or first corner).</param>
        /// <param name="width">Set the width of the plane.</param>
        /// <param name="height">Set the height of the plane.</param>
        /// <param name="widthCuts">Set the divisions on the X-axis.</param>
        /// <param name="heightCuts">Set the divisions on the Y-axis.</param>
        /// <param name="axis">Set the axis to build the plane on. For example, `ProBuilder.Axis.Up` is a plane with a normal of `Vector3.up`.</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        public static ProBuilderMesh GeneratePlane(PivotLocation pivotType, float width, float height, int widthCuts, int heightCuts, Axis axis)
        {
            int w = widthCuts + 1;
            int h = heightCuts + 1;

            Vector2[] p = new Vector2[(w * h) * 4];
            Vector3[] v = new Vector3[(w * h) * 4];
            Face[] f = new Face[w * h];

            int i = 0, j = 0;
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
            }

            switch (axis)
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

            ProBuilderMesh pb = ProBuilderMesh.Create(v, f);
            pb.gameObject.name = "Plane";
            pb.SetPivot(pivotType);

            return pb;
        }

        /// <summary>
        /// Creates a new pipe shape.
        /// </summary>
        /// <param name="pivotType">Set the location of the shape's pivot (center or first corner).</param>
        /// <param name="radius">Set the radius of the pipe.</param>
        /// <param name="height">Set the height of the pipe.</param>
        /// <param name="thickness">Set the thickness of the walls.</param>
        /// <param name="subdivAxis">Set the number of subdivisions on the axis.</param>
        /// <param name="subdivHeight">Set the number of subdivisions on the Y-axis.</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        public static ProBuilderMesh GeneratePipe(PivotLocation pivotType, float radius, float height, float thickness, int subdivAxis, int subdivHeight)
        {
            // template is outer ring - radius refers to outer ring always
            Vector2[] templateOut = new Vector2[subdivAxis];
            Vector2[] templateIn = new Vector2[subdivAxis];

            for (int i = 0; i < subdivAxis; i++)
            {
                templateOut[i] = Math.PointInCircumference(radius, i * (360f / subdivAxis), Vector2.zero);
                templateIn[i] = Math.PointInCircumference(radius - thickness, i * (360f / subdivAxis), Vector2.zero);
            }

            List<Vector3> v = new List<Vector3>();

            subdivHeight += 1;

            // build out sides
            Vector2 tmp, tmp2, tmp3, tmp4;
            for (int i = 0; i < subdivHeight; i++)
            {
                // height subdivisions
                float y = i * (height / subdivHeight);
                float y2 = (i + 1) * (height / subdivHeight);

                for (int n = 0; n < subdivAxis; n++)
                {
                    tmp = templateOut[n];
                    tmp2 = n < (subdivAxis - 1) ? templateOut[n + 1] : templateOut[0];

                    // outside quads
                    Vector3[] qvo = new Vector3[4]
                    {
                        new Vector3(tmp2.x, y, tmp2.y),
                        new Vector3(tmp.x, y, tmp.y),
                        new Vector3(tmp2.x, y2, tmp2.y),
                        new Vector3(tmp.x, y2, tmp.y)
                    };

                    // inside quad
                    tmp = templateIn[n];
                    tmp2 = n < (subdivAxis - 1) ? templateIn[n + 1] : templateIn[0];
                    Vector3[] qvi = new Vector3[4]
                    {
                        new Vector3(tmp.x, y, tmp.y),
                        new Vector3(tmp2.x, y, tmp2.y),
                        new Vector3(tmp.x, y2, tmp.y),
                        new Vector3(tmp2.x, y2, tmp2.y)
                    };

                    v.AddRange(qvo);
                    v.AddRange(qvi);
                }
            }

            // build top and bottom
            for (int i = 0; i < subdivAxis; i++)
            {
                tmp = templateOut[i];
                tmp2 = (i < subdivAxis - 1) ? templateOut[i + 1] : templateOut[0];
                tmp3 = templateIn[i];
                tmp4 = (i < subdivAxis - 1) ? templateIn[i + 1] : templateIn[0];

                // top
                Vector3[] tpt = new Vector3[4]
                {
                    new Vector3(tmp2.x, height, tmp2.y),
                    new Vector3(tmp.x,  height, tmp.y),
                    new Vector3(tmp4.x, height, tmp4.y),
                    new Vector3(tmp3.x, height, tmp3.y)
                };

                // top
                Vector3[] tpb = new Vector3[4]
                {
                    new Vector3(tmp.x, 0f, tmp.y),
                    new Vector3(tmp2.x, 0f, tmp2.y),
                    new Vector3(tmp3.x, 0f, tmp3.y),
                    new Vector3(tmp4.x, 0f, tmp4.y),
                };

                v.AddRange(tpb);
                v.AddRange(tpt);
            }

            ProBuilderMesh pb = ProBuilderMesh.CreateInstanceWithPoints(v.ToArray());

            pb.gameObject.name = "Pipe";
            //Keep backward compatibility
            if(pivotType == PivotLocation.Center)
                pb.SetPivot(pivotType);
            else
                pb.CenterPivot(new int[1] { 1 });

            return pb;
        }

        /// <summary>
        /// Creates a new cone shape.
        /// </summary>
        /// <param name="pivotType">Set the location of the shape's pivot (center or first corner).</param>
        /// <param name="radius">Set the radius of the generated cone.</param>
        /// <param name="height">Set the height of the cone.</param>
        /// <param name="subdivAxis">Set the number of subdivisions on the axis.</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        public static ProBuilderMesh GenerateCone(PivotLocation pivotType, float radius, float height, int subdivAxis)
        {
            // template is outer ring - radius refers to outer ring always
            Vector3[] template = new Vector3[subdivAxis];

            for (int i = 0; i < subdivAxis; i++)
            {
                Vector2 ct = Math.PointInCircumference(radius, i * (360f / subdivAxis), Vector2.zero);
                template[i] = new Vector3(ct.x, 0f, ct.y);
            }

            List<Vector3> v = new List<Vector3>();
            List<Face> f = new List<Face>();

            // build sides
            for (int i = 0; i < subdivAxis; i++)
            {
                // side face
                v.Add(template[i]);
                v.Add((i < subdivAxis - 1) ? template[i + 1] : template[0]);
                v.Add(Vector3.up * height);

                // bottom face
                v.Add(template[i]);
                v.Add((i < subdivAxis - 1) ? template[i + 1] : template[0]);
                v.Add(Vector3.zero);
            }

            List<Face> sideFaces = new List<Face>();
            for (int i = 0; i < subdivAxis * 6; i += 6)
            {
                Face face = new Face(new int[3] { i + 2, i + 1, i + 0 });
                f.Add(face);
                sideFaces.Add(face);
                f.Add(new Face(new int[3] {i + 3, i + 4, i + 5}));
            }

            ProBuilderMesh pb = ProBuilderMesh.Create(v.ToArray(), f.ToArray());
            pb.gameObject.name = "Cone";
            pb.SetPivot(pivotType);
            pb.unwrapParameters = new UnwrapParameters()
            {
                packMargin = 30f
            };

            // Set the UVs manually for the side faces, so that they are uniform.
            // Calculate the UVs for the first face, then set the others to the same.
            var firstFace = sideFaces[0];
            var uv = firstFace.uv;
            uv.anchor = AutoUnwrapSettings.Anchor.LowerLeft;
            firstFace.uv = uv;
            firstFace.manualUV = true;
            // Always use up vector for projection of side faces.
            // Otherwise the lines in the PB texture end up crooked.
            UvUnwrapping.Unwrap(pb, firstFace, projection: Vector3.up);
            for (int i = 1; i < sideFaces.Count; i++)
            {
                var sideFace = sideFaces[i];
                sideFace.manualUV = true;
                UvUnwrapping.CopyUVs(pb, firstFace, sideFace);
            }
            pb.RefreshUV(sideFaces);
            return pb;
        }

        /// <summary>
        /// Creates a new arch shape.
        /// </summary>
        /// <param name="pivotType">Set the location of the shape's pivot (center or first corner).</param>
        /// <param name="angle">Set the portion of a circle the arch takes up as an angle.</param>
        /// <param name="radius">Set the distance from the origin to the furthest extent of the bounding box.</param>
        /// <param name="width">Set the distance from the top of the arch to the inner radius.</param>
        /// <param name="depth">Set the depth of the arch blocks.</param>
        /// <param name="radialCuts">Set the number of blocks in the arch.</param>
        /// <param name="insideFaces">Set whether to render the inside faces.</param>
        /// <param name="outsideFaces">Set whether to render the outside faces.</param>
        /// <param name="frontFaces">Set whether to render the front faces.</param>
        /// <param name="backFaces">Set whether to render the back faces.</param>
        /// <param name="endCaps">Set to true to include the faces capping the ends of this arch. This value is ignored if the radius is 360 degrees.</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        public static ProBuilderMesh GenerateArch(PivotLocation pivotType, float angle, float radius, float width, float depth, int radialCuts, bool insideFaces, bool outsideFaces, bool frontFaces, bool backFaces, bool endCaps)
        {
            Vector2[] templateOut = new Vector2[radialCuts];
            Vector2[] templateIn = new Vector2[radialCuts];

            for (int i = 0; i < radialCuts; i++)
            {
                templateOut[i] = Math.PointInCircumference(radius, i * (angle / (radialCuts - 1)), Vector2.zero);
                templateIn[i] = Math.PointInCircumference(radius - width, i * (angle / (radialCuts - 1)), Vector2.zero);
            }

            List<Vector3> v = new List<Vector3>();

            Vector2 tmp, tmp2, tmp3, tmp4;

            float y = 0;

            for (int n = 0; n < radialCuts - 1; n++)
            {
                // outside faces
                tmp = templateOut[n];
                tmp2 = n < (radialCuts - 1) ? templateOut[n + 1] : templateOut[n];

                Vector3[] qvo = new Vector3[4]
                {
                    new Vector3(tmp.x, tmp.y, y),
                    new Vector3(tmp2.x, tmp2.y, y),
                    new Vector3(tmp.x, tmp.y, depth),
                    new Vector3(tmp2.x, tmp2.y, depth)
                };

                // inside faces
                tmp = templateIn[n];
                tmp2 = n < (radialCuts - 1) ? templateIn[n + 1] : templateIn[n];

                Vector3[] qvi = new Vector3[4]
                {
                    new Vector3(tmp2.x, tmp2.y, y),
                    new Vector3(tmp.x, tmp.y, y),
                    new Vector3(tmp2.x, tmp2.y, depth),
                    new Vector3(tmp.x, tmp.y, depth)
                };

                if (outsideFaces)
                    v.AddRange(qvo);

                if (n != radialCuts - 1 && insideFaces)
                    v.AddRange(qvi);

                // left side bottom face
                if (angle < 360f && endCaps)
                {
                    if (n == 0)
                    {
                        v.AddRange(
                            new Vector3[4]
                        {
                            new Vector3(templateOut[n].x, templateOut[n].y, depth),
                            new Vector3(templateIn[n].x, templateIn[n].y, depth),
                            new Vector3(templateOut[n].x, templateOut[n].y, y),
                            new Vector3(templateIn[n].x, templateIn[n].y, y)
                        });
                    }

                    // ride side bottom face
                    if (n == radialCuts - 2)
                    {
                        v.AddRange(
                            new Vector3[4]
                        {
                            new Vector3(templateIn[n + 1].x, templateIn[n + 1].y, depth),
                            new Vector3(templateOut[n + 1].x, templateOut[n + 1].y, depth),
                            new Vector3(templateIn[n + 1].x, templateIn[n + 1].y, y),
                            new Vector3(templateOut[n + 1].x, templateOut[n + 1].y, y)
                        });
                    }
                }
            }

            // build front and back faces
            for (int i = 0; i < radialCuts - 1; i++)
            {
                tmp = templateOut[i];
                tmp2 = (i < radialCuts - 1) ? templateOut[i + 1] : templateOut[i];
                tmp3 = templateIn[i];
                tmp4 = (i < radialCuts - 1) ? templateIn[i + 1] : templateIn[i];

                // front
                Vector3[] tpb = new Vector3[4]
                {
                    new Vector3(tmp.x, tmp.y, depth),
                    new Vector3(tmp2.x, tmp2.y, depth),
                    new Vector3(tmp3.x, tmp3.y, depth),
                    new Vector3(tmp4.x, tmp4.y, depth),
                };

                // back
                Vector3[] tpt = new Vector3[4]
                {
                    new Vector3(tmp2.x, tmp2.y, 0f),
                    new Vector3(tmp.x,  tmp.y, 0f),
                    new Vector3(tmp4.x, tmp4.y, 0f),
                    new Vector3(tmp3.x, tmp3.y, 0f)
                };

                if (frontFaces)
                    v.AddRange(tpb);
                if (backFaces)
                    v.AddRange(tpt);
            }

            ProBuilderMesh pb = ProBuilderMesh.CreateInstanceWithPoints(v.ToArray());

            pb.gameObject.name = "Arch";
            pb.SetPivot(pivotType);

            MeshValidation.EnsureMeshIsValid(pb, out _);

            return pb;
        }

        /// <summary>
        /// Creates a new icosphere shape.
        /// </summary>
        /// <param name="pivotType">Set the location of the shape's pivot (center or first corner).</param>
        /// <param name="radius">Set the radius of the sphere.</param>
        /// <param name="subdivisions">Set the number of subdivisions to perform.</param>
        /// <param name="weldVertices">By default, this value is true, meaning that it extracts shared indexes. Set this to false to show a preview, where speed of generation is more important than making the shape editable.</param>
        /// <param name="manualUvs">By default, this value is true, meaning that faces on icospheres are marked as manual UVs for performance reasons. Set this to false if you want ProBuilder to use auto-unwrapped UVs.</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        // <remarks> @DEVNOTE: Is this remark still valid?
        // This method does not build UVs, so after generating BoxProject for UVs.
        // </remarks>
        public static ProBuilderMesh GenerateIcosahedron(PivotLocation pivotType, float radius, int subdivisions, bool weldVertices = true, bool manualUvs = true)
        {
            // http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html

            Vector3[] v = new Vector3[k_IcosphereTriangles.Length];

            // Regular Icosahedron - 12 vertices, 20 faces.
            for (int i = 0; i < k_IcosphereTriangles.Length; i += 3)
            {
                v[i + 0] = k_IcosphereVertices[k_IcosphereTriangles[i + 0]].normalized * radius;
                v[i + 1] = k_IcosphereVertices[k_IcosphereTriangles[i + 1]].normalized * radius;
                v[i + 2] = k_IcosphereVertices[k_IcosphereTriangles[i + 2]].normalized * radius;
            }

            for (int i = 0; i < subdivisions; i++)
            {
                v = SubdivideIcosahedron(v, radius);
            }

            Face[] f = new Face[v.Length / 3];

            Vector3 bottomMostVertexPosition = Vector3.positiveInfinity;
            int bottomMostVertexIndex = -1;

            for (int i = 0; i < v.Length; i += 3)
            {
                f[i / 3] = new Face(new int[3] { i, i + 1, i + 2 });
                f[i / 3].manualUV = manualUvs;

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

            if (!manualUvs)
            {
                for (int i = 0; i < f.Length; i++)
                {
                    var nrm = Math.Normal(v[f[i].indexesInternal[0]], v[f[i].indexesInternal[1]], v[f[i].indexesInternal[2]]);
                    var axis = Projection.VectorToProjectionAxis(nrm);

                    if (axis == ProjectionAxis.X)
                        f[i].textureGroup = 2;
                    else if (axis == ProjectionAxis.Y)
                        f[i].textureGroup = 3;
                    else if (axis == ProjectionAxis.Z)
                        f[i].textureGroup = 4;
                    else if (axis == ProjectionAxis.XNegative)
                        f[i].textureGroup = 5;
                    else if (axis == ProjectionAxis.YNegative)
                        f[i].textureGroup = 6;
                    else if (axis == ProjectionAxis.ZNegative)
                        f[i].textureGroup = 7;
                }
            }

            GameObject go = new GameObject();
            ProBuilderMesh pb = go.AddComponent<ProBuilderMesh>();
            pb.Clear();
            pb.positionsInternal = v;
            pb.facesInternal = f;

            if (!weldVertices)
            {
                SharedVertex[] si = new SharedVertex[v.Length];
                for (int i = 0; i < si.Length; i++)
                    si[i] = new SharedVertex(new int[] {i});

                pb.sharedVerticesInternal = si;
            }
            else
            {
                pb.sharedVerticesInternal = SharedVertex.GetSharedVerticesWithPositions(v);
            }

            pb.ToMesh();
            pb.Refresh();
            pb.gameObject.name = "Icosphere";
            //Keep backward compatibility
            if(pivotType == PivotLocation.Center)
                pb.SetPivot(pivotType);
            else
                pb.CenterPivot(new int[1]{bottomMostVertexIndex});

            pb.unwrapParameters = new UnwrapParameters()
            {
                packMargin = 30f
            };

            return pb;
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

        static Vector3[] GetCirclePoints(int segments, float radius, float circumference, Quaternion rotation, float offset)
        {
            float seg = (float)segments - 1;

            Vector3[] v = new Vector3[(segments - 1) * 2];
            v[0] = new Vector3(Mathf.Cos(((0f / seg) * circumference) * Mathf.Deg2Rad) * radius, Mathf.Sin(((0f / seg) * circumference) * Mathf.Deg2Rad) * radius, 0f);
            v[1] = new Vector3(Mathf.Cos(((1f / seg) * circumference) * Mathf.Deg2Rad) * radius, Mathf.Sin(((1f / seg) * circumference) * Mathf.Deg2Rad) * radius, 0f);

            v[0] = rotation * ((v[0] + Vector3.right * offset));
            v[1] = rotation * ((v[1] + Vector3.right * offset));

            int n = 2;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 2; i < segments; i++)
            {
                float rad = ((i / seg) * circumference) * Mathf.Deg2Rad;
                sb.AppendLine(rad.ToString());

                v[n + 0] = v[n - 1];
                v[n + 1] = rotation * (new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f) + Vector3.right * offset);

                n += 2;
            }

            return v;
        }

        /// <summary>
        /// Creates a new torus mesh.
        /// </summary>
        /// <param name="pivotType">Set the location of the shape's pivot (center or first corner).</param>
        /// <param name="rows">Set the number of horizontal divisions to create.</param>
        /// <param name="columns">Set the number of vertical divisions to create.</param>
        /// <param name="innerRadius">Set the distance from the center to the inner bounds of this shape.</param>
        /// <param name="outerRadius">Set the distance from the center to the outer bounds of this shape.</param>
        /// <param name="smooth">Set to true to mark all faces as one smoothing group; false for no smoothing groups.</param>
        /// <param name="horizontalCircumference">Set the horizontal circumference in degrees.</param>
        /// <param name="verticalCircumference">Set the vertical circumference in degrees.</param>
        /// <param name="manualUvs">
        /// By default, this value is false, and ProBuilder uses automatic UV wrapping for textures.
        /// To achieve better results, set this value to true to use manual UV unwrapping for textures instead.
        /// </param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        public static ProBuilderMesh GenerateTorus(PivotLocation pivotType, int rows, int columns, float innerRadius, float outerRadius, bool smooth, float horizontalCircumference, float verticalCircumference, bool manualUvs = false)
        {
            int clampedRows = (int)Mathf.Clamp(rows + 1, 4, 128);
            int clampedColumns = (int)Mathf.Clamp(columns + 1, 4, 128);
            float clampedRadius = Mathf.Clamp(innerRadius, .01f, 2048f);
            float clampedTubeRadius = Mathf.Clamp(outerRadius, .01f, clampedRadius - .001f);
            clampedRadius -= clampedTubeRadius;
            float clampedHorizontalCircumference = Mathf.Clamp(horizontalCircumference, .01f, 360f);
            float clampedVerticalCircumference = Mathf.Clamp(verticalCircumference, .01f, 360f);

            List<Vector3> vertices = new List<Vector3>();

            int col = clampedColumns - 1;

            Vector3[] cir = GetCirclePoints(clampedRows, clampedTubeRadius, clampedVerticalCircumference, Quaternion.Euler(Vector3.up * 0f * clampedHorizontalCircumference), clampedRadius);

            for (int i = 1; i < clampedColumns; i++)
            {
                vertices.AddRange(cir);
                Quaternion rotation = Quaternion.Euler(Vector3.up * ((i / (float)col) * clampedHorizontalCircumference));
                cir = GetCirclePoints(clampedRows, clampedTubeRadius, clampedVerticalCircumference, rotation, clampedRadius);
                vertices.AddRange(cir);
            }

            // List<int> ind = new List<int>();
            List<Face> faces = new List<Face>();
            int fc = 0;

            // faces
            for (int i = 0; i < (clampedColumns - 1) * 2; i += 2)
            {
                for (int n = 0; n < clampedRows - 1; n++)
                {
                    int a = (i + 0) * ((clampedRows - 1) * 2) + (n * 2);
                    int b = (i + 1) * ((clampedRows - 1) * 2) + (n * 2);

                    int c = (i + 0) * ((clampedRows - 1) * 2) + (n * 2) + 1;
                    int d = (i + 1) * ((clampedRows - 1) * 2) + (n * 2) + 1;

                    faces.Add(new Face(new int[] { a, b, c, b, d, c }));
                    faces[fc].smoothingGroup = smooth ? 1 : -1;
                    faces[fc].manualUV = manualUvs;

                    fc++;
                }
            }

            ProBuilderMesh pb = ProBuilderMesh.Create(vertices.ToArray(), faces.ToArray());
            pb.gameObject.name = "Torus";
            pb.SetPivot(pivotType);

            return pb;
        }
    }
}
