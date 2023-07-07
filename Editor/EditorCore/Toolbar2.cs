using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder.UI
{
    class ToolbarMenuItem : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ToolbarMenuItem, UxmlTraits> { }
        public MenuAction action;
    }

    class Toolbar2 : EditorWindow
    {
        const string k_IconMode = "ToolbarIcon";
        const string k_TextMode = "ToolbarLabel";
        const string k_UI = "Packages/com.unity.probuilder/Content/UI";

        [MenuItem("Window/Toolbar 2")]
        static void init() => GetWindow<Toolbar2>();

        List<ToolbarMenuItem> m_Actions = new List<ToolbarMenuItem>();

        public void CreateGUI()
        {
            Reload();
            ProBuilderEditor.selectModeChanged += _ => RefreshVisibility();
            MeshSelection.objectSelectionChanged += RefreshVisibility;
            ProBuilderMesh.elementSelectionChanged += _ => RefreshVisibility();
        }

        void RefreshVisibility()
        {
            foreach (var element in m_Actions)
            {
                var state = element.action.menuActionState;

                element.style.display = (state & MenuAction.MenuActionState.Visible) == MenuAction.MenuActionState.Visible
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                element.SetEnabled((state & MenuAction.MenuActionState.Enabled) == MenuAction.MenuActionState.Enabled);

                var options = element.Q<VisualElement>("Options");

                if (element.action.optionsVisible)
                {
                    options.style.display = DisplayStyle.Flex;
                    options.SetEnabled(element.action.optionsEnabled);
                }
                else
                {
                    options.style.display = DisplayStyle.None;
                }
            }
        }

        void Reload()
        {
            rootVisualElement.Clear();
            m_Actions.Clear();

            var iconMode = ProBuilderEditor.s_IsIconGui;

            var menuContentAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{k_UI}/{(iconMode ? k_IconMode : k_TextMode)}.uxml");
            var actions = EditorToolbarLoader.GetActions(true);

            VisualElement scrollContentsRoot = new ScrollView(ScrollViewMode.Vertical);
            rootVisualElement.Add(scrollContentsRoot);

            if (iconMode)
            {
                var container = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{k_UI}/ToolbarIconContainer.uxml");
                var contents = container.Instantiate();
                scrollContentsRoot.Add(contents);
                scrollContentsRoot = contents.Q<VisualElement>("IconRoot");
            }

            for(int i = 0, c = actions.Count; i < c; ++i)
            {
                var ui = menuContentAsset.Instantiate();
                var action = actions[i];

                var menu = ui.Q<ToolbarMenuItem>();
                menu.action = action;
                m_Actions.Add(menu);

                if( iconMode ? SetupIcon(menu, action) : SetupText(menu, action) )
                    scrollContentsRoot.Add(menu);
            }

            rootVisualElement.Add(new Button(Reload) { text = "reload" });

            RefreshVisibility();
        }

        static bool SetupIcon(VisualElement ui, MenuAction action)
        {
            if (action.icon == null)
                return false;
            var color = ToolbarGroupUtility.GetColor(action.group);
            var button = ui.Q<Button>("Button");
            button.style.borderLeftColor = color;
            button.tooltip = action.tooltip.summary;
            button.clicked += () => action.PerformAction();
            button.iconImage = action.icon;
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
            button.clicked += () => action.PerformAction();

            options.style.borderLeftColor = color;
            options.style.borderRightColor = color;
            options.style.borderBottomColor = color;
            options.style.borderTopColor = color;

            options.clicked += action.PerformAltAction;
            return true;
        }
    }
}
