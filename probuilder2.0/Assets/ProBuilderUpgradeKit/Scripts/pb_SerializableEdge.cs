using UnityEngine;
using System.Collections;
using ProBuilder2.Common;

namespace ProBuilder2.UpgradeKit
{
	[System.Serializable()]
	public class pb_SerializableEdge
	{
		public int x, y;

		public static explicit operator pb_Edge(pb_SerializableEdge v)
		{
			return new pb_Edge(v.x, v.y);
		}

		public static explicit operator pb_SerializableEdge(pb_Edge v)
		{
			return new pb_SerializableEdge(v);
		}

		public pb_SerializableEdge() {}

		public pb_SerializableEdge(pb_Edge v)
		{
			this.x = v.x;
			this.y = v.y;
		}
	}
}