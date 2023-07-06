#if UNITY_2023_2_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder.UI
{
    abstract class SettingsToggle<T> : EditorToolbarButton where T : MenuAction, new()
    {
        MenuAction m_MenuAction;

        static SettingsToggle<T> s_Instance;

        const string k_ParentOverlayName = "Tool Settings";
        Overlay m_ParentOverlay;
        Overlay parentOverlay
        {
            get
            {
                if(m_ParentOverlay != null)
                    return m_ParentOverlay;

                foreach (var overlay in SceneView.lastActiveSceneView.overlayCanvas.overlays)
                {
                    if (overlay.displayName == k_ParentOverlayName)
                    {
                        m_ParentOverlay = overlay;
                        break;
                    }
                }

                return m_ParentOverlay;
            }
        }

        public SettingsToggle()
        {
            m_MenuAction = EditorToolbarLoader.GetInstance<T>();
            tooltip = m_MenuAction.tooltip.summary;

            s_Instance = this;

            UpdateContent(m_MenuAction);
            style.display = m_MenuAction.hidden ? DisplayStyle.None : DisplayStyle.Flex;

            clicked += OnClick;
            ProBuilderEditor.instance.iconModeChanged += () => UpdateContent(m_MenuAction);
            ProBuilderEditor.selectModeChanged += (s) => SelectModeUpdated();
            MenuAction.onPerformAction += UpdateContent;
        }

        void OnClick()
        {
            m_MenuAction.PerformAction();
            UpdateContent(m_MenuAction);
        }

        void UpdateContent(MenuAction action)
        {
            if (action is T)
            {
                var useIcons = ProBuilderEditor.s_IsIconGui || parentOverlay.layout == Layout.VerticalToolbar;

                // The action event is triggered before the value is changed, the delay call allows to change
                // the button after the value is updated
                EditorApplication.delayCall += () =>
                {
                    iconImage = useIcons ? m_MenuAction.icon : null;
                    text = useIcons ? String.Empty : m_MenuAction.menuTitle;
                };
            }
        }

        static void SelectModeUpdated()
        {
            s_Instance.style.display = s_Instance.m_MenuAction.hidden ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }

    [EditorToolbarElement("ProBuilder Settings/Drag Rect Mode")]
    class DragRectModeToggle : SettingsToggle<ToggleDragRectMode>
    {
    }

    [EditorToolbarElement("ProBuilder Settings/Select Back Faces")]
    class SelectBackFacesToggle : SettingsToggle<ToggleSelectBackFaces>
    {
    }

    [EditorToolbarElement("ProBuilder Settings/X Ray")]
    class XRayToggle : SettingsToggle<ToggleXRay>
    {
    }

    [EditorToolbarElement("ProBuilder Settings/Handle Orientation")]
    class HandleOrientationDropdown : EditorToolbarDropdown
    {
        ToggleHandleOrientation m_MenuAction;
        readonly List<GUIContent> m_OptionContents = new List<GUIContent>();

        public HandleOrientationDropdown()
        {
            m_MenuAction = EditorToolbarLoader.GetInstance<ToggleHandleOrientation>();
            name = "Handle Rotation";
            tooltip = m_MenuAction.tooltip.summary;

            var content = UnityEditor.EditorGUIUtility.TrTextContent(ToggleHandleOrientation.tooltips[(int)HandleOrientation.World].title,
                ToggleHandleOrientation.tooltips[(int)HandleOrientation.World].summary,
                "ToolHandleGlobal");
            m_OptionContents.Add(content);

            content = UnityEditor.EditorGUIUtility.TrTextContent(ToggleHandleOrientation.tooltips[(int)HandleOrientation.ActiveObject].title,
                ToggleHandleOrientation.tooltips[(int)HandleOrientation.ActiveObject].summary,
                "ToolHandleLocal");
            m_OptionContents.Add(content);

            content = UnityEditor.EditorGUIUtility.TrTextContent(ToggleHandleOrientation.tooltips[(int)HandleOrientation.ActiveElement].title,
                ToggleHandleOrientation.tooltips[(int)HandleOrientation.ActiveElement].summary,
                "ToolHandleLocal");
            m_OptionContents.Add(content);

            RegisterCallback<AttachToPanelEvent>(AttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(DetachedFromPanel);

            clicked += OpenContextMenu;
            RefreshElementContent();

            style.display = m_MenuAction.hidden ? DisplayStyle.None : DisplayStyle.Flex;
            ProBuilderEditor.selectModeChanged += (s) => SelectModeUpdated();
        }

        void OpenContextMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(m_OptionContents[(int)HandleOrientation.World], m_MenuAction.handleOrientation == HandleOrientation.World,
                () => SetHandleOrientationIfNeeded(HandleOrientation.World));

            menu.AddItem(m_OptionContents[(int)HandleOrientation.ActiveObject], m_MenuAction.handleOrientation == HandleOrientation.ActiveObject,
                () => SetHandleOrientationIfNeeded(HandleOrientation.ActiveObject));

            menu.AddItem(m_OptionContents[(int)HandleOrientation.ActiveElement], m_MenuAction.handleOrientation == HandleOrientation.ActiveElement,
                () => SetHandleOrientationIfNeeded(HandleOrientation.ActiveElement));

            menu.DropDown(worldBound);
        }

        void SetHandleOrientationIfNeeded(HandleOrientation handleOrientation)
        {
            if (m_MenuAction.handleOrientation != handleOrientation)
            {
                m_MenuAction.handleOrientation = handleOrientation;
                RefreshElementContent();
            }
        }

        void RefreshElementContent()
        {
            var content = m_OptionContents[(int)m_MenuAction.handleOrientation];
            text = content.text;
            tooltip = content.tooltip;
            icon = content.image as Texture2D;
        }

        void AttachedToPanel(AttachToPanelEvent evt)
            => VertexManipulationTool.handleOrientationChanged += RefreshElementContent;

        void DetachedFromPanel(DetachFromPanelEvent evt)
            => VertexManipulationTool.handleOrientationChanged -= RefreshElementContent;

        void SelectModeUpdated()
        {
            style.display = m_MenuAction.hidden ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
#endif
