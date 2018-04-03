using UnityEngine;

namespace ProBuilder.Core
{
	/// <summary>
	/// Vertex positions are sorted as integers to avoid floating point precision errors.
	/// </summary>
	struct pb_IntVec3 : System.IEquatable<pb_IntVec3>
	{
		public Vector3 vec;

		public float x { get { return vec.x; } }
		public float y { get { return vec.y; } }
		public float z { get { return vec.z; } }

		public const float RESOLUTION = pb_Vector.FLT_COMPARE_RESOLUTION;

		public pb_IntVec3(Vector3 vector)
		{
			this.vec = vector;
		}

		public override string ToString()
		{
			return string.Format("({0:F2}, {1:F2}, {2:F2})", x, y, z);
		}

		public static bool operator ==(pb_IntVec3 a, pb_IntVec3 b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(pb_IntVec3 a, pb_IntVec3 b)
		{
			return !(a == b);
		}

		public bool Equals(pb_IntVec3 p)
		{
			return  round(x) == round(p.x) &&
					round(y) == round(p.y) &&
					round(z) == round(p.z);
		}

		public bool Equals(Vector3 p)
		{
			return  round(x) == round(p.x) &&
					round(y) == round(p.y) &&
					round(z) == round(p.z);
		}

		public override bool Equals(System.Object b)
		{
			return 	(b is pb_IntVec3 && ( this.Equals((pb_IntVec3)b) )) ||
					(b is Vector3 && this.Equals((Vector3)b));
		}

		public override int GetHashCode()
		{
			return pb_Vector.GetHashCode(vec);
		}

		private static int round(float v)
		{
			return System.Convert.ToInt32(v * RESOLUTION);
		}

		public static implicit operator Vector3(pb_IntVec3 p)
		{
			return p.vec;
		}

		public static implicit operator pb_IntVec3(Vector3 p)
		{
			return new pb_IntVec3(p);
		}
	}
}
