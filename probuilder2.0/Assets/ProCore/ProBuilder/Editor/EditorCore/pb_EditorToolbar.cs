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

		bool isFloating { get { return pb_Editor.instance.isFloatingWindow; } }

		bool shiftOnlyTooltips = false;
		pb_Tuple<string, double> tooltipTimer = new pb_Tuple<string, double>("", 0.0);
		// the element currently being hovered
		string hoveringTooltipName = "";
		// the mouse has hovered > tooltipTimerRefresh
		bool showTooltipTimer = false;
		// how long a tooltip will wait before showing
		float tooltipTimerRefresh = 1f;
		
		Texture2D 	scrollIconUp = null,
					scrollIconDown = null,
					scrollIconRight = null,
					scrollIconLeft = null;

		[SerializeField] List<pb_MenuAction> actions;

		public void InitWindowProperties(EditorWindow win)
		{
			win.wantsMouseMove = true;
			win.autoRepaintOnSceneChange = true;
			win.minSize = actions[0].GetSize(win.position.width > win.position.height) + new Vector2(8, 12);
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
			scrollIconUp = pb_IconUtility.GetIcon("ShowNextPage_Up");
			scrollIconDown = pb_IconUtility.GetIcon("ShowNextPage_Down");
			scrollIconRight = pb_IconUtility.GetIcon("ShowNextPage_Right");
			scrollIconLeft = pb_IconUtility.GetIcon("ShowNextPage_Left");
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

			// do scroll animations
			if(doAnimateScroll)
			{
				double scrollTimer = EditorApplication.timeSinceStartup - scrollStartTime;
				scroll = Vector2.Lerp(scrollOrigin, scrollTarget, (float)scrollTimer / scrollTotalTime);

				if(scrollTimer >= scrollTotalTime)
					doAnimateScroll = false;

				window.Repaint();
			}
		}

		// animated scrolling vars
		bool doAnimateScroll = false;
		Vector2 scrollOrigin = Vector2.zero;
		Vector2 scrollTarget = Vector2.zero;
		double scrollStartTime = 0;
		float scrollTotalTime = 0f;
		const float SCROLL_PIXELS_PER_SECOND = 1250f;

		void StartScrollAnimation(float x, float y)
		{
			scrollOrigin = scroll;
			scrollTarget.x = x;
			scrollTarget.y = y;
			scrollStartTime = EditorApplication.timeSinceStartup;
			scrollTotalTime = Vector2.Distance(scrollOrigin, scrollTarget) / SCROLL_PIXELS_PER_SECOND;
			doAnimateScroll = true;
		}

		int SCROLL_BTN_SIZE { get { return isFloating ? 12 : 11; } }
		int windowWidth { get { return (int) Mathf.Ceil(window.position.width); } }
		int windowHeight { get { return (int) Mathf.Ceil(window.position.height); } }

		bool m_showScrollButtons = false;

		public void OnGUI()
		{
			Event e = Event.current;

			IEnumerable<pb_MenuAction> available = actions.Where(x => !x.IsHidden());

			int availableWidth = windowWidth;
			int availableHeight = windowHeight;
			int iconCount = available.Count();
			bool isHorizontal = windowWidth > windowHeight * 2;

			int iconWidth = (int)(actions[0].GetSize(isHorizontal).x + 4);
			int iconHeight = (int)(actions[0].GetSize(isHorizontal).y + 4);

			int columns;
			int rows;

			if(isHorizontal)
			{
				rows = ((windowHeight-4) / iconHeight);
				columns = System.Math.Max(windowWidth / iconWidth, (iconCount / rows) + (iconCount % rows != 0 ? 1 : 0));
			}
			else
			{
				columns = System.Math.Max((windowWidth - 4) / iconWidth, 1);
				rows = (iconCount / columns) + (iconCount % columns != 0 ? 1 : 0);
			}
			
			int contentWidth = (iconCount / rows) * iconWidth + 4;
			int contentHeight = rows * iconHeight + 4;

			bool showScrollButtons = isHorizontal ? contentWidth > availableWidth : contentHeight > availableHeight;

			if(showScrollButtons)
			{
				availableHeight -= SCROLL_BTN_SIZE * 2;
				availableWidth -= SCROLL_BTN_SIZE * 2;

				if(isHorizontal && e.type == EventType.ScrollWheel && e.delta.sqrMagnitude > .001f)
					scroll.x += e.delta.y * 10f;
			}
	
			int maxHorizontalScroll = contentWidth - availableWidth;
			int maxVerticalScroll = contentHeight - availableHeight;

			// only change before a layout event
			if(m_showScrollButtons != showScrollButtons && e.type == EventType.Layout)
				m_showScrollButtons = showScrollButtons;

			if(m_showScrollButtons)
			{
				if(isHorizontal)
				{
					GUILayout.BeginHorizontal();

					GUI.enabled = scroll.x > 0;
					if(GUILayout.Button(scrollIconLeft, pb_GUI_Utility.ButtonNoBackgroundSmallMarginStyle, GUILayout.ExpandHeight(true)))
						StartScrollAnimation(Mathf.Max(scroll.x - availableWidth, 0f), 0f);
					GUI.enabled = true;
				}
				else
				{
					GUI.enabled = scroll.y > 0;
					if(GUILayout.Button(scrollIconUp, pb_GUI_Utility.ButtonNoBackgroundSmallMarginStyle))
						StartScrollAnimation( 0f, Mathf.Max(scroll.y - availableHeight, 0f) );
					GUI.enabled = true;
				}
			}

			scroll = GUILayout.BeginScrollView(scroll, false, false, GUIStyle.none, GUIStyle.none, GUIStyle.none);

			bool 	tooltipShown = false,
					hovering = false;

			Rect optionRect = new Rect(0f, 0f, 0f, 0f);
			
			GUILayout.BeginHorizontal();

			int i = 0;
			foreach(pb_MenuAction action in available)
			{
				if( action.DoButton(isHorizontal, e.alt, ref optionRect) && !e.shift)
				{
					optionRect.x -= scroll.x;
					optionRect.y -= scroll.y;

					if(	e.type != EventType.Layout &&
						optionRect.Contains(e.mousePosition) )
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

				if(++i >= columns)
				{
					i = 0;

					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
				}
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.EndScrollView();

			if( m_showScrollButtons )
			{
				if(isHorizontal)
				{
					GUI.enabled = scroll.x < maxHorizontalScroll - 2;
					if(GUILayout.Button(scrollIconRight, pb_GUI_Utility.ButtonNoBackgroundSmallMarginStyle, GUILayout.ExpandHeight(true)))
						StartScrollAnimation( Mathf.Min(scroll.x + availableWidth + 2, maxHorizontalScroll), 0f );
					GUI.enabled = true;

					GUILayout.EndHorizontal();
				}
				else
				{
					GUI.enabled = scroll.y < maxVerticalScroll - 2;
					if(GUILayout.Button(scrollIconDown, pb_GUI_Utility.ButtonNoBackgroundSmallMarginStyle))
						StartScrollAnimation( 0f, Mathf.Min(scroll.y + availableHeight + 2, maxVerticalScroll) );
					GUI.enabled = true;
				}				
			}

			if((e.type == EventType.Repaint || e.type == EventType.MouseMove) && !tooltipShown)
				pb_TooltipWindow.Hide();

			if(e.type != EventType.Layout && !hovering)
				tooltipTimer.Item1 = "";

			if( (EditorWindow.mouseOverWindow == this && e.delta.sqrMagnitude > .001f) || e.isMouse )
				window.Repaint();
		}
	}
}
