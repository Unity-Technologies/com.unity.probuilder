using System;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// A container for normal, tangent, and bitangent values.
	/// </summary>
	public struct Normals : IEquatable<Normals>
	{
		public Vector3 normal { get; set; }
		public Vector4 tangent { get; set; }
		public Vector3 bitangent { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Normals && Equals((Normals)obj);
        }

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = normal.GetHashCode();
				hashCode = (hashCode * 397) ^ tangent.GetHashCode();
				hashCode = (hashCode * 397) ^ bitangent.GetHashCode();
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
