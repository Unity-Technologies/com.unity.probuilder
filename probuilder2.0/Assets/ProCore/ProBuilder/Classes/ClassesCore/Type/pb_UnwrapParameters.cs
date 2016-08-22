using UnityEngine;

namespace ProBuilder2.Common
{
	/**
	 *	Store UV2 unwrapping parameters.
	 */
	[System.Serializable]
	public class pb_UnwrapParameters
	{
		[Tooltip("Angle between neighbor triangles that will generate seam.")]
		[Range(1f, 180f)] public float hardAngle = 88f; 
		[Tooltip("Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.")]
		[Range(1f, 64f)] public float packMargin = 8f;
		[Tooltip("Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error measures deviation of UV triangles area from geometry triangles if they were uniformly scaled.")]
		[Range(1f, 75f)] public float angleError = 15f;
		[Tooltip("Does... something.")]
		[Range(1f, 75f)] public float areaError = 15f;

		public void Reset()
		{
			hardAngle 	= 88f; 
			packMargin 	= 8f;
			angleError 	= 15f;	// default is actually 8 for unity importing, but it's not usually enough
			areaError 	= 15f;
		}
	}
}
