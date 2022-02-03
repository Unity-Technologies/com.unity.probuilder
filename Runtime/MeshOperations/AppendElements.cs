using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using System;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Contains functions for appending elements to meshes.
    /// </summary>
    public static class AppendElements
    {
        /// <summary>
        /// Appends a new face to the ProBuilderMesh.
        /// </summary>
        /// <param name="mesh">The mesh target.</param>
        /// <param name="positions">The new vertex positions to add.</param>
        /// <param name="colors">An array of new colors to add (the length of this array must match positions length).</param>
        /// <param name="uv0s">An array of new UV0s to add (the length of this array must match positions length).</param>
        /// <param name="uv2s">An array of new UV2s to add (the length of this array must match positions length).</param>
        /// <param name="uv3s">An array of new UV3s to add (the length of this array must match positions length).</param>
        /// <param name="face">The face with the new triangle indices. The indices should be 0 indexed.</param>
        /// <param name="common">An array of the vertex indices that are shared.</param>
        /// <returns>The new face as referenced on the mesh.</returns>
        internal static Face AppendFace(
			this ProBuilderMesh mesh,
			Vector3[] positions,
			Color[] colors,
			Vector2[] uv0s,
			Vector4[] uv2s,
			Vector4[] uv3s,
            Face face,
			int[] common)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (positions == null)
                throw new ArgumentNullException("positions");

            if (face == null)
                throw new ArgumentNullException("face");

            int faceVertexCount = positions.Length;

            if (common == null)
            {
                common = new int[faceVertexCount];
                for (int i = 0; i < faceVertexCount; i++)
                    common[i] = -1;
            }

            int vertexCount = mesh.vertexCount;

            var mc = mesh.HasArrays(MeshArrays.Color);
            var fc = colors != null;
            var mt0 = mesh.HasArrays(MeshArrays.Texture0);
            var ft0 = uv0s != null;
			var mt2 = mesh.HasArrays(MeshArrays.Texture2);
            var ft2 = uv2s != null;
			var mt3 = mesh.HasArrays(MeshArrays.Texture3);
            var ft3 = uv3s != null;

            Vector3[] newPositions = new Vector3[vertexCount + faceVertexCount];
            Color[] newColors = (mc || fc) ? new Color[vertexCount + faceVertexCount] : null;
            Vector2[] newTexture0s = (mt0 || ft0) ? new Vector2[vertexCount + faceVertexCount] : null;
            List<Vector4> newTexture2s = (mt2 || ft2) ? new List<Vector4>() : null;
            List<Vector4> newTexture3s = (mt3 || ft3) ? new List<Vector4>() : null;

            List<Face> faces = new List<Face>(mesh.facesInternal);
            Array.Copy(mesh.positionsInternal, 0, newPositions, 0, vertexCount);
            Array.Copy(positions, 0, newPositions, vertexCount, faceVertexCount);

            if (mc || fc)
            {
                Array.Copy(mc ? mesh.colorsInternal : ArrayUtility.Fill(Color.white, vertexCount), 0, newColors, 0,
                    vertexCount);
                Array.Copy(fc ? colors : ArrayUtility.Fill(Color.white, faceVertexCount), 0, newColors, vertexCount,
                    colors.Length);
            }

            if (mt0 || ft0)
            {
                Array.Copy(mt0 ? mesh.texturesInternal : ArrayUtility.Fill(Vector2.zero, vertexCount), 0, newTexture0s, 0,
                    vertexCount);
                Array.Copy(ft0 ? uv0s : ArrayUtility.Fill(Vector2.zero, faceVertexCount), 0, newTexture0s,
                    mesh.texturesInternal.Length, faceVertexCount);
            }

			if (mt2 || ft2)
            {
				newTexture2s.AddRange(mt2 ? mesh.textures2Internal : new Vector4[vertexCount].ToList());
				newTexture2s.AddRange(ft2 ? uv2s : new Vector4[faceVertexCount]);
            }

			if (mt3 || ft3)
            {
				newTexture3s.AddRange(mt3 ? mesh.textures3Internal : new Vector4[vertexCount].ToList());
				newTexture3s.AddRange(ft3 ? uv3s : new Vector4[faceVertexCount]);
            }

            face.ShiftIndexesToZero();
            face.ShiftIndexes(vertexCount);

            faces.Add(face);

            for (int i = 0; i < common.Length; i++)
            {
                if (common[i] < 0)
                    mesh.AddSharedVertex(new SharedVertex(new int[] {i + vertexCount}));
                else
                    mesh.AddToSharedVertex(common[i], i + vertexCount);
            }

            mesh.positions = newPositions;
            mesh.colors = newColors;
            mesh.textures = newTexture0s;
            mesh.faces = faces;
			mesh.textures2Internal = newTexture2s;
			mesh.textures3Internal = newTexture3s;

            return face;
        }

        /// <summary>
        /// Appends a group of new faces to the ProBuilderMesh.
        /// </summary>
        /// <param name="mesh">The source mesh to append new faces to.</param>
        /// <param name="positions">An array of position arrays, where the indices correspond to the `faces` parameter.</param>
        /// <param name="colors">An array of colors arrays, where the indices correspond to the `faces` parameter.</param>
        /// <param name="uvs">An array of UVs arrays, where the indices correspond to the `faces` parameter.</param>
        /// <param name="faces">An array of Face arrays, which contain the triangle winding information for each new face. Face index values are 0 indexed.</param>
        /// <param name="shared">
        /// An optional mapping of each new vertex's common index. Common index refers to a triangle's index in the <see cref="ProBuilderMesh.sharedVertices"/> array.
        /// If you provide this value, include entries for each vertex position. For example, if there are four vertices in this face, there must be shared index entries for `{ 0, 1, 2, 3 }`.
        /// </param>
        /// <returns>An array of the new faces that this method successfully appended to the mesh; null if it failed.</returns>
        public static Face[] AppendFaces(
            this ProBuilderMesh mesh,
            Vector3[][] positions,
            Color[][] colors,
            Vector2[][] uvs,
            Face[] faces,
            int[][] shared)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (positions == null)
                throw new ArgumentNullException("positions");

            if (colors == null)
                throw new ArgumentNullException("colors");

            if (uvs == null)
                throw new ArgumentNullException("uvs");

            if (faces == null)
                throw new ArgumentNullException("faces");

            var newPositions = new List<Vector3>(mesh.positionsInternal);
            var newColors = new List<Color>(mesh.colorsInternal);
            var newTextures = new List<Vector2>(mesh.texturesInternal);
            var newFaces = new List<Face>(mesh.facesInternal);
            var lookup = mesh.sharedVertexLookup;

            int vc = mesh.vertexCount;

            for (int i = 0; i < faces.Length; i++)
            {
                newPositions.AddRange(positions[i]);
                newColors.AddRange(colors[i]);
                newTextures.AddRange(uvs[i]);

                faces[i].ShiftIndexesToZero();
                faces[i].ShiftIndexes(vc);
                newFaces.Add(faces[i]);

                if (shared != null && positions[i].Length != shared[i].Length)
                {
                    Debug.LogError("Append Face failed because shared array does not match new vertex array.");
                    return null;
                }

                var hasCommon = shared != null;

                for (int j = 0; j < shared[i].Length; j++)
                    lookup.Add(j + vc, hasCommon ? shared[i][j] : -1);

                vc = newPositions.Count;
            }

            mesh.positions = newPositions;
            mesh.colors = newColors;
            mesh.textures = newTextures;
            mesh.faces = newFaces;
            mesh.SetSharedVertices(lookup);

            return faces;
        }

        /// <summary>
        /// Creates a new face that connects existing vertices.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="indexes">The indices of the vertices to join with the new polygon.</param>
        /// <param name="unordered">
        /// False if the indices in an ordered path; true if not.
        /// For unordered indices, this function treats the polygon as a convex shape.
        /// ProBuilder allows concave shapes when triangulating ordered paths.
        /// </param>
        /// <returns>The new face that this action successfully created; null if action failed.</returns>
        public static Face CreatePolygon(this ProBuilderMesh mesh, IList<int> indexes, bool unordered)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            SharedVertex[] sharedIndexes = mesh.sharedVerticesInternal;
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;
            HashSet<int> common = mesh.GetSharedVertexHandles(indexes);
            List<Vertex> vertices = new List<Vertex>(mesh.GetVertices());
            List<Vertex> appendVertices = new List<Vertex>();

            foreach (int i in common)
            {
                int index = sharedIndexes[i][0];
                appendVertices.Add(new Vertex(vertices[index]));
            }

            FaceRebuildData data = FaceWithVertices(appendVertices, unordered);

            if (data != null)
            {
                data.sharedIndexes = common.ToList();
                List<Face> faces = new List<Face>(mesh.facesInternal);
                FaceRebuildData.Apply(new FaceRebuildData[] {data}, vertices, faces, lookup, null);
                mesh.SetVertices(vertices);
                mesh.faces = faces;
                mesh.SetSharedVertices(lookup);

                return data.face;
            }

            const string insufficientPoints = "Too Few Unique Points Selected";
            const string badWinding = "Points not ordered correctly";

            Log.Info(unordered ? insufficientPoints : badWinding);

            return null;
        }


        /// <summary>
        /// Creates a new face by connecting existing vertices.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="indexes">The indexes of the vertices to join with the new polygon.</param>
        /// <param name="holes">A list of indices defining holes.</param>
        /// <returns>The new face that this action successfully created; null if action failed.</returns>
        public static Face CreatePolygonWithHole(this ProBuilderMesh mesh, IList<int> indexes, IList<IList<int>> holes)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            SharedVertex[] sharedIndexes = mesh.sharedVerticesInternal;
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;
            List<Vertex> vertices = new List<Vertex>(mesh.GetVertices());

            HashSet<int> commonVertices = mesh.GetSharedVertexHandles(indexes);
            List<Vertex> appendVertices = new List<Vertex>();
            foreach (int i in commonVertices)
            {
                int index = sharedIndexes[i][0];
                appendVertices.Add(new Vertex(vertices[index]));
            }

            HashSet<int> common = commonVertices;
            List< HashSet<int> > commonHoles = new List<HashSet<int>>();
            List< List<Vertex> > appendHoles = new List<List<Vertex>>();
            for (int i = 0; i < holes.Count; i++)
            {
                commonHoles.Add(mesh.GetSharedVertexHandles(holes[i]));
                List<Vertex> currentHole = new List<Vertex>();
                appendHoles.Add(currentHole);

                foreach (int j in commonHoles[i])
                {
                    common.Add(j);
                    int index = sharedIndexes[j][0];
                    currentHole.Add(new Vertex(vertices[index]));
                }
            }

            FaceRebuildData data = FaceWithVerticesAndHole(appendVertices, appendHoles);

            if (data != null)
            {
                data.sharedIndexes = common.ToList();
                List<Face> faces = new List<Face>(mesh.facesInternal);
                FaceRebuildData.Apply(new FaceRebuildData[] { data }, vertices, faces, lookup, null);
                mesh.SetVertices(vertices);
                mesh.faces = faces;
                mesh.SetSharedVertices(lookup);

                return data.face;
            }

            return null;
        }

        /// <summary>
        /// Creates a custom polygon shape from a set of points on a plane. The points must be ordered.
        /// </summary>
        /// <param name="poly">The <see cref="PolyShape"/> component to rebuild.</param>
        /// <returns>An action result indicating the status of the operation.</returns>
        public static ActionResult CreateShapeFromPolygon(this PolyShape poly)
        {
            return poly.mesh.CreateShapeFromPolygon(poly.m_Points, poly.extrude, poly.flipNormals);
        }

        /// <summary>
        /// Clear and refresh mesh in case of failure to create a shape.
        /// </summary>
        /// <param name="mesh"></param>
        internal static void ClearAndRefreshMesh(this ProBuilderMesh mesh)
        {
            mesh.Clear();
            mesh.ToMesh();
            mesh.Refresh();
        }

        /// <summary>
        /// Rebuilds a mesh from an ordered set of points.
        /// </summary>
        /// <param name="mesh">The target mesh. This method clears and repopulates the mesh values with the shape extruded from points.</param>
        /// <param name="points">A path of points to triangulate and extrude.</param>
        /// <param name="extrude">The distance to extrude.</param>
        /// <param name="flipNormals">True to invert the faces when creating the <see cref="PolyShape"/>.</param>
        /// <returns>An ActionResult with the status of the operation.</returns>
        public static ActionResult CreateShapeFromPolygon(this ProBuilderMesh mesh, IList<Vector3> points,
            float extrude, bool flipNormals)
        {
            return CreateShapeFromPolygon(mesh, points, extrude, flipNormals, null);
        }

        /// <summary>
        /// Rebuilds a mesh from an ordered set of points.
        /// </summary>
        /// <param name="mesh">The target mesh. Clears and repopulates the mesh values with the shape extruded from points.</param>
        /// <param name="points">A path of points to triangulate and extrude.</param>
        /// <param name="extrude">The distance to extrude.</param>
        /// <param name="flipNormals">True to invert the faces when creating them.</param>
        /// <param name="cameraLookAt">This argument is ignored.</param>
        /// <param name="holePoints">Holes in the polygon.</param>
        /// <returns>An ActionResult with the status of the operation.</returns>
        [Obsolete("Face.CreateShapeFromPolygon is deprecated as it no longer relies on camera look at.")]
        public static ActionResult CreateShapeFromPolygon(this ProBuilderMesh mesh, IList<Vector3> points,
            float extrude, bool flipNormals, Vector3 cameraLookAt, IList<IList<Vector3>> holePoints = null)
        {
            return CreateShapeFromPolygon(mesh, points, extrude, flipNormals, null);
        }

        /// <summary>
        /// Rebuilds a mesh from an ordered set of points.
        /// </summary>
        /// <param name="mesh">The target mesh. Clears and repopulates the mesh values with the shape extruded from points.</param>
        /// <param name="points">A path of points to triangulate and extrude.</param>
        /// <param name="extrude">The distance to extrude.</param>
        /// <param name="flipNormals">True to invert the faces when creating them.</param>
        /// <param name="holePoints">Holes in the polygon. Specify null if you want this method to ignore this value.</param>
        /// <returns>An ActionResult with the status of the operation.</returns>
        public static ActionResult CreateShapeFromPolygon(this ProBuilderMesh mesh, IList<Vector3> points,
            float extrude, bool flipNormals, IList<IList<Vector3>> holePoints)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (points == null || points.Count < 3)
            {
                ClearAndRefreshMesh(mesh);
                return new ActionResult(ActionResult.Status.NoChange, "Too Few Points");
            }

            Vector3[] vertices = points.ToArray();

            Vector3[][] holeVertices = null;
            if (holePoints != null && holePoints.Count > 0)
            {
                holeVertices = new Vector3[holePoints.Count][];
                for (int i = 0; i < holePoints.Count; i++)
                {
                    if (holePoints[i] == null || holePoints[i].Count < 3)
                    {
                        ClearAndRefreshMesh(mesh);
                        return new ActionResult(ActionResult.Status.NoChange, "Too Few Points in hole " + i);
                    }

                    holeVertices[i] = holePoints[i].ToArray();
                }
            }

            List<int> triangles;

            Log.PushLogLevel(LogLevel.Error);

            if (Triangulation.TriangulateVertices(vertices, out triangles, holeVertices))
            {
                Vector3[] combinedVertices = null;
                if (holeVertices != null)
                {
                    combinedVertices = new Vector3[vertices.Length + holeVertices.Sum(arr => arr.Length)];
                    Array.Copy(vertices, combinedVertices, vertices.Length);
                    int destinationIndex = vertices.Length;
                    foreach (var hole in holeVertices)
                    {
                        Array.ConstrainedCopy(hole, 0, combinedVertices, destinationIndex, hole.Length);
                        destinationIndex += hole.Length;
                    }
                }
                else
                {
                    combinedVertices = vertices;
                }

                int[] indexes = triangles.ToArray();

                if (Math.PolygonArea(combinedVertices, indexes) < Mathf.Epsilon)
                {
                    ClearAndRefreshMesh(mesh);
                    Log.PopLogLevel();
                    return new ActionResult(ActionResult.Status.Failure, "Polygon Area < Epsilon");
                }

                mesh.Clear();

                mesh.positionsInternal = combinedVertices;
                var newFace = new Face(indexes);
                mesh.facesInternal = new[] {newFace};
                mesh.sharedVerticesInternal = SharedVertex.GetSharedVerticesWithPositions(combinedVertices);
                mesh.InvalidateCaches();

                // check that all points are represented in the triangulation
                if (newFace.distinctIndexesInternal.Length != combinedVertices.Length)
                {
                    ClearAndRefreshMesh(mesh);
                    Log.PopLogLevel();
                    return new ActionResult(ActionResult.Status.Failure, "Triangulation missing points");
                }

                Vector3 nrm = Math.Normal(mesh, mesh.facesInternal[0]);
                nrm = mesh.gameObject.transform.TransformDirection(nrm);
                if ((flipNormals
                    ? Vector3.Dot(mesh.gameObject.transform.up, nrm) > 0f
                    : Vector3.Dot(mesh.gameObject.transform.up, nrm) < 0f))
                {
                    mesh.facesInternal[0].Reverse();
                }

                if (extrude != 0.0f)
                {
                    mesh.DuplicateAndFlip(mesh.facesInternal);

                    mesh.Extrude(new Face[] {(flipNormals ? mesh.facesInternal[1] : mesh.facesInternal[0])},
                        ExtrudeMethod.IndividualFaces, extrude);

                    if ((extrude < 0f && !flipNormals) || (extrude > 0f && flipNormals))
                    {
                        foreach (var face in mesh.facesInternal)
                            face.Reverse();
                    }
                }

                mesh.ToMesh();
                mesh.Refresh();
            }
            else
            {
                // clear mesh instead of showing an invalid one
                ClearAndRefreshMesh(mesh);
                Log.PopLogLevel();
                return new ActionResult(ActionResult.Status.Failure, "Failed Triangulating Points");
            }

            Log.PopLogLevel();

            return new ActionResult(ActionResult.Status.Success, "Create Polygon Shape");
        }

        /// <summary>
        /// Create a new face given a set of unordered vertices (or ordered, if unordered param is set to false).
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="unordered"></param>
        /// <returns></returns>
        internal static FaceRebuildData FaceWithVertices(List<Vertex> vertices, bool unordered = true)
        {
            List<int> triangles;

            if (Triangulation.TriangulateVertices(vertices, out triangles, unordered))
            {
                FaceRebuildData data = new FaceRebuildData();
                data.vertices = vertices;
                data.face = new Face(triangles);
                return data;
            }

            return null;
        }

        /// <summary>
        /// Create a new face given a set of ordered vertices and vertices making holes in the face.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="holes"></param>
        /// <returns></returns>
        internal static FaceRebuildData FaceWithVerticesAndHole(List<Vertex> borderVertices, List<List<Vertex>> holes)
        {
            List<int> triangles;

            Vector3[] verticesV3 = borderVertices.Select(v => v.position).ToArray();
            Vector3[][] holesV3 = new Vector3[holes.Count][];

            for (int i = 0; i < holesV3.Length; i++)
            {
                holesV3[i] = holes[i].Select(v => v.position).ToArray();
            }

            if (Triangulation.TriangulateVertices(verticesV3, out triangles, holesV3))
            {
                List<Vertex> vertices = new List<Vertex>();
                vertices.AddRange(borderVertices);
                foreach (var hole in holes)
                {
                    vertices.AddRange(hole);
                }

                FaceRebuildData data = new FaceRebuildData();
                data.vertices = vertices;
                data.face = new Face(triangles);
                return data;
            }

            return null;
        }


        /// <summary>
        /// Given a path of vertices, inserts a new vertex in the center inserts triangles along the path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static List<FaceRebuildData> TentCapWithVertices(List<Vertex> path)
        {
            int count = path.Count;
            Vertex center = Vertex.Average(path);
            List<FaceRebuildData> faces = new List<FaceRebuildData>();

            for (int i = 0; i < count; i++)
            {
                List<Vertex> vertices = new List<Vertex>()
                {
                    path[i],
                    center,
                    path[(i + 1) % count]
                };

                FaceRebuildData data = new FaceRebuildData();
                data.vertices = vertices;
                data.face = new Face(new int[] {0, 1, 2});

                faces.Add(data);
            }

            return faces;
        }

        /// <summary>
        /// Duplicates and reverses the winding direction for each face.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <param name="faces">The faces to duplicate, reverse triangle winding order, and append to mesh.</param>
        public static void DuplicateAndFlip(this ProBuilderMesh mesh, Face[] faces)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (faces == null)
                throw new ArgumentNullException("faces");

            List<FaceRebuildData> rebuild = new List<FaceRebuildData>();
            List<Vertex> vertices = new List<Vertex>(mesh.GetVertices());
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;

            foreach (Face face in faces)
            {
                FaceRebuildData data = new FaceRebuildData();

                data.vertices = new List<Vertex>();
                data.face = new Face(face);
                data.sharedIndexes = new List<int>();

                Dictionary<int, int> map = new Dictionary<int, int>();
                int len = data.face.indexesInternal.Length;

                for (int i = 0; i < len; i++)
                {
                    if (map.ContainsKey(face.indexesInternal[i]))
                        continue;

                    map.Add(face.indexesInternal[i], map.Count);
                    data.vertices.Add(vertices[face.indexesInternal[i]]);
                    data.sharedIndexes.Add(lookup[face.indexesInternal[i]]);
                }

                int[] tris = new int[len];

                for (var i = 0; i < len; i++)
                    tris[len - (i + 1)] = map[data.face[i]];

                data.face.SetIndexes(tris);

                rebuild.Add(data);
            }

            FaceRebuildData.Apply(rebuild, mesh, vertices);
        }

        /// <summary>
        /// Inserts a face between two edges.
        ///
        /// This is the equivalent of the [Bridge Edges](../manual/Edge_Bridge.html) action.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="a">First edge.</param>
        /// <param name="b">Second edge</param>
        /// <param name="allowNonManifoldGeometry">If true, this function will allow edges to be bridged that create overlapping (non-manifold) faces.</param>
        /// <returns>The new face, or null of the action failed.</returns>
        public static Face Bridge(this ProBuilderMesh mesh, Edge a, Edge b, bool allowNonManifoldGeometry = false)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            SharedVertex[] sharedVertices = mesh.sharedVerticesInternal;
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;

            // Check to see if a face already exists
            if (!allowNonManifoldGeometry)
            {
                if (ElementSelection.GetNeighborFaces(mesh, a).Count > 1 ||
                    ElementSelection.GetNeighborFaces(mesh, b).Count > 1)
                {
                    return null;
                }
            }

            foreach (Face face in mesh.facesInternal)
            {
                if (mesh.IndexOf(face.edgesInternal, a) >= 0 && mesh.IndexOf(face.edgesInternal, b) >= 0)
                {
                    Log.Warning("Face already exists between these two edges!");
                    return null;
                }
            }

            Vector3[] positions = mesh.positionsInternal;
            bool hasColors = mesh.HasArrays(MeshArrays.Color);
            Color[] colors = hasColors ? mesh.colorsInternal : null;

            Vector3[] v;
            Color[] c;
            int[] s;
            AutoUnwrapSettings uvs = AutoUnwrapSettings.tile;
            int submeshIndex = 0;

            // Get material and UV stuff from the first edge face
            SimpleTuple<Face, Edge> faceAndEdge;

            if (EdgeUtility.ValidateEdge(mesh, a, out faceAndEdge) ||
                EdgeUtility.ValidateEdge(mesh, b, out faceAndEdge))
            {
                uvs = new AutoUnwrapSettings(faceAndEdge.item1.uv);
                submeshIndex = faceAndEdge.item1.submeshIndex;
            }

            // Bridge will form a triangle
            if (a.Contains(b.a, lookup) || a.Contains(b.b, lookup))
            {
                v = new Vector3[3];
                c = new Color[3];
                s = new int[3];

                bool axbx = Array.IndexOf(sharedVertices[mesh.GetSharedVertexHandle(a.a)].arrayInternal, b.a) > -1;
                bool axby = Array.IndexOf(sharedVertices[mesh.GetSharedVertexHandle(a.a)].arrayInternal, b.b) > -1;

                bool aybx = Array.IndexOf(sharedVertices[mesh.GetSharedVertexHandle(a.b)].arrayInternal, b.a) > -1;
                bool ayby = Array.IndexOf(sharedVertices[mesh.GetSharedVertexHandle(a.b)].arrayInternal, b.b) > -1;

                if (axbx)
                {
                    v[0] = positions[a.a];
                    if (hasColors) c[0] = colors[a.a];
                    s[0] = mesh.GetSharedVertexHandle(a.a);
                    v[1] = positions[a.b];
                    if (hasColors) c[1] = colors[a.b];
                    s[1] = mesh.GetSharedVertexHandle(a.b);
                    v[2] = positions[b.b];
                    if (hasColors) c[2] = colors[b.b];
                    s[2] = mesh.GetSharedVertexHandle(b.b);
                }
                else if (axby)
                {
                    v[0] = positions[a.a];
                    if (hasColors) c[0] = colors[a.a];
                    s[0] = mesh.GetSharedVertexHandle(a.a);
                    v[1] = positions[a.b];
                    if (hasColors) c[1] = colors[a.b];
                    s[1] = mesh.GetSharedVertexHandle(a.b);
                    v[2] = positions[b.a];
                    if (hasColors) c[2] = colors[b.a];
                    s[2] = mesh.GetSharedVertexHandle(b.a);
                }
                else if (aybx)
                {
                    v[0] = positions[a.b];
                    if (hasColors) c[0] = colors[a.b];
                    s[0] = mesh.GetSharedVertexHandle(a.b);
                    v[1] = positions[a.a];
                    if (hasColors) c[1] = colors[a.a];
                    s[1] = mesh.GetSharedVertexHandle(a.a);
                    v[2] = positions[b.b];
                    if (hasColors) c[2] = colors[b.b];
                    s[2] = mesh.GetSharedVertexHandle(b.b);
                }
                else if (ayby)
                {
                    v[0] = positions[a.b];
                    if (hasColors) c[0] = colors[a.b];
                    s[0] = mesh.GetSharedVertexHandle(a.b);
                    v[1] = positions[a.a];
                    if (hasColors) c[1] = colors[a.a];
                    s[1] = mesh.GetSharedVertexHandle(a.a);
                    v[2] = positions[b.a];
                    if (hasColors) c[2] = colors[b.a];
                    s[2] = mesh.GetSharedVertexHandle(b.a);
                }

                return mesh.AppendFace(
                    v,
                    hasColors ? c : null,
                    new Vector2[v.Length],
                    new Vector4[v.Length],
                    new Vector4[v.Length],
                    new Face(axbx || axby ? new int[3] {2, 1, 0} : new int[3] {0, 1, 2}, submeshIndex, uvs, 0, -1, -1,
                        false),
                    s);
            }

            // Else, bridge will form a quad

            v = new Vector3[4];
            c = new Color[4];
            s = new int[4]; // shared indexes index to add to

            v[0] = positions[a.a];
            if (hasColors)
                c[0] = mesh.colorsInternal[a.a];
            s[0] = mesh.GetSharedVertexHandle(a.a);
            v[1] = positions[a.b];
            if (hasColors)
                c[1] = mesh.colorsInternal[a.b];
            s[1] = mesh.GetSharedVertexHandle(a.b);

            Vector3 nrm = Vector3.Cross(positions[b.a] - positions[a.a], positions[a.b] - positions[a.a]).normalized;
            Vector2[] planed =
                Projection.PlanarProject(
                    new Vector3[4] {positions[a.a], positions[a.b], positions[b.a], positions[b.b]}, null, nrm);

            Vector2 ipoint = Vector2.zero;
            bool intersects = Math.GetLineSegmentIntersect(planed[0], planed[2], planed[1], planed[3], ref ipoint);

            if (!intersects)
            {
                v[2] = positions[b.a];
                if (hasColors)
                    c[2] = mesh.colorsInternal[b.a];
                s[2] = mesh.GetSharedVertexHandle(b.a);
                v[3] = positions[b.b];
                if (hasColors)
                    c[3] = mesh.colorsInternal[b.b];
                s[3] = mesh.GetSharedVertexHandle(b.b);
            }
            else
            {
                v[2] = positions[b.b];
                if (hasColors)
                    c[2] = mesh.colorsInternal[b.b];
                s[2] = mesh.GetSharedVertexHandle(b.b);
                v[3] = positions[b.a];
                if (hasColors)
                    c[3] = mesh.colorsInternal[b.a];
                s[3] = mesh.GetSharedVertexHandle(b.a);
            }

            return mesh.AppendFace(
                v,
                hasColors ? c : null,
                new Vector2[v.Length],
                new Vector4[v.Length],
                new Vector4[v.Length],
                new Face(new int[6] {2, 1, 0, 2, 3, 1}, submeshIndex, uvs, 0, -1, -1, false),
                s);
        }

        // backwards compatibility prevents us from just using insertOnEdge as an optional parameter
        /// <summary>
        /// Adds a set of points to a face and re-triangulates. Points are added to the nearest edge.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="face">The face to append points to.</param>
        /// <param name="points">Points to add to the face.</param>
        /// <returns>The face created by appending the points.</returns>
        public static Face AppendVerticesToFace(this ProBuilderMesh mesh, Face face, Vector3[] points)
        {
            return AppendVerticesToFace(mesh, face, points, true);
        }

        /// <summary>
        /// Adds a set of points to a face and re-triangulates.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="face">The face to append points to.</param>
        /// <param name="points">Points to add to the face.</param>
        /// <param name="insertOnEdge">True to force new points to snap to edges.</param>
        /// <returns>The face created by appending the points.</returns>
        public static Face AppendVerticesToFace(this ProBuilderMesh mesh, Face face, Vector3[] points, bool insertOnEdge)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (face == null)
                throw new ArgumentNullException("face");

            if (points == null)
                throw new ArgumentNullException("points");

            List<Vertex> vertices = mesh.GetVertices().ToList();
            List<Face> faces = new List<Face>(mesh.facesInternal);
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;
            Dictionary<int, int> lookupUV = null;

            if (mesh.sharedTextures != null)
            {
                lookupUV = new Dictionary<int, int>();
                SharedVertex.GetSharedVertexLookup(mesh.sharedTextures, lookupUV);
            }

            List<Edge> wound = WingedEdge.SortEdgesByAdjacency(face);

            List<Vertex> n_vertices = new List<Vertex>();
            List<int> n_shared = new List<int>();
            List<int> n_sharedUV = lookupUV != null ? new List<int>() : null;

            for (int i = 0; i < wound.Count; i++)
            {
                n_vertices.Add(vertices[wound[i].a]);
                n_shared.Add(lookup[wound[i].a]);

                if (lookupUV != null)
                {
                    int uv;

                    if (lookupUV.TryGetValue(wound[i].a, out uv))
                        n_sharedUV.Add(uv);
                    else
                        n_sharedUV.Add(-1);
                }
            }

            if (insertOnEdge)
            {
                // now insert the new points on the nearest edge
                for (int i = 0; i < points.Length; i++)
                {
                    int index = -1;
                    float best = Mathf.Infinity;
                    Vector3 p = points[i];
                    int vc = n_vertices.Count;

                    for (int n = 0; n < vc; n++)
                    {
                        Vector3 v = n_vertices[n].position;
                        Vector3 w = n_vertices[(n + 1) % vc].position;

                        float dist = Math.DistancePointLineSegment(p, v, w);

                        if (dist < best)
                        {
                            best = dist;
                            index = n;
                        }
                    }

                    Vertex left = n_vertices[index], right = n_vertices[(index + 1) % vc];

                    float x = (p - left.position).sqrMagnitude;
                    float y = (p - right.position).sqrMagnitude;

                    Vertex insert = Vertex.Mix(left, right, x / (x + y));

                    n_vertices.Insert((index + 1) % vc, insert);
                    n_shared.Insert((index + 1) % vc, -1);
                    if (n_sharedUV != null) n_sharedUV.Insert((index + 1) % vc, -1);
                }
            }
            else
            {
                for (int i = 0; i < points.Length; i++)
                {
                    int index = -1;
                    Vector3 p = points[i];
                    int vc = n_vertices.Count;

                    Vertex insert = new Vertex();//Vertex.Mix(left, right, x / (x + y));
                    insert.position = p;

                    n_vertices.Insert((index + 1) % vc, insert);
                    n_shared.Insert((index + 1) % vc, -1);
                    if (n_sharedUV != null) n_sharedUV.Insert((index + 1) % vc, -1);
                }

            }

            List<int> triangles;

            try
            {
                Triangulation.TriangulateVertices(n_vertices, out triangles, true);
            }
            catch
            {
                Debug.Log("Failed triangulating face after appending vertices.");
                return null;
            }

            FaceRebuildData data = new FaceRebuildData();

            data.face = new Face(triangles.ToArray(), face.submeshIndex, new AutoUnwrapSettings(face.uv),
                face.smoothingGroup, face.textureGroup, -1, face.manualUV);
            data.vertices = n_vertices;
            data.sharedIndexes = n_shared;
            data.sharedIndexesUV = n_sharedUV;

            FaceRebuildData.Apply(new List<FaceRebuildData>() {data},
                vertices,
                faces,
                lookup,
                lookupUV);

            var newFace = data.face;

            mesh.SetVertices(vertices);
            mesh.faces = faces;
            mesh.SetSharedVertices(lookup);
            mesh.SetSharedTextures(lookupUV);

            // check old normal and make sure this new face is pointing the same direction
            Vector3 oldNrm = Math.Normal(mesh, face);
            Vector3 newNrm = Math.Normal(mesh, newFace);

            if (Vector3.Dot(oldNrm, newNrm) < 0)
                newFace.Reverse();

            mesh.DeleteFace(face);

            return newFace;
        }

        /// <summary>
        /// Inserts a number of new points on an edge. Points are evenly spaced out along the edge.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="edge">The edge to split with points.</param>
        /// <param name="count">The number of new points to insert. Must be greater than 0.</param>
        /// <returns>The new edges created by inserting points.</returns>
        public static List<Edge> AppendVerticesToEdge(this ProBuilderMesh mesh, Edge edge, int count)
        {
            return AppendVerticesToEdge(mesh, new Edge[] {edge}, count);
        }

        /// <summary>
        /// Inserts a number of new points on each edge in the specified set of edges. Points are evenly spaced out along the edge.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="edges">The edges to split with points.</param>
        /// <param name="count">The number of new points to insert. Must be greater than 0.</param>
        /// <returns>The new edges created by inserting points.</returns>
        public static List<Edge> AppendVerticesToEdge(this ProBuilderMesh mesh, IList<Edge> edges, int count)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (edges == null)
                throw new ArgumentNullException("edges");

            if (count < 1 || count > 512)
            {
                Log.Error("New edge vertex count is less than 1 or greater than 512.");
                return null;
            }

            List<Vertex> vertices = new List<Vertex>(mesh.GetVertices());
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;
            Dictionary<int, int> lookupUV = mesh.sharedTextureLookup;
            List<int> indexesToDelete = new List<int>();
            IEnumerable<Edge> commonEdges = EdgeUtility.GetSharedVertexHandleEdges(mesh, edges);
            List<Edge> distinctEdges = commonEdges.Distinct().ToList();

            Dictionary<Face, FaceRebuildData> modifiedFaces = new Dictionary<Face, FaceRebuildData>();

            int originalSharedIndexesCount = lookup.Count();
            int sharedIndexesCount = originalSharedIndexesCount;

            foreach (Edge edge in distinctEdges)
            {
                Edge localEdge = EdgeUtility.GetEdgeWithSharedVertexHandles(mesh, edge);

                // Generate the new vertices that will be inserted on this edge
                List<Vertex> verticesToAppend = new List<Vertex>(count);

                for (int i = 0; i < count; i++)
                    verticesToAppend.Add(Vertex.Mix(vertices[localEdge.a], vertices[localEdge.b],
                        (i + 1) / ((float) count + 1)));

                List<SimpleTuple<Face, Edge>> adjacentFaces = ElementSelection.GetNeighborFaces(mesh, localEdge);
                Edge edgeLookUp = new Edge(lookup[localEdge.a], lookup[localEdge.b]);
                Edge e = new Edge();

                // foreach face attached to common edge, append vertices
                foreach (SimpleTuple<Face, Edge> tup in adjacentFaces)
                {
                    Face face = tup.item1;

                    FaceRebuildData data;

                    if (!modifiedFaces.TryGetValue(face, out data))
                    {
                        data = new FaceRebuildData();
                        data.face = new Face(new int[0], face.submeshIndex, new AutoUnwrapSettings(face.uv),
                            face.smoothingGroup, face.textureGroup, -1, face.manualUV);
                        data.vertices =
                            new List<Vertex>(ArrayUtility.ValuesWithIndexes(vertices, face.distinctIndexesInternal));
                        data.sharedIndexes = new List<int>();
                        data.sharedIndexesUV = new List<int>();

                        foreach (int i in face.distinctIndexesInternal)
                        {
                            int shared;

                            if (lookup.TryGetValue(i, out shared))
                                data.sharedIndexes.Add(shared);

                            if (lookupUV.TryGetValue(i, out shared))
                                data.sharedIndexesUV.Add(shared);
                        }

                        indexesToDelete.AddRange(face.distinctIndexesInternal);

                        modifiedFaces.Add(face, data);

                        //Ordering vertices in the new face
                        List<Vertex> orderedVertices = new List<Vertex>();
                        List<int> orderedSharedIndexes = new List<int>();
                        List<int> orderedSharedUVIndexes = new List<int>();
                        List<Edge> peripheralEdges = WingedEdge.SortEdgesByAdjacency(face);

                        for (int i = 0; i < peripheralEdges.Count; i++)
                        {
                            e.a = peripheralEdges[i].a;
                            e.b = peripheralEdges[i].b;

                            orderedVertices.Add(vertices[e.a]);

                            int shared;
                            if (lookup.TryGetValue(e.a, out shared))
                                orderedSharedIndexes.Add(shared);

                            if (lookupUV.TryGetValue(i, out shared))
                                data.sharedIndexesUV.Add(shared);

                            if (edgeLookUp.a == lookup[e.a] && edgeLookUp.b == lookup[e.b])
                            {
                                for (int j = 0; j < count; j++)
                                {
                                    orderedVertices.Add(verticesToAppend[j]);
                                    orderedSharedIndexes.Add(sharedIndexesCount + j);
                                    orderedSharedUVIndexes.Add(-1);
                                }
                            }
                            else if (edgeLookUp.a == lookup[e.b] && edgeLookUp.b == lookup[e.a])
                            {
                                for (int j = count - 1; j >= 0; j--)
                                {
                                    orderedVertices.Add(verticesToAppend[j]);
                                    orderedSharedIndexes.Add(sharedIndexesCount + j);
                                    orderedSharedUVIndexes.Add(-1);
                                }
                            }
                        }

                        data.vertices = orderedVertices;
                        data.sharedIndexes = orderedSharedIndexes;
                        data.sharedIndexesUV = orderedSharedUVIndexes;
                    }
                    else
                    {
                        //Get ordered vertices in the existing face and add new ones
                        List<Vertex> orderedVertices = data.vertices;
                        List<int> orderedSharedIndexes = data.sharedIndexes;
                        List<int> orderedSharedUVIndexes = data.sharedIndexesUV;

                        for (int i = 0; i < orderedVertices.Count; i++)
                        {
                            Vertex edgeStart = orderedVertices[i];
                            int edgeStartIndex = vertices.IndexOf(edgeStart);

                            Vertex edgeEnd = orderedVertices[(i + 1) % orderedVertices.Count];
                            int edgeEndIndex = vertices.IndexOf(edgeEnd);

                            if (edgeStartIndex == -1 || edgeEndIndex == -1)
                                continue;

                            if (lookup[edgeStartIndex] == lookup[localEdge.a] &&
                                lookup[edgeEndIndex] == lookup[localEdge.b])
                            {
                                orderedVertices.InsertRange(i + 1, verticesToAppend);
                                for (int j = 0; j < count; j++)
                                {
                                    orderedSharedIndexes.Insert(i + j + 1, sharedIndexesCount + j);
                                    orderedSharedUVIndexes.Add(-1);
                                }

                            }
                            else if (lookup[edgeStartIndex] == lookup[localEdge.b] &&
                                     lookup[edgeEndIndex] == lookup[localEdge.a])
                            {
                                verticesToAppend.Reverse();
                                orderedVertices.InsertRange(i + 1, verticesToAppend);
                                for (int j = count - 1; j >= 0; j--)
                                {
                                    orderedSharedIndexes.Insert(i + 1, sharedIndexesCount + j);
                                    orderedSharedUVIndexes.Add(-1);
                                }
                            }
                        }

                        data.vertices = orderedVertices;
                        data.sharedIndexes = orderedSharedIndexes;
                        data.sharedIndexesUV = orderedSharedUVIndexes;

                    }
                }

                sharedIndexesCount += count;

            }

            // now apply the changes
            List<Face> dic_face = modifiedFaces.Keys.ToList();
            List<FaceRebuildData> dic_data = modifiedFaces.Values.ToList();
            List<EdgeLookup> appendedEdges = new List<EdgeLookup>();

            for (int i = 0; i < dic_face.Count; i++)
            {
                Face face = dic_face[i];
                FaceRebuildData data = dic_data[i];

                int vertexCount = vertices.Count;
                // triangulate and set new face indexes to end of current vertex list
                List<int> triangles;

                if (Triangulation.TriangulateVertices(data.vertices, out triangles, false))
                    data.face = new Face(triangles);
                else
                    continue;

                //Keep submesh index when rebuilding to maintain material references
                data.face.submeshIndex = face.submeshIndex;
                data.face.ShiftIndexes(vertexCount);
                face.CopyFrom(data.face);

                for (int n = 0; n < data.vertices.Count; n++)
                    lookup.Add(vertexCount + n, data.sharedIndexes[n]);

                if (data.sharedIndexesUV.Count == data.vertices.Count)
                {
                    for (int n = 0; n < data.vertices.Count; n++)
                        lookupUV.Add(vertexCount + n, data.sharedIndexesUV[n]);
                }

                vertices.AddRange(data.vertices);

                foreach (Edge e in face.edgesInternal)
                {
                    EdgeLookup el = new EdgeLookup(new Edge(lookup[e.a], lookup[e.b]), e);

                    if (el.common.a >= originalSharedIndexesCount || el.common.b >= originalSharedIndexesCount)
                        appendedEdges.Add(el);
                }
            }

            indexesToDelete = indexesToDelete.Distinct().ToList();
            int delCount = indexesToDelete.Count;

            var newEdges = appendedEdges.Distinct().Select(x => x.local - delCount).ToList();

            mesh.SetVertices(vertices);
            mesh.SetSharedVertices(lookup);
            mesh.SetSharedTextures(lookupUV);
            mesh.DeleteVertices(indexesToDelete);

            return newEdges;
        }

        /// <summary>
        /// Adds a set of points to a face and retriangulates. Points are added to the nearest edge.
        ///
        /// This is the equivalent of the [Connect Vertices](../manual/Vert_Connect.html) action.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="face">The face to append points to.</param>
        /// <param name="point">Point to add to the face.</param>
        /// <returns>The face created by appending the points.</returns>
        public static Face[] InsertVertexInFace(this ProBuilderMesh mesh, Face face, Vector3 point)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (face == null)
                throw new ArgumentNullException("face");

            if (point == null)
                throw new ArgumentNullException("point");

            List<Vertex> vertices = mesh.GetVertices().ToList();
            List<Face> faces = new List<Face>(mesh.facesInternal);
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;
            Dictionary<int, int> lookupUV = null;

            if (mesh.sharedTextures != null)
            {
                lookupUV = new Dictionary<int, int>();
                SharedVertex.GetSharedVertexLookup(mesh.sharedTextures, lookupUV);
            }

            List<Edge> wound = WingedEdge.SortEdgesByAdjacency(face);
            List<FaceRebuildData> newFacesData = new List<FaceRebuildData>();

            Vertex newVertex = new Vertex();
            newVertex.position = point;

            for (int i = 0; i < wound.Count; i++)
            {
                List<Vertex> n_vertices = new List<Vertex>();
                List<int> n_shared = new List<int>();
                List<int> n_sharedUV = lookupUV != null ? new List<int>() : null;

                n_vertices.Add(vertices[wound[i].a]);
                n_vertices.Add(vertices[wound[i].b]);
                n_vertices.Add(newVertex);

                n_shared.Add(lookup[wound[i].a]);
                n_shared.Add(lookup[wound[i].b]);
                n_shared.Add(vertices.Count);

                if (lookupUV != null)
                {
                    int uv;
                    lookupUV.Clear();

                    if (lookupUV.TryGetValue(wound[i].a, out uv))
                        n_sharedUV.Add(uv);
                    else
                        n_sharedUV.Add(-1);

                    if (lookupUV.TryGetValue(wound[i].b, out uv))
                        n_sharedUV.Add(uv);
                    else
                        n_sharedUV.Add(-1);

                    n_sharedUV.Add(vertices.Count);
                }

                List<int> triangles;

                try
                {
                    Triangulation.TriangulateVertices(n_vertices, out triangles, true);
                }
                catch
                {
                    Debug.Log("Failed triangulating face after appending vertices.");
                    return null;
                }

                FaceRebuildData data = new FaceRebuildData();

                data.face = new Face(triangles.ToArray(), face.submeshIndex, new AutoUnwrapSettings(face.uv), face.smoothingGroup, face.textureGroup, -1, face.manualUV);
                data.vertices           = n_vertices;
                data.sharedIndexes      = n_shared;
                data.sharedIndexesUV    = n_sharedUV;

                newFacesData.Add(data);
            }

            FaceRebuildData.Apply(newFacesData,
                vertices,
                faces,
                lookup,
                lookupUV);


            mesh.SetVertices(vertices);
            mesh.faces = faces;
            mesh.SetSharedVertices(lookup);
            mesh.SetSharedTextures(lookupUV);

            Face[] newFaces = newFacesData.Select(f => f.face).ToArray();

            foreach (FaceRebuildData data in newFacesData)
            {
                var newFace = data.face;

                // check old normal and make sure this new face is pointing the same direction
                Vector3 oldNrm = UnityEngine.ProBuilder.Math.Normal(mesh, face);
                Vector3 newNrm = UnityEngine.ProBuilder.Math.Normal(mesh, newFace);

                if (Vector3.Dot(oldNrm, newNrm) < 0)
                    newFace.Reverse();
            }

            mesh.DeleteFace(face);

            return newFaces;
        }

        /// <summary>
        /// Inserts a new point on an edge. Points are evenly spaced out along the edge.
        ///
        /// This is the equivalent of the [Subdivide Edges](../manual/Edge_Subdivide.html) action.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="originalEdge">The edge to add the point to.</param>
        /// <param name="point">The point to insert on the edge.</param>
        /// <returns>The new Vertex created.</returns>//
        public static Vertex InsertVertexOnEdge(this ProBuilderMesh mesh, Edge originalEdge, Vector3 point)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (originalEdge == null)
                throw new ArgumentNullException("edge");

            List<Vertex> vertices = new List<Vertex>(mesh.GetVertices());
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;
            Dictionary<int, int> lookupUV = mesh.sharedTextureLookup;
            List<int> indexesToDelete = new List<int>();
            Dictionary<Face, FaceRebuildData> modifiedFaces = new Dictionary<Face, FaceRebuildData>();

            int originalSharedIndexesCount = lookup.Count();

            //Ensure the new point is on the edge
            //Using Scalar projection
            Vector3 a = point -
                        vertices[originalEdge.a].position;
            Vector3 b = vertices[originalEdge.b].position -
                        vertices[originalEdge.a].position;

            float weight = Vector3.Magnitude(a) * Mathf.Cos(Vector3.Angle(b, a) * Mathf.Deg2Rad) / Vector3.Magnitude(b);

            Vertex newVertex = Vertex.Mix(vertices[originalEdge.a], vertices[originalEdge.b], weight);

            List<SimpleTuple<Face, Edge>> adjacentFaces = ElementSelection.GetNeighborFaces(mesh, originalEdge);
            Edge uni = new Edge(lookup[originalEdge.a], lookup[originalEdge.b]);
            Edge e = new Edge();

            // foreach face attached to common edge, append vertices
            foreach (SimpleTuple<Face, Edge> tup in adjacentFaces)
            {
                Face face = tup.item1;

                FaceRebuildData data;

                if (!modifiedFaces.TryGetValue(face, out data))
                {
                    data = new FaceRebuildData();
                    data.face = new Face(new int[0], face.submeshIndex, new AutoUnwrapSettings(face.uv), face.smoothingGroup, face.textureGroup, -1, face.manualUV);
                    data.vertices = new List<Vertex>(ArrayUtility.ValuesWithIndexes(vertices, face.distinctIndexesInternal));
                    data.sharedIndexes = new List<int>();
                    data.sharedIndexesUV = new List<int>();

                    foreach (int i in face.distinctIndexesInternal)
                    {
                        int shared;

                        if (lookup.TryGetValue(i, out shared))
                            data.sharedIndexes.Add(shared);

                        if (lookupUV.TryGetValue(i, out shared))
                            data.sharedIndexesUV.Add(shared);
                    }

                    indexesToDelete.AddRange(face.distinctIndexesInternal);

                    modifiedFaces.Add(face, data);
                }

                data.vertices.Add(newVertex);

                data.sharedIndexes.Add(originalSharedIndexesCount);
                data.sharedIndexesUV.Add(-1);

                List<Vertex> orderedVertices = new List<Vertex>();
                List<int> orderedSharedIndexes = new List<int>();
                List<Edge> peripheralEdges = WingedEdge.SortEdgesByAdjacency(face);

                bool canAdd = true;
                for (int i = 0; i < peripheralEdges.Count; i++)
                {
                    e.a = peripheralEdges[i].a;
                    e.b = peripheralEdges[i].b;

                    orderedVertices.Add(vertices[e.a]);

                    int shared;
                    if (lookup.TryGetValue(e.a, out shared))
                        orderedSharedIndexes.Add(shared);

                    if (canAdd &&
                        (uni.a == lookup[e.a] && uni.b == lookup[e.b]) ||
                        (uni.a == lookup[e.b] && uni.b == lookup[e.a]))
                    {
                        canAdd = false;
                        orderedVertices.Add(data.vertices[data.vertices.Count-1]);
                        orderedSharedIndexes.Add(originalSharedIndexesCount);
                    }
                }

                data.vertices = orderedVertices;
                data.sharedIndexes = orderedSharedIndexes;
            }

            // now apply the changes
            List<Face> dic_face = modifiedFaces.Keys.ToList();
            List<FaceRebuildData> dic_data = modifiedFaces.Values.ToList();

            for (int i = 0; i < dic_face.Count; i++)
            {
                Face face = dic_face[i];
                FaceRebuildData data = dic_data[i];

                int vertexCount = vertices.Count;

                // triangulate and set new face indexes to end of current vertex list
                List<int> triangles;

                if (Triangulation.TriangulateVertices(data.vertices, out triangles, false))
                    data.face = new Face(triangles);
                else
                    continue;

                //Keep submesh index when rebuilding to maintain material references
                data.face.submeshIndex = face.submeshIndex;
                data.face.ShiftIndexes(vertexCount);
                face.CopyFrom(data.face);

                for (int n = 0; n < data.vertices.Count; n++)
                    lookup.Add(vertexCount + n, data.sharedIndexes[n]);

                if (data.sharedIndexesUV.Count == data.vertices.Count)
                {
                    for (int n = 0; n < data.vertices.Count; n++)
                        lookupUV.Add(vertexCount + n, data.sharedIndexesUV[n]);
                }

                vertices.AddRange(data.vertices);

            }

            indexesToDelete = indexesToDelete.Distinct().ToList();

            mesh.SetVertices(vertices);
            mesh.SetSharedVertices(lookup);
            mesh.SetSharedTextures(lookupUV);
            mesh.DeleteVertices(indexesToDelete);


            return newVertex;
        }

        /// <summary>
        /// Adds a point to a mesh.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="point">Point to add to the face.</param>
        /// <param name="normal">The inserted point's normal.</param>
        /// <returns>The new inserted Vertex.</returns>
        public static Vertex InsertVertexInMesh(this ProBuilderMesh mesh, Vector3 point, Vector3 normal)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (point == null)
                throw new ArgumentNullException("point");

            List<Vertex> vertices = mesh.GetVertices().ToList();
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;
            Dictionary<int, int> lookupUV = null;

            // List<int> indexesToDelete = new List<int>();
            int originalSharedIndexesCount = lookup.Count();

            if (mesh.sharedTextures != null)
            {
                lookupUV = new Dictionary<int, int>();
                SharedVertex.GetSharedVertexLookup(mesh.sharedTextures, lookupUV);
            }

            Vertex newVertex = new Vertex();
            newVertex.position = point;
            newVertex.normal = normal.normalized;
            vertices.Add(newVertex);

            lookup.Add(originalSharedIndexesCount,originalSharedIndexesCount);
            lookupUV.Add(originalSharedIndexesCount,-1);

            mesh.SetVertices(vertices);
            mesh.SetSharedVertices(lookup);
            mesh.SetSharedTextures(lookupUV);

            return newVertex;
        }

    }
}
