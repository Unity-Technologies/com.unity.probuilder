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
