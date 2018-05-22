using UnityEngine;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Store UV2 unwrapping parameters.
	/// </summary>
	[System.Serializable]
	public sealed class UnwrapParameters
	{
		/// <value>
		/// Angle between neighbor triangles that will generate seam.
		/// </value>
		[Tooltip("Angle between neighbor triangles that will generate seam.")]
		[Range(1f, 180f)]
		public float hardAngle = 88f;

		/// <value>
		/// Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.
		/// </value>
		[Tooltip("Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.")]
		[Range(1f, 64f)]
		public float packMargin = 4f;

		/// <value>
		/// Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error measures deviation of UV triangles area from geometry triangles if they were uniformly scaled.
		/// </value>
		[Tooltip(
			"Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error measures deviation of UV triangles area from geometry triangles if they were uniformly scaled.")]
		[Range(1f, 75f)]
		public float angleError = 8f;

		/// <value>
		/// Does... something.
		/// </value>
		[Tooltip("Does... something.")]
		[Range(1f, 75f)]
		public float areaError = 15f;

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
			hardAngle = 88f;
			packMargin = 4f;
			angleError = 8f;
			areaError = 15f;
		}
	}
}
