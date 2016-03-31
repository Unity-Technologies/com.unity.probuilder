using System;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	public class pb_EdgeLookup : IEquatable<pb_EdgeLookup>
	{
		public pb_Edge common;
		public pb_Edge local;

		public pb_EdgeLookup(pb_Edge common, pb_Edge local)
		{
			this.common = common;
			this.local = local;
		}
		public override bool Equals(System.Object b)
		{
			return this.common.Equals(b);
		}

		public bool Equals(pb_EdgeLookup b)
		{
			return this.common.Equals(b);
		}

		public override int GetHashCode()
		{
			return common.GetHashCode();
		}
	}
}
