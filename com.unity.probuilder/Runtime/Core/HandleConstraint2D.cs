using UnityEngine;
using System.Collections;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A class for storing and applying Vector2 masks.
    /// </summary>
    sealed class HandleConstraint2D
    {
        public int x, y;

        public HandleConstraint2D(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public HandleConstraint2D Inverse()
        {
            return new HandleConstraint2D(x == 1 ? 0 : 1, y == 1 ? 0 : 1);
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

        public static readonly HandleConstraint2D None = new HandleConstraint2D(1, 1);

        public static bool operator==(HandleConstraint2D a, HandleConstraint2D b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator!=(HandleConstraint2D a, HandleConstraint2D b)
        {
            return a.x != b.x || a.y != b.y;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object o)
        {
            return o is HandleConstraint2D && ((HandleConstraint2D)o).x == this.x && ((HandleConstraint2D)o).y == this.y;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
    }
}
