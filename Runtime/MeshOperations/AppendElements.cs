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
		/// <returns>The new face as referenced on the mesh.</returns>
		public static Face AppendFace(this ProBuilderMesh mesh, Vector3[] positions, Color[] colors, Vector2[] uvs, Face face)
		{
            if (positions == null)
                throw new ArgumentNullException("positions");
			int[] shared = new int[positions.Length];
			for(int i = 0; i < positions.Length; i++)
				shared[i] = -1;
			return mesh.AppendFace(positions, colors, uvs, face, shared);
		}

		internal static Face AppendFace(this ProBuilderMesh mesh, Vector3[] positions, Color[] colors, Vector2[] uvs, Face face, int[] sharedIndexes)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (positions == null)
                throw new ArgumentNullException("positions");

            if (colors == null)
                throw new ArgumentNullException("colors");

            if (uvs == null)
                throw new ArgumentNullException("uvs");

            if (face == null)
                throw new ArgumentNullException("face");

            if (sharedIndexes == null)
                throw new ArgumentNullException("sharedIndexes");

			int vertexCount = mesh.vertexCount;

			Vector3[] newPositions = new Vector3[vertexCount + positions.Length];
			Color[] newColors = new Color[vertexCount + colors.Length];
			Vector2[] newTextures = new Vector2[mesh.texturesInternal.Length + uvs.Length];

			List<Face> faces = new List<Face>(mesh.facesInternal);
			IntArray[] sharedIndices = mesh.sharedIndicesInternal;

			Array.Copy(mesh.positionsInternal, 0, newPositions, 0, vertexCount);
			Array.Copy(positions, 0, newPositions, vertexCount, positions.Length);
			Array.Copy(mesh.colorsInternal, 0, newColors, 0, vertexCount);
			Array.Copy(colors, 0, newColors, vertexCount, colors.Length);
			Array.Copy(mesh.texturesInternal, 0, newTextures, 0, mesh.texturesInternal.Length);
			Array.Copy(uvs, 0, newTextures, mesh.texturesInternal.Length, uvs.Length);

			face.ShiftIndexesToZero();
			face.ShiftIndexes(vertexCount);

			faces.Add(face);

			for(int i = 0; i < sharedIndexes.Length; i++)
				IntArrayUtility.AddValueAtIndex(ref sharedIndices, sharedIndexes[i], i+vertexCount);

			mesh.SetPositions(newPositions);
			mesh.SetColors(newColors);
			mesh.SetUVs(newTextures);
			mesh.SetSharedIndexes(sharedIndices);
			mesh.SetFaces(faces.ToArray());

			return face;
		}

		/// <summary>
		/// Append a group of new faces to the mesh. Significantly faster than calling AppendFace multiple times.
		/// </summary>
		/// <param name="mesh">The source mesh to append new faces to.</param>
		/// <param name="appendedVertices">An array of position arrays, where indices correspond to the appendedFaces parameter.</param>
		/// <param name="appendedColors">An array of colors arrays, where indices correspond to the appendedFaces parameter.</param>
		/// <param name="appendedUvs">An array of uvs arrays, where indices correspond to the appendedFaces parameter.</param>
		/// <param name="appendedFaces">An array of faces arrays, where indices correspond to the appendedFaces parameter.</param>
		/// <param name="appendedSharedIndexes">An optional mapping of each new vertice's common index. Common index refers to a triangle's index in the @"UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes" array.</param>
		/// <returns>An array of the new faces that where successfully appended to the mesh.</returns>
		public static Face[] AppendFaces(this ProBuilderMesh mesh, Vector3[][] appendedVertices, Color[][] appendedColors, Vector2[][] appendedUvs, Face[] appendedFaces, int[][] appendedSharedIndexes)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (appendedVertices == null)
                throw new ArgumentNullException("appendedVertices");

            if (appendedColors == null)
                throw new ArgumentNullException("appendedColors");

            if (appendedUvs == null)
                throw new ArgumentNullException("appendedUvs");

            if (appendedFaces == null)
                throw new ArgumentNullException("appendedFaces");

            List<Vector3> vertices = new List<Vector3>(mesh.positionsInternal);
			List<Color> colors = new List<Color>(mesh.colorsInternal);
			List<Vector2> uvs = new List<Vector2>(mesh.texturesInternal);

			List<Face> faces = new List<Face>(mesh.facesInternal);
			IntArray[] sharedIndices = mesh.sharedIndicesInternal;

			int vc = mesh.vertexCount;

			for(int i = 0; i < appendedFaces.Length; i++)
			{
				vertices.AddRange(appendedVertices[i]);
				colors.AddRange(appendedColors[i]);
				uvs.AddRange(appendedUvs[i]);

				appendedFaces[i].ShiftIndexesToZero();
				appendedFaces[i].ShiftIndexes(vc);
				faces.Add(appendedFaces[i]);

				if(appendedSharedIndexes != null && appendedVertices[i].Length != appendedSharedIndexes[i].Length)
				{
					Debug.LogError("Append Face failed because sharedIndex array does not match new vertex array.");
					return null;
				}

				if(appendedSharedIndexes != null)
				{
					for(int j = 0; j < appendedSharedIndexes[i].Length; j++)
					{
						IntArrayUtility.AddValueAtIndex(ref sharedIndices, appendedSharedIndexes[i][j], j+vc);
					}
				}
				else
				{
					for(int j = 0; j < appendedVertices[i].Length; j++)
					{
						IntArrayUtility.AddValueAtIndex(ref sharedIndices, -1, j+vc);
					}
				}

				vc = vertices.Count;
			}

			mesh.SetPositions(vertices.ToArray());
			mesh.SetColors(colors.ToArray());
			mesh.SetUVs(uvs.ToArray());
			mesh.SetFaces(faces.ToArray());
			mesh.sharedIndicesInternal = sharedIndices;

			return appendedFaces;
		}

	    /// <summary>
        /// Create a new face connecting existing vertices.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="indexes">The indices of the vertices to join with the new polygon.</param>
        /// <param name="unordered">Are the indexes in an ordered path (false), or not (true)?</param>
        /// <returns>The new face created if the action was successfull, null if action failed.</returns>
        public static Face CreatePolygon(this ProBuilderMesh mesh, IList<int> indexes, bool unordered)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

			IntArray[] sharedIndices = mesh.sharedIndicesInternal;
			Dictionary<int, int> lookup = sharedIndices.ToDictionary();
			HashSet<int> common = IntArrayUtility.GetCommonIndices(lookup, indexes);
			List<Vertex> vertices = new List<Vertex>(Vertex.GetVertices(mesh));
			List<Vertex> appendVertices = new List<Vertex>();

			foreach(int i in common)
			{
				int index = sharedIndices[i][0];
				appendVertices.Add(new Vertex(vertices[index]));
			}

			FaceRebuildData data = FaceWithVertices(appendVertices, unordered);

			if(data != null)
			{
				data.sharedIndices = common.ToList();
				List<Face> faces = new List<Face>(mesh.facesInternal);
				FaceRebuildData.Apply(new FaceRebuildData[] { data }, vertices, faces, lookup, null);
				mesh.SetVertices(vertices);
				mesh.SetFaces(faces.ToArray());
				mesh.SetSharedIndexes(lookup);

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
			var mesh = poly.mesh;
			var material = poly.material;

			if (material == null)
			{
				var renderer = poly.GetComponent<MeshRenderer>();
				material = renderer.sharedMaterial;
			}

			var res = mesh.CreateShapeFromPolygon(poly.m_Points, poly.extrude, poly.flipNormals);

			if (material != null)
			{
				foreach (var face in mesh.faces)
					face.material = material;

				// no need to do a ToMesh and Refresh here because we know every face is set to the same material
				poly.GetComponent<MeshRenderer>().sharedMaterial = material;
			}

			return res;
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

			if(Triangulation.TriangulateVertices(vertices, out triangles, false))
			{
				int[] indices = triangles.ToArray();

				if(Math.PolygonArea(vertices, indices) < Mathf.Epsilon )
				{
					mesh.Clear();
					Log.PopLogLevel();
					return new ActionResult(ActionResult.Status.Failure, "Polygon Area < Epsilon");
				}

				mesh.Clear();
				mesh.GeometryWithVerticesFaces(vertices, new Face[] { new Face(indices) });

				Vector3 nrm = Math.Normal(mesh, mesh.facesInternal[0]);

				if (Vector3.Dot(Vector3.up, nrm) > 0f)
					mesh.facesInternal[0].Reverse();

				mesh.DuplicateAndFlip(mesh.facesInternal);

				mesh.Extrude(new Face[] { mesh.facesInternal[1] }, ExtrudeMethod.IndividualFaces, extrude);

				if ((extrude < 0f && !flipNormals) || (extrude > 0f && flipNormals))
				{
					foreach(var face in mesh.facesInternal)
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

			if(Triangulation.TriangulateVertices(vertices, out triangles, unordered))
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

			for(int i = 0; i < count; i++)
			{
				List<Vertex> vertices = new List<Vertex>()
				{
					path[i],
					center,
					path[(i+1)%count]
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
			List<Vertex> vertices = new List<Vertex>(Vertex.GetVertices(mesh));
			Dictionary<int, int> lookup = mesh.sharedIndicesInternal.ToDictionary();

			foreach(Face face in faces)
			{
				FaceRebuildData data = new FaceRebuildData();

				data.vertices = new List<Vertex>();
				data.face = new Face(face);
				data.sharedIndices = new List<int>();

				Dictionary<int, int> map = new Dictionary<int, int>();
				int len = data.face.indices.Length;

				for(int i = 0; i < len; i++)
				{
					if(map.ContainsKey(face.indices[i]))
						continue;

					map.Add(face.indices[i], map.Count);
					data.vertices.Add(vertices[face.indices[i]]);
					data.sharedIndices.Add(lookup[face.indices[i]]);
				}

				int[] tris = new int[len];

				for(var i = 0; i < len; i++)
					tris[len - (i+1)] = map[data.face[i]];

				data.face.SetIndexes(tris);

				rebuild.Add(data);
			}

			FaceRebuildData.Apply(rebuild, mesh, vertices, null, lookup, null);
		}

		/// <summary>
		/// Insert a face between two edges.
		/// </summary>
		/// <param name="mesh">The source mesh.</param>
		/// <param name="a">First edge.</param>
		/// <param name="b">Second edge</param>
		/// <param name="enforcePerimiterEdgesOnly">If true, this function will not create a face bridging manifold edges.</param>
		/// <returns>The new face, or null of the action failed.</returns>
		public static Face Bridge(this ProBuilderMesh mesh, Edge a, Edge b, bool enforcePerimiterEdgesOnly = false)
		{
			IntArray[] sharedIndices = mesh.GetSharedIndexes();
			Dictionary<int, int> lookup = sharedIndices.ToDictionary();

			// Check to see if a face already exists
			if(enforcePerimiterEdgesOnly)
			{
				if( ElementSelection.GetNeighborFaces(mesh, a).Count > 1 || ElementSelection.GetNeighborFaces(mesh, b).Count > 1 )
				{
					return null;
				}
			}

			foreach(Face face in mesh.facesInternal)
			{
				if(face.edgesInternal.IndexOf(a, lookup) >= 0 && face.edgesInternal.IndexOf(b, lookup) >= 0)
				{
					Log.Warning("Face already exists between these two edges!");
					return null;
				}
			}

			Vector3[] verts = mesh.positionsInternal;
			Vector3[] v;
			Color[] c;
			int[] s;
			AutoUnwrapSettings uvs = new AutoUnwrapSettings();
			Material mat = BuiltinMaterials.DefaultMaterial;

			// Get material and UV stuff from the first edge face
			SimpleTuple<Face, Edge> faceAndEdge = null;

			if(!EdgeExtension.ValidateEdge(mesh, a, out faceAndEdge))
				EdgeExtension.ValidateEdge(mesh, b, out faceAndEdge);

			if(faceAndEdge != null)
			{
				uvs = new AutoUnwrapSettings(faceAndEdge.item1.uv);
				mat = faceAndEdge.item1.material;
			}

			// Bridge will form a triangle
			if( a.Contains(b.x, sharedIndices) || a.Contains(b.y, sharedIndices) )
			{
				v = new Vector3[3];
				c = new Color[3];
				s = new int[3];

				bool axbx = System.Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.x)], b.x) > -1;
				bool axby = System.Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.x)], b.y) > -1;

				bool aybx = System.Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.y)], b.x) > -1;
				bool ayby = System.Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.y)], b.y) > -1;

				if(axbx)
				{
					v[0] = verts[a.x];
					c[0] = mesh.colorsInternal[a.x];
					s[0] = sharedIndices.IndexOf(a.x);
					v[1] = verts[a.y];
					c[1] = mesh.colorsInternal[a.y];
					s[1] = sharedIndices.IndexOf(a.y);
					v[2] = verts[b.y];
					c[2] = mesh.colorsInternal[b.y];
					s[2] = sharedIndices.IndexOf(b.y);
				}
				else
				if(axby)
				{
					v[0] = verts[a.x];
					c[0] = mesh.colorsInternal[a.x];
					s[0] = sharedIndices.IndexOf(a.x);
					v[1] = verts[a.y];
					c[1] = mesh.colorsInternal[a.y];
					s[1] = sharedIndices.IndexOf(a.y);
					v[2] = verts[b.x];
					c[2] = mesh.colorsInternal[b.x];
					s[2] = sharedIndices.IndexOf(b.x);
				}
				else
				if(aybx)
				{
					v[0] = verts[a.y];
					c[0] = mesh.colorsInternal[a.y];
					s[0] = sharedIndices.IndexOf(a.y);
					v[1] = verts[a.x];
					c[1] = mesh.colorsInternal[a.x];
					s[1] = sharedIndices.IndexOf(a.x);
					v[2] = verts[b.y];
					c[2] = mesh.colorsInternal[b.y];
					s[2] = sharedIndices.IndexOf(b.y);
				}
				else
				if(ayby)
				{
					v[0] = verts[a.y];
					c[0] = mesh.colorsInternal[a.y];
					s[0] = sharedIndices.IndexOf(a.y);
					v[1] = verts[a.x];
					c[1] = mesh.colorsInternal[a.x];
					s[1] = sharedIndices.IndexOf(a.x);
					v[2] = verts[b.x];
					c[2] = mesh.colorsInternal[b.x];
					s[2] = sharedIndices.IndexOf(b.x);
				}

				return mesh.AppendFace(
					v,
					c,
					new Vector2[v.Length],
					new Face( axbx || axby ? new int[3] {2, 1, 0} : new int[3] {0, 1, 2}, mat, uvs, 0, -1, -1, false ),
					s);;
			}

			// Else, bridge will form a quad

			v = new Vector3[4];
			c = new Color[4];
			s = new int[4]; // shared indices index to add to

			v[0] = verts[a.x];
			c[0] = mesh.colorsInternal[a.x];
			s[0] = sharedIndices.IndexOf(a.x);
			v[1] = verts[a.y];
			c[1] = mesh.colorsInternal[a.y];
			s[1] = sharedIndices.IndexOf(a.y);

			Vector3 nrm = Vector3.Cross( verts[b.x]-verts[a.x], verts[a.y]-verts[a.x] ).normalized;
			Vector2[] planed = Projection.PlanarProject( new Vector3[4] {verts[a.x], verts[a.y], verts[b.x], verts[b.y] }, nrm );

			Vector2 ipoint = Vector2.zero;
			bool intersects = Math.GetLineSegmentIntersect(planed[0], planed[2], planed[1], planed[3], ref ipoint);

			if(!intersects)
			{
				v[2] = verts[b.x];
				c[2] = mesh.colorsInternal[b.x];
				s[2] = sharedIndices.IndexOf(b.x);
				v[3] = verts[b.y];
				c[3] = mesh.colorsInternal[b.y];
				s[3] = sharedIndices.IndexOf(b.y);
			}
			else
			{
				v[2] = verts[b.y];
				c[2] = mesh.colorsInternal[b.y];
				s[2] = sharedIndices.IndexOf(b.y);
				v[3] = verts[b.x];
				c[3] = mesh.colorsInternal[b.x];
				s[3] = sharedIndices.IndexOf(b.x);
			}

			return mesh.AppendFace(
				v,
				c,
				new Vector2[v.Length],
				new Face( new int[6] {2, 1, 0, 2, 3, 1 }, mat, uvs, 0, -1, -1, false ),
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

            if (face == null || !face.IsValid())
                throw new ArgumentNullException("face");

            if (points == null)
                throw new ArgumentNullException("points");

            List<Vertex> vertices = Vertex.GetVertices(mesh).ToList();
            List<Face> faces = new List<Face>(mesh.facesInternal);
            Dictionary<int, int> lookup = mesh.sharedIndicesInternal.ToDictionary();
            Dictionary<int, int> lookupUV = mesh.sharedIndicesUVInternal == null ? null : mesh.sharedIndicesUVInternal.ToDictionary();

            List<Edge> wound = WingedEdge.SortEdgesByAdjacency(face);

            List<Vertex> n_vertices = new List<Vertex>();
            List<int> n_shared = new List<int>();
            List<int> n_sharedUV = lookupUV != null ? new List<int>() : null;

            for (int i = 0; i < wound.Count; i++)
			{
				n_vertices.Add(vertices[wound[i].x]);
				n_shared.Add(lookup[wound[i].x]);

				if(lookupUV != null)
				{
					int uv;

					if(lookupUV.TryGetValue(wound[i].x, out uv))
						n_sharedUV.Add(uv);
					else
						n_sharedUV.Add(-1);
				}
			}

			// now insert the new points on the nearest edge
			for(int i = 0; i < points.Length; i++)
			{
				int index = -1;
				float best = Mathf.Infinity;
				Vector3 p = points[i];
				int vc = n_vertices.Count;

				for(int n = 0; n < vc; n++)
				{
					Vector3 v = n_vertices[n].position;
					Vector3 w = n_vertices[(n + 1) % vc].position;

					float dist = Math.DistancePointLineSegment(p, v, w);

					if(dist < best)
					{
						best = dist;
						index = n;
					}
				}

				Vertex left = n_vertices[index], right = n_vertices[(index+1) % vc];

				float x = (p - left.position).sqrMagnitude;
				float y = (p - right.position).sqrMagnitude;

				Vertex insert = Vertex.Mix(left, right, x / (x + y));

				n_vertices.Insert((index + 1) % vc, insert);
				n_shared.Insert((index + 1) % vc, -1);
				if(n_sharedUV != null) n_sharedUV.Insert((index + 1) % vc, -1);
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

			data.face = new Face(triangles.ToArray(), face.material, new AutoUnwrapSettings(face.uv), face.smoothingGroup, face.textureGroup, -1, face.manualUV);
			data.vertices 			= n_vertices;
			data.sharedIndices 		= n_shared;
			data.sharedIndicesUV 	= n_sharedUV;

			FaceRebuildData.Apply(	new List<FaceRebuildData>() { data },
										vertices,
										faces,
										lookup,
										lookupUV);

			var newFace = data.face;

			mesh.SetVertices(vertices);
			mesh.SetFaces(faces.ToArray());
			mesh.SetSharedIndexes(lookup);
			mesh.SetSharedIndexesUV(lookupUV);

			// check old normal and make sure this new face is pointing the same direction
			Vector3 oldNrm = Math.Normal(mesh, face);
			Vector3 newNrm = Math.Normal(mesh, newFace);

			if( Vector3.Dot(oldNrm, newNrm) < 0 )
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

            List<Vertex> vertices = new List<Vertex>(Vertex.GetVertices(mesh));
            Dictionary<int, int> lookup = mesh.sharedIndicesInternal.ToDictionary();
            Dictionary<int, int> lookupUV = mesh.sharedIndicesUVInternal.ToDictionary();
            List<int> indicesToDelete = new List<int>();
            Edge[] commonEdges = EdgeExtension.GetUniversalEdges(edges.ToArray(), lookup);
            List<Edge> distinctEdges = commonEdges.Distinct().ToList();

            Dictionary<Face, FaceRebuildData> modifiedFaces = new Dictionary<Face, FaceRebuildData>();

			int originalSharedIndicesCount = lookup.Count();
			int sharedIndicesCount = originalSharedIndicesCount;

			foreach(Edge edge in distinctEdges)
			{
				Edge localEdge = EdgeExtension.GetLocalEdgeFast(edge, mesh.sharedIndicesInternal);

				// Generate the new vertices that will be inserted on this edge
				List<Vertex> verticesToAppend = new List<Vertex>(count);

				for(int i = 0; i < count; i++)
					verticesToAppend.Add(Vertex.Mix(vertices[localEdge.x], vertices[localEdge.y], (i+1)/((float)count + 1)));

				List<SimpleTuple<Face, Edge>> adjacentFaces = ElementSelection.GetNeighborFaces(mesh, localEdge);

				// foreach face attached to common edge, append vertices
				foreach(SimpleTuple<Face, Edge> tup in adjacentFaces)
				{
					Face face = tup.item1;

					FaceRebuildData data;

					if( !modifiedFaces.TryGetValue(face, out data) )
					{
						data = new FaceRebuildData();
						data.face = new Face(new int[0], face.material, new AutoUnwrapSettings(face.uv), face.smoothingGroup, face.textureGroup, -1, face.manualUV);
						data.vertices = new List<Vertex>(ArrayUtility.ValuesWithIndices(vertices, face.distinctIndices));
						data.sharedIndices = new List<int>();
						data.sharedIndicesUV = new List<int>();

						foreach(int i in face.distinctIndices)
						{
							int shared;

							if(lookup.TryGetValue(i, out shared))
								data.sharedIndices.Add(shared);

							if(lookupUV.TryGetValue(i, out shared))
								data.sharedIndicesUV.Add(shared);
						}

						indicesToDelete.AddRange(face.distinctIndices);

						modifiedFaces.Add(face, data);
					}

					data.vertices.AddRange(verticesToAppend);

					for(int i = 0; i < count; i++)
					{
						data.sharedIndices.Add(sharedIndicesCount + i);
						data.sharedIndicesUV.Add(-1);
					}
				}

				sharedIndicesCount += count;
			}

			// now apply the changes
			List<Face> dic_face = modifiedFaces.Keys.ToList();
			List<FaceRebuildData> dic_data = modifiedFaces.Values.ToList();
			List<EdgeLookup> appendedEdges = new List<EdgeLookup>();

			for(int i = 0; i < dic_face.Count; i++)
			{
				Face face = dic_face[i];
				FaceRebuildData data = dic_data[i];

				Vector3 nrm = Math.Normal(mesh, face);
				Vector2[] projection = Projection.PlanarProject(data.vertices.Select(x => x.position).ToArray(), nrm);

				int vertexCount = vertices.Count;

				// triangulate and set new face indices to end of current vertex list
				List<int> indices;

				if(Triangulation.SortAndTriangulate(projection, out indices))
					data.face.indices = indices.ToArray();
				else
					continue;

				data.face.ShiftIndexes(vertexCount);
				face.CopyFrom(data.face);

				for(int n = 0; n < data.vertices.Count; n++)
					lookup.Add(vertexCount + n, data.sharedIndices[n]);

				if(data.sharedIndicesUV.Count == data.vertices.Count)
				{
					for(int n = 0; n < data.vertices.Count; n++)
						lookupUV.Add(vertexCount + n, data.sharedIndicesUV[n]);
				}

				vertices.AddRange(data.vertices);

				foreach(Edge e in face.edgesInternal)
				{
					EdgeLookup el = new EdgeLookup(new Edge(lookup[e.x], lookup[e.y]), e);

					if(el.common.x >= originalSharedIndicesCount || el.common.y >= originalSharedIndicesCount)
						appendedEdges.Add(el);
				}
			}

			indicesToDelete = indicesToDelete.Distinct().ToList();
			int delCount = indicesToDelete.Count;

			var newEdges = appendedEdges.Distinct().Select(x => x.local - delCount).ToList();

			mesh.SetVertices(vertices);
			mesh.SetSharedIndexes(lookup.ToIntArray());
			mesh.SetSharedIndexesUV(lookupUV.ToIntArray());
			mesh.DeleteVertices(indicesToDelete);

            return newEdges;
		}

	}
}
