using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.MeshOperations
{
	static class pb_VertexOps
	{
		/// <summary>
		/// Collapses all passed indices to a single shared index.  Retains vertex normals.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="indices"></param>
		/// <param name="collapsedIndex"></param>
		/// <param name="collapseToFirst"></param>
		/// <returns></returns>
		public static bool MergeVertices(this pb_Object pb, int[] indices, out int collapsedIndex, bool collapseToFirst = false)
		{
			pb_Vertex[] vertices = pb_Vertex.GetVertices(pb);

			pb_Vertex cen = collapseToFirst ? vertices[indices[0]] : pb_Vertex.Average(vertices, indices);

			pb_IntArray[] sharedIndices = pb.sharedIndices;
			pb_IntArray[] sharedIndicesUV = pb.sharedIndicesUV;

			int newIndex = pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, indices);
			pb_IntArrayUtility.MergeSharedIndices(ref sharedIndicesUV, indices);

			pb.SetSharedIndices(sharedIndices);
			pb.SetSharedIndicesUV(sharedIndicesUV);

			pb.SetSharedVertexValues(newIndex, cen);

			int[] mergedSharedIndex = pb.GetSharedIndices()[newIndex].array;

			int[] removedIndices;
			pb.RemoveDegenerateTriangles(out removedIndices);

			// get a non-deleted index to work with
			int ind = -1;
			for(int i = 0; i < mergedSharedIndex.Length; i++)
				if(!removedIndices.Contains(mergedSharedIndex[i]))
					ind = mergedSharedIndex[i];

			int t = ind;
			for(int i = 0; i < removedIndices.Length; i++)
				if(ind > removedIndices[i])
					t--;

			if(t > -1)
			{
				collapsedIndex = t;
				return true;
			}
			else
			{
				collapsedIndex = -1;
				return false;
			}
		}

		/// <summary>
		/// Creates separate entries in sharedIndices cache for all passed indices, and all indices in the shared array they belong to.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		public static bool SplitCommonVertices(this pb_Object pb, int[] indices)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();

			pb_IntArray[] sharedIndices = pb.sharedIndices;

			List<int> usedIndex = new List<int>();
			List<int> splits = new List<int>();

			for(int i = 0; i < indices.Length; i++)
			{
				int universal = lookup[indices[i]];

				if(!usedIndex.Contains(universal))
				{
					usedIndex.Add(universal);
					splits.AddRange(sharedIndices[universal].array);
				}
			}

			pb_IntArrayUtility.RemoveValues(ref sharedIndices, splits.ToArray());

			foreach(int i in splits)
				pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, -1, i);

			pb.SetSharedIndices(sharedIndices);

			return true;
		}

		/// <summary>
		/// Split individual indices from their common groups.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="edge"></param>
		public static void SplitVertices(this pb_Object pb, pb_Edge edge)
		{
			SplitVertices(pb, new int[] { edge.x, edge.y });
		}

		/// <summary>
		/// Split individual indices from their common groups.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="indices"></param>
		public static void SplitVertices(this pb_Object pb, IEnumerable<int> indices)
		{
			// ToDictionary always sets the universal indices in ascending order from 0+.
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			int max = lookup.Count();
			foreach(int i in indices)
				lookup[i] = ++max;
			pb.SetSharedIndices(lookup);
		}

		/// <summary>
		/// Given a face and a point, this will add a vertex to the pb_Object and retriangulate the face.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="face"></param>
		/// <param name="points"></param>
		/// <param name="addColors"></param>
		/// <param name="newFace"></param>
		/// <returns></returns>
		public static bool AppendVerticesToFace(this pb_Object pb, pb_Face face, Vector3[] points, Color[] addColors, out pb_Face newFace)
		{
			if(!face.IsValid())
			{
				newFace = face;
				return false;
			}

			List<pb_Vertex> vertices = pb_Vertex.GetVertices(pb).ToList();
			List<pb_Face> faces = new List<pb_Face>( pb.faces );
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> lookupUV = pb.sharedIndicesUV == null ? null : pb.sharedIndicesUV.ToDictionary();

			List<pb_Edge> wound = pb_WingedEdge.SortEdgesByAdjacency(face);

			List<pb_Vertex> n_vertices 	= new List<pb_Vertex>();
			List<int> n_shared 			= new List<int>();
			List<int> n_sharedUV 		= lookupUV != null ? new List<int>() : null;

			for(int i = 0; i < wound.Count; i++)
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

					float dist = pb_Math.DistancePointLineSegment(p, v, w);

					if(dist < best)
					{
						best = dist;
						index = n;
					}
				}

				pb_Vertex left = n_vertices[index], right = n_vertices[(index+1) % vc];

				float x = (p - left.position).sqrMagnitude;
				float y = (p - right.position).sqrMagnitude;

				pb_Vertex insert = pb_Vertex.Mix(left, right, x / (x + y));

				n_vertices.Insert((index + 1) % vc, insert);
				n_shared.Insert((index + 1) % vc, -1);
				if(n_sharedUV != null) n_sharedUV.Insert((index + 1) % vc, -1);
			}

			List<int> triangles;

			try
			{
				pb_Triangulation.TriangulateVertices(n_vertices, out triangles, false);
			}
			catch
			{
				Debug.Log("Failed triangulating face after appending vertices.");
				newFace = null;
				return false;
			}

			pb_FaceRebuildData data = new pb_FaceRebuildData();

			data.face = new pb_Face(triangles.ToArray(), face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, -1, face.manualUV);
			data.vertices 			= n_vertices;
			data.sharedIndices 		= n_shared;
			data.sharedIndicesUV 	= n_sharedUV;

			pb_FaceRebuildData.Apply(	new List<pb_FaceRebuildData>() { data },
										vertices,
										faces,
										lookup,
										lookupUV);

			newFace = data.face;

			pb.SetVertices(vertices);
			pb.SetFaces(faces.ToArray());
			pb.SetSharedIndices(lookup);
			pb.SetSharedIndicesUV(lookupUV);

			// check old normal and make sure this new face is pointing the same direction
			Vector3 oldNrm = pb_Math.Normal(pb, face);
			Vector3 newNrm = pb_Math.Normal(pb, newFace);

			if( Vector3.Dot(oldNrm, newNrm) < 0 )
				newFace.ReverseIndices();

			pb.DeleteFace(face);

			return true;
		}

		/// <summary>
		/// Appends count number of vertices to an edge.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="edge"></param>
		/// <param name="count"></param>
		/// <param name="newEdges"></param>
		/// <returns></returns>
		public static pb_ActionResult AppendVerticesToEdge(this pb_Object pb, pb_Edge edge, int count, out List<pb_Edge> newEdges)
		{
			return AppendVerticesToEdge(pb, new pb_Edge[] { edge }, count, out newEdges);
		}

		/// <summary>
		/// Appends count number of vertices to an edge.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="edges"></param>
		/// <param name="count"></param>
		/// <param name="newEdges"></param>
		/// <returns></returns>
		public static pb_ActionResult AppendVerticesToEdge(this pb_Object pb, IList<pb_Edge> edges, int count, out List<pb_Edge> newEdges)
		{
			newEdges = new List<pb_Edge>();

			if(count < 1 || count > 512)
				return new pb_ActionResult(Status.Failure, "New edge vertex count is less than 1 or greater than 512.");

			List<pb_Vertex> vertices 		= new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			Dictionary<int, int> lookup 	= pb.sharedIndices.ToDictionary();
			Dictionary<int, int> lookupUV	= pb.sharedIndicesUV.ToDictionary();
			List<int> indicesToDelete		= new List<int>();
			pb_Edge[] commonEdges	 		= pb_EdgeExtension.GetUniversalEdges(edges.ToArray(), lookup);
			List<pb_Edge> distinctEdges 	= commonEdges.Distinct().ToList();

			Dictionary<pb_Face, pb_FaceRebuildData> modifiedFaces = new Dictionary<pb_Face, pb_FaceRebuildData>();

			int originalSharedIndicesCount = lookup.Count();
			int sharedIndicesCount = originalSharedIndicesCount;

			foreach(pb_Edge edge in distinctEdges)
			{
				pb_Edge localEdge = pb_EdgeExtension.GetLocalEdgeFast(edge, pb.sharedIndices);

				// Generate the new vertices that will be inserted on this edge
				List<pb_Vertex> verticesToAppend = new List<pb_Vertex>(count);

				for(int i = 0; i < count; i++)
					verticesToAppend.Add(pb_Vertex.Mix(vertices[localEdge.x], vertices[localEdge.y], (i+1)/((float)count + 1)));

				List<pb_Tuple<pb_Face, pb_Edge>> adjacentFaces = pb_MeshUtils.GetNeighborFaces(pb, localEdge);

				// foreach face attached to common edge, append vertices
				foreach(pb_Tuple<pb_Face, pb_Edge> tup in adjacentFaces)
				{
					pb_Face face = tup.Item1;

					pb_FaceRebuildData data;

					if( !modifiedFaces.TryGetValue(face, out data) )
					{
						data = new pb_FaceRebuildData();
						data.face = new pb_Face(null, face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, -1, face.manualUV);
						data.vertices = new List<pb_Vertex>(pb_Util.ValuesWithIndices(vertices, face.distinctIndices));
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
			List<pb_Face> dic_face = modifiedFaces.Keys.ToList();
			List<pb_FaceRebuildData> dic_data = modifiedFaces.Values.ToList();
			List<pb_EdgeLookup> appendedEdges = new List<pb_EdgeLookup>();

			for(int i = 0; i < dic_face.Count; i++)
			{
				pb_Face face = dic_face[i];
				pb_FaceRebuildData data = dic_data[i];

				Vector3 nrm = pb_Math.Normal(pb, face);
				Vector2[] projection = pb_Projection.PlanarProject(data.vertices.Select(x=>x.position).ToArray(), nrm);

				int vertexCount = vertices.Count;

				// triangulate and set new face indices to end of current vertex list
				List<int> indices;

				if(pb_Triangulation.SortAndTriangulate(projection, out indices))
					data.face.SetIndices(indices.ToArray());
				else
					continue;

				data.face.ShiftIndices(vertexCount);
				face.CopyFrom(data.face);

				for(int n = 0; n < data.vertices.Count; n++)
					lookup.Add(vertexCount + n, data.sharedIndices[n]);

				if(data.sharedIndicesUV.Count == data.vertices.Count)
				{
					for(int n = 0; n < data.vertices.Count; n++)
						lookupUV.Add(vertexCount + n, data.sharedIndicesUV[n]);
				}

				vertices.AddRange(data.vertices);

				foreach(pb_Edge e in face.edges)
				{
					pb_EdgeLookup el = new pb_EdgeLookup(new pb_Edge(lookup[e.x], lookup[e.y]), e);

					if(el.common.x >= originalSharedIndicesCount || el.common.y >= originalSharedIndicesCount)
						appendedEdges.Add(el);
				}
			}

			indicesToDelete = indicesToDelete.Distinct().ToList();
			int delCount = indicesToDelete.Count;

			newEdges = appendedEdges.Distinct().Select(x => x.local - delCount).ToList();

			pb.SetVertices(vertices);
			pb.SetSharedIndices(lookup.ToSharedIndices());
			pb.SetSharedIndicesUV(lookupUV.ToSharedIndices());
			pb.DeleteVerticesWithIndices(indicesToDelete);

			return new pb_ActionResult(Status.Success, "Subdivide Edges");
		}

		/// <summary>
		/// Split a common index on a face into two vertices and slide each vertex backwards along it's feeding edge by distance.
		///	This method does not perform any input validation, so make sure edgeAndCommonIndex is distinct and all winged edges belong
		///	to the same face.
		///<pre>
		///	`appendedVertices` is common index and a list of the new face indices it was split into.
		///
		///	_ _ _ _          _ _ _
		///	|              /
		///	|         ->   |
		///	|              |
		/// </pre>
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="edgeAndCommonIndex"></param>
		/// <param name="distance"></param>
		/// <param name="appendedVertices"></param>
		/// <returns></returns>
		public static pb_FaceRebuildData ExplodeVertex(
			IList<pb_Vertex> vertices,
			IList<pb_Tuple<pb_WingedEdge, int>> edgeAndCommonIndex,
			float distance,
			out Dictionary<int, List<int>> appendedVertices)
		{
			pb_Face face = edgeAndCommonIndex.FirstOrDefault().Item1.face;
			List<pb_Edge> perimeter = pb_WingedEdge.SortEdgesByAdjacency(face);
			appendedVertices = new Dictionary<int, List<int>>();
			Vector3 oldNormal = pb_Math.Normal(vertices, face.indices);

			// store local and common index of split points
			Dictionary<int, int> toSplit = new Dictionary<int, int>();

			foreach(pb_Tuple<pb_WingedEdge, int> v in edgeAndCommonIndex)
			{
				if( v.Item2 == v.Item1.edge.common.x)
					toSplit.Add(v.Item1.edge.local.x, v.Item2);
				else
					toSplit.Add(v.Item1.edge.local.y, v.Item2);
			}

			int pc = perimeter.Count;
			List<pb_Vertex> n_vertices = new List<pb_Vertex>();

			for(int i = 0; i < pc; i++)
			{
				int index = perimeter[i].y;

				// split this index into two
				if(toSplit.ContainsKey(index))
				{
					// a --- b --- c
					pb_Vertex a = vertices[perimeter[i].x];
					pb_Vertex b = vertices[perimeter[i].y];
					pb_Vertex c = vertices[perimeter[(i+1) % pc].y];

					pb_Vertex leading_dir = a - b;
					pb_Vertex following_dir = c - b;
					leading_dir.Normalize();
					following_dir.Normalize();

					pb_Vertex leading_insert = vertices[index] + leading_dir * distance;
					pb_Vertex following_insert = vertices[index] + following_dir * distance;

					appendedVertices.AddOrAppend(toSplit[index], n_vertices.Count);
					n_vertices.Add(leading_insert);

					appendedVertices.AddOrAppend(toSplit[index], n_vertices.Count);
					n_vertices.Add(following_insert);
				}
				else
				{
					n_vertices.Add(vertices[index]);
				}
			}

			List<int> triangles;

			if( pb_Triangulation.TriangulateVertices(n_vertices, out triangles, false) )
			{
				pb_FaceRebuildData data = new pb_FaceRebuildData();
				data.vertices = n_vertices;
				data.face = new pb_Face(face);

				Vector3 newNormal = pb_Math.Normal(n_vertices, triangles);

				if(Vector3.Dot(oldNormal, newNormal) < 0f)
					triangles.Reverse();

				data.face.SetIndices(triangles.ToArray());

				return data;
			}

			return null;
		}

		static pb_Edge AlignEdgeWithDirection(pb_EdgeLookup edge, int commonIndex)
		{
			if(edge.common.x == commonIndex)
				return new pb_Edge(edge.local.x, edge.local.y);
			else
				return new pb_Edge(edge.local.y, edge.local.x);
		}

		/// <summary>
		/// Snap all vertices to an increment of @snapValue in world space.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="indices"></param>
		/// <param name="snap"></param>
		public static void Quantize(pb_Object pb, IList<int> indices, Vector3 snap)
		{
			Vector3[] verts = pb.vertices;

			for(int n = 0; n < indices.Count; n++)
				verts[indices[n]] = pb.transform.InverseTransformPoint(pb_Snap.SnapValue(pb.transform.TransformPoint(verts[indices[n]]), snap));

		}
	}
}
