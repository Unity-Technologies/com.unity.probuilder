
namespace UnityEditor.ProBuilder
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
	/// <remarks>
	/// This overrides the Unity Pivot / Global setting when editing vertices, faces, or edges.
	/// </remarks>
	/// <remarks>Editor only.</remarks>
	public enum HandleAlignment
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
		Plane = 2
	}

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

	/// <summary>
	/// How should Unity represent selected objects?
	/// </summary>
	/// <remarks>Editor only.</remarks>
	[System.Flags]
	enum SelectionRenderState
	{
		None = 0x0,
		Wireframe = 0x1,
		Outline = 0x2
	}
}