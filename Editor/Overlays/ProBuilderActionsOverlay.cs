using System;
using System.Collections.Generic;
using UnityEditor.Actions;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
    class ProBuilderActionButton : VisualElement
    {
        const string k_UxmlPath = "Packages/com.unity.probuilder/Editor/Overlays/UXML/ActionButton.uxml";
        static VisualTreeAsset s_ButtonAsset;

        bool m_Initialized = false;

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
            Initialize();
        }

        internal ProBuilderActionButton(MenuAction act) : this()
        {
            Bind(act);
        }

        void Initialize()
        {
            if (s_ButtonAsset == null)
                s_ButtonAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_UxmlPath);

            if (s_ButtonAsset != null)
            {
                m_Initialized = true;
                s_ButtonAsset.CloneTree(this);

                m_Button = this.Q<Button>();
                m_Label = this.Q<Label>();
                m_Color = this.Q<VisualElement>("ActionColor");
                m_Icon = this.Q<Image>("ActionIcon");
            }
        }

        internal void Bind(MenuAction act)
        {
            if (!m_Initialized)
                Initialize();

            action = act;

            if (m_Initialized)
            {
                if (m_ClickHandler != null)
                    m_Button.clicked -= m_ClickHandler;
                m_ClickHandler = () => EditorAction.Start(new MenuActionSettings(action, HasPreview(action)));
                m_Button.clicked += m_ClickHandler;

                m_Color.style.backgroundColor = ToolbarGroupUtility.GetColor(m_Action.group);
                m_Label.text = action.menuTitle;
                m_Icon.image = iconTexture;
            }
        }

        static bool HasPreview(MenuAction action)
        {
            return !(action is DetachFaces || action is DuplicateFaces);
        }

        void CleanUpStyles()
        {
            //Remove all styles
            m_Button?.RemoveFromClassList("toolbarHorizontalMode");
            m_Button?.RemoveFromClassList("toolbarVerticalMode");
            m_Button?.RemoveFromClassList("enabledAction");
            m_Button?.RemoveFromClassList("unity-overlay");
            m_Button?.RemoveFromClassList("unity-toolbar-toggle");
            m_Label?.RemoveFromClassList("toolbarMode");
            m_Color?.RemoveFromClassList("toolbarMode");
            m_Icon?.RemoveFromClassList("toolbarMode");
        }

        internal void UpdateContent()
        {
            if (!m_Initialized)
                return;

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
            m_Icon.style.display = ProBuilderActionsOverlay.s_CurrentMode == ProBuilderActionsOverlay.DisplayMode.Text ? DisplayStyle.None : DisplayStyle.Flex;
            m_Label.style.display = ProBuilderActionsOverlay.s_CurrentMode == ProBuilderActionsOverlay.DisplayMode.Icon ? DisplayStyle.None : DisplayStyle.Flex;
        }

        internal void UpdateContentForToolbar(Layout layout)
        {
            if (!m_Initialized)
                return;

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

    [Overlay(typeof(SceneView), overlayId, k_DisplayName, minHeight = 150f, maxHeight = 500f, minWidth = 200f, maxWidth = 600f, defaultDisplay = false)]
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
        List<ProBuilderActionButton> m_ActionButtons = new List<ProBuilderActionButton>();

        GridView m_Grid;
        VisualElement m_ContentViewport;
        OverlayToolbar m_Toolbar;

        public enum DisplayMode
        {
            Icon,
            Text,
            Full
        }

        [UserSetting("Actions Overlay", "Display Mode", "Change the display mode of ProBuilder actions in the overlay when using Panel mode")]
        internal static Pref<DisplayMode> s_CurrentMode = new Pref<DisplayMode>("pb_overlay.displaymode", DisplayMode.Full, SettingsScope.User);

        [UserSetting("Actions Overlay", "Display Editors", "Toggle the display of editors actions in the ProBuilder overlay")]
        static Pref<bool> s_DisplayEditors = new Pref<bool>("pb_overlay.displayeditors", true, SettingsScope.User);

        [UserSetting("Actions Overlay", "Display Selection", "Toggle the display of selection actions in the ProBuilder overlay")]
        static Pref<bool> s_DisplaySelection = new Pref<bool>("pb_overlay.displayselection", false, SettingsScope.User);

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
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            Selection.selectionChanged -= UpdateContent;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            ProBuilderEditor.selectionUpdated -= OnSelectionUpdated;
            ToolManager.activeContextChanged -= UpdateContent;
            rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
        }

        void UpdateContent()
        {
            RefreshAvailableActions();
            UpdateGrid();
            UpdateToolbar();
        }

        private void OnSelectModeChanged(SelectMode obj) => UpdateContent();

        private void OnSelectionUpdated(IEnumerable<ProBuilderMesh> obj) => UpdateContent();

        void OnGeometryChangedEvent(GeometryChangedEvent _)
        {
            UpdateGrid();
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            root.name = "ProbuilderActions";
            m_Grid = new GridView(m_AvailableActions, 0, 0, MakeItem, BindItem);
            m_ContentViewport = m_Grid.Q("unity-content-viewport");
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
                e.UpdateContent();
                e.style.flexGrow = 1f;
            }
        }

        private void RefreshAvailableActions()
        {
            m_AvailableActions.Clear();

            var initActionButtons = m_ActionButtons.Count == 0;

            if (!initActionButtons)
            {
                foreach (var element in m_ActionButtons)
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

                var shouldDisplayAsEditor = s_DisplayEditors && action.group == ToolbarGroup.Tool;
                var shouldDisplayAsSelection = s_DisplaySelection && action.group == ToolbarGroup.Selection;
                var shouldDisplay = action.group != ToolbarGroup.Tool && action.group != ToolbarGroup.Selection;

                var hidden = action.hidden;
                hidden |= (action.group == ToolbarGroup.Object) ? !isGOContext : isGOContext;

                if (initActionButtons)
                    m_ActionButtons.Add( new ProBuilderActionButton(action) );

                if (!hidden)
                {
                    if (shouldDisplayAsEditor || shouldDisplayAsSelection || shouldDisplay)
                    {
                        m_AvailableActions.Add(action);
                        m_ActionButtons[actionIndex].style.display = DisplayStyle.Flex;
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
                var fullWidth = float.IsFinite(m_ContentViewport.resolvedStyle.width) ?
                    Mathf.Clamp(m_ContentViewport.resolvedStyle.width - 10f, 0f, 180f) : 180f;
                m_Grid.fixedItemWidth = s_CurrentMode == DisplayMode.Icon ? 40f : fullWidth;
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
                    s_CurrentMode == DisplayMode.Icon ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                menu.AppendAction(L10n.Tr("Text Mode"), _ => { SetMode(DisplayMode.Text); },
                    s_CurrentMode == DisplayMode.Text ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                menu.AppendAction(L10n.Tr("Text & Icon Mode"), _ => { SetMode(DisplayMode.Full); },
                    s_CurrentMode == DisplayMode.Full ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                menu.AppendSeparator();
            }

            menu.AppendAction(L10n.Tr("Display Editors Actions"), _ =>
                {
                    s_DisplayEditors.SetValue(!s_DisplayEditors);
                    UpdateContent();
                },
                s_DisplayEditors ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            menu.AppendAction(L10n.Tr("Display Select Actions"), _ =>
                {
                    s_DisplaySelection.SetValue(!s_DisplaySelection);
                    UpdateContent();
                },
                s_DisplaySelection ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

        }

        private void SetMode(DisplayMode mode)
        {
            s_CurrentMode.SetValue(mode);
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
                foreach (var proBuilderAction in m_ActionButtons)
                {
                    proBuilderAction.UpdateContentForToolbar(layout);
                    m_Toolbar.Add(proBuilderAction);
                }
            }
        }

    }
}
