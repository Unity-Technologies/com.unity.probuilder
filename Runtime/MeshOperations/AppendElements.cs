using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using System;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Functions for appending elements to meshes.
    /// </summary>
    public static class AppendElements
    {
        /// <summary>
        /// Append a new face to the ProBuilderMesh.
        /// </summary>
        /// <param name="mesh">The mesh target.</param>
        /// <param name="positions">The new vertex positions to add.</param>
        /// <param name="colors">The new colors to add (must match positions length).</param>
        /// <param name="uvs">The new uvs to add (must match positions length).</param>
        /// <param name="face">A face with the new triangle indexes. The indexes should be 0 indexed.</param>
        /// <param name="common"></param>
        /// <returns>The new face as referenced on the mesh.</returns>
        internal static Face AppendFace(this ProBuilderMesh mesh, Vector3[] positions, Color[] colors, Vector2[] uvs, Face face, int[] common)
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
            var mt = mesh.HasArrays(MeshArrays.Texture0);
            var ft = uvs != null;

            Vector3[] newPositions = new Vector3[vertexCount + faceVertexCount];
            Color[] newColors = (mc || fc) ? new Color[vertexCount + faceVertexCount] : null;
            Vector2[] newTextures = (mt || ft) ? new Vector2[vertexCount + faceVertexCount] : null;

            List<Face> faces = new List<Face>(mesh.facesInternal);
            Array.Copy(mesh.positionsInternal, 0, newPositions, 0, vertexCount);
            Array.Copy(positions, 0, newPositions, vertexCount, faceVertexCount);

            if (mc || fc)
            {
                Array.Copy(mc ? mesh.colorsInternal : ArrayUtility.Fill(Color.white, vertexCount), 0, newColors, 0, vertexCount);
                Array.Copy(fc ? colors : ArrayUtility.Fill(Color.white, faceVertexCount), 0, newColors, vertexCount, colors.Length);
            }

            if (mt || ft)
            {
                Array.Copy(mt ? mesh.texturesInternal : ArrayUtility.Fill(Vector2.zero, vertexCount), 0, newTextures, 0, vertexCount);
                Array.Copy(ft ? uvs : ArrayUtility.Fill(Vector2.zero, faceVertexCount), 0, newTextures, mesh.texturesInternal.Length, faceVertexCount);
            }

            face.ShiftIndexesToZero();
            face.ShiftIndexes(vertexCount);

            faces.Add(face);

            for (int i = 0; i < common.Length; i++)
            {
                if (common[i] < 0)
                    mesh.AddSharedVertex(new SharedVertex(new int[] { i + vertexCount }));
                else
                    mesh.AddToSharedVertex(common[i], i + vertexCount);
            }

            mesh.positions = newPositions;
            mesh.colors = newColors;
            mesh.textures = newTextures;
            mesh.faces = faces;

            return face;
        }

        /// <summary>
        /// Append a group of new faces to the mesh. Significantly faster than calling AppendFace multiple times.
        /// </summary>
        /// <param name="mesh">The source mesh to append new faces to.</param>
        /// <param name="positions">An array of position arrays, where indexes correspond to the appendedFaces parameter.</param>
        /// <param name="colors">An array of colors arrays, where indexes correspond to the appendedFaces parameter.</param>
        /// <param name="uvs">An array of uvs arrays, where indexes correspond to the appendedFaces parameter.</param>
        /// <param name="faces">An array of faces arrays, which contain the triangle winding information for each new face. Face index values are 0 indexed.</param>
        /// <param name="shared">An optional mapping of each new vertex's common index. Common index refers to a triangle's index in the @"UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes" array. If this value is provided, it must contain entries for each vertex position. Ex, if there are 4 vertices in this face, there must be shared index entries for { 0, 1, 2, 3 }.</param>
        /// <returns>An array of the new faces that where successfully appended to the mesh.</returns>
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
        /// Create a new face connecting existing vertices.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="indexes">The indexes of the vertices to join with the new polygon.</param>
        /// <param name="unordered">Are the indexes in an ordered path (false), or not (true)? If indexes are not ordered this function will treat the polygon as a convex shape. Ordered paths will be triangulated allowing concave shapes.</param>
        /// <returns>The new face created if the action was successfull, null if action failed.</returns>
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
                FaceRebuildData.Apply(new FaceRebuildData[] { data }, vertices, faces, lookup, null);
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
        /// Create a poly shape from a set of points on a plane. The points must be ordered.
        /// </summary>
        /// <param name="poly"></param>
        /// <returns>An action result indicating the status of the operation.</returns>
        internal static ActionResult CreateShapeFromPolygon(this PolyShape poly)
        {
            return poly.mesh.CreateShapeFromPolygon(poly.m_Points, poly.extrude, poly.flipNormals);
        }

        /// <summary>
        /// Rebuild a mesh from an ordered set of points.
        /// </summary>
        /// <param name="mesh">The target mesh. The mesh values will be cleared and repopulated with the shape extruded from points.</param>
        /// <param name="points">A path of points to triangulate and extrude.</param>
        /// <param name="extrude">The distance to extrude.</param>
        /// <param name="flipNormals">If true the faces will be inverted at creation.</param>
        /// <returns>An ActionResult with the status of the operation.</returns>
        public static ActionResult CreateShapeFromPolygon(this ProBuilderMesh mesh, IList<Vector3> points, float extrude, bool flipNormals)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (points == null || points.Count < 3)
            {
                mesh.Clear();
                mesh.ToMesh();
                mesh.Refresh();
                return new ActionResult(ActionResult.Status.NoChange, "Too Few Points");
            }

            Vector3[] vertices = points.ToArray();
            List<int> triangles;

            Log.PushLogLevel(LogLevel.Error);

            if (Triangulation.TriangulateVertices(vertices, out triangles, false))
            {
                int[] indexes = triangles.ToArray();

                if (Math.PolygonArea(vertices, indexes) < Mathf.Epsilon)
                {
                    mesh.Clear();
                    Log.PopLogLevel();
                    return new ActionResult(ActionResult.Status.Failure, "Polygon Area < Epsilon");
                }

                mesh.Clear();

                mesh.positionsInternal = vertices;
                mesh.facesInternal = new[] { new Face(indexes) };
                mesh.sharedVerticesInternal = SharedVertex.GetSharedVerticesWithPositions(vertices);
                mesh.InvalidateCaches();

                Vector3 nrm = Math.Normal(mesh, mesh.facesInternal[0]);

                if (Vector3.Dot(Vector3.up, nrm) > 0f)
                    mesh.facesInternal[0].Reverse();

                mesh.DuplicateAndFlip(mesh.facesInternal);

                mesh.Extrude(new Face[] { mesh.facesInternal[1] }, ExtrudeMethod.IndividualFaces, extrude);

                if ((extrude < 0f && !flipNormals) || (extrude > 0f && flipNormals))
                {
                    foreach (var face in mesh.facesInternal)
                        face.Reverse();
                }

                mesh.ToMesh();
                mesh.Refresh();
            }
            else
            {
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
                data.face = new Face(triangles.ToArray());
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
                data.face = new Face(new int[] {0 , 1, 2});

                faces.Add(data);
            }

            return faces;
        }

        /// <summary>
        /// Duplicate and reverse the winding direction for each face.
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
        /// Insert a face between two edges.
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
                if (ElementSelection.GetNeighborFaces(mesh, a).Count > 1 || ElementSelection.GetNeighborFaces(mesh, b).Count > 1)
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

            if (EdgeUtility.ValidateEdge(mesh, a, out faceAndEdge) || EdgeUtility.ValidateEdge(mesh, b, out faceAndEdge))
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
                    new Face(axbx || axby ? new int[3] {2, 1, 0} : new int[3] {0, 1, 2}, submeshIndex, uvs, 0, -1, -1, false),
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
            Vector2[] planed = Projection.PlanarProject(new Vector3[4] { positions[a.a], positions[a.b], positions[b.a], positions[b.b] }, null, nrm);

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
                new Face(new int[6] {2, 1, 0, 2, 3, 1 }, submeshIndex, uvs, 0, -1, -1, false),
                s);
        }

        /// <summary>
        /// Add a set of points to a face and retriangulate. Points are added to the nearest edge.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="face">The face to append points to.</param>
        /// <param name="points">Points to added to the face.</param>
        /// <returns>The face created by appending the points.</returns>
        public static Face AppendVerticesToFace(this ProBuilderMesh mesh, Face face, Vector3[] points)
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

            List<int> triangles;

            try
            {
                Triangulation.TriangulateVertices(n_vertices, out triangles, false);
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

            FaceRebuildData.Apply(new List<FaceRebuildData>() { data },
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
        /// Insert a number of new points to an edge. Points are evenly spaced out along the edge.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="edge">The edge to split with points.</param>
        /// <param name="count">The number of new points to insert. Must be greater than 0.</param>
        /// <returns>The new edges created by inserting points.</returns>
        public static List<Edge> AppendVerticesToEdge(this ProBuilderMesh mesh, Edge edge, int count)
        {
            return AppendVerticesToEdge(mesh, new Edge[] { edge }, count);
        }

        /// <summary>
        /// Insert a number of new points to each edge. Points are evenly spaced out along the edge.
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
                    verticesToAppend.Add(Vertex.Mix(vertices[localEdge.a], vertices[localEdge.b], (i + 1) / ((float)count + 1)));

                List<SimpleTuple<Face, Edge>> adjacentFaces = ElementSelection.GetNeighborFaces(mesh, localEdge);

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

                    data.vertices.AddRange(verticesToAppend);

                    for (int i = 0; i < count; i++)
                    {
                        data.sharedIndexes.Add(sharedIndexesCount + i);
                        data.sharedIndexesUV.Add(-1);
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

                Vector3 nrm = Math.Normal(mesh, face);
                Vector2[] projection = Projection.PlanarProject(data.vertices.Select(x => x.position).ToArray(), null, nrm);

                int vertexCount = vertices.Count;

                // triangulate and set new face indexes to end of current vertex list
                List<int> indexes;

                if (Triangulation.SortAndTriangulate(projection, out indexes))
                    data.face.indexesInternal = indexes.ToArray();
                else
                    continue;

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
    }
}
