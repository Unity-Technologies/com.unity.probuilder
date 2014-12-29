/**
 *	Used to describe split face actions.
 */
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	public class pb_EdgeConnection : System.IEquatable<pb_EdgeConnection>
	{
		public pb_EdgeConnection(pb_Face face, List<pb_Edge> edges)
		{
			this.face = face;
			this.edges = edges;
		}

		public pb_Face face;
		public List<pb_Edge> edges;	// IMPORTANT - these edges may not be local to the specified face - always use face.edges.IndexOf(edge, sharedIndices) to get the actual edges

		public bool isValid {
			get { return edges != null && edges.Count > 1; }
		}

		public override bool Equals(System.Object b)
		{
			return b is pb_EdgeConnection ? this.face == ((pb_EdgeConnection)b).face : false;
		}

		public bool Equals(pb_EdgeConnection fc)
		{
			return this.face == fc.face;
		}

		public static explicit operator pb_Face(pb_EdgeConnection fc)
		{
			return fc.face;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return face.ToString() + " : " + edges.ToFormattedString(", ");
		}

		public static List<int> AllTriangles(List<pb_EdgeConnection> ec)
		{
			List<pb_Edge> edges = new List<pb_Edge>();
			foreach(pb_EdgeConnection e in ec)	
				edges.AddRange(e.edges);
			return edges.AllTriangles();
		}
	}
}