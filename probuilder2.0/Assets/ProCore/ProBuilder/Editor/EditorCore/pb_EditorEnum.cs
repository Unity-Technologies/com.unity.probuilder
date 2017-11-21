
namespace ProBuilder.EditorCore
{
	/// <summary>
	/// The default tool to use when opening the vertex color editor from the pb_Editor window.
	/// </summary>
	enum VertexColorTool
	{
		Palette,
		Painter
	}

	/// <summary>
	/// Where the sceneview toolbar sits in relation the scene view rect.
	/// </summary>
	enum SceneToolbarLocation
	{
		UpperCenter,
		UpperLeft,
		UpperRight,
		BottomCenter,
		BottomLeft,
		BottomRight
	}
	
	/// <summary>
	/// How the handle gizmo is oriented with regards to the current element selection.
	/// </summary>
	/// <remarks>Editor only.</remarks>
	public enum HandleAlignment
	{
		World = 0,
		Local = 1,
		Plane = 2
	}

	/// <summary>
	/// When drag selecting elements, how does the shift key modify selection.
	/// </summary>
	/// <remarks>Editor only.</remarks>
	public enum DragSelectMode
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

	/// <summary>
	/// How should Unity represent selected objects?
	/// </summary>
	/// <remarks>Editor only.</remarks>
	[System.Flags]
	public enum SelectionRenderState
	{
		None = 0x0,
		Wireframe = 0x1,
		Outline = 0x2
	}
}