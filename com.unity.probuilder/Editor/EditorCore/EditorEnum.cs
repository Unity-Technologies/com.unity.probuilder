using System;

namespace UnityEditor.ProBuilder
{
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
