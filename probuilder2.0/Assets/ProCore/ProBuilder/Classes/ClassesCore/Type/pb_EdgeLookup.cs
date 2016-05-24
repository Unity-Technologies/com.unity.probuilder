using System;
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
			return common.Equals(b.common);
		}

		public override bool Equals(System.Object b)
		{
			return b is pb_EdgeLookup && common.Equals(((pb_EdgeLookup)b).common);
		}

		public static bool operator ==(pb_EdgeLookup a, pb_EdgeLookup b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(pb_EdgeLookup a, pb_EdgeLookup b)
		{
			return !a.Equals(b);
		}

		public override int GetHashCode()
		{
			return common.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("c({0}, {1})  l({2}, {3})", common.x, common.y, local.x, local.y);
		}
	}
}
