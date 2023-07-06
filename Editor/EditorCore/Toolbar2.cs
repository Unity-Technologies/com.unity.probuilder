using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder.UI
{
    class ToolbarLabel : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ToolbarLabel, UxmlTraits> { }

        public MenuAction action;
    }

    class Toolbar2 : EditorWindow
    {
        const string k_UI = "Packages/com.unity.probuilder/Content/UI";

        [MenuItem("Window/Toolbar 2")]
        static void init() => GetWindow<Toolbar2>();

        List<ToolbarLabel> m_Actions = new List<ToolbarLabel>();

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

                if (element.action.optionsVisible)
                {
                    element.Q<Button>("Options").style.display = DisplayStyle.Flex;
                    element.Q<Button>("Options").SetEnabled(element.action.optionsEnabled);
                }
                else
                    element.Q<Button>("Options").style.display = DisplayStyle.None;
            }
        }

        void Reload()
        {
            rootVisualElement.Clear();
            m_Actions.Clear();

            VisualTreeAsset textLabelAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{k_UI}/ToolbarLabel.uxml");
            var actions = EditorToolbarLoader.GetActions(true);
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            rootVisualElement.Add(scroll);

            for(int i = 0, c = actions.Count; i < c; ++i)
            {
                var ui = textLabelAsset.Instantiate();

                var action = actions[i];
                var color = ToolbarGroupUtility.GetColor(action.group);

                var menu = ui.Q<ToolbarLabel>();
                menu.action = action;
                m_Actions.Add(menu);

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

                scroll.Add(ui);
            }

            rootVisualElement.Add(new Button(Reload) { text = "reload" });

            RefreshVisibility();
        }
    }
}
