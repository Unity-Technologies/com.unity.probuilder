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

		const float MIN_WIDTH = 128;
		const float MAX_WIDTH = 512;
		const float MIN_HEIGHT = 0;
		const float MAX_HEIGHT = 1024;

		static void InitStyles()
		{
			_titleStyle = new GUIStyle();
			_titleStyle.margin = new RectOffset(4,4,4,4);
			_titleStyle.padding = new RectOffset(4,4,4,4);
			_titleStyle.fontSize = 14;
			_titleStyle.fontStyle = FontStyle.Bold;
			_titleStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
			_titleStyle.richText = true;

			EditorStyles.wordWrappedLabel.richText = true;
		}

		static readonly Color separatorColor = new Color(.65f, .65f, .65f, .5f);

		public string name;
		public string summary;

		public static pb_TooltipContent TempContent = new pb_TooltipContent("", "");

		public pb_TooltipContent(string name, string summary)
		{
			this.name = name;
			this.summary = summary;
		}

		public Vector2 CalcSize()
		{
			const float pad = 8;
			Vector2 total = new Vector2(MIN_WIDTH, MIN_HEIGHT);

			bool hasName = !string.IsNullOrEmpty(name);
			bool hasSummary = !string.IsNullOrEmpty(summary);

			if(hasName)
			{
				Vector2 ns = TitleStyle.CalcSize(pb_GUI_Utility.TempGUIContent(name));
				total.x += Mathf.Max(ns.x, 256);
				total.y += ns.y;
			}

			if(hasSummary)
			{
				if(!hasName)
				{
					Vector2 sumSize = EditorStyles.wordWrappedLabel.CalcSize(pb_GUI_Utility.TempGUIContent(summary));
					total.x = Mathf.Min(sumSize.x, MAX_WIDTH);
				}

				float summaryHeight = EditorStyles.wordWrappedLabel.CalcHeight(pb_GUI_Utility.TempGUIContent(summary), total.x);
				total.y += summaryHeight;
			}

			if(hasName && hasSummary)
				total.y += 16;

			total.x += pad;
			total.y += pad;

			return total;
		}

		public void Draw()
		{
			if(!string.IsNullOrEmpty(name))
			{
				GUILayout.Label(name, TitleStyle);

				pb_GUI_Utility.DrawSeparator(1, separatorColor);
				GUILayout.Space(2);
			}

			if(!string.IsNullOrEmpty(summary))
			{
				GUILayout.Label(summary, EditorStyles.wordWrappedLabel);
			}
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
