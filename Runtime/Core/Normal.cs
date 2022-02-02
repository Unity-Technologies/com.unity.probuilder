using System;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A container for normal, tangent, and bitangent values.
    /// </summary>
    public struct Normal : IEquatable<Normal>
    {
        /// <summary>
        /// A unit normal.
        /// </summary>
        public Vector3 normal { get; set; }

        /// <summary>
        /// A unit tangent.
        /// </summary>
        public Vector4 tangent { get; set; }

        /// <summary>
        /// A unit bitangent (sometimes called binormal).
        /// </summary>
        public Vector3 bitangent { get; set; }

        /// <summary>
        /// Tests whether this object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>True if the objects are equal, false if not.</returns>
        public override bool Equals(object obj)
        {
            return obj is Normal && Equals((Normal)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>An integer that is the hash code for this instance.</returns>
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

        /// <summary>
        /// Tests whether the specified Normal is equal to this one.
        /// </summary>
        /// <param name="other">The Normal object to compare against.</param>
        /// <returns>True if the objects are equal, false if not.</returns>
        public bool Equals(Normal other)
        {
            return Math.Approx3(normal, other.normal) &&
                Math.Approx3(tangent, other.tangent) &&
                Math.Approx3(bitangent, other.bitangent);
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="a">The first Normal instance.</param>
        /// <param name="b">The second Normal instance.</param>
        /// <returns>True if the objects are equal; false if not.</returns>
        public static bool operator==(Normal a, Normal b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares two objects for inequality.
        /// </summary>
        /// <param name="a">The first Normal instance.</param>
        /// <param name="b">The second Normal instance.</param>
        /// <returns>True if the objects are not equal; false if not.</returns>
        public static bool operator!=(Normal a, Normal b)
        {
            return !(a == b);
        }
    }
}
