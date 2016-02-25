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
			int actionWidth = (int)actions[0].GetSize().x;
			Vector2 iconSize = new Vector2(actions[0].icon.width, actions[0].icon.height);
			int columns = System.Math.Max(max / actionWidth - 1, 1);
			int rows = (actions.Count / columns) + (actions.Count % columns != 0 ? 1 : 0);

			GUILayout.BeginHorizontal();

			for(int i = 0; i < rows; i++)
			{
				for(int n = 0; n < columns; n++)
				{
					int index = i * columns + n;

					if(index < actions.Count)
					{
						pb_MenuAction action = actions[index];
						action.DoButton();
					}
					else
					{
						pb_MenuAction.DoSpace(iconSize);
					}
				}

				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
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
