#if UNITY_2019_1_OR_NEWER
#define UNITY_INTERNALS_VISIBLE
#endif

using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder;
using System.Collections.Generic;
using UnityEngine.Assertions;
#if !UNITY_INTERNALS_VISIBLE
using System.Reflection;
#endif

namespace UnityEditor.ProBuilder.UI
{
    /// <summary>
    /// Additional GUI functions for Editor use.
    /// </summary>
    static class EditorGUILayout
    {
#if !UNITY_INTERNALS_VISIBLE
        static readonly object[] s_GetSliderRectParams = new object[2];
        static readonly MethodInfo s_GetSliderRectMethod;

        static EditorGUILayout()
        {
            s_GetSliderRectMethod = typeof(UnityEditor.EditorGUILayout).GetMethod(
                "GetSliderRect",
                BindingFlags.Static | BindingFlags.NonPublic,
                null, CallingConventions.Any, new [] { typeof(bool), typeof(GUILayoutOption[])}, null);

            Assert.IsNotNull(s_GetSliderRectMethod, "Couldn't find internal method EditorGUILayout.GetSliderRect(bool, GUILayoutOption) in UnityEditor namespace");
        }
#endif

        static bool s_RowToggle = true;
        static readonly Color s_RowOddColor = new Color(.45f, .45f, .45f, .2f);
        static readonly Color s_RowEvenColor = new Color(.30f, .30f, .30f, .2f);

        public static void BeginRow(int index = -1)
        {
            if (index > -1)
                s_RowToggle = index % 2 == 0;

            EditorGUIUtility.PushBackgroundColor(s_RowToggle ? s_RowEvenColor : s_RowOddColor);
            GUILayout.BeginHorizontal(EditorStyles.rowStyle);
            s_RowToggle = !s_RowToggle;
            EditorGUIUtility.PopBackgroundColor();
        }

