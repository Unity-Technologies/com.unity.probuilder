using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using UnityEditor.Actions;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.Search;
using UnityEngine;
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

        private MenuAction action
        {
            get => m_Action;
            set
            {
                m_Action = value;
                m_IconTexture = null;
            }
        }

        Button m_Button;
        Label m_Label;
        VisualElement m_Color;
        Image m_Icon;

        Action m_ClickHandler;

        Texture2D m_IconTexture = null;
        Texture2D iconTexture
        {
            get
            {
                if (m_IconTexture == null && m_Action != null)
                    m_IconTexture = m_Action.icon;

                if (m_IconTexture == null)
                    m_IconTexture = IconUtility.GetIcon("Tools/EditShape");

                return m_IconTexture;
            }
        }

        internal ProBuilderActionButton()
        {
            if (s_ButtonAsset == null)
                s_ButtonAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_UxmlPath);

            if (s_CommonStyleSheet == null)
                s_CommonStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheetPath);

            s_ButtonAsset.CloneTree(this);

            m_Button = this.Q<Button>();
            m_Label = this.Q<Label>();
            m_Color = this.Q<VisualElement>("ActionColor");
            m_Icon = this.Q<Image>("ActionIcon");
        }

        internal ProBuilderActionButton(MenuAction act) : this()
        {
            Bind(act);
        }

        internal void Bind(MenuAction act)
        {
            action = act;

            if (m_ClickHandler != null)
                m_Button.clicked -= m_ClickHandler;
            m_ClickHandler = () => EditorAction.Start(new MenuActionSettings(action, HasPreview(action)));
            m_Button.clicked += m_ClickHandler;

            m_Color.style.backgroundColor = ToolbarGroupUtility.GetColor(m_Action.group);
            m_Label.text = action.menuTitle;
            m_Icon.image = iconTexture;
        }

        static bool HasPreview(MenuAction action)
        {
            return !(action is DetachFaces || action is DuplicateFaces);
        }

        void CleanUpStyles()
        {
            //Remove all styles
            m_Button.RemoveFromClassList("toolbarHorizontalMode");
            m_Button.RemoveFromClassList("toolbarVerticalMode");
            m_Button.RemoveFromClassList("enabledAction");
            m_Button.RemoveFromClassList("unity-overlay");
            m_Button.RemoveFromClassList("unity-toolbar-toggle");
            m_Label.RemoveFromClassList("toolbarMode");
            m_Color.RemoveFromClassList("toolbarMode");
            m_Icon.RemoveFromClassList("toolbarMode");
        }

        internal void UpdateContent(ProBuilderActionsOverlay.DisplayMode mode)
        {
            style.flexGrow = 1f;
            m_Button.style.flexGrow = 1f;
            m_Button.enabledSelf = m_Action.enabled;
            m_Button.tooltip = m_Action.menuTitle;
            m_Icon.style.display = DisplayStyle.Flex;
            m_Label.style.display = DisplayStyle.Flex;

            CleanUpStyles();

            if(m_Action.enabled)
                m_Button.AddToClassList("enabledAction");
            m_Button.AddToClassList("unity-overlay");
            m_Button.AddToClassList("unity-toolbar-toggle");
            m_Icon.style.display = mode == ProBuilderActionsOverlay.DisplayMode.Text ? DisplayStyle.None : DisplayStyle.Flex;
            m_Label.style.display = mode == ProBuilderActionsOverlay.DisplayMode.Icon ? DisplayStyle.None : DisplayStyle.Flex;
        }

        internal void UpdateContentForToolbar(Layout layout)
        {
            var hidden = m_Action.hidden;
            var isGOContext = EditorToolManager.activeToolContext is GameObjectToolContext;
            hidden |= (m_Action.group == ToolbarGroup.Object) ? !isGOContext : isGOContext;
            m_Button.style.display = hidden ? DisplayStyle.None : DisplayStyle.Flex;
            m_Button.enabledSelf = m_Action.enabled;
            m_Button.tooltip = m_Action.menuTitle;

            CleanUpStyles();

            if(layout == Layout.HorizontalToolbar)
                m_Button.AddToClassList("toolbarHorizontalMode");
            else
                m_Button.AddToClassList("toolbarVerticalMode");

            if(m_Action.enabled)
                m_Button.AddToClassList("enabledAction");
            m_Button.AddToClassList("unity-overlay");
            m_Button.AddToClassList("unity-toolbar-toggle");
            m_Label.AddToClassList("toolbarMode");
            m_Color.AddToClassList("toolbarMode");
            m_Icon.AddToClassList("toolbarMode");
        }
    }

    [Overlay(typeof(SceneView), overlayId, k_DisplayName, minHeight = 150f, maxHeight = 500f, minWidth = 200f, maxWidth = 600f)]
    [Icon("Packages/com.unity.probuilder/Editor Default Resources/Icons/EditableMesh/EditMeshContext.png")]
    class ProBuilderActionsOverlay : Overlay, ICreateHorizontalToolbar, ICreateVerticalToolbar
    {
        const string k_DisplayName = "ProBuilder Actions";
        private const string overlayId = "ProBuilder/ActionsOverlay";

        private static readonly HashSet<Type> k_ContextMenuBlacklist = new HashSet<Type>()
        {
            typeof(ToggleHandleOrientation),
            typeof(ToggleDragRectMode),
            typeof(ToggleSelectBackFaces),
            typeof(NewBezierShape)
        };

        private List<MenuAction> m_Actions;
        private List<MenuAction> m_AvailableActions = new ();
        List<ProBuilderActionButton> s_ActionButtons = new List<ProBuilderActionButton>();

        GridView m_Grid;
        OverlayToolbar m_Toolbar;

        internal enum DisplayMode
        {
            Icon,
            Text,
            Full
        }

        [SerializeField] private DisplayMode m_CurrentMode = DisplayMode.Full;
        [SerializeField] private bool m_DisplayEditors = true;
        [SerializeField] private bool m_DisplaySelection = false;

        public ProBuilderActionsOverlay()
        {
            m_Actions = EditorToolbarLoader.GetActions();

            RefreshAvailableActions();

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
            RefreshAvailableActions();
            UpdateGrid();
            UpdateToolbar();
        }

        private void OnSelectModeChanged(SelectMode obj) => UpdateContent();

        private void OnSelectionUpdated(IEnumerable<ProBuilderMesh> obj) => UpdateContent();

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            root.name = "ProbuilderActions";
            m_Grid = new GridView(m_AvailableActions, 0, 0, MakeItem, BindItem);
            root.Add(m_Grid);

            OnSelectModeChanged(ProBuilderEditor.selectMode);
            var contextClickGrid = new ContextualMenuManipulator(BuildContextMenu);
            m_Grid.AddManipulator(contextClickGrid);
            return root;
        }

        private VisualElement MakeItem()
        {
            return new ProBuilderActionButton();
        }

        private void BindItem(VisualElement element, int index)
        {
            var e = (ProBuilderActionButton)element;
            if (index >= 0 && index < m_AvailableActions.Count)
            {
                e.Bind(m_AvailableActions[index]);
                e.UpdateContent(m_CurrentMode);
                e.style.flexGrow = 1f;
            }
        }

        private void RefreshAvailableActions()
        {
            m_AvailableActions.Clear();

            var initActionButtons = s_ActionButtons.Count == 0;

            if (!initActionButtons)
            {
                foreach (var element in s_ActionButtons)
                    element.style.display = DisplayStyle.None;
            }

            int actionIndex = 0;
            var isGOContext = EditorToolManager.activeToolContext is GameObjectToolContext;
            for (int i = 0; i < m_Actions.Count; i++)
            {
                var action = m_Actions[i];
                if (k_ContextMenuBlacklist.Contains(action.GetType()))
                    continue;

                if (action.group == ToolbarGroup.Entity)
                    continue;

                var shouldDisplayAsEditor = m_DisplayEditors && action.group == ToolbarGroup.Tool;
                var shouldDisplayAsSelection = m_DisplaySelection && action.group == ToolbarGroup.Selection;
                var shouldDisplay = action.group != ToolbarGroup.Tool && action.group != ToolbarGroup.Selection;

                var hidden = action.hidden;
                hidden |= (action.group == ToolbarGroup.Object) ? !isGOContext : isGOContext;

                if (initActionButtons)
                    s_ActionButtons.Add( new ProBuilderActionButton(action) );

                if (!hidden)
                {
                    if (shouldDisplayAsEditor || shouldDisplayAsSelection || shouldDisplay)
                    {
                        m_AvailableActions.Add(action);
                        s_ActionButtons[actionIndex].style.display = DisplayStyle.Flex;
                    }
                }

                actionIndex++;
            }
        }

        private void UpdateGrid()
        {
            if (m_Grid != null)
            {
                m_Grid.itemsSource = m_AvailableActions;
                m_Grid.fixedItemWidth = m_CurrentMode == DisplayMode.Icon ? 40f : 180f;
                m_Grid.Rebuild();
                m_Grid.RefreshItems();
            }
        }

        void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            var menu = evt.menu;

            if (layout == Layout.Panel)
            {
                menu.AppendAction(L10n.Tr("Icon Mode"), _ => { SetMode(DisplayMode.Icon); },
                    m_CurrentMode == DisplayMode.Icon ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                menu.AppendAction(L10n.Tr("Text Mode"), _ => { SetMode(DisplayMode.Text); },
                    m_CurrentMode == DisplayMode.Text ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                menu.AppendAction(L10n.Tr("Text & Icon Mode"), _ => { SetMode(DisplayMode.Full); },
                    m_CurrentMode == DisplayMode.Full ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                menu.AppendSeparator();
            }

            menu.AppendAction(L10n.Tr("Display Editors Actions"), _ =>
                {
                    m_DisplayEditors = !m_DisplayEditors;
                    UpdateContent();
                },
                m_DisplayEditors ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            menu.AppendAction(L10n.Tr("Display Select Actions"), _ =>
                {
                    m_DisplaySelection = !m_DisplaySelection;
                    UpdateContent();
                },
                m_DisplaySelection ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

        }

        private void SetMode(DisplayMode mode)
        {
            m_CurrentMode = mode;
            UpdateContent();
        }

        public OverlayToolbar CreateHorizontalToolbarContent() => CreateToolbarOverlay();

        public OverlayToolbar CreateVerticalToolbarContent() => CreateToolbarOverlay();

        OverlayToolbar CreateToolbarOverlay()
        {
            if (m_Toolbar == null)
            {
                m_Toolbar = new OverlayToolbar();
                var contextClick = new ContextualMenuManipulator(BuildContextMenu);
                m_Toolbar.AddManipulator(contextClick);
            }
            else
                m_Toolbar.Clear();

            UpdateContent();
            return m_Toolbar;
        }

        void UpdateToolbar()
        {
            if (m_Toolbar != null)
            {
                foreach (var proBuilderAction in s_ActionButtons)
                {
                    proBuilderAction.UpdateContentForToolbar(layout);
                    m_Toolbar.Add(proBuilderAction);
                }
            }
        }

    }
}
