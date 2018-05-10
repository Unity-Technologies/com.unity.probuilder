using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using System;

namespace UnityEngine.ProBuilder.MeshOperations
{
	/// <summary>
	/// Functions for appending or deleting faces from meshes.
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
			return poly.mesh.CreateShapeFromPolygon(poly.points, poly.extrude, poly.flipNormals);
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

				for(int i = 0; i < len; i++)
					data.face.indices[i] = map[data.face.indices[i]];

				data.face.InvalidateCache();
				rebuild.Add(data);
			}

			FaceRebuildData.Apply(rebuild, mesh, vertices, null, lookup, null);
		}

		/// <summary>
		/// Insert a face between two edges.
		/// </summary>
		/// <param name="mesh"></param>
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
	}
}
