using UnityEngine;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Vertex positions are sorted as integers to avoid floating point precision errors.
	/// </summary>
	struct IntVec2 : System.IEquatable<IntVec2>
	{
		public Vector2 value;

		public float x { get { return value.x; } }
		public float y { get { return value.y; } }

		public IntVec2(Vector2 vector)
		{
			this.value = vector;
		}

		public override string ToString()
		{
			return string.Format("({0:F2}, {1:F2})", x, y);
		}

		public static bool operator==(IntVec2 a, IntVec2 b)
		{
			return a.Equals(b);
		}

		public static bool operator!=(IntVec2 a, IntVec2 b)
		{
			return !(a == b);
		}

		public bool Equals(IntVec2 p)
		{
			return round(x) == round(p.x) &&
				round(y) == round(p.y);
		}

		public bool Equals(Vector2 p)
		{
			return round(x) == round(p.x) &&
				round(y) == round(p.y);
		}

		public override bool Equals(System.Object b)
		{
			return (b is IntVec2 && (this.Equals((IntVec2)b))) ||
				(b is Vector2 && this.Equals((Vector2)b));
		}

		public override int GetHashCode()
		{
			return VectorHash.GetHashCode(value);
		}

		private static int round(float v)
		{
			return System.Convert.ToInt32(v * VectorHash.FltCompareResolution);
		}

		public static implicit operator Vector2(IntVec2 p)
		{
			return p.value;
		}

		public static implicit operator IntVec2(Vector2 p)
		{
			return new IntVec2(p);
		}
	}
}
