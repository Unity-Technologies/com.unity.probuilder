using UnityEngine;
using System.Collections;

namespace ProBuilder2.Common {

	/**
	 * A class for storing and applying Vector2 masks.
	 */
	public class pb_HandleConstraint2D
	{
		public int x, y;
		
		public pb_HandleConstraint2D(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
		
		public pb_HandleConstraint2D Inverse()
		{
			return new pb_HandleConstraint2D(x == 1 ? 0 : 1, y == 1 ? 0 : 1);
		}

		public Vector2 Mask(Vector2 v)
		{
			v.x *= this.x;
			v.y *= this.y;
			return v;
		}

		public Vector2 InverseMask(Vector2 v)
		{
			v.x *= this.x == 1 ? 0f : 1f;
			v.y *= this.y == 1 ? 0f : 1f;
			return v;
		}

		public static readonly pb_HandleConstraint2D None = new pb_HandleConstraint2D(1, 1);

		public static bool operator==(pb_HandleConstraint2D a, pb_HandleConstraint2D b)
		{
			return a.x == b.x && a.y == b.y;
		}

		public static bool operator!=(pb_HandleConstraint2D a, pb_HandleConstraint2D b)
		{
			return a.x != b.x || a.y != b.y;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object o)
		{
			return o is pb_HandleConstraint2D && ((pb_HandleConstraint2D)o).x == this.x && ((pb_HandleConstraint2D)o).y == this.y;
		}

		public override string ToString()
		{
			return "(" + x + ", " + y + ")";
		}
	}
}