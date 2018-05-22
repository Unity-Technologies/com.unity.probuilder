using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// A set of common and local index edges.
	/// </summary>
	[System.Obsolete("Use pb_ConnectEdges class directly.")]
	sealed class EdgeConnection : System.IEquatable<EdgeConnection>
	{
		public EdgeConnection(Face face, List<Edge> edges)
		{
			this.face = face;
			this.edges = edges;
		}

		public Face face;

		// IMPORTANT - these edges may not be local to the specified face - always use face.edges.IndexOf(edge, sharedIndices) to get the actual edges
		public List<Edge> edges;

		public bool isValid {
			get { return edges != null && edges.Count > 1; }
		}

		public override bool Equals(System.Object b)
		{
			return b is EdgeConnection ? this.face == ((EdgeConnection)b).face : false;
		}

		public bool Equals(EdgeConnection obj)
		{
			return obj != null && this.face == obj.face;
		}

		public static explicit operator Face(EdgeConnection fc)
		{
			return fc.face;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return face.ToString() + " : " + edges.ToString(", ");
		}

		public static List<int> AllTriangles(List<EdgeConnection> ec)
		{
			List<Edge> edges = new List<Edge>();
			foreach(EdgeConnection e in ec)
				edges.AddRange(e.edges);
			return edges.AllTriangles();
		}
	}
}
