using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// Methods for making sure adjacent face normals are consistent.
	/// </summary>
	static class pb_ConformNormals
	{
		/**
		 *	Conform groups of adjacent faces.  This function supports multiple islands of interconnected faces, but
		 *	it may not unify each island the same way.
		 */
		public static pb_ActionResult ConformNormals(this pb_Object pb, IList<pb_Face> faces)
		{
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb, faces);
			HashSet<pb_Face> used = new HashSet<pb_Face>();
			int count = 0;

			// this loop adds support for multiple islands of grouped selections
			for(int i = 0; i < wings.Count; i++)
			{
				if(used.Contains(wings[i].face))
					continue;

				Dictionary<pb_Face, bool> flags = new Dictionary<pb_Face, bool>();

				GetWindingFlags(wings[i], true, flags);

				int flip = 0;

				foreach(var kvp in flags)
					flip += kvp.Value ? 1 : -1;

				bool direction = flip > 0;

				foreach(var kvp in flags)
				{
					if(direction != kvp.Value)
					{
						count++;
						kvp.Key.ReverseIndices();
					}
				}

				used.UnionWith(flags.Keys);
			}

			if(count > 0)
				return new pb_ActionResult(Status.Success, count > 1 ? string.Format("Flipped {0} faces", count) : "Flipped 1 face");
			else
				return new pb_ActionResult(Status.NoChange, "Faces Uniform");
		}

		private static void GetWindingFlags(pb_WingedEdge edge, bool flag, Dictionary<pb_Face, bool> flags)
		{
			flags.Add(edge.face, flag);

			pb_WingedEdge next = edge;

			do
			{
				pb_WingedEdge opp = next.opposite;

				if(opp != null && !flags.ContainsKey(opp.face))
				{
					pb_Edge cea = GetCommonEdgeInWindingOrder(next);
					pb_Edge ceb = GetCommonEdgeInWindingOrder(opp);

					GetWindingFlags(opp, cea.x == ceb.x ? !flag : flag, flags);
				}

				next = next.next;

			} while(next != edge);
		}

		/**
		 *	Ensure the opposite face to source matches the winding order.
		 */
		public static pb_ActionResult ConformOppositeNormal(pb_WingedEdge source)
		{
			if(source == null || source.opposite == null)
				return new pb_ActionResult(Status.Failure, "Source edge does not share an edge with another face.");

			pb_Edge cea = GetCommonEdgeInWindingOrder(source);
			pb_Edge ceb = GetCommonEdgeInWindingOrder(source.opposite);

			if( cea.x == ceb.x )
			{
				source.opposite.face.ReverseIndices();

				return new pb_ActionResult(Status.Success, "Reversed target face winding order.");
			}

			return new pb_ActionResult(Status.NoChange, "Faces already unified.");
		}

		/**
		 *	Iterate a face and return a new common edge where the edge indices are true to the triangle winding order.
		 */
		private static pb_Edge GetCommonEdgeInWindingOrder(pb_WingedEdge wing)
		{
			int[] indices = wing.face.indices;
			int len = indices.Length;

			for(int i = 0; i < len; i += 3)
			{
				pb_Edge e = wing.edge.local;
				int a = indices[i], b = indices[i+1], c = indices[i+2];

				if(e.x == a && e.y == b)
					return wing.edge.common;
				else if(e.x == b && e.y == a)
					return new pb_Edge(wing.edge.common.y, wing.edge.common.x);
				else if(e.x == b && e.y == c)
					return wing.edge.common;
				else if(e.x == c && e.y == b)
					return new pb_Edge(wing.edge.common.y, wing.edge.common.x);
				else if(e.x == c && e.y == a)
					return wing.edge.common;
				else if(e.x == a && e.y == c)
					return new pb_Edge(wing.edge.common.y, wing.edge.common.x);
			}

			return pb_Edge.Empty;
		}

		public static void MatchNormal(pb_Face source, pb_Face target, Dictionary<int, int> lookup)
		{
			List<pb_EdgeLookup> source_edges = pb_EdgeLookup.GetEdgeLookup(source.edges, lookup).ToList();
			List<pb_EdgeLookup> target_edges = pb_EdgeLookup.GetEdgeLookup(target.edges, lookup).ToList();

			bool superBreak = false;

			pb_Edge src, tar;

			for(int i = 0; !superBreak && i < source_edges.Count; i++)
			{
				src = source_edges[i].common;

				for(int n = 0; !superBreak && n < target_edges.Count; n++)
				{
					tar = target_edges[n].common;

					if(src.Equals(tar))
					{
						if(src.x == tar.x)
							target.ReverseIndices();

						superBreak = true;
					}
				}
			}
		}
	}
}
