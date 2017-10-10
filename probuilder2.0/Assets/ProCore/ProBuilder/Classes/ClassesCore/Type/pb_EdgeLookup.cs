using System;
using System.Linq;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	/**
	 *	Stores 2 edges, one made up of the shared indices and one of actual triangle indices.
	 *	When sorting and comparing the shared edge is used.
	 */
	public class pb_EdgeLookup : IEquatable<pb_EdgeLookup>
	{
		public pb_Edge local;
		public pb_Edge common;

		public pb_EdgeLookup(pb_Edge common, pb_Edge local)
		{
			this.common = common;
			this.local = local;
		}

		public pb_EdgeLookup(int cx, int cy, int x, int y)
		{
			this.common = new pb_Edge(cx, cy);
			this.local = new pb_Edge(x, y);
		}

		public bool Equals(pb_EdgeLookup b)
		{
			return common.Equals(b == null ? pb_Edge.Empty : b.common);
		}

		public override bool Equals(System.Object b)
		{
			pb_EdgeLookup be = b as pb_EdgeLookup;
			return be != null && common.Equals(be.common);
		}

		public static bool operator ==(pb_EdgeLookup a, pb_EdgeLookup b)
		{
			if (a == null || b == null)
				return false;

			return a.Equals(b);
		}

		public static bool operator !=(pb_EdgeLookup a, pb_EdgeLookup b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return common.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("({0}, {1})", common.x, common.y);
			// return string.Format("common ({0}, {1})\nlocal({2}, {3})", common.x, common.y, local.x, local.y);
			// return string.Format("c({0}, {1})  l({2}, {3})  > {4}", common.x, common.y, local.x, local.y, GetHashCode());
		}

		public static IEnumerable<pb_EdgeLookup> GetEdgeLookup(IEnumerable<pb_Edge> edges, Dictionary<int, int> lookup)
		{
			return edges.Select(x => new pb_EdgeLookup(new pb_Edge(lookup[x.x], lookup[x.y]), x));
		}
	}
}
