using System;
using System.Collections.Generic;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder.UI
{
    class SelectModeToggle : EditorToolbarToggle
    {
        SelectMode m_Mode;

        public SelectModeToggle(SelectMode mode)
        {
            m_Mode = mode;

            switch (mode)
            {
                case SelectMode.Face:
                case SelectMode.TextureFace:
                    icon = IconUtility.GetIcon("Modes/Mode_Face");
                    tooltip = "Face Selection";
                    break;

                case SelectMode.Edge:
                case SelectMode.TextureEdge:
                    icon = IconUtility.GetIcon("Modes/Mode_Edge");
                    tooltip = "Edge Selection";
                    break;

                case SelectMode.Vertex:
                case SelectMode.TextureVertex:
                    icon = IconUtility.GetIcon("Modes/Mode_Vertex");
                    tooltip = "Vertex Selection";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }

            RegisterCallback<AttachToPanelEvent>(evt => { ProBuilderEditor.selectModeChanged += UpdateSelectMode; });
            RegisterCallback<DetachFromPanelEvent>(evt => { ProBuilderEditor.selectModeChanged -= UpdateSelectMode; });
            this.RegisterValueChangedCallback(evt =>
            {
                if(evt.newValue)
                    ProBuilderEditor.selectMode = m_Mode;
                //Trying to set the edit mode to false: Not allowed
                else
                    SetValueWithoutNotify(true);
            });

            UpdateSelectMode(ProBuilderEditor.selectMode);
        }

        void UpdateSelectMode(SelectMode mode) => SetValueWithoutNotify(m_Mode == mode);
    }

    [EditorToolbarElement("ProBuilder Settings/Select Mode")]
    class SelectModeToolbar : VisualElement
    {
        public SelectModeToolbar()
        {
            Add(new SelectModeToggle(SelectMode.Vertex));
            Add(new SelectModeToggle(SelectMode.Edge));
            Add(new SelectModeToggle(SelectMode.Face));
            EditorToolbarUtility.SetupChildrenAsButtonStrip(this);
        }
    }

    [EditorToolbarElement("ProBuilder Settings/Drag Rect Mode")]
    class DragRectModeToggle : EditorToolbarToggle
    {
        DragRectModeToggle()
        {
            var action =  EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
            tooltip = action.tooltip.summary;
            style.display = action.hidden ? DisplayStyle.None : DisplayStyle.Flex;

            offIcon = action.icons[0];
            onIcon = action.icons[1];
            SetValueWithoutNotify(ProBuilderEditor.rectSelectMode == RectSelectMode.Complete);
            RegisterCallback<ChangeEvent<bool>>(evt => ProBuilderEditor.rectSelectMode = evt.newValue ? RectSelectMode.Complete : RectSelectMode.Partial);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            ProBuilderEditor.selectModeChanged += SelectModeUpdated;
            ProBuilderEditor.rectSelectModeChanged += UpdateVisual;
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            ProBuilderEditor.selectModeChanged -= SelectModeUpdated;
            ProBuilderEditor.rectSelectModeChanged -= UpdateVisual;
        }

        void SelectModeUpdated(SelectMode s)
        {
            style.display = EditorToolbarLoader.GetInstance<ToggleDragRectMode>().hidden ? DisplayStyle.None : DisplayStyle.Flex;
        }

        void UpdateVisual()
        {
            SetValueWithoutNotify(ProBuilderEditor.rectSelectMode == RectSelectMode.Complete);
        }
    }

    [EditorToolbarElement("ProBuilder Settings/Select Back Faces")]
    class SelectBackFacesToggle : EditorToolbarToggle
    {
        SelectBackFacesToggle()
        {
            var action =  EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
            tooltip = action.tooltip.summary;
            style.display = action.hidden ? DisplayStyle.None : DisplayStyle.Flex;

            offIcon = action.icons[0];
            onIcon = action.icons[1];
            SetValueWithoutNotify(ProBuilderEditor.backfaceSelectionEnabled);
            RegisterCallback<ChangeEvent<bool>>(evt => ProBuilderEditor.backfaceSelectionEnabled = evt.newValue);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            ProBuilderEditor.selectModeChanged += SelectModeUpdated;
            ProBuilderEditor.backfaceSelectionEnabledChanged += UpdateVisual;
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            ProBuilderEditor.selectModeChanged -= SelectModeUpdated;
            ProBuilderEditor.backfaceSelectionEnabledChanged += UpdateVisual;
        }

        void SelectModeUpdated(SelectMode s)
        {
            style.display = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>().hidden ? DisplayStyle.None : DisplayStyle.Flex;
        }

        void UpdateVisual()
        {
            SetValueWithoutNotify(ProBuilderEditor.backfaceSelectionEnabled);
        }
    }

    [EditorToolbarElement("ProBuilder Settings/Handle Orientation")]
    class HandleOrientationDropdown : EditorToolbarDropdown
    {
        ToggleHandleOrientation m_MenuAction;
        readonly List<GUIContent> m_OptionContents = new List<GUIContent>();

        const string k_NormalRotationIconPath = "Packages/com.unity.probuilder/Content/Icons/Modes/ToolHandleElement.png";

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
                k_NormalRotationIconPath);
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
