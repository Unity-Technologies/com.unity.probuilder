using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using System.Linq;

namespace ProBuilder2.MeshOperations
{
	public static class pbBevel
	{
		/**
		 *	Do the damn thing.
		 */
		public static bool Bevel(this pb_Object pb, IEnumerable<pb_Edge> edges)
		{
			float amount = .3f;

			pb_Edge edge = edges.FirstOrDefault();
			if(edge == null) return false;

			List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			List<pb_Tuple<pb_Face, pb_Edge>> neighbors = pbMeshUtils.GetNeighborFaces(pb, edge);

			foreach(pb_Tuple<pb_Face, pb_Edge> fe in neighbors)
			{
				pb_Face f = fe.Item1;
				pb_Edge e = fe.Item2;
				pb.SplitVertices(e);

				Vector3 nrm = pb_Math.Normal(pb, f);

				vertices[e.x].position += nrm * amount;
				vertices[e.y].position += nrm * amount;
			}

			pb.SetVertices(vertices);

			Debug.Log("Did the bevel)");
			return true;
		}
	}
}
