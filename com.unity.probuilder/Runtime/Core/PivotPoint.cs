namespace UnityEngine.ProBuilder
{
	public enum PivotPoint
	{
		/// <summary>
		/// Transforms are applied from the center point of the selection bounding box.
		/// Corresponds with <see cref="UnityEditor.PivotMode.Center"/>.
		/// </summary>
		WorldBoundingBoxCenter = 1 << 0,

		/// <summary>
		/// Transforms are applied in model space from the average point of selected elements.
		/// Corresponds with <see cref="UnityEditor.PivotMode.Pivot"/>.
		/// </summary>
		ModelBoundingBoxCenter = 1 << 1,

		/// <summary>
		/// Transforms are applied from the origin of each selection group.
		/// </summary>
		IndividualOrigins = 1 << 2,

		/// <summary>
		/// Transforms are applied from a user-defined pivot point.
		/// </summary>
		Custom = 1 << 3
	}
}
