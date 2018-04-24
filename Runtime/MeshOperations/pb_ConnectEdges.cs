using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// Store face rebuild data with indices to mark which vertices are new.
	/// </summary>
	class ConnectFaceRebuildData
	{
		public FaceRebuildData faceRebuildData;
		public List<int> newVertexIndices;

		public ConnectFaceRebuildData(FaceRebuildData faceRebuildData, List<int> newVertexIndices)
		{
			this.faceRebuildData = faceRebuildData;
			this.newVertexIndices = newVertexIndices;
		}
	};

	/// <summary>
	/// Utility class for connecting edges.
	/// </summary>
	public static class pb_ConnectEdges
	{
		/// <summary>
		/// Subdivide faces.
		/// </summary>
		/// <param name="pb">pb_Object target.</param>
		/// <param name="faces">The faces to subdivide (more accurately, poke).</param>
		/// <param name="subdividedFaces">The resulting faces.</param>
		/// <returns>An action result indicating the status of the operation.</returns>
		public static ActionResult Connect(this ProBuilderMesh pb, IEnumerable<Face> faces, out Face[] subdividedFaces)
		{
			IEnumerable<Edge> edges = faces.SelectMany(x => x.edges);
			HashSet<Face> mask = new HashSet<Face>(faces);
			Edge[] empty;
			return Connect(pb, edges, out subdividedFaces, out empty, true, false, mask);
		}

		public static ActionResult Connect(this ProBuilderMesh pb, IEnumerable<Edge> edges, out Face[] faces)
		{
			Edge[] empty;
			return Connect(pb, edges, out faces, out empty, true, false);
		}

		public static ActionResult Connect(this ProBuilderMesh pb, IEnumerable<Edge> edges, out Edge[] connections)
		{
			Face[] empty;
			return Connect(pb, edges, out empty, out connections, false, true);
		}

		/// <summary>
		/// Inserts new edges connecting the passed edges, optionally restricting new edge insertion to faces in faceMask.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="edges"></param>
		/// <param name="addedFaces"></param>
		/// <param name="connections"></param>
		/// <param name="returnFaces"></param>
		/// <param name="returnEdges"></param>
		/// <param name="faceMask"></param>
		/// <returns></returns>
		static ActionResult Connect(
			this ProBuilderMesh pb,
			IEnumerable<Edge> edges,
			out Face[] addedFaces,
			out Edge[] connections,
			bool returnFaces = false,
			bool returnEdges = false,
			HashSet<Face> faceMask = null)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> lookupUV = pb.sharedIndicesUV != null ? pb.sharedIndicesUV.ToDictionary() : null;
			HashSet<EdgeLookup> distinctEdges = new HashSet<EdgeLookup>(EdgeLookup.GetEdgeLookup(edges, lookup));
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);

			// map each edge to a face so that we have a list of all touched faces with their to-be-subdivided edges
			Dictionary<Face, List<pb_WingedEdge>> touched = new Dictionary<Face, List<pb_WingedEdge>>();
			List<pb_WingedEdge> faceEdges;

			foreach(pb_WingedEdge wing in wings)
			{
				if( distinctEdges.Contains(wing.edge) )
				{
					if(touched.TryGetValue(wing.face, out faceEdges))
						faceEdges.Add(wing);
					else
						touched.Add(wing.face, new List<pb_WingedEdge>() { wing });
				}
			}

			Dictionary<Face, List<pb_WingedEdge>> affected = new Dictionary<Face, List<pb_WingedEdge>>();

			// weed out edges that won't actually connect to other edges (if you don't play ya' can't stay)
			foreach(KeyValuePair<Face, List<pb_WingedEdge>> kvp in touched)
			{
				if(kvp.Value.Count <= 1)
				{
					pb_WingedEdge opp = kvp.Value[0].opposite;

					if(opp == null)
						continue;

					List<pb_WingedEdge> opp_list;

					if(!touched.TryGetValue(opp.face, out opp_list))
						continue;

					if(opp_list.Count <= 1)
						continue;
				}

				affected.Add(kvp.Key, kvp.Value);
			}

			List<pb_Vertex> vertices = new List<pb_Vertex>( pb_Vertex.GetVertices(pb) );
			List<ConnectFaceRebuildData> results = new List<ConnectFaceRebuildData>();
			// just the faces that where connected with > 1 edge
			List<Face> connectedFaces = new List<Face>();

			HashSet<int> usedTextureGroups = new HashSet<int>(pb.faces.Select(x => x.textureGroup));
			int newTextureGroupIndex = 1;

			// do the splits
			foreach(KeyValuePair<Face, List<pb_WingedEdge>> split in affected)
			{
				Face face = split.Key;
				List<pb_WingedEdge> targetEdges = split.Value;
				int inserts = targetEdges.Count;
				Vector3 nrm = ProBuilderMath.Normal(vertices, face.indices);

				if(inserts == 1 || (faceMask != null && !faceMask.Contains(face)))
				{
					ConnectFaceRebuildData c = InsertVertices(face, targetEdges, vertices);

					Vector3 fn = ProBuilderMath.Normal(c.faceRebuildData.vertices, c.faceRebuildData.face.indices);

					if(Vector3.Dot(nrm, fn) < 0)
						c.faceRebuildData.face.ReverseIndices();

					results.Add( c );
				}
				else
				if(inserts > 1)
				{
					List<ConnectFaceRebuildData> res = inserts == 2 ?
						ConnectEdgesInFace(face, targetEdges[0], targetEdges[1], vertices) :
						ConnectEdgesInFace(face, targetEdges, vertices);

					if(face.textureGroup < 0)
					{
						while(usedTextureGroups.Contains(newTextureGroupIndex))
							newTextureGroupIndex++;

						usedTextureGroups.Add(newTextureGroupIndex);
					}

					foreach(ConnectFaceRebuildData c in res)
					{
						connectedFaces.Add(c.faceRebuildData.face);

						Vector3 fn = ProBuilderMath.Normal(c.faceRebuildData.vertices, c.faceRebuildData.face.indices);

						if(Vector3.Dot(nrm, fn) < 0)
							c.faceRebuildData.face.ReverseIndices();

						c.faceRebuildData.face.textureGroup 	= face.textureGroup < 0 ? newTextureGroupIndex : face.textureGroup;
						c.faceRebuildData.face.uv 				= new pb_UV(face.uv);
						c.faceRebuildData.face.smoothingGroup 	= face.smoothingGroup;
						c.faceRebuildData.face.manualUV 		= face.manualUV;
						c.faceRebuildData.face.material 		= face.material;
					}

					results.AddRange(res);
				}
			}

			FaceRebuildData.Apply(results.Select(x => x.faceRebuildData), pb, vertices, null, lookup, lookupUV);

			pb.SetSharedIndicesUV(new IntArray[0]);
			int removedVertexCount = pb.DeleteFaces(affected.Keys).Length;
			pb.SetSharedIndices(IntArrayUtility.ExtractSharedIndices(pb.positions));
			pb.ToMesh();

			// figure out where the new edges where inserted
			if(returnEdges)
			{
				// offset the newVertexIndices by whatever the FaceRebuildData did so we can search for the new edges by index
				HashSet<int> appendedIndices = new HashSet<int>();

				for(int n = 0; n < results.Count; n++)
					for(int i = 0; i < results[n].newVertexIndices.Count; i++)
						appendedIndices.Add( ( results[n].newVertexIndices[i] + results[n].faceRebuildData.Offset() ) - removedVertexCount );

				Dictionary<int, int> lup = pb.sharedIndices.ToDictionary();
				IEnumerable<Edge> newEdges = results.SelectMany(x => x.faceRebuildData.face.edges).Where(x => appendedIndices.Contains(x.x) && appendedIndices.Contains(x.y));
				IEnumerable<EdgeLookup> distNewEdges = EdgeLookup.GetEdgeLookup(newEdges, lup);

				connections = distNewEdges.Distinct().Select(x => x.local).ToArray();
			}
			else
			{
				connections = null;
			}

			if(returnFaces)
				addedFaces = connectedFaces.ToArray();
			else
				addedFaces = null;

			return new ActionResult(Status.Success, string.Format("Connected {0} Edges", results.Count / 2));
		}

		/// <summary>
		/// Accepts a face and set of edges to split on.
		/// </summary>
		/// <param name="face"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="vertices"></param>
		/// <returns></returns>
		static List<ConnectFaceRebuildData> ConnectEdgesInFace(
			Face face,
			pb_WingedEdge a,
			pb_WingedEdge b,
			List<pb_Vertex> vertices)
		{
			List<Edge> perimeter = pb_WingedEdge.SortEdgesByAdjacency(face);

			List<pb_Vertex>[] n_vertices = new List<pb_Vertex>[2] {
				new List<pb_Vertex>(),
				new List<pb_Vertex>()
			};

			List<int>[] n_indices = new List<int>[2] {
				new List<int>(),
				new List<int>()
			};

			int index = 0;

			// creates two new polygon perimeter lines by stepping the current face perimeter and inserting new vertices where edges match
			for(int i = 0; i < perimeter.Count; i++)
			{
				n_vertices[index % 2].Add(vertices[perimeter[i].x]);

				if(perimeter[i].Equals(a.edge.local) || perimeter[i].Equals(b.edge.local))
				{
					pb_Vertex mix = pb_Vertex.Mix(vertices[perimeter[i].x], vertices[perimeter[i].y], .5f);

					n_indices[index % 2].Add(n_vertices[index % 2].Count);
					n_vertices[index % 2].Add(mix);
					index++;
					n_indices[index % 2].Add(n_vertices[index % 2].Count);
					n_vertices[index % 2].Add(mix);
				}
			}

			List<ConnectFaceRebuildData> faces = new List<ConnectFaceRebuildData>();

			for(int i = 0; i < n_vertices.Length; i++)
			{
				FaceRebuildData f = pb_AppendPolygon.FaceWithVertices(n_vertices[i], false);
				faces.Add(new ConnectFaceRebuildData(f, n_indices[i]));
			}

			return faces;
		}

		/// <summary>
		/// Insert a new vertex at the center of a face and connect the center of all edges to it.
		/// </summary>
		/// <param name="face"></param>
		/// <param name="edges"></param>
		/// <param name="vertices"></param>
		/// <returns></returns>
		static List<ConnectFaceRebuildData> ConnectEdgesInFace(
			Face face,
			List<pb_WingedEdge> edges,
			List<pb_Vertex> vertices)
		{
			List<Edge> perimeter = pb_WingedEdge.SortEdgesByAdjacency(face);

			int splitCount = edges.Count;

			pb_Vertex centroid = pb_Vertex.Average(vertices, face.distinctIndices);

			List<List<pb_Vertex>> n_vertices = pb_Util.Fill<List<pb_Vertex>>(x => { return new List<pb_Vertex>(); }, splitCount);
			List<List<int>> n_indices = pb_Util.Fill<List<int>>(x => { return new List<int>(); }, splitCount);

			HashSet<Edge> edgesToSplit = new HashSet<Edge>(edges.Select(x => x.edge.local));

			int index = 0;

			// creates two new polygon perimeter lines by stepping the current face perimeter and inserting new vertices where edges match
			for(int i = 0; i < perimeter.Count; i++)
			{
				n_vertices[index % splitCount].Add(vertices[perimeter[i].x]);

				if( edgesToSplit.Contains(perimeter[i]) )
				{
					pb_Vertex mix = pb_Vertex.Mix(vertices[perimeter[i].x], vertices[perimeter[i].y], .5f);

					// split current poly line
					n_indices[index].Add(n_vertices[index].Count);
					n_vertices[index].Add(mix);

					// add the centroid vertex
					n_indices[index].Add(n_vertices[index].Count);
					n_vertices[index].Add(centroid);

					// advance the poly line index
					index = (index + 1) % splitCount;

					// then add the edge center vertex and move on
					n_vertices[index].Add(mix);
				}
			}

			List<ConnectFaceRebuildData> faces = new List<ConnectFaceRebuildData>();

			for(int i = 0; i < n_vertices.Count; i++)
			{
				FaceRebuildData f = pb_AppendPolygon.FaceWithVertices(n_vertices[i], false);
				faces.Add(new ConnectFaceRebuildData(f, n_indices[i]));
			}

			return faces;
		}

		static ConnectFaceRebuildData InsertVertices(Face face, List<pb_WingedEdge> edges, List<pb_Vertex> vertices)
		{
			List<Edge> perimeter = pb_WingedEdge.SortEdgesByAdjacency(face);
			List<pb_Vertex> n_vertices = new List<pb_Vertex>();
			List<int> newVertexIndices = new List<int>();
			HashSet<Edge> affected = new HashSet<Edge>( edges.Select(x=>x.edge.local) );

			for(int i = 0; i < perimeter.Count; i++)
			{
				n_vertices.Add(vertices[perimeter[i].x]);

				if(affected.Contains(perimeter[i]))
				{
					newVertexIndices.Add(n_vertices.Count);
					n_vertices.Add(pb_Vertex.Mix(vertices[perimeter[i].x], vertices[perimeter[i].y], .5f));
				}
			}

			FaceRebuildData res = pb_AppendPolygon.FaceWithVertices(n_vertices, false);

			res.face.textureGroup 	= face.textureGroup;
			res.face.uv 			= new pb_UV(face.uv);
			res.face.smoothingGroup = face.smoothingGroup;
			res.face.manualUV 		= face.manualUV;
			res.face.material 		= face.material;

			return new ConnectFaceRebuildData(res, newVertexIndices);
		}
	}
}
