using System.Collections.Generic;
using System.Linq;

namespace ProBuilder.Core
{
	/// <summary>
	/// Used to describe split face actions.
	/// </summary>
	class pb_VertexConnection : System.IEquatable<pb_VertexConnection>
	{
		public pb_VertexConnection(pb_Face face, List<int> indices)
		{
			this.face = face;
			this.indices = indices;
		}

		public pb_Face face;
		public List<int> indices;

		public bool isValid {
			get { return indices != null && indices.Count > 1; }
		}

		public pb_VertexConnection Distinct(pb_IntArray[] sharedIndices)
		{
			return new pb_VertexConnection(this.face, sharedIndices.UniqueIndicesWithValues(indices).ToList());
		}

		public override bool Equals(System.Object b)
		{
			return b is pb_VertexConnection ? this.face == ((pb_VertexConnection)b).face : false;
		}

		public bool Equals(pb_VertexConnection vc)
		{
			return this.face == vc.face;
		}

		public static implicit operator pb_Face(pb_VertexConnection vc)
		{
			return vc.face;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return face.ToString() + " : " + indices.ToString(", ");
		}

		/**
		 * Returns a List<int> of all the indices in this List of pb_VertexConnections.
		 */
		public static List<int> AllTriangles(List<pb_VertexConnection> vcs)
		{
			List<int> tris = new List<int>();
			for(int i = 0; i < vcs.Count; i++)
				tris.AddRange(vcs[i].indices);
			return tris;
		}
	}
}
