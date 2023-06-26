using System;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.Toolbars;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder.UI
{
    abstract class SettingsToggle<T> : EditorToolbarButton where T : MenuAction, new()
    {
        MenuAction m_MenuAction;

        static SettingsToggle<T> s_Instance;

        public SettingsToggle()
        {
            m_MenuAction = EditorToolbarLoader.GetInstance<T>();
            UpdateContent(m_MenuAction);
            tooltip = m_MenuAction.tooltip.summary;

            clicked += OnClick;
            ProBuilderEditor.instance.iconModeChanged += () => UpdateContent(m_MenuAction);
            ProBuilderEditor.selectModeChanged += (s) => SelectModeUpdated(s);
            MenuAction.onPerformAction += UpdateContent;

            style.display = m_MenuAction.hidden ? DisplayStyle.None : DisplayStyle.Flex;
            s_Instance = this;
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
                // The action event is triggered before the value is changed, the delay call allows to change
                // the button after the value is updated
                EditorApplication.delayCall += () =>
                {
                    iconImage = ProBuilderEditor.s_IsIconGui ? m_MenuAction.icon : null;
                    text = ProBuilderEditor.s_IsIconGui ? String.Empty : m_MenuAction.menuTitle;
                };
            }
        }

        static void SelectModeUpdated(SelectMode _)
        {
            s_Instance.style.display = s_Instance.m_MenuAction.hidden ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }

    [EditorToolbarElement("ProBuilder Settings/Drag Selection Mode")]
    class DragSelectionModeToggle : SettingsToggle<ToggleDragSelectionMode>
    {
    }

    [EditorToolbarElement("ProBuilder Settings/Drag Rect Mode")]
    class DragRectModeToggle : SettingsToggle<ToggleDragRectMode>
    {
    }

    [EditorToolbarElement("ProBuilder Settings/Select Back Faces")]
    class SelectBackFacesToggle : SettingsToggle<ToggleSelectBackFaces>
    {
    }

    [EditorToolbarElement("ProBuilder Settings/Handle Orientation")]
    class HandleOrientationToggle : SettingsToggle<ToggleHandleOrientation>
    {
    }

    [EditorToolbarElement("ProBuilder Settings/X Ray")]
    class XRayToggle : SettingsToggle<ToggleXRay>
    {
    }
}
