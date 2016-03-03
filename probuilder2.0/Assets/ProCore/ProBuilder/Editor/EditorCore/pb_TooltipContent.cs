using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Interface;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Describes a menu action.
	 */
	[System.Serializable]
	public class pb_TooltipContent : System.IEquatable<pb_TooltipContent>
	{
		static GUIStyle TitleStyle { get { if(_titleStyle == null) InitStyles(); return _titleStyle; } }
		static GUIStyle _titleStyle = null;

		static void InitStyles()
		{
			_titleStyle = new GUIStyle();
			_titleStyle.margin = new RectOffset(4,4,4,4);
			_titleStyle.padding = new RectOffset(4,4,4,4);
			_titleStyle.fontSize = 14;
			_titleStyle.fontStyle = FontStyle.Bold;
			_titleStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
			_titleStyle.richText = true;
		}

		static readonly Color separatorColor = new Color(.65f, .65f, .65f, .5f);

		public string name;
		public string summary;

		public pb_TooltipContent(string name, string summary)
		{
			this.name = name;
			this.summary = summary;
		}

		public Vector2 CalcSize()
		{
			const float pad = 20;
			Vector2 total = new Vector2(256, 256);

			Vector2 ns = TitleStyle.CalcSize(pb_GUI_Utility.TempGUIContent(name));

			float width = Mathf.Max(ns.x + pad, 256);

			float dh = EditorStyles.wordWrappedLabel.CalcHeight(pb_GUI_Utility.TempGUIContent(summary), width);

			total.x = width;
			total.y = ns.y + dh + pad;

			return total;
		}

		public void Draw()
		{
			GUILayout.Label(name, TitleStyle);

			pb_GUI_Utility.DrawSeparator(1, separatorColor);
			GUILayout.Space(2);

			GUILayout.Label(summary, EditorStyles.wordWrappedLabel);
		}

		public bool Equals(pb_TooltipContent tooltip)
		{
			return tooltip.name.Equals(this.name);
		}

		public override bool Equals(System.Object b)
		{
			return b is pb_TooltipContent && ((pb_TooltipContent)b).name.Equals(name);
		}

		public override int GetHashCode()
		{
			return name.GetHashCode();
		}

		public static implicit operator string(pb_TooltipContent content)
		{
			return content.name;
		}

		public static implicit operator pb_TooltipContent(string name)
		{
			return new pb_TooltipContent(name, "");
		}
	}
}
