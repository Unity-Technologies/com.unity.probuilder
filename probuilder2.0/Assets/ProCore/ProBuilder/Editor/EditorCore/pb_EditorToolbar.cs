using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	public class pb_EditorToolbar_Mockup : EditorWindow
	{
		[MenuItem("Tools/ProBuilder Window")]
		static void Init()
		{
			EditorWindow.GetWindow<pb_EditorToolbar_Mockup>(false, "ProBuilder", true);
		}

		List<pb_MenuAction> actions;

		void OnEnable()
		{
			actions = pb_EditorToolbarLoader.GetActions();
			this.wantsMouseMove = true;
			this.minSize = actions[0].GetSize() + new Vector2(6, 6);
		}

		void OnGUI()
		{
			int max = ((int)this.position.width);
			int rows = max / (int)actions[0].GetSize().x;

			int i = 1;

			GUILayout.BeginHorizontal();

			foreach(pb_MenuAction action in actions)
			{
				action.DoButton();

				if(++i >= rows)
				{
					i = 1;
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
				}
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
	
			Event e = Event.current;
			if((e.mousePosition.x > 0f &&
				e.mousePosition.x < this.position.width &&
				e.mousePosition.y > 0f &&
				e.mousePosition.y < this.position.height &&
			 	e.delta.sqrMagnitude > .001f) ||
				e.isMouse )
				Repaint();
		}
	}
}
