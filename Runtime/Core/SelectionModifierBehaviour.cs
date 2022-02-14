namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Defines how the Shift key modifies the selection when drag-selecting mesh elements.
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
