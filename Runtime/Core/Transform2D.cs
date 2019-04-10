using UnityEngine;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A Transform class limited to 2D
    /// </summary>
    sealed class Transform2D
    {
        /// <summary>
        /// Position in 2D space.
        /// </summary>
        public Vector2  position;

        /// <summary>
        /// Rotation in degrees.
        /// </summary>
        public float rotation;

        /// <summary>
        /// Scale in 2D space.
        /// </summary>
        public Vector2  scale;

        public Transform2D(Vector2 position, float rotation, Vector2 scale)
        {
            this.position   = position;
            this.rotation   = rotation;
            this.scale      = scale;
        }

        public Vector2 TransformPoint(Vector2 p)
        {
            p += position;
            p.RotateAroundPoint(p, rotation);
            p.ScaleAroundPoint(p, scale);
            return p;
        }

        public override string ToString()
        {
            return "T: " + position + "\nR: " + rotation + PreferenceKeys.DEGREE_SYMBOL + "\nS: " + scale;
        }
    }
}
