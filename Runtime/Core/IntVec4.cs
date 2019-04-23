using UnityEngine;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Vertex positions are sorted as integers to avoid floating point precision errors.
	/// </summary>
	struct IntVec4 : System.IEquatable<IntVec4>
	{
		public Vector4 value;

		public float x { get { return value.x; } }
		public float y { get { return value.y; } }
		public float z { get { return value.z; } }
		public float w { get { return value.w; } }

		public IntVec4(Vector4 vector)
		{
			this.value = vector;
		}

		public override string ToString()
		{
			return string.Format("({0:F2}, {1:F2}, {2:F2}, {3:F2})", x, y, z, w);
		}

		public static bool operator==(IntVec4 a, IntVec4 b)
		{
			return a.Equals(b);
		}

		public static bool operator!=(IntVec4 a, IntVec4 b)
		{
			return !(a == b);
		}

		public bool Equals(IntVec4 p)
		{
			return round(x) == round(p.x) &&
				round(y) == round(p.y) &&
				round(z) == round(p.z) &&
				round(w) == round(p.w);
		}

		public bool Equals(Vector4 p)
		{
			return round(x) == round(p.x) &&
				round(y) == round(p.y) &&
				round(z) == round(p.z) &&
				round(w) == round(p.w);
		}

		public override bool Equals(System.Object b)
		{
			return (b is IntVec4 && (this.Equals((IntVec4)b))) ||
				(b is Vector4 && this.Equals((Vector4)b));
		}

		public override int GetHashCode()
		{
			return VectorHash.GetHashCode(value);
		}

		private static int round(float v)
		{
			return System.Convert.ToInt32(v * VectorHash.FltCompareResolution);
		}

		public static implicit operator Vector4(IntVec4 p)
		{
			return p.value;
		}

		public static implicit operator IntVec4(Vector4 p)
		{
			return new IntVec4(p);
		}
	}
}
