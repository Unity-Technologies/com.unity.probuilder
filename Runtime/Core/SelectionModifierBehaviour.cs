namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// When drag selecting mesh elements, this defines how the Shift key will modify the selection.
	/// </summary>
	/// <remarks>Editor only.</remarks>
	public enum SelectionModifierBehavior
	{
		/// <summary>
		/// Always add to the selection.
		/// </summary>
		Add,
		/// <summary>
		/// Always subtract from the selection.
		/// </summary>
		Subtract,
		/// <summary>
		/// Invert the selected faces (default).
		/// </summary>
		Difference
	}
}
