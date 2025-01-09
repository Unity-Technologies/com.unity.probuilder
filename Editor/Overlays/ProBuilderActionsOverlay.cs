using System;
using System.Collections.Generic;
using UnityEditor.Actions;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder.Actions;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
    class ProBuilderActionButton : VisualElement
    {
        const string k_StyleSheetPath = "Packages/com.unity.probuilder/Editor/Resources/ActionOverlay.uss";
        const string k_UxmlPath = "Packages/com.unity.probuilder/Editor/Resources/ActionButton.uxml";

        static StyleSheet s_CommonStyleSheet;
        static VisualTreeAsset s_ButtonAsset;

        MenuAction m_Action;

        Button m_Button;
        Label m_Label;
        VisualElement m_Color;
        VisualElement m_Icon;

        static Texture2D s_IconTexture = null;
        static Texture2D iconTexture
        {
            get
            {
                if (s_IconTexture == null)
                    s_IconTexture = IconUtility.GetIcon("Tools/EditShape");

                return s_IconTexture;
            }
        }

        internal ProBuilderActionButton(MenuAction action)
        {
            if (s_ButtonAsset == null)
                s_ButtonAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_UxmlPath);

            if (s_CommonStyleSheet == null)
                s_CommonStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheetPath);

            s_ButtonAsset.CloneTree(this);

            m_Button = this.Q<Button>();
            m_Label = this.Q<Label>();
            m_Color = this.Q<VisualElement>("ActionColor");
            m_Icon = this.Q<VisualElement>("ActionIcon");

            m_Action = action;

            m_Button.clicked += () => EditorAction.Start(new MenuActionSettings(action, HasPreview(action)));
            m_Color.style.backgroundColor = ToolbarGroupUtility.GetColor(m_Action.group);
            m_Label.text = action.menuTitle;

            m_Icon.style.backgroundImage = new StyleBackground(iconTexture);
        }

        static bool HasPreview(MenuAction action)
        {
            return !(action is DetachFaces || action is DuplicateFaces);
        }

        internal void UpdateContent(Layout layout, bool isInToolbar, Layout preferredLayout)
        {
            var hidden = m_Action.hidden;
            var isGOContext = EditorToolManager.activeToolContext is GameObjectToolContext;
            hidden |= (m_Action.group == ToolbarGroup.Object) ? !isGOContext : isGOContext;
            m_Button.style.display = hidden ? DisplayStyle.None : DisplayStyle.Flex;
            m_Button.enabledSelf = m_Action.enabled;
            m_Button.tooltip = m_Action.menuTitle;

            //Remove all styles
            m_Button.RemoveFromClassList("panelMode");
            m_Button.RemoveFromClassList("toolbarHorizontalMode");
            m_Button.RemoveFromClassList("toolbarVerticalMode");
            m_Label.RemoveFromClassList("toolbarMode");
            m_Color.RemoveFromClassList("toolbarMode");
            m_Icon.RemoveFromClassList("toolbarMode");

            if (!isInToolbar)
            {
                //Add relevant ones
                switch (layout)
                {
                    case Layout.VerticalToolbar:
                        m_Button.AddToClassList("toolbarVerticalMode");
                        m_Label.AddToClassList("toolbarMode");
                        m_Color.AddToClassList("toolbarMode");
                        m_Icon.AddToClassList("toolbarMode");
                        break;
                    case Layout.HorizontalToolbar:
                        m_Button.AddToClassList("toolbarHorizontalMode");
                        m_Label.AddToClassList("toolbarMode");
                        m_Color.AddToClassList("toolbarMode");
                        m_Icon.AddToClassList("toolbarMode");
                        break;
                    default:
                        m_Button.AddToClassList("panelMode");
                        break;
                }
            }
            else
            {
                if(preferredLayout == Layout.HorizontalToolbar)
                    m_Button.AddToClassList("toolbarHorizontalMode");
                else
                    m_Button.AddToClassList("toolbarVerticalMode");
                m_Label.AddToClassList("toolbarMode");
                m_Color.AddToClassList("toolbarMode");
                m_Icon.AddToClassList("toolbarMode");
            }
        }
    }

    [Overlay(typeof(SceneView), overlayId, k_DisplayName)]
    class ProBuilderActionsOverlay : Overlay, ICreateHorizontalToolbar, ICreateVerticalToolbar
    {
        const string k_DisplayName = "ProBuilder Actions";
        internal const string overlayId = "ProBuilder/ActionsOverlay";

        static readonly HashSet<Type> k_ContextMenuBlacklist = new HashSet<Type>()
        {
            typeof(OpenSmoothingEditor),
            typeof(OpenMaterialEditor),
            typeof(OpenUVEditor),
            typeof(OpenVertexColorEditor),
            typeof(ToggleHandleOrientation),
            typeof(ToggleDragRectMode),
            typeof(ToggleSelectBackFaces)
        };

        //static List<(MenuAction action, Button button)> s_Actions = null;
        List<ProBuilderActionButton> s_ActionButtons = new List<ProBuilderActionButton>();

        public ProBuilderActionsOverlay()
        {
            var actions = EditorToolbarLoader.GetActions();

            // grouping and filtering is bespoke for demo reasons
            foreach (var action in actions)
            {
                if (k_ContextMenuBlacklist.Contains(action.GetType()))
                    continue;

                if (action.group == ToolbarGroup.Entity || action.group == ToolbarGroup.Tool)
                    continue;

                s_ActionButtons.Add( new ProBuilderActionButton(action) );
            }

            rootVisualElement.RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            rootVisualElement.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            layoutChanged += _ =>  UpdateContent();
            floatingChanged += _ =>  UpdateContent();
            dockingCompleted += _ =>  UpdateContent();
        }


        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            Selection.selectionChanged += UpdateContent;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
            ProBuilderEditor.selectionUpdated += OnSelectionUpdated;
            ToolManager.activeContextChanged += UpdateContent;
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            Selection.selectionChanged -= UpdateContent;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            ProBuilderEditor.selectionUpdated -= OnSelectionUpdated;
            ToolManager.activeContextChanged -= UpdateContent;
        }

        void UpdateContent()
        {
            foreach (var actionButton in s_ActionButtons)
            {
                actionButton.UpdateContent(layout, isInToolbar, container.preferredLayout);
            }
        }

        private void OnSelectModeChanged(SelectMode obj) => UpdateContent();

        private void OnSelectionUpdated(IEnumerable<ProBuilderMesh> obj) => UpdateContent();

        public override VisualElement CreatePanelContent()
        {

            var root = new VisualElement();
            foreach (var actionButton in s_ActionButtons)
            {
                root.Add(actionButton);
            }

            OnSelectModeChanged(ProBuilderEditor.selectMode);

            return root;
        }

        public OverlayToolbar CreateHorizontalToolbarContent()
        {
            return CreateToolbarOverlay();
        }

        public OverlayToolbar CreateVerticalToolbarContent()
        {
            return CreateToolbarOverlay();
        }

        OverlayToolbar CreateToolbarOverlay()
        {
            var toolbar = new OverlayToolbar();

            foreach (var proBuilderAction in s_ActionButtons)
            {
                proBuilderAction.UpdateContent(layout, isInToolbar, container.preferredLayout);
                toolbar.Add(proBuilderAction);
            }

            return toolbar;
        }

    }
}
