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
		public string name;
		public string documentationLink;
		public string summary;

		public pb_TooltipContent(string name, string summary, string documentationLink = "")
		{
			this.name = name;
			this.summary = summary;
			this.documentationLink = documentationLink;
		}

		public Vector2 CalcSize()
		{
			const float pad = 16;
			Vector2 total = new Vector2(256, 256);

			Vector2 ns = EditorStyles.boldLabel.CalcSize(pb_GUI_Utility.TempGUIContent(name));

			float width = Mathf.Max(ns.x + pad, 256);

			float dh = EditorStyles.wordWrappedLabel.CalcHeight(pb_GUI_Utility.TempGUIContent(summary), width);

			total.x = width;
			total.y = ns.y + dh + pad;

			return total;
		}

		public void Draw()
		{
			GUILayout.Label(name, EditorStyles.boldLabel);

			if(!string.IsNullOrEmpty(documentationLink))
			{
				if(GUILayout.Button(documentationLink))
					Application.OpenURL(documentationLink);
			}

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
