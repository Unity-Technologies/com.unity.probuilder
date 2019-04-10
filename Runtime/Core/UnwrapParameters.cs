using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Store UV2 unwrapping parameters.
    /// </summary>
    [System.Serializable]
    public sealed class UnwrapParameters
    {
        internal const float k_HardAngle = 88f;
        internal const float k_PackMargin = 20f;
        internal const float k_AngleError = 8f;
        internal const float k_AreaError = 15f;

        [Tooltip("Angle between neighbor triangles that will generate seam.")]
        [Range(1f, 180f)]
        [SerializeField]
        [FormerlySerializedAs("hardAngle")]
        float m_HardAngle = k_HardAngle;

        [Tooltip("Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.")]
        [Range(1f, 64f)]
        [SerializeField]
        [FormerlySerializedAs("packMargin")]
        float m_PackMargin = k_PackMargin;

        [Tooltip("Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error "
             + "measures deviation of UV triangles area from geometry triangles if they were uniformly scaled.")]
        [Range(1f, 75f)]
        [SerializeField]
        [FormerlySerializedAs("angleError")]
        float m_AngleError = k_AngleError;

        [Range(1f, 75f)]
        [SerializeField]
        [FormerlySerializedAs("areaError")]
        float m_AreaError = k_AreaError;

        /// <value>
        /// Angle between neighbor triangles that will generate seam.
        /// </value>
        public float hardAngle { get { return m_HardAngle; } set { m_HardAngle = value; } }

        /// <value>
        /// Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.
        /// </value>
        public float packMargin { get { return m_PackMargin; } set { m_PackMargin = value; } }

        /// <value>
        /// Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error measures
        /// deviation of UV triangles area from geometry triangles if they were uniformly scaled.
        /// </value>
        public float angleError { get { return m_AngleError; } set { m_AngleError = value; } }

        /// <value>
        /// Does... something.
        /// </value>
        public float areaError { get { return m_AreaError; } set { m_AreaError = value; } }

        public UnwrapParameters()
        {
            Reset();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="other">The UnwrapParameters to copy properties from.</param>
        public UnwrapParameters(UnwrapParameters other)
        {
            if (other == null)
                throw new System.ArgumentNullException("other");

            hardAngle = other.hardAngle;
            packMargin = other.packMargin;
            angleError = other.angleError;
            areaError = other.areaError;
        }

        /// <summary>
        /// Reset the unwrap parameters to default values.
        /// </summary>
        public void Reset()
        {
            hardAngle = k_HardAngle;
            packMargin = k_PackMargin;
            angleError = k_AngleError;
            areaError = k_AreaError;
        }

        public override string ToString()
        {
            return string.Format("hardAngle: {0}\npackMargin: {1}\nangleError: {2}\nareaError: {3}",
                hardAngle,
                packMargin,
                angleError,
                areaError);
        }
    }
}
