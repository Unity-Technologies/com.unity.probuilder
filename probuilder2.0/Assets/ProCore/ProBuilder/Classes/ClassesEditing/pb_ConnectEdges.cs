using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	/**
	 *	Utility class for connecting edges.
	 */
	public static class pb_ConnectEdges
	{
		/**
		 * Store face rebuild data with indices to mark which vertices are new.
		 */
		class ConnectFaceRebuildData
		{
			public pb_FaceRebuildData faceRebuildData;
			public List<int> newVertexIndices;

			public ConnectFaceRebuildData(pb_FaceRebuildData faceRebuildData, List<int> newVertexIndices)
			{
				this.faceRebuildData = faceRebuildData;
				this.newVertexIndices = newVertexIndices;
			}
		};

		public static pb_ActionResult Connect(this pb_Object pb, IList<pb_Edge> edges, out pb_Edge[] connections)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> lookupUV = pb.sharedIndicesUV != null ? pb.sharedIndicesUV.ToDictionary() : null;
			List<pb_EdgeLookup> distinctEdges = pb_EdgeLookup.GetEdgeLookup(edges, lookup).Distinct().ToList();
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);

			Dictionary<pb_Face, List<pb_WingedEdge>> affected = new Dictionary<pb_Face, List<pb_WingedEdge>>();

			// map each edge to a face so that we have a list of all touched faces with their to-be-subdivided edges
			foreach(pb_EdgeLookup e in distinctEdges)
			{
				IEnumerable<pb_WingedEdge> touching = wings.Where(x => x.edge.Equals(e));

				foreach(pb_WingedEdge wing in touching)
				{
					if(affected.ContainsKey(wing.face))
						affected[wing.face].Add(wing);
					else
						affected.Add(wing.face, new List<pb_WingedEdge>() { wing });
				}
			}

			// ////// DEBUG {
			// foreach(var k in affected)
			// {
			// 	Debug.Log(k.Key + "\n" + k.Value.Count);
			// }
			// ////// DEBUG }

			List<pb_Vertex> vertices = new List<pb_Vertex>( pb_Vertex.GetVertices(pb) );
			List<ConnectFaceRebuildData> results = new List<ConnectFaceRebuildData>();

			HashSet<int> usedTextureGroups = new HashSet<int>(pb.faces.Select(x => x.textureGroup));
			int newTextureGroupIndex = 1;

			// do the splits
			foreach(KeyValuePair<pb_Face, List<pb_WingedEdge>> split in affected)
			{
				pb_Face face = split.Key;
				List<pb_WingedEdge> targetEdges = split.Value;

				int inserts = targetEdges.Count;

				if(inserts == 1)
				{
					results.Add( InsertVertices(face, targetEdges, vertices) );
				}
				else
				if(inserts == 2)
				{
					List<ConnectFaceRebuildData> res = ConnectEdgesInFace(face, targetEdges[0], targetEdges[1], vertices, lookup, lookupUV);

					foreach(ConnectFaceRebuildData c in res)
					{
						if(face.textureGroup < 0)
						{
							while(usedTextureGroups.Contains(newTextureGroupIndex))
								newTextureGroupIndex++;

							usedTextureGroups.Add(newTextureGroupIndex);
						}

						c.faceRebuildData.face.textureGroup 	= face.textureGroup < 0 ? newTextureGroupIndex : face.textureGroup;
						c.faceRebuildData.face.uv 				= new pb_UV(face.uv);
						c.faceRebuildData.face.smoothingGroup 	= face.smoothingGroup;
						c.faceRebuildData.face.manualUV 		= face.manualUV;
						c.faceRebuildData.face.material 		= face.material;
					}

					results.AddRange(res);
				}
				if(inserts > 2)
				{

				}
			}

			List<int> offsets = pb_FaceRebuildData.Apply(results.Select(x => x.faceRebuildData), pb, vertices, null, lookup, lookupUV);
			pb.SetSharedIndicesUV(new pb_IntArray[0]);
			pb.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(pb.vertices));
			int removedVertexCount = pb.DeleteFaces(affected.Keys).Length;
			pb.ToMesh();

			// offset the newVertexIndices by whatever the FaceRebuildData did so we can search for the new edges by index
			HashSet<int> appendedIndices = new HashSet<int>();

			for(int n = 0; n < results.Count; n++)
				for(int i = 0; i < results[n].newVertexIndices.Count; i++)
					appendedIndices.Add( ( results[n].newVertexIndices[i] + offsets[n] ) - removedVertexCount );
			Debug.Log(appendedIndices.ToString("\n"));

			Dictionary<int, int> lup = pb.sharedIndices.ToDictionary();
			IEnumerable<pb_Edge> newEdges = results.SelectMany(x => x.faceRebuildData.face.edges).Where(x => appendedIndices.Contains(x.x) && appendedIndices.Contains(x.y));
			IEnumerable<pb_EdgeLookup> distNewEdges = pb_EdgeLookup.GetEdgeLookup(newEdges, lup);
			connections = distNewEdges.Distinct().Select(x => x.local).ToArray();

			return new pb_ActionResult(Status.Success, string.Format("Connected {0} Edges", results.Count));
		}

		/**
		 *	Accepts a key value pair of face and list of edges to split on.
		 */
		private static List<ConnectFaceRebuildData> ConnectEdgesInFace(
			pb_Face face,
			pb_WingedEdge a,
			pb_WingedEdge b,
			List<pb_Vertex> vertices,
			Dictionary<int, int> lookup,
			Dictionary<int, int> lookupUV)
		{
			List<pb_Edge> perimeter = pb_WingedEdge.SortEdgesByAdjacency(face);

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
				pb_FaceRebuildData f = pb_AppendPolygon.FaceWithVertices(n_vertices[i], false);
				faces.Add(new ConnectFaceRebuildData(f, n_indices[i]));
			}

			return faces;
		}

		// public static List<ConnectFaceRebuildData> ConnectEdgesInFace(
		// 	pb_Face face,
		// 	List<pb_WingedEdge> edges,
		// 	List<pb_Vertex> vertices,
		// 	Dictionary<int, int> lookup,
		// 	Dictionary<int, int> lookupUV)
		// {
		// 	List<pb_Edge> perimeter = pb_WingedEdge.SortEdgesByAdjacency(face);

		// 	List<pb_Vertex>[] n_vertices = new List<pb_Vertex>[2] {
		// 		new List<pb_Vertex>(),
		// 		new List<pb_Vertex>()
		// 	};

		// 	List<int>[] n_indices = new List<int>[2] {
		// 		new List<int>(),
		// 		new List<int>()
		// 	};

		// 	for(int i = 0; i < )
		// }

		private static ConnectFaceRebuildData InsertVertices(pb_Face face, List<pb_WingedEdge> edges, List<pb_Vertex> vertices)
		{
			List<pb_Edge> perimeter = pb_WingedEdge.SortEdgesByAdjacency(face);
			List<pb_Vertex> n_vertices = new List<pb_Vertex>();
			List<int> newVertexIndices = new List<int>();
			HashSet<pb_Edge> affected = new HashSet<pb_Edge>( edges.Select(x=>x.edge.local) );

			for(int i = 0; i < perimeter.Count; i++)
			{
				n_vertices.Add(vertices[perimeter[i].x]);

				if(affected.Contains(perimeter[i]))
				{
					newVertexIndices.Add(n_vertices.Count);
					n_vertices.Add(pb_Vertex.Mix(vertices[perimeter[i].x], vertices[perimeter[i].y], .5f));
				}
			}

			pb_FaceRebuildData res = pb_AppendPolygon.FaceWithVertices(n_vertices, false);

			res.face.textureGroup 	= face.textureGroup;
			res.face.uv 			= new pb_UV(face.uv);
			res.face.smoothingGroup = face.smoothingGroup;
			res.face.manualUV 		= face.manualUV;
			res.face.material 		= face.material;

			return new ConnectFaceRebuildData(res, newVertexIndices);
		}
	}
}
