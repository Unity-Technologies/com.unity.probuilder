﻿using System;
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
        public SelectModeToggle(SelectMode mode)
        {
            switch (mode)
            {
                case SelectMode.Face:
                case SelectMode.TextureFace:
                    icon = IconUtility.GetIcon("Modes/Mode_Face");
                    break;

                case SelectMode.Edge:
                case SelectMode.TextureEdge:
                    icon = IconUtility.GetIcon("Modes/Mode_Edge");
                    break;

                case SelectMode.Vertex:
                case SelectMode.TextureVertex:
                    icon = IconUtility.GetIcon("Modes/Mode_Vertex");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
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

            ProBuilderEditor.selectModeChanged += (s) => SelectModeUpdated();
        }

        void SelectModeUpdated()
        {
            style.display = EditorToolbarLoader.GetInstance<ToggleDragRectMode>().hidden ? DisplayStyle.None : DisplayStyle.Flex;
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

            ProBuilderEditor.selectModeChanged += (s) => SelectModeUpdated();
        }

        void SelectModeUpdated()
        {
            style.display = EditorToolbarLoader.GetInstance<ToggleDragRectMode>().hidden ? DisplayStyle.None : DisplayStyle.Flex;
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
