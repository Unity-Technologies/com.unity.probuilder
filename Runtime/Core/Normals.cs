using System;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// A container for normal, tangent, and bitangent values.
	/// </summary>
	public struct Normals : IEquatable<Normals>
	{
		/// <value>
		/// A unit normal.
		/// </value>
		public Vector3 normal { get; set; }

		/// <value>
		/// A unit tangent.
		/// </value>
		public Vector4 tangent { get; set; }

		/// <value>
		/// A unit bitangent (sometimes called binormal).
		/// </value>
		public Vector3 bitangent { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Normals && Equals((Normals)obj);
        }

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = VectorHash.GetHashCode(normal);
				hashCode = (hashCode * 397) ^ VectorHash.GetHashCode(tangent);
				hashCode = (hashCode * 397) ^ VectorHash.GetHashCode(bitangent);
				return hashCode;
			}
		}

		public bool Equals(Normals other)
        {
            return Math.Approx3(normal, other.normal) &&
                Math.Approx3(tangent, other.tangent) &&
                Math.Approx3(bitangent, other.bitangent);
        }

        public static bool operator ==(Normals a, Normals b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Normals a, Normals b)
        {
            return !(a == b);
        }
    }
}
