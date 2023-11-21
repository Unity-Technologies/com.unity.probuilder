using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder.UI
{
    class ToolbarMenuItem : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ToolbarMenuItem, UxmlTraits> { }
        public MenuAction action;
        public bool iconMode;

        public void RefreshContents()
        {
            var state = action.menuActionState;
            var valid = iconMode ? SetupIcon(this, action) : SetupText(this, action);

            style.display = (state & MenuAction.MenuActionState.Visible) == MenuAction.MenuActionState.Visible && valid
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            SetEnabled((state & MenuAction.MenuActionState.Enabled) == MenuAction.MenuActionState.Enabled);

            var options = this.Q<VisualElement>("Options");

            if (action.optionsVisible)
            {
                options.style.display = DisplayStyle.Flex;
                options.SetEnabled(action.optionsEnabled);
            }
            else
            {
                options.style.display = DisplayStyle.None;
            }
        }

        static bool SetupIcon(VisualElement ui, MenuAction action)
        {
            if (action.icon == null)
                return false;
            var color = ToolbarGroupUtility.GetColor(action.group);
            var button = ui.Q<Button>("Button");
            button.style.borderLeftColor = color;
            button.tooltip = action.tooltip.summary;
            #if !UNITY_2023_2_OR_NEWER
            var icon = button.Q<Image>();
            if(icon == null)
                button.Add(icon = new Image() {name = "LegacyIcon" });
            icon.image = action.icon;
            #else
            button.iconImage = action.icon;
            #endif
            // todo context click opens options
            return true;
        }

        static bool SetupText(VisualElement ui, MenuAction action)
        {
            var color = ToolbarGroupUtility.GetColor(action.group);

            var button = ui.Q<Button>("Button");
            var label = button.Q<Label>("Label");
            var swatch = ui.Q<VisualElement>("CategorySwatch");
            var options = ui.Q<Button>("Options");

            swatch.style.backgroundColor = color;
            button.tooltip = action.tooltip.summary;
            label.text = action.menuTitle;

            options.style.borderLeftColor = color;
            options.style.borderRightColor = color;
            options.style.borderBottomColor = color;
            options.style.borderTopColor = color;

            return true;
        }
    }

    class ProBuilderToolbar : VisualElement
    {
        const string k_IconMode = "ToolbarIcon";
        const string k_TextMode = "ToolbarLabel";
        const string k_UI = "Packages/com.unity.probuilder/Content/UI";
        const string k_USS_Common = k_UI + "/ToolbarCommon.uss";
        const string k_USS_Light = k_UI + "/ToolbarLight.uss";
        const string k_USS_Dark = k_UI + "/ToolbarDark.uss";

        readonly List<ToolbarMenuItem> m_Actions = new List<ToolbarMenuItem>();

        bool m_IconMode, m_Horizontal;

        public bool iconMode => m_IconMode;

        public bool horizontalMode => m_Horizontal;

        public ProBuilderToolbar(bool iconMode, bool horizontal)
        {
            m_IconMode = iconMode;
            m_Horizontal = horizontal;

            CreateGUI();

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                ProBuilderEditor.selectModeChanged += RefreshVisibility;
                MeshSelection.objectSelectionChanged += RefreshVisibility;
                ProBuilderMesh.elementSelectionChanged += RefreshVisibility;
            });

            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                ProBuilderEditor.selectModeChanged -= RefreshVisibility;
                MeshSelection.objectSelectionChanged -= RefreshVisibility;
                ProBuilderMesh.elementSelectionChanged -= RefreshVisibility;
            });
        }

        void RefreshVisibility(ProBuilderMesh obj) => RefreshVisibility();

        void RefreshVisibility(SelectMode obj) => RefreshVisibility();

        void RefreshVisibility()
        {
            foreach (var element in m_Actions)
                element.RefreshContents();
        }

        public void CreateGUI()
        {
            m_Actions.Clear();

            var menuContentAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{k_UI}/{(iconMode ? k_IconMode : k_TextMode)}.uxml");
            var actions = EditorToolbarLoader.GetActions(true);

            VisualElement scrollContentsRoot = new ScrollView(m_Horizontal ? ScrollViewMode.Horizontal : ScrollViewMode.Vertical);
            Add(scrollContentsRoot);

            if (iconMode)
            {
                if (m_Horizontal)
                {
                    var container = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{k_UI}/ToolbarIconContainer.uxml");
#if UNITY_2021_3_OR_NEWER
                    var contents = container.Instantiate();
#else
                    var contents = new VisualElement();
                    container.CloneTree(contents);
#endif
                    scrollContentsRoot.Add(contents);
                    scrollContentsRoot = contents.Q<VisualElement>("IconRoot");
                }
                else
                {
                    scrollContentsRoot.contentContainer.AddToClassList("icon-container-vertical");
                }
            }

            var common = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_USS_Common);
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(UnityEditor.EditorGUIUtility.isProSkin
                ? k_USS_Dark
                : k_USS_Light);

            scrollContentsRoot.styleSheets.Add(common);
            scrollContentsRoot.styleSheets.Add(uss);

            for(int i = 0, c = actions.Count; i < c; ++i)
            {
#if UNITY_2021_3_OR_NEWER
                var menu = menuContentAsset.Instantiate().Q<ToolbarMenuItem>();
#else
                var root = new VisualElement();
                menuContentAsset.CloneTree(root);
                var menu = root.Q<ToolbarMenuItem>();
#endif
                var action = actions[i];

                menu.iconMode = iconMode;
                menu.action = action;
                action.changed += menu.RefreshContents;
                action.RegisterChangedCallbacks();
                menu.RegisterCallback<DetachFromPanelEvent>(_ => action.UnregisterChangedCallbacks());
                menu.RefreshContents();

                var button = menu.Q<Button>("Button");
                button.clicked += () => action.PerformAction();

                if (iconMode)
                {
                    button.AddManipulator(new ContextClickManipulator(action.PerformAltAction));
                }
                else
                {
                    var options = menu.Q<Button>("Options");
                    options.clicked += action.PerformAltAction;
                }

                m_Actions.Add(menu);
                scrollContentsRoot.Add(menu);
            }

            RefreshVisibility();
        }

    }
}
