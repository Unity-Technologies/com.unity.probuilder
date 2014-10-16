/**
 *	Used to describe split face actions.
 */
using System.Collections.Generic;
#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.Common
{
	public class VertexConnection : System.IEquatable<VertexConnection>
	{
		public VertexConnection(pb_Face face, List<int> indices)
		{
			this.face = face;
			this.indices = indices;
		}

		public pb_Face face;
		public List<int> indices;

		public bool isValid {
			get { return indices != null && indices.Count > 1; }
		}

		public VertexConnection Distinct(pb_IntArray[] sharedIndices)
		{
			return new VertexConnection(this.face, sharedIndices.UniqueIndicesWithValues(indices));
		}

		public override bool Equals(System.Object b)
		{
			return b is VertexConnection ? this.face == ((VertexConnection)b).face : false;
		}

		public bool Equals(VertexConnection vc)
		{
			return this.face == vc.face;
		}

		public static implicit operator pb_Face(VertexConnection vc)
		{
			return vc.face;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return face.ToString() + " : " + indices.ToFormattedString(", ");
		}

		/**
		 * Returns a List<int> of all the indices in this List of VertexConnections.
		 */
		public static List<int> AllTriangles(List<VertexConnection> vcs)
		{
			List<int> tris = new List<int>();
			for(int i = 0; i < vcs.Count; i++)
				tris.AddRange(vcs[i].indices);
			return tris;
		}
	}
}