        public static void EndRow()
        {
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// An automatically laid out toolbar that returns the index of the selected button. Optionally allows no selection.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="content"></param>
        /// <param name="style"></param>
        /// <param name="allowNoSelection"></param>
        /// <param name="addlParams"></param>
        /// <returns></returns>
        public static int Toolbar(int index, GUIContent[] content, GUIStyle style, bool allowNoSelection = false, params GUILayoutOption[] addlParams)
        {
            return Toolbar(index, content, style, style, style, allowNoSelection, addlParams);
        }

        public static int Toolbar(int index, GUIContent[] content, GUIStyle left, GUIStyle mid, GUIStyle right, bool allowNoSelection = false, params GUILayoutOption[] addlParams)
        {
            GUILayout.BeginHorizontal();

            for (int i = 0; i < content.Length; i++)
            {
                GUIStyle m_Style = i < 1 ? left : (i >= content.Length - 1 ? right : mid);

                if (index == i)
                    m_Style = EditorGUIUtility.GetOnStyle(m_Style);

                if (GUILayout.Button(content[i], m_Style, addlParams))
                {
                    if (index == i && allowNoSelection)
                        index = -1;
                    else
                        index = i;
                }
            }

            GUILayout.EndHorizontal();

            return index;
        }

        /**
         *  An automatically laid out toolbar that toggles flags. Content corresponds to the bits starting at 1 - ex:
         *      - content[0] = 0x1
         *      - content[1] = 0x2
         *      - content[2] = 0x4
         */
        public static int FlagToolbar(int index, GUIContent[] content, GUIStyle style, bool allowNoSelection = false, bool allowMultipleSelected = true, params GUILayoutOption[] addlParams)
        {
            return FlagToolbar(index, content, style, style, style, allowNoSelection, allowMultipleSelected, addlParams);
        }

        public static int FlagToolbar(int index, GUIContent[] content, bool allowNoSelection = false, bool allowMultipleSelected = true, params GUILayoutOption[] addlParams)
        {
            return FlagToolbar(index, content, UnityEditor.EditorStyles.miniButtonLeft, UnityEditor.EditorStyles.miniButtonMid, UnityEditor.EditorStyles.miniButtonRight, allowNoSelection, allowMultipleSelected, addlParams);
        }

        public static int FlagToolbar(int index, GUIContent[] content, GUIStyle left, GUIStyle mid, GUIStyle right, bool allowNoSelection = false, bool allowMultipleSelected = true, params GUILayoutOption[] addlParams)
        {
            GUILayout.BeginHorizontal();

            for (int i = 0; i < content.Length; i++)
            {
                GUIStyle m_Style = i < 1 ? left : (i >= content.Length - 1 ? right : mid);

                if ((index & (0x1 << i)) > 0)
                    m_Style = EditorGUIUtility.GetOnStyle(m_Style);

                if (GUILayout.Button(content[i], m_Style, addlParams))
                {
                    if (!allowMultipleSelected)
                        index = (index & (0x1 << i));

                    index ^= (0x1 << i);

                    if (!allowNoSelection && index == 0x0)
                        index = 0x1 << i;
                }
            }

            GUILayout.EndHorizontal();

            return index;
        }

        class ResizeHandleState
        {
            public Vector2 origin;
            public Rect startingRect;
        }

        delegate Rect ComputeResize(Rect currentRect, ResizeHandleState handleState, Event currentEvent, int minimumWidth, int minimumHeight);

        static KeyValuePair<int, Tuple<Rect, ComputeResize>> CreateResizeHandleControl(Rect activeRect, String suffix, Rect parentWindowRect, UnityEditor.MouseCursor cursor, ComputeResize resizeDelegate)
        {
            int id = GUIUtility.GetControlID(("ProBuilderWindowResize" + suffix).GetHashCode(), FocusType.Passive, parentWindowRect);
            HandleUtility.AddControl(id, Vector2.Distance(activeRect.center, Event.current.mousePosition));
            UnityEditor.EditorGUIUtility.AddCursorRect(activeRect, cursor);
            return new KeyValuePair<int, Tuple<Rect, ComputeResize>>(id, new Tuple<Rect, ComputeResize>(activeRect, resizeDelegate));
        }

        static Rect ResizeBottomRight(Rect currentRect, ResizeHandleState handleState, Event currentEvent, int minimumWidth, int minimumHeight)
        {
            currentRect.width = handleState.startingRect.width + (currentEvent.mousePosition.x - handleState.origin.x);
            currentRect.height = handleState.startingRect.height + (currentEvent.mousePosition.y - handleState.origin.y);
            return currentRect;
        }

        static Rect ResizeBottomLeft(Rect currentRect, ResizeHandleState handleState, Event currentEvent, int minimumWidth, int minimumHeight)
        {
            currentRect.width = currentRect.width - (currentEvent.mousePosition.x - handleState.origin.x);
            currentRect.width = Mathf.Max(currentRect.width, minimumWidth);
            currentRect.x = handleState.startingRect.xMax - currentRect.width;
            currentRect.height = handleState.startingRect.height + (currentEvent.mousePosition.y - handleState.origin.y);
            return currentRect;
        }

        static Rect ResizeBottom(Rect currentRect, ResizeHandleState handleState, Event currentEvent, int minimumWidth, int minimumHeight)
        {
            currentRect.height = handleState.startingRect.height + (currentEvent.mousePosition.y - handleState.origin.y);
            return currentRect;
        }

        static Rect ResizeTop(Rect currentRect, ResizeHandleState handleState, Event currentEvent, int minimumWidth, int minimumHeight)
        {
            currentRect.height = currentRect.height - (currentEvent.mousePosition.y - handleState.origin.y);
            currentRect.height = Mathf.Max(currentRect.height, minimumHeight);
            currentRect.y = handleState.startingRect.yMax - currentRect.height;
            return currentRect;
        }

        static Rect ResizeLeft(Rect currentRect, ResizeHandleState handleState, Event currentEvent, int minimumWidth, int minimumHeight)
        {
            currentRect.width = currentRect.width - (currentEvent.mousePosition.x - handleState.origin.x);
            currentRect.width = Mathf.Max(currentRect.width, minimumWidth);
            currentRect.x = handleState.startingRect.xMax - currentRect.width;            
            return currentRect;
        }

        static Rect ResizeRight(Rect currentRect, ResizeHandleState handleState, Event currentEvent, int minimumWidth, int minimumHeight)
        {
            currentRect.width = handleState.startingRect.width + (currentEvent.mousePosition.x - handleState.origin.x);
            return currentRect;
        }

        static Rect ResizeTopLeft(Rect currentRect, ResizeHandleState handleState, Event currentEvent, int minimumWidth, int minimumHeight)
        {
            currentRect.height = currentRect.height - (currentEvent.mousePosition.y - handleState.origin.y);
            currentRect.height = Mathf.Max(currentRect.height, minimumHeight);
            currentRect.y = handleState.startingRect.yMax - currentRect.height;
            currentRect.width = currentRect.width - (currentEvent.mousePosition.x - handleState.origin.x);
            currentRect.width = Mathf.Max(currentRect.width, minimumWidth);
            currentRect.x = handleState.startingRect.xMax - currentRect.width;
            return currentRect;
        }

        static Rect ResizeTopRight(Rect currentRect, ResizeHandleState handleState, Event currentEvent, int minimumWidth, int minimumHeight)
        {
            currentRect.height = currentRect.height - (currentEvent.mousePosition.y - handleState.origin.y);
            currentRect.height = Mathf.Max(currentRect.height, minimumHeight);
            currentRect.y = handleState.startingRect.yMax - currentRect.height;
            currentRect.width = handleState.startingRect.width + (currentEvent.mousePosition.x - handleState.origin.x);
            return currentRect;
        }

        static int s_ResizeHandleAreaDimension = 6;
        static int s_MoveWindowAreaHeight = 30;

        public static Rect DoResizeHandle(Rect rect, int minimumWidth, int minimumHeight)
        {
            var evt = Event.current;
            if (evt.type == EventType.Used)
            {
                return rect;
            }
            IDictionary<int, Tuple<Rect, ComputeResize>> resizeHandles = new Dictionary<int, Tuple<Rect, ComputeResize>>();
            resizeHandles.Add(CreateResizeHandleControl(new Rect(rect.width - s_ResizeHandleAreaDimension, rect.height - s_ResizeHandleAreaDimension, s_ResizeHandleAreaDimension, s_ResizeHandleAreaDimension), "BottomRight", rect, MouseCursor.ResizeUpLeft, ResizeBottomRight));
            resizeHandles.Add(CreateResizeHandleControl(new Rect(0, rect.height - s_ResizeHandleAreaDimension, s_ResizeHandleAreaDimension, s_ResizeHandleAreaDimension), "BottomLeft", rect, MouseCursor.ResizeUpRight, ResizeBottomLeft));
            resizeHandles.Add(CreateResizeHandleControl(new Rect(s_ResizeHandleAreaDimension, rect.height - s_ResizeHandleAreaDimension, rect.width - 2*s_ResizeHandleAreaDimension, s_ResizeHandleAreaDimension), "Bottom", rect, MouseCursor.ResizeVertical, ResizeBottom));
            resizeHandles.Add(CreateResizeHandleControl(new Rect(s_ResizeHandleAreaDimension, 0, rect.width - 2* s_ResizeHandleAreaDimension, s_ResizeHandleAreaDimension), "Top", rect, MouseCursor.ResizeVertical, ResizeTop));
            resizeHandles.Add(CreateResizeHandleControl(new Rect(0, s_MoveWindowAreaHeight, s_ResizeHandleAreaDimension, rect.height - (s_MoveWindowAreaHeight + s_ResizeHandleAreaDimension)), "Left", rect, MouseCursor.ResizeHorizontal, ResizeLeft));
            resizeHandles.Add(CreateResizeHandleControl(new Rect(rect.width - s_ResizeHandleAreaDimension, s_MoveWindowAreaHeight, s_ResizeHandleAreaDimension, rect.height - (s_MoveWindowAreaHeight + s_ResizeHandleAreaDimension)), "Right", rect, MouseCursor.ResizeHorizontal, ResizeRight));
            resizeHandles.Add(CreateResizeHandleControl(new Rect(0, 0, s_ResizeHandleAreaDimension, s_ResizeHandleAreaDimension), "TopLeft", rect, MouseCursor.ResizeUpLeft, ResizeTopLeft));
            resizeHandles.Add(CreateResizeHandleControl(new Rect(rect.width - s_ResizeHandleAreaDimension, 0, s_ResizeHandleAreaDimension, s_ResizeHandleAreaDimension), "TopRight", rect, MouseCursor.ResizeUpRight, ResizeTopRight));

            if (evt.type == EventType.MouseDown)
            {
                bool initializedControl = false;
                foreach (KeyValuePair<int, Tuple<Rect,ComputeResize>> kvp in resizeHandles)
                {
                    if (kvp.Value.Item1.Contains(evt.mousePosition))
                    {
                        GUIUtility.hotControl = kvp.Key;
                        initializedControl = true;
                        break;
                    }
                }                   
               
                if (!initializedControl)
                {
                    return rect;
                }
                GUI.changed = true;
                var state = (ResizeHandleState)GUIUtility.GetStateObject(typeof(ResizeHandleState), GUIUtility.hotControl);
                state.origin = evt.mousePosition;
                state.startingRect = rect;
                evt.Use();
            }
            else if (!resizeHandles.ContainsKey(GUIUtility.hotControl))
            {
                return rect;
            }
            if (evt.type == EventType.MouseUp)
            {
                GUIUtility.hotControl = 0;
                GUI.changed = true;
                evt.Use();
            }
            else if (evt.type == EventType.MouseDrag)
            {
                var state = (ResizeHandleState)GUIUtility.GetStateObject(typeof(ResizeHandleState), GUIUtility.hotControl);
                rect = resizeHandles[GUIUtility.hotControl].Item2(rect, state, evt, minimumWidth, minimumHeight);
                
                GUI.changed = true;
                evt.Use();
            }

            return rect;
        }

        public static Rect GetSliderRect(bool hasLabel, params GUILayoutOption[] options)
        {
#if UNITY_INTERNALS_VISIBLE
            return UnityEditor.EditorGUILayout.GetSliderRect(hasLabel, options);
#else
            if (s_GetSliderRectMethod == null)
                return Rect.zero;

            s_GetSliderRectParams[0] = hasLabel;
            s_GetSliderRectParams[1] = options;
            return (Rect)s_GetSliderRectMethod.Invoke(null, s_GetSliderRectParams);
#endif
        }
    }
}
