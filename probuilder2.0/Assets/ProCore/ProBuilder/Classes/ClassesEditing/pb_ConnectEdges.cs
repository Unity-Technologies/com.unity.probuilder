using UnityEngine;
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
		public static pb_ActionResult Connect(this pb_Object pb, IList<pb_Edge> edges, out pb_Edge[] connectingEdges)
		{
			connectingEdges = null;

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

			////// DEBUG {
			foreach(var k in affected)
			{
				Debug.Log(k.Key + "\n" + k.Value.Count);
			}
			////// DEBUG }

			List<pb_Vertex> vertices = new List<pb_Vertex>( pb_Vertex.GetVertices(pb) );
			List<pb_FaceRebuildData> results = new List<pb_FaceRebuildData>();

			foreach(KeyValuePair<pb_Face, List<pb_WingedEdge>> split in affected)
			{
				int inserts = split.Value.Count;

				if(inserts == 1)
				{
					results.Add( InsertVertices(split.Key, split.Value, vertices) );
				}
				else
				if(inserts == 2)
				{
					List<pb_FaceRebuildData> res = ConnectEdgesInFace(split.Key, split.Value[0], split.Value[1], vertices, lookup, lookupUV);
					results.AddRange(res);
				}
				if(inserts > 2)
				{

				}
			}

			pb_FaceRebuildData.Apply(results, pb, vertices, null, lookup, lookupUV);
			pb.SetSharedIndicesUV(new pb_IntArray[0]);
			pb.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(pb.vertices));
			pb.DeleteFaces(affected.Keys );
			pb.ToMesh();

			return pb_ActionResult.NoSelection;
		}

		/**
		 *	Accepts a key value pair of face and list of edges to split on.
		 */
		private static List<pb_FaceRebuildData> ConnectEdgesInFace(
			pb_Face face,
			pb_WingedEdge a,
			pb_WingedEdge b,
			List<pb_Vertex> vertices,
			Dictionary<int, int> lookup,
			Dictionary<int, int> lookupUV)
		{
			List<pb_Edge> perimeter = pb_WingedEdge.SortEdgesByAdjacency(face);

			List<pb_Vertex>[] n_vertices = new List<pb_Vertex>[2]
			{
				new List<pb_Vertex>(),
				new List<pb_Vertex>()
			};

			List<int>[] n_indices = new List<int>[2]
			{
				new List<int>(),
				new List<int>()
			};

			int index = 0;

			// creates two new polygon perimeter lines by stepping the current face perimeter and inserting new vertices where edges match
			for(int i = 0; i < perimeter.Count; i++)
			{
				n_vertices[index % 2].Add(vertices[perimeter[i].x]);
				n_indices[index % 2].Add(lookup[perimeter[i].x]);

				if(perimeter[i].Equals(a.edge.local) || perimeter[i].Equals(b.edge.local))
				{
					pb_Vertex mix = pb_Vertex.Mix(vertices[perimeter[i].x], vertices[perimeter[i].y], .5f);

					n_vertices[index % 2].Add(mix);
					n_indices[index % 2].Add(perimeter[i].Equals(a.edge.local) ? -1 : -2);
					index++;
					n_vertices[index % 2].Add(mix);
					n_indices[index % 2].Add(perimeter[i].Equals(a.edge.local) ? -1 : -2);
				}
			}

			List<pb_FaceRebuildData> faces = new List<pb_FaceRebuildData>();

			foreach(List<pb_Vertex> poly in n_vertices)
				faces.Add(pb_AppendPolygon.FaceWithVertices(poly, false));

			return faces;
		}

		private static pb_FaceRebuildData InsertVertices(pb_Face face, List<pb_WingedEdge> edges, List<pb_Vertex> vertices)
		{
			List<pb_Edge> perimeter = pb_WingedEdge.SortEdgesByAdjacency(face);
			List<pb_Vertex> n_vertices = new List<pb_Vertex>();
			HashSet<pb_Edge> affected = new HashSet<pb_Edge>( edges.Select(x=>x.edge.local) );

			for(int i = 0; i < perimeter.Count; i++)
			{
				n_vertices.Add(vertices[perimeter[i].x]);

				if(affected.Contains(perimeter[i]))
					n_vertices.Add(pb_Vertex.Mix(vertices[perimeter[i].x], vertices[perimeter[i].y], .5f));
			}

			pb_FaceRebuildData res = pb_AppendPolygon.FaceWithVertices(n_vertices, false);

			// make sure face is aligned with old
			Vector3 o = pb_Math.Normal(vertices, face.indices);
			Vector3 n = pb_Math.Normal(n_vertices, res.face.indices);

			if(Vector3.Dot(o, n) < 0f)
				res.face.ReverseIndices();

			return res;
		}
	}
}
