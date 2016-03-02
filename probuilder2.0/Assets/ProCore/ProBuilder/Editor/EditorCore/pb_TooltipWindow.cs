using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace ProBuilder2.EditorCommon
{
	public class pb_TooltipWindow : EditorWindow
	{
		// yoinked from EditorWindow class, sans automatic focusing
		internal void Show(Rect buttonRect)
		{
			this.position = buttonRect;
			this.ShowPopup();
		}

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
