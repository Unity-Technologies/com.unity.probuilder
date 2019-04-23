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

        static int HashFloat(float f)
        {
            ulong u = (ulong)(f * FltCompareResolution);
            return (int)(u % int.MaxValue);
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
                hash = hash * 29 + HashFloat(v.x);
                hash = hash * 29 + HashFloat(v.y);
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
                hash = hash * 29 + HashFloat(v.x);
                hash = hash * 29 + HashFloat(v.y);
                hash = hash * 29 + HashFloat(v.z);
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
                hash = hash * 29 + HashFloat(v.x);
                hash = hash * 29 + HashFloat(v.y);
                hash = hash * 29 + HashFloat(v.z);
                hash = hash * 29 + HashFloat(v.w);
            }

            return hash;
        }
    }
}
