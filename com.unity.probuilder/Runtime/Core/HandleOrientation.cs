namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// How the handle gizmo is oriented with regards to the current element selection.
	/// </summary>
	/// <remarks>
	/// This overrides the Unity Pivot / Global setting when editing vertices, faces, or edges.
	/// </remarks>
	/// <remarks>Editor only.</remarks>
	public enum HandleOrientation
	{
		/// <summary>
		/// The gizmo is aligned in world space.
		/// </summary>
		World = 0,
		/// <summary>
		/// The gizmo is aligned relative to the mesh transform. Also called coordinate or model space.
		/// </summary>
		Local = 1,
		/// <summary>
		/// The gizmo is aligned relative to the currently selected face. When editing vertices or edges, this falls back to <see cref="Local"/> alignment.
		/// </summary>
		Normal = 2
	}
}
