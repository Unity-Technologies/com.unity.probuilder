using System;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.UIElements;

namespace Editor.EditorCore
{
    class Toolbar2 : EditorWindow
    {
        const string k_UI = "Packages/com.unity.probuilder/Content/UI";

        [MenuItem("Window/Toolbar 2")]
        static void init() => GetWindow<Toolbar2>();

        public void CreateGUI()
        {
            Reload();
        }

        void Reload()
        {
            rootVisualElement.Clear();
            VisualTreeAsset textLabelAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{k_UI}/ToolbarLabel.uxml");
            var actions = EditorToolbarLoader.GetActions(true);
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            rootVisualElement.Add(scroll);

            for(int i = 0, c= actions.Count; i < c; ++i)
            {
                var action = actions[i];
                var color = ToolbarGroupUtility.GetColor(action.group);

                VisualElement ui = textLabelAsset.Instantiate();
                var button = ui.Q<Button>("Button");
                var label = button.Q<Label>("Label");
                var swatch = ui.Q<VisualElement>("CategorySwatch");
                var options = ui.Q<Button>("Options");

                swatch.style.backgroundColor = color;
                label.text = action.menuTitle;
                button.clicked += () => action.PerformAction();
                options.style.borderLeftColor = color;
                options.style.borderRightColor = color;
                options.style.borderBottomColor = color;
                options.style.borderTopColor = color;
                scroll.Add(ui);
            }

            rootVisualElement.Add(new Button(Reload) { text = "reload" });
        }
    }
}
