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

        private static GUIStyle CreateBackgroundStyleTemplate()
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = EditorGUIUtility.isProSkin ? MenuActionStyles.TEXT_COLOR_WHITE_NORMAL : Color.black;
            style.hover.textColor = EditorGUIUtility.isProSkin ? MenuActionStyles.TEXT_COLOR_WHITE_HOVER : Color.black;
            style.active.textColor = EditorGUIUtility.isProSkin ? MenuActionStyles.TEXT_COLOR_WHITE_ACTIVE : Color.black;
            style.alignment = TextAnchor.MiddleCenter;
            style.border = new RectOffset(3, 3, 3, 3);
            style.stretchWidth = true;
            style.stretchHeight = false;
            return style;
        }

        private static Dictionary<string, GUIStyle> s_IconBackgroundStyles = new Dictionary<string, GUIStyle>();

        /**
         * Where @group corresponds to:
         * - Geo
         * - Object
         * - Selection
         * - Tool
         */
        private static GUIStyle GetBackgroundStyle(string group, bool isHorizontal)
        {
            GUIStyle style;

            string groupKey = string.Format("{0}_{1}", group, isHorizontal ? "_horizontal" : "_vertical");

            if (s_IconBackgroundStyles.TryGetValue(groupKey, out style))
                return style;

            style = CreateBackgroundStyleTemplate();

            style.normal.background = IconUtility.GetIcon(
                    string.Format("Toolbar/Background/{0}_Normal_{1}", group, isHorizontal ? "Horizontal" : "Vertical"));
            style.hover.background = IconUtility.GetIcon(
                    string.Format("Toolbar/Background/{0}_Hover_{1}", group, isHorizontal ? "Horizontal" : "Vertical"));
            style.active.background = IconUtility.GetIcon(
                    string.Format("Toolbar/Background/{0}_Pressed_{1}", group, isHorizontal ? "Horizontal" : "Vertical"));

            s_IconBackgroundStyles.Add(groupKey, style);
            style.margin = isHorizontal ? new RectOffset(4, 4, 4, 5) : new RectOffset(4, 3, 4, 4);
            style.padding = isHorizontal ? new RectOffset(3, 3, 6, 3) : new RectOffset(6, 3, 3, 3);

            return style;
        }

        /**
         * Get the background button style for a toolbar group.
         */
        public static GUIStyle GetStyle(ToolbarGroup group, bool isHorizontal)
        {
            if (group == ToolbarGroup.Tool)
                return GetBackgroundStyle("Tool", isHorizontal);
            else if (group == ToolbarGroup.Selection)
                return GetBackgroundStyle("Selection", isHorizontal);
            else if (group == ToolbarGroup.Object || group == ToolbarGroup.Entity)
                return GetBackgroundStyle("Object", isHorizontal);
            else // if( group == pb_ToolbarGroup.Geometry )
                return GetBackgroundStyle("Geo", isHorizontal);
        }
    }
}
