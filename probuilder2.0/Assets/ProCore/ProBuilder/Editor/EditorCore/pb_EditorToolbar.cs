using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Interface;

namespace ProBuilder2.EditorCommon
{
	[System.Serializable]
	public class pb_EditorToolbar : ScriptableObject
	{
		[SerializeField] EditorWindow window;
		[SerializeField] bool shiftOnlyTooltips = false;
		pb_Tuple<string, double> tooltipTimer = new pb_Tuple<string, double>("", 0.0);
		const double TOOLTIP_TIMER = 1.0;

		[SerializeField] List<pb_MenuAction> actions;

		public void InitWindowProperties(EditorWindow win)
		{
			win.wantsMouseMove = true;
			win.autoRepaintOnSceneChange = true;
			win.minSize = actions[0].GetSize() + new Vector2(10, 10);
			this.window = win;
		}

		void OnEnable()
		{
			actions = pb_EditorToolbarLoader.GetActions();
			pb_Editor.OnSelectionUpdate -= OnElementSelectionChange;
			pb_Editor.OnSelectionUpdate += OnElementSelectionChange;
			shiftOnlyTooltips = pb_Preferences_Internal.GetBool(pb_Constant.pbShiftOnlyTooltips);
		}

		void OnDisable()
		{
			pb_Editor.OnSelectionUpdate -= OnElementSelectionChange;
		}

		void OnElementSelectionChange(pb_Object[] selection)
		{
			if(!window)
				GameObject.DestroyImmediate(this);
			else
				window.Repaint();
		}

		Vector2 scroll = Vector2.zero;

		private void ShowTooltip(Rect rect, string content, Vector2 scrollOffset)
		{
			pb_TooltipContent c = pb_TooltipContent.TempContent;
			c.summary = content;
			ShowTooltip(rect, c, scrollOffset);
		}

		private void ShowTooltip(Rect rect, pb_TooltipContent content, Vector2 scrollOffset)
		{
			Rect buttonRect = new Rect(
				(window.position.x + rect.x) - scrollOffset.x,
				(window.position.y + rect.y) - scrollOffset.y, 
				rect.width,
				rect.height);

			pb_TooltipWindow.Show(buttonRect, content);
		}

		public void OnGUI()
		{
			Event e = Event.current;

			int max = (int)window.position.width - 24;
			int rows = System.Math.Max(max / (int)actions[0].GetSize().x, 1);

			int i = 1;

			scroll = GUILayout.BeginScrollView(scroll, false, false, GUIStyle.none, GUIStyle.none, GUIStyle.none);

			bool 	tooltipShown = false,
					hovering = false;

			GUILayout.BeginHorizontal();

			Rect optionRect = new Rect(0f, 0f, 0f, 0f);

			foreach(pb_MenuAction action in actions)
			{
				if( action.DoButton(e.alt, ref optionRect) && !e.shift)
				{
					optionRect.x -= scroll.x;
					optionRect.y -= scroll.y;

					if(optionRect.Contains(e.mousePosition) && e.type != EventType.Layout)
					{
						tooltipShown = true;
						ShowTooltip(optionRect, "Alt+Click for Options", scroll);
					}	
				}

				Rect buttonRect = GUILayoutUtility.GetLastRect();

				if( e.type != EventType.Layout )
				{
					if( !tooltipShown && buttonRect.Contains(e.mousePosition) )
					{
						if(!shiftOnlyTooltips)
						{
							if( !tooltipTimer.Item1.Equals(action.tooltip.name) )
							{
								tooltipTimer.Item1 = action.tooltip.name;
								tooltipTimer.Item2 = EditorApplication.timeSinceStartup;
							}
						}

						if( e.shift || ( 	!shiftOnlyTooltips &&
											tooltipTimer.Item1.Equals(action.tooltip.name) &&
											EditorApplication.timeSinceStartup - tooltipTimer.Item2 > TOOLTIP_TIMER ))
						{
							tooltipShown = true;
							ShowTooltip(buttonRect, action.tooltip, scroll);
						}

						hovering = true;
					}
				}

				if(++i >= rows)
				{
					i = 1;
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
				}
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.EndScrollView();

			if((e.type == EventType.Repaint || e.type == EventType.MouseMove) && !tooltipShown)
				pb_TooltipWindow.Hide();

			if(e.type != EventType.Layout && !hovering)
				tooltipTimer.Item1 = "";

			if( (EditorWindow.mouseOverWindow == this && e.delta.sqrMagnitude > .001f) || e.isMouse )
				window.Repaint();
		}
	}
}
