using UnityEngine;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Vertex positions are sorted as integers to avoid floating point precision errors.
    /// </summary>
    struct IntVec3 : System.IEquatable<IntVec3>
    {
        public Vector3 vec;

        public float x { get { return vec.x; } }
        public float y { get { return vec.y; } }
        public float z { get { return vec.z; } }

        public const float RESOLUTION = VectorHash.FLT_COMPARE_RESOLUTION;

        public IntVec3(Vector3 vector)
        {
            this.vec = vector;
        }

        public override string ToString()
        {
            return string.Format("({0:F2}, {1:F2}, {2:F2})", x, y, z);
        }

        public static bool operator==(IntVec3 a, IntVec3 b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(IntVec3 a, IntVec3 b)
        {
            return !(a == b);
        }

        public bool Equals(IntVec3 p)
        {
            return round(x) == round(p.x) &&
                round(y) == round(p.y) &&
                round(z) == round(p.z);
        }

        public bool Equals(Vector3 p)
        {
            return round(x) == round(p.x) &&
                round(y) == round(p.y) &&
                round(z) == round(p.z);
        }

        public override bool Equals(System.Object b)
        {
            return (b is IntVec3 && (this.Equals((IntVec3)b))) ||
                (b is Vector3 && this.Equals((Vector3)b));
        }

        public override int GetHashCode()
        {
            return VectorHash.GetHashCode(vec);
        }

        private static int round(float v)
        {
            return System.Convert.ToInt32(v * RESOLUTION);
        }

        public static implicit operator Vector3(IntVec3 p)
        {
            return p.vec;
        }

        public static implicit operator IntVec3(Vector3 p)
        {
            return new IntVec3(p);
        }
    }
}
