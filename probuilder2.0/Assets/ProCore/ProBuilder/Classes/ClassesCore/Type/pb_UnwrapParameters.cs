using UnityEngine;

namespace ProBuilder.Core
{
	/// <summary>
	/// Store UV2 unwrapping parameters.
	/// </summary>
	[System.Serializable]
	public class pb_UnwrapParameters
	{
		/// <summary>
		/// Angle between neighbor triangles that will generate seam.
		/// </summary>
		[Tooltip("Angle between neighbor triangles that will generate seam.")]
		[Range(1f, 180f)] public float hardAngle = 88f;

		/// <summary>
		/// Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.
		/// </summary>
		[Tooltip("Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.")]
		[Range(1f, 64f)] public float packMargin = 4f;

		/// <summary>
		/// Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error measures deviation of UV triangles area from geometry triangles if they were uniformly scaled.
		/// </summary>
		[Tooltip("Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error measures deviation of UV triangles area from geometry triangles if they were uniformly scaled.")]
		[Range(1f, 75f)] public float angleError = 8f;

		/// <summary>
		/// Does... something.
		/// </summary>
		[Tooltip("Does... something.")]
		[Range(1f, 75f)] public float areaError = 15f;

		/// <summary>
		/// Reset the unwrap parameters to default values.
		/// </summary>
		public void Reset()
		{
			hardAngle 	= 88f;
			packMargin 	= 4f;
			angleError 	= 8f;
			areaError 	= 15f;
		}
	}
}
