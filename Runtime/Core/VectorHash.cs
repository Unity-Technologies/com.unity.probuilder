using System;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Fuzzy hashing functions for vector types. Exists as a shortcut to create hashcodes for Vector3 in the style of
    /// IntVector3 without the overhead of casting.
    /// </summary>
    static class VectorHash
    {
        public const float FltCompareResolution = 1000f;

        /// <summary>
        /// Quantize a float to FltCompareResolution. Shared by IntVec2/IntVec3/IntVec4 equality and
        /// by the hash functions below, so that values which compare equal always hash equally.
        /// </summary>
        internal static int RoundToInt(float f)
        {
            if (float.IsNaN(f))
                return 0;

            float scaled = f * FltCompareResolution;

            // Casting an out-of-range float to an integer type is unspecified in ECMA-335 - Mono on
            // x64 wraps around while CoreCLR and Mono on Arm64 saturate, which also collapses every
            // negative component to 0 - so clamp explicitly (UUM-148935, UUM-111993).
            if (scaled <= int.MinValue)
                return int.MinValue;

            if (scaled >= int.MaxValue)
                return int.MaxValue;

            // System.Math, not UnityEngine.ProBuilder.Math
            return (int)System.Math.Round(scaled, MidpointRounding.ToEven);
        }

        /// <summary>
        /// Return the rounded hashcode for a vector2
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static int GetHashCode(Vector2 v)
        {
            // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
            int hash = 27;

            unchecked
            {
                hash = hash * 29 + RoundToInt(v.x);
                hash = hash * 29 + RoundToInt(v.y);
            }

            return hash;
        }

        /// <summary>
        /// Return the hashcode for a vector3 without first converting it to pb_IntVec3.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static int GetHashCode(Vector3 v)
        {
            // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
            int hash = 27;

            unchecked
            {
                hash = hash * 29 + RoundToInt(v.x);
                hash = hash * 29 + RoundToInt(v.y);
                hash = hash * 29 + RoundToInt(v.z);
            }

            return hash;
        }

        /// <summary>
        /// Return the hashcode for a vector3 without first converting it to pb_IntVec3.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static int GetHashCode(Vector4 v)
        {
            // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
            int hash = 27;

            unchecked
            {
                hash = hash * 29 + RoundToInt(v.x);
                hash = hash * 29 + RoundToInt(v.y);
                hash = hash * 29 + RoundToInt(v.z);
                hash = hash * 29 + RoundToInt(v.w);
            }

            return hash;
        }
    }
}
