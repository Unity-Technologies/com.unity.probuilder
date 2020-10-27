using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
    sealed class EditorToolbar : IDisposable
    {
        Pref<Vector2> m_Scroll = new Pref<Vector2>("editor.scrollPosition", Vector2.zero, SettingsScope.User);
        public EditorWindow window;

        bool isFloating { get { return ProBuilderEditor.instance != null && ProBuilderEditor.instance.isFloatingWindow; } }
        internal bool isIconMode = true;

        [UserSetting("Toolbar", "Shift Key Tooltips", "Tooltips will only show when the Shift key is held")]
        internal static Pref<bool> s_ShiftOnlyTooltips = new Pref<bool>("editor.shiftOnlyTooltips", false, SettingsScope.User);

        SimpleTuple<string, double> tooltipTimer = new SimpleTuple<string, double>("", 0.0);
        // the element currently being hovered
        string hoveringTooltipName = "";
        // the mouse has hovered > tooltipTimerRefresh
        bool showTooltipTimer = false;
        // how long a tooltip will wait before showing
        float tooltipTimerRefresh = 1f;

        Texture2D   scrollIconUp = null,
                    scrollIconDown = null,
                    scrollIconRight = null,
                    scrollIconLeft = null;

        List<MenuAction> m_Actions;
        int m_ActionsLength = 0;

        public EditorToolbar(EditorWindow parent)
        {
            m_Actions = EditorToolbarLoader.GetActions(true);
            m_ActionsLength = m_Actions.Count();

            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            tooltipTimer.item1 = "";
            tooltipTimer.item2 = 0.0;
            showTooltipTimer = false;
            scrollIconUp    = IconUtility.GetIcon("Toolbar/ShowNextPage_Up");
            scrollIconDown  = IconUtility.GetIcon("Toolbar/ShowNextPage_Down");
            scrollIconRight = IconUtility.GetIcon("Toolbar/ShowNextPage_Right");
            scrollIconLeft  = IconUtility.GetIcon("Toolbar/ShowNextPage_Left");

            isIconMode = ProBuilderEditor.s_IsIconGui;

            window = parent;
            window.wantsMouseMove = true;
            window.autoRepaintOnSceneChange = true;

            CalculateMaxIconSize();
        }

        public void Dispose()
        {
            // don't unsubscribe here because on exiting playmode OnEnable/OnDisable
            // is called. no clue why.
            // EditorApplication.update -= Update;
            MenuActionStyles.ResetStyles();
        }

        void ShowTooltip(Rect rect, string content, Vector2 scrollOffset)
        {
            TooltipContent c = TooltipContent.TempContent;
            c.summary = content;
            ShowTooltip(rect, c, scrollOffset);
        }

        void ShowTooltip(Rect rect, TooltipContent content, Vector2 scrollOffset)
        {
            Rect buttonRect = new Rect(
                    (window.position.x + rect.x) - scrollOffset.x,
                    (window.position.y + rect.y) - scrollOffset.y,
                    rect.width,
                    rect.height);

            TooltipEditor.Show(buttonRect, content);
        }

        void Update()
        {
            if (!window)
                return;

            if (!s_ShiftOnlyTooltips)
            {
                if (!tooltipTimer.item1.Equals(hoveringTooltipName))
                {
                    tooltipTimer.item1 = hoveringTooltipName;
                    tooltipTimer.item2 = EditorApplication.timeSinceStartup;
                }

                if (!string.IsNullOrEmpty(tooltipTimer.item1))
                {
                    if (EditorApplication.timeSinceStartup - tooltipTimer.item2 > tooltipTimerRefresh)
                    {
                        if (!showTooltipTimer)
                        {
                            showTooltipTimer = true;
                            RepaintIfFocused();
                        }
                    }
                    else
                    {
                        showTooltipTimer = false;
                    }
                }
            }

            // do scroll animations
            if (doAnimateScroll)
            {
                double scrollTimer = EditorApplication.timeSinceStartup - scrollStartTime;
                m_Scroll.value = Vector2.Lerp(scrollOrigin, scrollTarget, (float)scrollTimer / scrollTotalTime);

                if (scrollTimer >= scrollTotalTime)
                    doAnimateScroll = false;

                RepaintIfFocused();
            }
        }

        void CalculateMaxIconSize()
        {
            if (!window) return;

            m_IsHorizontalMenu = window.position.width > window.position.height;

            Vector2 iconSize = m_Actions[0].GetSize(m_IsHorizontalMenu);

            m_ContentWidth = (int)iconSize.x + 4;
            m_ContentHeight = (int)iconSize.y + 4;

            // if not in icon mode, we have to iterate all buttons to figure out what the maximum size is
            if (!isIconMode)
            {
                for (int i = 1; i < m_Actions.Count; i++)
                {
                    iconSize = m_Actions[i].GetSize(m_IsHorizontalMenu);
                    m_ContentWidth = System.Math.Max(m_ContentWidth, (int)iconSize.x);
                    m_ContentHeight = System.Math.Max(m_ContentHeight, (int)iconSize.y);
                }

                m_ContentWidth += 4;
                m_ContentHeight += 4;
            }

            window.minSize = new Vector2(m_ContentWidth + 6, m_ContentHeight + 4);
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
            scrollOrigin = m_Scroll;
            scrollTarget.x = x;
            scrollTarget.y = y;
            scrollStartTime = EditorApplication.timeSinceStartup;
            scrollTotalTime = Vector2.Distance(scrollOrigin, scrollTarget) / SCROLL_PIXELS_PER_SECOND;
            doAnimateScroll = true;
        }

        int SCROLL_BTN_SIZE { get { return isFloating ? 12 : 11; } }
        int windowWidth { get { return (int)Mathf.Ceil(window.position.width); } }
        int windowHeight { get { return (int)Mathf.Ceil(window.position.height); } }

        bool m_ShowScrollButtons = false;
        bool m_IsHorizontalMenu = false;
        int m_ContentWidth = 1, m_ContentHeight = 1;

        int m_Columns;
        int m_Rows;

        bool m_WantsRepaint;

        bool IsActionValid(MenuAction action)
        {
            return !action.hidden && (!isIconMode || action.icon != null);
        }

        public void OnGUI()
        {
            Event evt = Event.current;
            Vector2 mpos = evt.mousePosition;

            // if icon mode and no actions are found, that probably means icons failed to load. revert to text mode.
            int menuActionsCount = 0;

            for (int i = 0; i < m_Actions.Count; i++)
                if (IsActionValid(m_Actions[i]))
                    menuActionsCount++;

            if (isIconMode && menuActionsCount < 1)
            {
                isIconMode = false;
                ProBuilderEditor.s_IsIconGui.value = isIconMode;
                CalculateMaxIconSize();
                Debug.LogWarning("ProBuilder: Toolbar icons failed to load, reverting to text mode.  Please ensure that the ProBuilder folder contents are unmodified.  If the menu is still not visible, try closing and re-opening the Editor Window.");
                return;
            }

            int availableWidth = windowWidth;
            int availableHeight = windowHeight;
            bool isHorizontal = windowWidth > windowHeight * 2;

            if (m_IsHorizontalMenu != isHorizontal || m_Rows < 1 || m_Columns < 1)
                CalculateMaxIconSize();

            if (evt.type == EventType.Layout)
            {
                if (isHorizontal)
                {
                    m_Rows = ((windowHeight - 4) / m_ContentHeight);
                    m_Columns = System.Math.Max(windowWidth / m_ContentWidth, (menuActionsCount / m_Rows) + (menuActionsCount % m_Rows != 0 ? 1 : 0));
                }
                else
                {
                    m_Columns = System.Math.Max((windowWidth - 4) / m_ContentWidth, 1);
                    m_Rows = (menuActionsCount / m_Columns) + (menuActionsCount % m_Columns != 0 ? 1 : 0);
                }
            }

            // happens when maximizing/unmaximizing the window
            if (m_Rows < 1 || m_Columns < 1)
                return;

            int contentWidth = (menuActionsCount / m_Rows) * m_ContentWidth + 4;
            int contentHeight = m_Rows * m_ContentHeight + 4;

            bool showScrollButtons = isHorizontal ? contentWidth > availableWidth : contentHeight > availableHeight;

            if (showScrollButtons)
            {
                availableHeight -= SCROLL_BTN_SIZE * 2;
                availableWidth -= SCROLL_BTN_SIZE * 2;
            }

            if (isHorizontal && evt.type == EventType.ScrollWheel && evt.delta.sqrMagnitude > .001f)
            {
                m_Scroll.value = new Vector2(m_Scroll.value.x + evt.delta.y * 10f, m_Scroll.value.y);
               ScheduleRepaint();
            }

            // the math for matching layout group width for icons is easy enough, but text
            // is a lot more complex.  so for horizontal text toolbars always show the horizontal
            // scroll buttons.
            int maxHorizontalScroll = !isIconMode ? 10000 : contentWidth - availableWidth;
            int maxVerticalScroll = contentHeight - availableHeight;

            // only change before a layout event
            if (m_ShowScrollButtons != showScrollButtons && evt.type == EventType.Layout)
                m_ShowScrollButtons = showScrollButtons;

            if (m_ShowScrollButtons)
            {
                if (isHorizontal)
                {
                    GUILayout.BeginHorizontal();

                    GUI.enabled = ((Vector2)m_Scroll).x > 0;

                    if (GUILayout.Button(scrollIconLeft, UI.EditorGUIUtility.ButtonNoBackgroundSmallMarginStyle, GUILayout.ExpandHeight(true)))
                        StartScrollAnimation(Mathf.Max(((Vector2)m_Scroll).x - availableWidth, 0f), 0f);

                    GUI.enabled = true;
                }
                else
                {
                    GUI.enabled = ((Vector2)m_Scroll).y > 0;

                    if (GUILayout.Button(scrollIconUp, UI.EditorGUIUtility.ButtonNoBackgroundSmallMarginStyle))
                        StartScrollAnimation(0f, Mathf.Max(((Vector2)m_Scroll).y - availableHeight, 0f));

                    GUI.enabled = true;
                }
            }

            m_Scroll.value = GUILayout.BeginScrollView(m_Scroll.value, false, false, GUIStyle.none, GUIStyle.none, GUIStyle.none);

            bool tooltipShown = false,
                hovering = false;

                Rect optionRect = new Rect(0f, 0f, 0f, 0f);

            GUILayout.BeginHorizontal();

            // e.mousePosition != mpos at this point - @todo figure out why
            bool windowContainsMouse =  window == EditorWindow.mouseOverWindow
                && mpos.x > 0 && mpos.x < window.position.width &&
                mpos.y > 0 && mpos.y < window.position.height;

            int columnCount = 0;

            for (int actionIndex = 0; actionIndex < m_ActionsLength; actionIndex++)
            {
                var action = m_Actions[actionIndex];

                if (!IsActionValid(action))
                    continue;

                if (isIconMode)
                {
                    if (action.DoButton(isHorizontal, evt.alt, ref optionRect, GUILayout.MaxHeight(m_ContentHeight + 12)) && !evt.shift)
                    {
                        // test for alt click / hover
                        optionRect.x -= m_Scroll.value.x;
                        optionRect.y -= m_Scroll.value.y;

                        if (windowContainsMouse &&
                            evt.type != EventType.Layout &&
                            optionRect.Contains(evt.mousePosition))
                        {
                            hoveringTooltipName = action.tooltip.title + "_alt";
                            tooltipTimerRefresh = .5f;
                            hovering = true;

                            if (showTooltipTimer)
                            {
                                tooltipShown = true;
                                ShowTooltip(optionRect, "Alt + Click for Options ", m_Scroll);
                            }
                        }
                    }
                }
                else
                {
                    if (m_Columns < 2)
                        action.DoButton(isHorizontal, evt.alt, ref optionRect);
                    else
                        action.DoButton(isHorizontal, evt.alt, ref optionRect, GUILayout.MinWidth(m_ContentWidth));
                }

                Rect buttonRect = GUILayoutUtility.GetLastRect();

                if (windowContainsMouse &&
                    evt.type != EventType.Layout &&
                    !hovering &&
                    buttonRect.Contains(evt.mousePosition))
                {
                    hoveringTooltipName = action.tooltip.title;
                    tooltipTimerRefresh = 1f;

                    if (evt.shift || showTooltipTimer)
                    {
                        tooltipShown = true;
                        ShowTooltip(buttonRect, action.tooltip, m_Scroll);
                    }

                    hovering = true;
                }

                if (++columnCount >= m_Columns)
                {
                    columnCount = 0;

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            if (m_ShowScrollButtons)
            {
                if (isHorizontal)
                {
                    GUI.enabled = m_Scroll.value.x < maxHorizontalScroll - 2;
                    if (GUILayout.Button(scrollIconRight, UI.EditorGUIUtility.ButtonNoBackgroundSmallMarginStyle, GUILayout.ExpandHeight(true)))
                        StartScrollAnimation(Mathf.Min(m_Scroll.value.x + availableWidth + 2, maxHorizontalScroll), 0f);
                    GUI.enabled = true;

                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUI.enabled = m_Scroll.value.y < maxVerticalScroll - 2;
                    if (GUILayout.Button(scrollIconDown, UI.EditorGUIUtility.ButtonNoBackgroundSmallMarginStyle))
                        StartScrollAnimation(0f, Mathf.Min(m_Scroll.value.y + availableHeight + 2, maxVerticalScroll));
                    GUI.enabled = true;
                }
            }

            if ((evt.type == EventType.Repaint || evt.type == EventType.MouseMove) && !tooltipShown)
                TooltipEditor.Hide();

            if (evt.type != EventType.Layout && !hovering)
                tooltipTimer.item1 = "";

            m_WantsRepaint |= EditorWindow.mouseOverWindow == window && evt.type == EventType.MouseMove;

            if (Application.isFocused && m_WantsRepaint)
                window.Repaint();

            m_WantsRepaint = false;
        }

        void ScheduleRepaint()
        {
            m_WantsRepaint = true;
        }

        void RepaintIfFocused()
        {
            if(Application.isFocused)
                window.Repaint();
        }
    }
}
