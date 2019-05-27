using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Face and edge extrusion.
    /// </summary>
    public static class ExtrudeElements
    {
        /// <summary>
        /// Extrude a collection of faces.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faces">The faces to extrude.</param>
        /// <param name="method">Describes how faces are extruded.</param>
        /// <param name="distance">The distance to extrude faces.</param>
        /// <returns>An array of the faces created as a result of the extrusion. Null if the faces paramater is null or empty.</returns>
        public static Face[] Extrude(this ProBuilderMesh mesh, IEnumerable<Face> faces, ExtrudeMethod method, float distance)
        {
            switch (method)
            {
                case ExtrudeMethod.IndividualFaces:
                    return ExtrudePerFace(mesh, faces, distance);

                default:
                    return ExtrudeAsGroups(mesh, faces, method == ExtrudeMethod.FaceNormal, distance);
            }
        }

        /// <summary>
        /// Extrude a collection of edges.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="edges">The edges to extrude.</param>
        /// <param name="distance">The distance to extrude.</param>
        /// <param name="extrudeAsGroup">If true adjacent edges will be extruded retaining a shared vertex, if false the shared vertex will be split.</param>
        /// <param name="enableManifoldExtrude">Pass true to allow this function to extrude manifold edges, false to disallow.</param>
        /// <returns>The extruded edges, or null if the action failed due to manifold check or an empty edges parameter.</returns>
        public static Edge[] Extrude(this ProBuilderMesh mesh, IEnumerable<Edge> edges, float distance, bool extrudeAsGroup, bool enableManifoldExtrude)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (edges == null)
                throw new ArgumentNullException("edges");

            SharedVertex[] sharedIndexes = mesh.sharedVerticesInternal;

            List<Edge> validEdges = new List<Edge>();
            List<Face> edgeFaces = new List<Face>();

            foreach (Edge e in edges)
            {
                int faceCount = 0;
                Face fa = null;

                foreach (Face face in mesh.facesInternal)
                {
                    if (mesh.IndexOf(face.edgesInternal, e) > -1)
                    {
                        fa = face;

                        if (++faceCount > 1)
                            break;
                    }
                }

                if (enableManifoldExtrude || faceCount < 2)
                {
                    validEdges.Add(e);
                    edgeFaces.Add(fa);
                }
            }

            if (validEdges.Count < 1)
                return null;

            Vector3[] localVerts = mesh.positionsInternal;
            if (!mesh.HasArrays(MeshArrays.Normal))
                mesh.Refresh(RefreshMask.Normals);
            IList<Vector3> oNormals = mesh.normals;

            int[] allEdgeIndexes = new int[validEdges.Count * 2];
            int c = 0;
            for (int i = 0; i < validEdges.Count; i++)
            {
                allEdgeIndexes[c++] = validEdges[i].a;
                allEdgeIndexes[c++] = validEdges[i].b;
            }

            List<Edge> extrudedIndexes = new List<Edge>();
            // used to set the editor selection to the newly created edges
            List<Edge> newEdges = new List<Edge>();
            bool hasColors = mesh.HasArrays(MeshArrays.Color);

            // build out new faces around validEdges
            for (int i = 0; i < validEdges.Count; i++)
            {
                Edge edge = validEdges[i];
                Face face = edgeFaces[i];

                // Averages the normals using only vertices that are on the edge
                Vector3 xnorm = extrudeAsGroup
                    ? InternalMeshUtility.AverageNormalWithIndexes(sharedIndexes[mesh.GetSharedVertexHandle(edge.a)], allEdgeIndexes, oNormals)
                    : Math.Normal(mesh, face);

                Vector3 ynorm = extrudeAsGroup
                    ? InternalMeshUtility.AverageNormalWithIndexes(sharedIndexes[mesh.GetSharedVertexHandle(edge.b)], allEdgeIndexes, oNormals)
                    : Math.Normal(mesh, face);

                int x_sharedIndex = mesh.GetSharedVertexHandle(edge.a);
                int y_sharedIndex = mesh.GetSharedVertexHandle(edge.b);

                var positions = new Vector3[4]
                {
                    localVerts[edge.a],
                    localVerts[edge.b],
                    localVerts[edge.a] + xnorm.normalized * distance,
                    localVerts[edge.b] + ynorm.normalized * distance
                };

                var colors = hasColors
                    ? new Color[4]
                    {
                        mesh.colorsInternal[edge.a],
                        mesh.colorsInternal[edge.b],
                        mesh.colorsInternal[edge.a],
                        mesh.colorsInternal[edge.b]
                    }
                    : null;

                Face newFace = mesh.AppendFace(
                        positions,
                        colors,
                        new Vector2[4],
                        new Face(new int[6] { 2, 1, 0, 2, 3, 1 }, face.submeshIndex, AutoUnwrapSettings.tile, 0, -1, -1, false),
                        new int[4] { x_sharedIndex, y_sharedIndex, -1, -1 });

                newEdges.Add(new Edge(newFace.indexesInternal[3], newFace.indexesInternal[4]));

                extrudedIndexes.Add(new Edge(x_sharedIndex, newFace.indexesInternal[3]));
                extrudedIndexes.Add(new Edge(y_sharedIndex, newFace.indexesInternal[4]));
            }

            // merge extruded vertex indexes with each other
            if (extrudeAsGroup)
            {
                for (int i = 0; i < extrudedIndexes.Count; i++)
                {
                    int val = extrudedIndexes[i].a;

                    for (int n = 0; n < extrudedIndexes.Count; n++)
                    {
                        if (n == i)
                            continue;

                        if (extrudedIndexes[n].a == val)
                        {
                            mesh.SetVerticesCoincident(new int[] { extrudedIndexes[n].b, extrudedIndexes[i].b });
                            break;
                        }
                    }
                }
            }

            // todo Should only need to invalidate caches on affected faces
            foreach (Face f in mesh.facesInternal)
                f.InvalidateCache();

            return newEdges.ToArray();
        }

        /// <summary>
        /// Split any shared vertices so that this face may be moved independently of the main object.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faces">The faces to split from the mesh.</param>
        /// <returns>The faces created forming the detached face group.</returns>
        public static List<Face> DetachFaces(this ProBuilderMesh mesh, IEnumerable<Face> faces)
        {
            return DetachFaces(mesh, faces, true);
        }

        /// <summary>
        /// Split any shared vertices so that this face may be moved independently of the main object.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faces">The faces to split from the mesh.</param>
        /// <param name="deleteSourceFaces">Whether or not to delete the faces on the source geometry which were detached.</param>
        /// <returns>The faces created forming the detached face group.</returns>
        public static List<Face> DetachFaces(this ProBuilderMesh mesh, IEnumerable<Face> faces, bool deleteSourceFaces)
        {
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            if (faces == null)
                throw new System.ArgumentNullException("faces");

            List<Vertex> vertices = new List<Vertex>(mesh.GetVertices());
            int sharedIndexOffset = mesh.sharedVerticesInternal.Length;
            var lookup = mesh.sharedVertexLookup;

            List<FaceRebuildData> detached = new List<FaceRebuildData>();

            foreach (Face face in faces)
            {
                FaceRebuildData data = new FaceRebuildData();
                data.vertices = new List<Vertex>();
                data.sharedIndexes = new List<int>();
                data.face = new Face(face);

                Dictionary<int, int> match = new Dictionary<int, int>();
                int[] indexes = new int[face.indexesInternal.Length];

                for (int i = 0; i < face.indexesInternal.Length; i++)
                {
                    int local;

                    if (match.TryGetValue(face.indexesInternal[i], out local))
                    {
                        indexes[i] = local;
                    }
                    else
                    {
                        local = data.vertices.Count;
                        indexes[i] = local;
                        match.Add(face.indexesInternal[i], local);
                        data.vertices.Add(vertices[face.indexesInternal[i]]);
                        data.sharedIndexes.Add(lookup[face.indexesInternal[i]] + sharedIndexOffset);
                    }
                }

                data.face.indexesInternal = indexes.ToArray();
                detached.Add(data);
            }

            FaceRebuildData.Apply(detached, mesh, vertices);
            if (deleteSourceFaces)
            {
                mesh.DeleteFaces(faces);
            }

            mesh.ToMesh();

            return detached.Select(x => x.face).ToList();
        }


        /// <summary>
        /// Extrude each face in faces individually along it's normal by distance.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="faces"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        static Face[] ExtrudePerFace(ProBuilderMesh pb, IEnumerable<Face> faces, float distance)
        {
            Face[] faceArray = faces as Face[] ?? faces.ToArray();

            if (!faceArray.Any())
                return null;

            List<Vertex> vertices = new List<Vertex>(pb.GetVertices());
            int sharedIndexMax = pb.sharedVerticesInternal.Length;
            int sharedIndexOffset = 0;
            int faceIndex = 0;
            Dictionary<int, int> lookup = pb.sharedVertexLookup;
            Dictionary<int, int> lookupUV = pb.sharedTextureLookup;
            Dictionary<int, int> used = new Dictionary<int, int>();
            Face[] newFaces = new Face[faceArray.Sum(x => x.edges.Count)];

            foreach (Face face in faceArray)
            {
                face.smoothingGroup = Smoothing.smoothingGroupNone;
                face.textureGroup = -1;

                Vector3 delta = Math.Normal(pb, face) * distance;
                Edge[] edges = face.edgesInternal;

                used.Clear();

                for (int i = 0; i < edges.Length; i++)
                {
                    int vc = vertices.Count;
                    int x = edges[i].a, y = edges[i].b;

                    if (!used.ContainsKey(x))
                    {
                        used.Add(x, lookup[x]);
                        lookup[x] = sharedIndexMax + (sharedIndexOffset++);
                    }

                    if (!used.ContainsKey(y))
                    {
                        used.Add(y, lookup[y]);
                        lookup[y] = sharedIndexMax + (sharedIndexOffset++);
                    }

                    lookup.Add(vc + 0, used[x]);
                    lookup.Add(vc + 1, used[y]);
                    lookup.Add(vc + 2, lookup[x]);
                    lookup.Add(vc + 3, lookup[y]);

                    Vertex xx = new Vertex(vertices[x]), yy = new Vertex(vertices[y]);
                    xx.position += delta;
                    yy.position += delta;

                    vertices.Add(new Vertex(vertices[x]));
                    vertices.Add(new Vertex(vertices[y]));

                    vertices.Add(xx);
                    vertices.Add(yy);

                    Face bridge = new Face(
                            new int[6] { vc + 0, vc + 1, vc + 2, vc + 1, vc + 3, vc + 2 },
                            face.submeshIndex,
                            new AutoUnwrapSettings(face.uv),
                            face.smoothingGroup,
                            -1,
                            -1,
                            false
                            );

                    newFaces[faceIndex++] = bridge;
                }

                for (int i = 0; i < face.distinctIndexesInternal.Length; i++)
                {
                    vertices[face.distinctIndexesInternal[i]].position += delta;

                    // Break any UV shared connections
                    if (lookupUV != null && lookupUV.ContainsKey(face.distinctIndexesInternal[i]))
                        lookupUV.Remove(face.distinctIndexesInternal[i]);
                }
            }

            pb.SetVertices(vertices);

            var fc = pb.faceCount;
            var nc = newFaces.Length;
            var appended = new Face[fc + nc];
            Array.Copy(pb.facesInternal, 0, appended, 0, fc);
            Array.Copy(newFaces, 0, appended, fc, nc);
            pb.faces = appended;
            pb.SetSharedVertices(lookup);
            pb.SetSharedTextures(lookupUV);

            return newFaces;
        }

        /// <summary>
        /// Extrude faces as groups.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="faces"></param>
        /// <param name="compensateAngleVertexDistance"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        static Face[] ExtrudeAsGroups(ProBuilderMesh mesh, IEnumerable<Face> faces, bool compensateAngleVertexDistance, float distance)
        {
            if (faces == null || !faces.Any())
                return null;

            List<Vertex> vertices = new List<Vertex>(mesh.GetVertices());
            int sharedIndexMax = mesh.sharedVerticesInternal.Length;
            int sharedIndexOffset = 0;
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;
            Dictionary<int, int> lookupUV = mesh.sharedTextureLookup;

            List<Face> newFaces = new List<Face>();
            // old triangle index -> old shared index
            Dictionary<int, int> oldSharedMap = new Dictionary<int, int>();
            // old shared index -> new shared index
            Dictionary<int, int> newSharedMap = new Dictionary<int, int>();
            // bridge face extruded edges, maps vertex index to new extruded vertex position
            Dictionary<int, int> delayPosition = new Dictionary<int, int>();
            // used to average the direction of vertices shared by perimeter edges
            // key[shared index], value[normal count, normal sum]
            Dictionary<int, SimpleTuple<Vector3, Vector3, List<int>>> extrudeMap = new Dictionary<int, SimpleTuple<Vector3, Vector3, List<int>>>();

            List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh, faces, true);
            List<HashSet<Face>> groups = GetFaceGroups(wings);

            foreach (HashSet<Face> group in groups)
            {
                Dictionary<EdgeLookup, Face> perimeter = GetPerimeterEdges(group, lookup);

                newSharedMap.Clear();
                oldSharedMap.Clear();

                foreach (var edgeAndFace in perimeter)
                {
                    EdgeLookup edge = edgeAndFace.Key;
                    Face face = edgeAndFace.Value;

                    int vc = vertices.Count;
                    int x = edge.local.a, y = edge.local.b;

                    if (!oldSharedMap.ContainsKey(x))
                    {
                        oldSharedMap.Add(x, lookup[x]);
                        int newSharedIndex = -1;

                        if (newSharedMap.TryGetValue(lookup[x], out newSharedIndex))
                        {
                            lookup[x] = newSharedIndex;
                        }
                        else
                        {
                            newSharedIndex = sharedIndexMax + (sharedIndexOffset++);
                            newSharedMap.Add(lookup[x], newSharedIndex);
                            lookup[x] = newSharedIndex;
                        }
                    }

                    if (!oldSharedMap.ContainsKey(y))
                    {
                        oldSharedMap.Add(y, lookup[y]);
                        int newSharedIndex = -1;

                        if (newSharedMap.TryGetValue(lookup[y], out newSharedIndex))
                        {
                            lookup[y] = newSharedIndex;
                        }
                        else
                        {
                            newSharedIndex = sharedIndexMax + (sharedIndexOffset++);
                            newSharedMap.Add(lookup[y], newSharedIndex);
                            lookup[y] = newSharedIndex;
                        }
                    }

                    lookup.Add(vc + 0, oldSharedMap[x]);
                    lookup.Add(vc + 1, oldSharedMap[y]);
                    lookup.Add(vc + 2, lookup[x]);
                    lookup.Add(vc + 3, lookup[y]);

                    delayPosition.Add(vc + 2, x);
                    delayPosition.Add(vc + 3, y);

                    vertices.Add(new Vertex(vertices[x]));
                    vertices.Add(new Vertex(vertices[y]));

                    // extruded edge will be positioned later
                    vertices.Add(null);
                    vertices.Add(null);

                    Face bridge = new Face(
                            new int[6] { vc + 0, vc + 1, vc + 2, vc + 1, vc + 3, vc + 2 },
                            face.submeshIndex,
                            new AutoUnwrapSettings(face.uv),
                            Smoothing.smoothingGroupNone,
                            -1,
                            -1,
                            false
                            );

                    newFaces.Add(bridge);
                }

                foreach (Face face in group)
                {
                    // @todo keep together if possible
                    face.textureGroup = -1;

                    Vector3 normal = Math.Normal(mesh, face);

                    for (int i = 0; i < face.distinctIndexesInternal.Length; i++)
                    {
                        int idx = face.distinctIndexesInternal[i];

                        // If this vertex is on the perimeter but not part of a perimeter edge
                        // move the sharedIndex to match it's new value.
                        if (!oldSharedMap.ContainsKey(idx) && newSharedMap.ContainsKey(lookup[idx]))
                            lookup[idx] = newSharedMap[lookup[idx]];

                        int com = lookup[idx];

                        // Break any UV shared connections
                        if (lookupUV != null && lookupUV.ContainsKey(face.distinctIndexesInternal[i]))
                            lookupUV.Remove(face.distinctIndexesInternal[i]);

                        // add the normal to the list of normals for this shared vertex
                        SimpleTuple<Vector3, Vector3, List<int>> dir;

                        if (extrudeMap.TryGetValue(com, out dir))
                        {
                            dir.item1 += normal;
                            dir.item3.Add(idx);
                            extrudeMap[com] = dir;
                        }
                        else
                        {
                            extrudeMap.Add(com, new SimpleTuple<Vector3, Vector3, List<int>>(normal, normal, new List<int>() { idx }));
                        }
                    }
                }
            }

            foreach (var kvp in extrudeMap)
            {
                Vector3 direction = (kvp.Value.item1 / kvp.Value.item3.Count);
                direction.Normalize();

                // If extruding by face normal extend vertices on seams by the hypotenuse
                float modifier = compensateAngleVertexDistance ? Math.Secant(Vector3.Angle(direction, kvp.Value.item2) * Mathf.Deg2Rad) : 1f;

                direction.x *= distance * modifier;
                direction.y *= distance * modifier;
                direction.z *= distance * modifier;

                foreach (int i in kvp.Value.item3)
                {
                    vertices[i].position += direction;
                }
            }

            foreach (var kvp in delayPosition)
                vertices[kvp.Key] = new Vertex(vertices[kvp.Value]);

            mesh.SetVertices(vertices);

            var fc = mesh.faceCount;
            var nc = newFaces.Count;
            var appended = new Face[fc + nc];
            Array.Copy(mesh.facesInternal, 0, appended, 0, fc);
            for (int i = fc, c = fc + nc; i < c; i++)
                appended[i] = newFaces[i - fc];
            mesh.faces = appended;
            mesh.SetSharedVertices(lookup);
            mesh.SetSharedTextures(lookupUV);

            return newFaces.ToArray();
        }

        static List<HashSet<Face>> GetFaceGroups(List<WingedEdge> wings)
        {
            HashSet<Face> used = new HashSet<Face>();
            List<HashSet<Face>> groups = new List<HashSet<Face>>();

            foreach (WingedEdge wing in wings)
            {
                if (used.Add(wing.face))
                {
                    HashSet<Face> group = new HashSet<Face>() { wing.face };

                    ElementSelection.Flood(wing, group);

                    foreach (Face f in group)
                        used.Add(f);

                    groups.Add(group);
                }
            }

            return groups;
        }

        static Dictionary<EdgeLookup, Face> GetPerimeterEdges(HashSet<Face> faces, Dictionary<int, int> lookup)
        {
            Dictionary<EdgeLookup, Face> perimeter = new Dictionary<EdgeLookup, Face>();
            HashSet<EdgeLookup> used = new HashSet<EdgeLookup>();

            foreach (Face face in faces)
            {
                foreach (Edge edge in face.edgesInternal)
                {
                    EdgeLookup e = new EdgeLookup(lookup[edge.a], lookup[edge.b], edge.a, edge.b);

                    if (!used.Add(e))
                    {
                        if (perimeter.ContainsKey(e))
                            perimeter.Remove(e);
                    }
                    else
                    {
                        perimeter.Add(e, face);
                    }
                }
            }

            return perimeter;
        }
    }
}
