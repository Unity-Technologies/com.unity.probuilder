#if UNITY_2023_2_OR_NEWER
using System;
using UnityEditor.Overlays;
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
#endif
