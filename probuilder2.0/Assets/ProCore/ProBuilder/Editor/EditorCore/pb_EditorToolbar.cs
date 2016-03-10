using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Interface;
using System.Linq;

namespace ProBuilder2.EditorCommon
{
	[System.Serializable]
	public class pb_EditorToolbar : ScriptableObject
	{
		[SerializeField] EditorWindow window;

		bool shiftOnlyTooltips = false;
		pb_Tuple<string, double> tooltipTimer = new pb_Tuple<string, double>("", 0.0);
		// the element currently being hovered
		string hoveringTooltipName = "";
		// the mouse has hovered > tooltipTimerRefresh
		bool showTooltipTimer = false;
		// how long a tooltip will wait before showing
		float tooltipTimerRefresh = 1f;
		
		[SerializeField] List<pb_MenuAction> actions;

		public void InitWindowProperties(EditorWindow win)
		{
			win.wantsMouseMove = true;
			win.autoRepaintOnSceneChange = true;
			win.minSize = actions[0].GetSize() + new Vector2(6, 6);
			this.window = win;
		}

		void OnEnable()
		{
			actions = pb_EditorToolbarLoader.GetActions();
			pb_Editor.OnSelectionUpdate -= OnElementSelectionChange;
			pb_Editor.OnSelectionUpdate += OnElementSelectionChange;
			EditorApplication.update -= Update;
			EditorApplication.update += Update;
			shiftOnlyTooltips = pb_Preferences_Internal.GetBool(pb_Constant.pbShiftOnlyTooltips);

			tooltipTimer.Item1 = "";
			tooltipTimer.Item2 = 0.0;
			showTooltipTimer = false;
		}

		void OnDisable()
		{
			pb_Editor.OnSelectionUpdate -= OnElementSelectionChange;
			EditorApplication.update -= Update;
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

		void Update()
		{
			if(!shiftOnlyTooltips)
			{
				if( !tooltipTimer.Item1.Equals(hoveringTooltipName) )
				{
					tooltipTimer.Item1 = hoveringTooltipName;
					tooltipTimer.Item2 = EditorApplication.timeSinceStartup;
				}

				if(string.IsNullOrEmpty(tooltipTimer.Item1))
					return;

				if( EditorApplication.timeSinceStartup - tooltipTimer.Item2 > tooltipTimerRefresh )
				{
					if( !showTooltipTimer )
					{
						showTooltipTimer = true;
						window.Repaint();
					}
				}
				else
				{
					showTooltipTimer = false;
				}
			}
		}

		public void OnGUI()
		{
			Event e = Event.current;

			int max = (int)window.position.width - 4;
			int rows = System.Math.Max(max / (int)(actions[0].GetSize().x + 4), 1);
			IEnumerable<pb_MenuAction> available = actions.Where(x => !x.IsHidden());

			int i = 0;

			scroll = GUILayout.BeginScrollView(scroll, false, false, GUIStyle.none, GUIStyle.none, GUIStyle.none);

			bool 	tooltipShown = false,
					hovering = false;

			Rect optionRect = new Rect(0f, 0f, 0f, 0f);

			GUILayout.BeginHorizontal();


			foreach(pb_MenuAction action in available)
			{
				if( action.DoButton(e.alt, ref optionRect) && !e.shift)
				{
					optionRect.x -= scroll.x;
					optionRect.y -= scroll.y;

					if(optionRect.Contains(e.mousePosition) && e.type != EventType.Layout)
					{
						hoveringTooltipName = action.tooltip.name + "_alt";
						tooltipTimerRefresh = .5f;
						hovering = true;
						
						if( showTooltipTimer )
						{
							tooltipShown = true;
							ShowTooltip(optionRect, "Alt + Click for Options", scroll);
						}
					}	
				}

				Rect buttonRect = GUILayoutUtility.GetLastRect();

				if( e.type != EventType.Layout &&
					!hovering &&
					buttonRect.Contains(e.mousePosition) )
				{
					hoveringTooltipName = action.tooltip.name;
					tooltipTimerRefresh = 1f;

					if( e.shift || showTooltipTimer )
					{
						tooltipShown = true;
						ShowTooltip(buttonRect, action.tooltip, scroll);
					}

					hovering = true;
				}

				if(++i >= rows)
				{
					i = 0;

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
