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

        public bool Equals(object obj)
        {
            return obj is Normals && Equals((Normals)obj);
        }

        public bool Equals(Normals other)
        {
            if (object.ReferenceEquals(other, null))
                return false;
            return ProBuilderMath.Approx3(normal, other.normal) &&
                ProBuilderMath.Approx3(tangent, other.tangent) &&
                ProBuilderMath.Approx3(bitangent, other.bitangent);
        }

        public static bool operator ==(Normals a, Normals b)
        {
            if (object.ReferenceEquals(a, null))
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(Normals a, Normals b)
        {
            return !(a == b);
        }
    }
}
