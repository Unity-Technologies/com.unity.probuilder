using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

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
		public static ActionResult ConformNormals(this ProBuilderMesh pb, IList<Face> faces)
		{
			List<WingedEdge> wings = WingedEdge.GetWingedEdges(pb, faces);
			HashSet<Face> used = new HashSet<Face>();
			int count = 0;

			// this loop adds support for multiple islands of grouped selections
			for(int i = 0; i < wings.Count; i++)
			{
				if(used.Contains(wings[i].face))
					continue;

				Dictionary<Face, bool> flags = new Dictionary<Face, bool>();

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
				return new ActionResult(Status.Success, count > 1 ? string.Format("Flipped {0} faces", count) : "Flipped 1 face");
			else
				return new ActionResult(Status.NoChange, "Faces Uniform");
		}

		private static void GetWindingFlags(WingedEdge edge, bool flag, Dictionary<Face, bool> flags)
		{
			flags.Add(edge.face, flag);

			WingedEdge next = edge;

			do
			{
				WingedEdge opp = next.opposite;

				if(opp != null && !flags.ContainsKey(opp.face))
				{
					Edge cea = GetCommonEdgeInWindingOrder(next);
					Edge ceb = GetCommonEdgeInWindingOrder(opp);

					GetWindingFlags(opp, cea.x == ceb.x ? !flag : flag, flags);
				}

				next = next.next;

			} while(next != edge);
		}

		/**
		 *	Ensure the opposite face to source matches the winding order.
		 */
		public static ActionResult ConformOppositeNormal(WingedEdge source)
		{
			if(source == null || source.opposite == null)
				return new ActionResult(Status.Failure, "Source edge does not share an edge with another face.");

			Edge cea = GetCommonEdgeInWindingOrder(source);
			Edge ceb = GetCommonEdgeInWindingOrder(source.opposite);

			if( cea.x == ceb.x )
			{
				source.opposite.face.ReverseIndices();

				return new ActionResult(Status.Success, "Reversed target face winding order.");
			}

			return new ActionResult(Status.NoChange, "Faces already unified.");
		}

		/**
		 *	Iterate a face and return a new common edge where the edge indices are true to the triangle winding order.
		 */
		private static Edge GetCommonEdgeInWindingOrder(WingedEdge wing)
		{
			int[] indices = wing.face.indices;
			int len = indices.Length;

			for(int i = 0; i < len; i += 3)
			{
				Edge e = wing.edge.local;
				int a = indices[i], b = indices[i+1], c = indices[i+2];

				if(e.x == a && e.y == b)
					return wing.edge.common;
				else if(e.x == b && e.y == a)
					return new Edge(wing.edge.common.y, wing.edge.common.x);
				else if(e.x == b && e.y == c)
					return wing.edge.common;
				else if(e.x == c && e.y == b)
					return new Edge(wing.edge.common.y, wing.edge.common.x);
				else if(e.x == c && e.y == a)
					return wing.edge.common;
				else if(e.x == a && e.y == c)
					return new Edge(wing.edge.common.y, wing.edge.common.x);
			}

			return Edge.Empty;
		}

		public static void MatchNormal(Face source, Face target, Dictionary<int, int> lookup)
		{
			List<EdgeLookup> source_edges = EdgeLookup.GetEdgeLookup(source.edges, lookup).ToList();
			List<EdgeLookup> target_edges = EdgeLookup.GetEdgeLookup(target.edges, lookup).ToList();

			bool superBreak = false;

			Edge src, tar;

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
