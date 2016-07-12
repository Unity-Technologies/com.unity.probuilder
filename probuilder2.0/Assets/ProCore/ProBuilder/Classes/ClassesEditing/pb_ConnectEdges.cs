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
			List<pb_EdgeLookup> distinctEdges = pb_EdgeLookup.GetEdgeLookup(edges, lookup).Distinct().ToList();
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);

			Dictionary<pb_Face, List<pb_WingedEdge>> affected = new Dictionary<pb_Face, List<pb_WingedEdge>>();

			// map each edge to a face
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
			List<Vector3> positions = vertices.Select(x => x.position).ToList();

			foreach(KeyValuePair<pb_Face, List<pb_WingedEdge>> splits in affected)
			{
				pb_Face f = splits.Key;
				List<pb_WingedEdge> e = splits.Value;
				int[] indices = f.distinctIndices;

				Vector3 normal = pb_Projection.FindBestPlane(positions, indices).normal;
				Vector2[] uv = pb_Projection.PlanarProject(positions, normal, pb_Projection.VectorToProjectionAxis(normal), indices);

				if(e.Count < 3)
				{

				}
			}

			return pb_ActionResult.NoSelection;
		}
	}
}
