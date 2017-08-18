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
		public EditorWindow window;

		bool isFloating { get { return pb_Editor.instance != null && pb_Editor.instance.isFloatingWindow; } }
		bool isIconMode = true;
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

		[SerializeField] internal List<pb_MenuAction> actions;

		public void InitWindowProperties(EditorWindow win)
		{
			win.wantsMouseMove = true;
			win.autoRepaintOnSceneChange = true;
			this.window = win;
		}

		void OnEnable()
		{
			actions = pb_EditorToolbarLoader.GetActions(true);

			pb_Editor.OnSelectionUpdate -= OnElementSelectionChange;
			pb_Editor.OnSelectionUpdate += OnElementSelectionChange;

			EditorApplication.update -= Update;
			EditorApplication.update += Update;

			shiftOnlyTooltips = pb_PreferencesInternal.GetBool(pb_Constant.pbShiftOnlyTooltips);

			tooltipTimer.Item1 = "";
			tooltipTimer.Item2 = 0.0;
			showTooltipTimer = false;
			scrollIconUp 	= pb_IconUtility.GetIcon("Toolbar/ShowNextPage_Up");
			scrollIconDown 	= pb_IconUtility.GetIcon("Toolbar/ShowNextPage_Down");
			scrollIconRight = pb_IconUtility.GetIcon("Toolbar/ShowNextPage_Right");
			scrollIconLeft 	= pb_IconUtility.GetIcon("Toolbar/ShowNextPage_Left");

			isIconMode = pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI);
			this.window = pb_Editor.instance;
			CalculateMaxIconSize();

			scroll.x = pb_PreferencesInternal.GetFloat("pbEditorScroll.x", 0f);
			scroll.y = pb_PreferencesInternal.GetFloat("pbEditorScroll.y", 0f);
		}

		void OnDisable()
		{
			// don't unsubscribe here because on exiting playmode OnEnable/OnDisable
			// is called.  no clue why.
			// EditorApplication.update -= Update;
			pb_Editor.OnSelectionUpdate -= OnElementSelectionChange;
			pb_PreferencesInternal.SetFloat("pbEditorScroll.x", scroll.x);
			pb_PreferencesInternal.SetFloat("pbEditorScroll.y", scroll.y);
		}

		void OnDestroy()
		{
			// store the scroll in both disable & destroy because there are
			// situations where one gets updated over the other and it's all
			// screwy.  script reloads in particular?
			pb_PreferencesInternal.SetFloat("pbEditorScroll.x", scroll.x);
			pb_PreferencesInternal.SetFloat("pbEditorScroll.y", scroll.y);
			pb_MenuActionStyles.ResetStyles();
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

		private void ShowTooltip(Rect rect, pb_TooltipContent content, Vector2 scrollOffset, bool isProOnly = false)
		{
			Rect buttonRect = new Rect(
				(window.position.x + rect.x) - scrollOffset.x,
				(window.position.y + rect.y) - scrollOffset.y,
				rect.width,
				rect.height);

			pb_TooltipWindow.Show(buttonRect, content, isProOnly);
		}

		void Update()
		{
			if(!window)
				return;

			if(!shiftOnlyTooltips)
			{
				if( !tooltipTimer.Item1.Equals(hoveringTooltipName) )
				{
					tooltipTimer.Item1 = hoveringTooltipName;
					tooltipTimer.Item2 = EditorApplication.timeSinceStartup;
				}

				if(!string.IsNullOrEmpty(tooltipTimer.Item1))
				{
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

		void CalculateMaxIconSize()
		{
			if(!window) return;

			isHorizontalMenu = window.position.width > window.position.height;

			Vector2 iconSize = actions[0].GetSize(isHorizontalMenu);

			iconWidth = (int)iconSize.x + 4;
			iconHeight = (int)iconSize.y + 4;

			// if not in icon mode, we have to iterate all buttons to figure out what the maximum size is
			if(!isIconMode)
			{
				for(int i = 1; i < actions.Count; i++)
				{
					iconSize = actions[i].GetSize(isHorizontalMenu);
					iconWidth = System.Math.Max(iconWidth, (int)iconSize.x);
					iconHeight = System.Math.Max(iconHeight, (int)iconSize.y);
				}

				iconWidth += 4;
				iconHeight += 4;
			}

			window.minSize = new Vector2(iconWidth + 6, iconHeight + 12);
			window.Repaint();
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
		bool isHorizontalMenu = false;
		int iconWidth = 1, iconHeight = 1;

		public void OnGUI()
		{
			Event e = Event.current;
			Vector2 mpos = e.mousePosition;
			bool forceRepaint = false;

			IEnumerable<pb_MenuAction> available = actions.Where(x => !x.IsHidden() && (!isIconMode || x.icon != null) );

			// if icon mode and no actions are found, that probably means icons failed to load.  revert to text mode.
			if(isIconMode && available.Count() < 1)
			{
				isIconMode = false;
				pb_PreferencesInternal.SetBool(pb_Constant.pbIconGUI, isIconMode);
				CalculateMaxIconSize();
				Debug.LogWarning("ProBuilder: Toolbar icons failed to load, reverting to text mode.  Please ensure that the ProBuilder folder contents are unmodified.  If the menu is still not visible, try closing and re-opening the Editor Window.");
				return;
			}

			int availableWidth = windowWidth;
			int availableHeight = windowHeight;
			int iconCount = available.Count();
			bool isHorizontal = windowWidth > windowHeight * 2;

			if(isHorizontalMenu != isHorizontal)
				CalculateMaxIconSize();

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
			}

			if(isHorizontal && e.type == EventType.ScrollWheel && e.delta.sqrMagnitude > .001f)
			{
				scroll.x += e.delta.y * 10f;
				forceRepaint = true;
			}

			// the math for matching layout group width for icons is easy enough, but text
			// is a lot more complex.  so for horizontal text toolbars always show the horizontal
			// scroll buttons.
			int maxHorizontalScroll = !isIconMode ? 10000 : contentWidth - availableWidth;
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

			// e.mousePosition != mpos at this point - @todo figure out why
			bool windowContainsMouse = 	mpos.x > 0 && mpos.x < window.position.width &&
										mpos.y > 0 && mpos.y < window.position.height;

			int columnCount = 0;
			foreach(pb_MenuAction action in available)
			{
				if(isIconMode)
				{
					if( action.DoButton(isHorizontal, e.alt, ref optionRect, GUILayout.MaxHeight(iconHeight + 12)) && !e.shift )
					{
						// test for alt click / hover
						optionRect.x -= scroll.x;
						optionRect.y -= scroll.y;

						if(	windowContainsMouse &&
							e.type != EventType.Layout &&
							optionRect.Contains(e.mousePosition) )
						{
							hoveringTooltipName = action.tooltip.title + "_alt";
							tooltipTimerRefresh = .5f;
							hovering = true;

							if( showTooltipTimer )
							{
								tooltipShown = true;
								ShowTooltip(optionRect, "Alt + Click for Options", scroll);
							}
						}
					}
				}
				else
				{
					if(columns < 2)
						action.DoButton(isHorizontal, e.alt, ref optionRect);
					else
						action.DoButton(isHorizontal, e.alt, ref optionRect, GUILayout.MinWidth(iconWidth));
				}

				Rect buttonRect = GUILayoutUtility.GetLastRect();

				if( windowContainsMouse &&
					e.type != EventType.Layout &&
					!hovering &&
					buttonRect.Contains(e.mousePosition) )
				{
					hoveringTooltipName = action.tooltip.title;
					tooltipTimerRefresh = 1f;

					if( e.shift || showTooltipTimer )
					{
						tooltipShown = true;
						ShowTooltip(buttonRect, action.tooltip, scroll, action.isProOnly);
					}

					hovering = true;
					forceRepaint = true;
				}

				if(++columnCount >= columns)
				{
					columnCount = 0;

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

			if( forceRepaint || (EditorWindow.mouseOverWindow == this && e.delta.sqrMagnitude > .001f) || e.isMouse )
				window.Repaint();
		}
	}
}
