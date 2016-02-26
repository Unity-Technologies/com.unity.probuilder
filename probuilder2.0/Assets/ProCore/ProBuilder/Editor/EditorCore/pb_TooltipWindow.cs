using UnityEngine;
using UnityEditor;

namespace ProBuilder2.EditorCommon
{
	public class pb_TooltipWindow : EditorWindow
	{
		public GUIContent content = new GUIContent("");

		public void SetTooltip(string text)
		{
			this.content.text = text;
		}

		void OnGUI()
		{
			GUI.Label(new Rect(0,0,position.width, position.height), content.text, EditorStyles.boldLabel);
		}
	}
}
