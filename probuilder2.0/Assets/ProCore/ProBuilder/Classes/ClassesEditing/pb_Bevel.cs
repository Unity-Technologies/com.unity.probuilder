using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	/**
	 *	Functions for beveling edges.
	 */
	public static class pb_Bevel
	{
		// class pb_BevelEdge
		// {
		// 	public pb_WingedEdge a, b;
		// }

		public static pb_ActionResult BevelEdges(pb_Object pb, IList<pb_Edge> edges, float amount)
		{
			int maxCommonIndex = pb.sharedIndices.Length;
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> lookupUV = pb.sharedIndicesUV.ToDictionary();
			List<pb_EdgeLookup> m_edges = pb_EdgeLookup.GetEdgeLookup(edges, lookup).ToList();
			m_edges.Distinct();
			List<pb_WingedEdge> wings = pb_WingedEdge.GenerateWingedEdges(pb);
			List<pb_Vertex> vertices = new List<pb_Vertex>( pb_Vertex.GetVertices(pb) );
			List<pb_FaceRebuildData> appendFaces = new List<pb_FaceRebuildData>();

			foreach(pb_EdgeLookup lup in m_edges)
			{
				pb_WingedEdge we = wings.FirstOrDefault(x => x.edge.Equals(lup));

				if(we == null || we.opposite == null)
					continue;

				SlideEdge(vertices, we, amount);
				SlideEdge(vertices, we.opposite, amount);

				we.edge.common.x = -1;
				we.edge.common.y = -1;
				we.opposite.edge.common.x = -1;
				we.opposite.edge.common.y = -1;
				lookup[we.edge.local.x] = we.edge.common.x;
				lookup[we.edge.local.y] = we.edge.common.y;
				lookup[we.opposite.edge.local.x] = we.opposite.edge.common.x;
				lookup[we.opposite.edge.local.y] = we.opposite.edge.common.y;

				appendFaces.AddRange( GetBridgeFaces(vertices, we, we.opposite) );
			}
			
			List<pb_Face> faces = new List<pb_Face>(pb.faces);
			pb_FaceRebuildData.Apply(appendFaces, ref vertices, ref faces, ref lookup, ref lookupUV);

			pb.SetFaces(faces.ToArray());
			pb.SetVertices(vertices);
			pb.SetSharedIndices(lookup.ToSharedIndices());
			pb.ToMesh();

			return new pb_ActionResult(Status.Success, "Bevel Edges");
 		}

 		private static List<pb_FaceRebuildData> GetBridgeFaces(IList<pb_Vertex> vertices, pb_WingedEdge left, pb_WingedEdge right)
 		{
 			List<pb_FaceRebuildData> faces = new List<pb_FaceRebuildData>();

 			pb_FaceRebuildData rf = new pb_FaceRebuildData();

 			pb_EdgeLookup a = left.edge;
 			pb_EdgeLookup b = right.edge;

 			rf.vertices = new List<pb_Vertex>() 
 			{
 				vertices[a.local.x],
 				vertices[a.local.y],
 				vertices[b.local.y],
 				vertices[b.local.x]
 			};

 			rf.sharedIndices = new List<int>()
 			{
 				a.common.x,
 				a.common.y,
 				b.common.y,
 				b.common.x
 			};

 			rf.face = new pb_Face(
 				new int[] { 2, 1, 0, 2, 3, 1 },
 				left.face.material,
 				new pb_UV(),
 				-1,
 				-1,
 				-1,
 				false);

 			faces.Add(rf);

 			return faces;
 		}

 		private static void SlideEdge(IList<pb_Vertex> vertices, pb_WingedEdge we, float amount)
 		{
			we.face.manualUV = true;
			we.face.textureGroup = -1;

			pb_Edge local = we.edge.local;
			int[] i = we.face.indices;
			Vector3 n = Vector3.Cross(vertices[i[1]].position - vertices[i[0]].position, vertices[i[2]].position - vertices[i[0]].position);
			Vector3 e = vertices[local.y].position - vertices[local.x].position;
			Vector3 c = Vector3.Cross(n, e);

			vertices[local.x].position += c.normalized * amount;
			vertices[local.y].position += c.normalized * amount;
		}
	}
}
