using System.Collections.Generic;
using ProBuilder.Core;
using UnityEngine;
using UnityEditor;

namespace ProBuilder.EditorCore
{
	[System.Obsolete("Use pb_ToolbarGroup instead")]
	public enum pb_IconGroup
	{
		Tool = 0,
		Selection = 1,
		Object = 2,
		Geometry = 3,
		Entity = 4,
		Export = 5
	}

	/// <summary>
	/// Defines what area of the ProBuilder toolbar a pb_MenuAction should be grouped into.
	/// </summary>
	public enum pb_ToolbarGroup
	{
		/// <summary>
		/// This is an editor window.
		/// </summary>
		Tool = 0,
		/// <summary>
		/// This is an interface toggle or an element selection action.
		/// </summary>
		Selection = 1,
		/// <summary>
		/// This action affects objects (as opposed to elements).
		/// </summary>
		Object = 2,
		/// <summary>
		/// This action affects geometry elements (vertices, edges, faces).
		/// </summary>
		Geometry = 3,
		/// <summary>
		/// This is an entity toggle.
		/// </summary>
		Entity = 4,
		/// <summary>
		/// This action exports meshes.
		/// </summary>
		Export = 5
	}

	static class pb_ToolbarGroupUtility
	{
		static readonly Color ToolColor = new Color(0.6666f, 0.4f, 0.2f, 1f);
		static readonly Color SelectionColor = new Color(0.1411f, 0.4941f, 0.6392f, 1f);
		static readonly Color ObjectColor = new Color(0.4f, 0.6f, 0.1333f, 1f);
		static readonly Color GeometryColor = new Color(0.7333f, 0.1333f, 0.2f, 1f);

		public static Color GetColor(pb_ToolbarGroup group)
		{
			if (group == pb_ToolbarGroup.Tool)
				return ToolColor;
			else if (group == pb_ToolbarGroup.Selection)
				return SelectionColor;
			else if (group == pb_ToolbarGroup.Object || group == pb_ToolbarGroup.Entity)
				return ObjectColor;
			else if (group == pb_ToolbarGroup.Geometry)
				return GeometryColor;

			return Color.white;
		}

		private static GUIStyle CreateBackgroundStyleTemplate()
		{
			GUIStyle style = new GUIStyle();
			style.normal.textColor = EditorGUIUtility.isProSkin ? pb_MenuActionStyles.TEXT_COLOR_WHITE_NORMAL : Color.black;
			style.hover.textColor = EditorGUIUtility.isProSkin ? pb_MenuActionStyles.TEXT_COLOR_WHITE_HOVER : Color.black;
			style.active.textColor = EditorGUIUtility.isProSkin ? pb_MenuActionStyles.TEXT_COLOR_WHITE_ACTIVE : Color.black;
			style.alignment = TextAnchor.MiddleCenter;
			style.border = new RectOffset(3, 3, 3, 3);
			style.stretchWidth = true;
			style.stretchHeight = false;
			return style;
		}

		private static Dictionary<string, GUIStyle> m_IconBackgroundStyles = new Dictionary<string, GUIStyle>();

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

			if(m_IconBackgroundStyles.TryGetValue(groupKey, out style))
				return style;

			style = CreateBackgroundStyleTemplate();

			style.normal.background = pb_IconUtility.GetIcon(
				string.Format("Toolbar/Background/{0}_Normal_{1}", group, isHorizontal ? "Horizontal" : "Vertical"));
			style.hover.background = pb_IconUtility.GetIcon(
				string.Format("Toolbar/Background/{0}_Hover_{1}", group, isHorizontal ? "Horizontal" : "Vertical"));
			style.active.background = pb_IconUtility.GetIcon(
				string.Format("Toolbar/Background/{0}_Pressed_{1}", group, isHorizontal ? "Horizontal" : "Vertical"));

			m_IconBackgroundStyles.Add(groupKey, style);
			style.margin = isHorizontal ? new RectOffset(4, 4, 4, 5) : new RectOffset(4, 3, 4, 4);
			style.padding = isHorizontal ? new RectOffset(3, 3, 6, 3) : new RectOffset(6, 3, 3, 3);

			return style;
		}

		/**
		 * Get the background button style for a toolbar group.
		 */
		public static GUIStyle GetStyle(pb_ToolbarGroup group, bool isHorizontal)
		{
			if (group == pb_ToolbarGroup.Tool)
				return GetBackgroundStyle("Tool", isHorizontal);
			else if (group == pb_ToolbarGroup.Selection)
				return GetBackgroundStyle("Selection", isHorizontal);
			else if (group == pb_ToolbarGroup.Object || group == pb_ToolbarGroup.Entity)
				return GetBackgroundStyle("Object", isHorizontal);
			else // if( group == pb_ToolbarGroup.Geometry )
				return GetBackgroundStyle("Geo", isHorizontal);
		}
	}
}