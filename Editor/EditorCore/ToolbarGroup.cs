using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Defines the [tool category](../manual/toolbar.html#category) for a MenuAction.
    /// In the Unity Editor, ProBuilder groups actions with the same tool category on
    /// the ProBuilder toolbar using color coding.
    /// </summary>
    public enum ToolbarGroup
    {
        /// <summary>
        /// A tool that opens it's own window. Example, UV Editor, Smoothing Groups, Vertex Color Painter, etc.
        /// </summary>
        Tool = 0,
        /// <summary>
        /// This is an interface toggle or an element selection action.
        /// </summary>
        Selection = 1,
        /// <summary>
        /// This action affects objects (as opposed to mesh attributes like vertex or face).
        /// </summary>
        Object = 2,
        /// <summary>
        /// This action affects geometry elements (vertices, edges, faces).
        /// </summary>
        Geometry = 3,
        /// <summary>
        /// An action for creating or modifying @"UnityEngine.ProBuilder.EntityBehaviour" types.
        /// </summary>
        Entity = 4,
        /// <summary>
        /// This action exports meshes.
        /// </summary>
        Export = 5
    }

    static class ToolbarGroupUtility
    {
        static readonly Color ToolColor = new Color(0.6666f, 0.4f, 0.2f, 1f);
        static readonly Color SelectionColor = new Color(0.1411f, 0.4941f, 0.6392f, 1f);
        static readonly Color ObjectColor = new Color(0.4f, 0.6f, 0.1333f, 1f);
        static readonly Color GeometryColor = new Color(0.7333f, 0.1333f, 0.2f, 1f);

        public static Color GetColor(ToolbarGroup group)
        {
            if (group == ToolbarGroup.Tool)
                return ToolColor;
            else if (group == ToolbarGroup.Selection)
                return SelectionColor;
            else if (group == ToolbarGroup.Object || group == ToolbarGroup.Entity)
                return ObjectColor;
            else if (group == ToolbarGroup.Geometry)
                return GeometryColor;

            return Color.white;
        }
    }
}
