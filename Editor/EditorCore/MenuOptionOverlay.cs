using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using UnityEditor.Overlays;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Options menu overlay. Do not instantiate this yourself, the toolbar will handle opening option windows.
    /// </summary>
    [Overlay(typeof(SceneView), k_Name, true)]
    sealed class MenuOptionOverlay : Overlay
    {
        public static MenuOptionOverlay instance;
        const string k_Name = "Options";

        string k_UxmlPath = "Packages/com.unity.probuilder/Editor/Overlays/UXML/menu-option.uxml";
        static VisualTreeAsset s_TreeAsset;

        VisualElement m_RootVisualElement = new VisualElement();

        System.Action onSettingsGUI = null;
        System.Action onSettingsDisable = null;

        public override void OnCreated()
        {
            m_HasMenuEntry = false;

            if(s_TreeAsset == null)
                s_TreeAsset = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath(k_UxmlPath, typeof(VisualTreeAsset));

            if(s_TreeAsset != null)
                s_TreeAsset.CloneTree(m_RootVisualElement);

            instance = this;
        }

        public override VisualElement CreatePanelContent()
        {
            return m_RootVisualElement;
        }

        internal static MenuOptionOverlay Show(VisualElement panelContent, System.Action onSettingsGUI, System.Action onSettingsEnable, System.Action onSettingsDisable)
        {
            if (instance.onSettingsDisable != null)
                instance.onSettingsDisable();

            if (onSettingsEnable != null)
                onSettingsEnable();

            instance.onSettingsDisable = onSettingsDisable;
            instance.onSettingsGUI = onSettingsGUI;

            // TODO: handle commented out code
            /*
            // don't let window hang around after a script reload nukes the pb_MenuAction instances
            object parent = ReflectionUtility.GetValue(win, typeof(EditorWindow), "m_Parent");
            object window = ReflectionUtility.GetValue(parent, typeof(EditorWindow), "window");
            ReflectionUtility.SetValue(parent, "mouseRayInvisible", true);
            ReflectionUtility.SetValue(window, "m_DontSaveToLayout", true);
            */

            instance.Show(panelContent);
            return instance;
        }

        void Show(VisualElement panelContent)
        {
            var imguiContainer = m_RootVisualElement.Q<IMGUIContainer>();
            imguiContainer.onGUIHandler = onSettingsGUI;

            m_RootVisualElement.Add(panelContent);

            layout = Layout.Panel;
            displayed = true;
        }

        void Hide()
        {
            displayed = false;
        }

        public override void OnWillBeDestroyed()
        {
            Hide();
        }

        // TODO: handle commented out code
        /*

        /// <summary>
        /// Close any currently open option windows.
        /// </summary>
        public static void CloseAll()
        {
            foreach (MenuOption win in Resources.FindObjectsOfTypeAll<MenuOption>())
                win.Close();
        }

        void OnEnable()
        {
            autoRepaintOnSceneChange = true;
        }

        void OnDisable()
        {
            if (onSettingsDisable != null)
                onSettingsDisable();
        }

        void OnSelectionChange()
        {
            Repaint();
        }

        void OnHierarchyChange()
        {
            Repaint();
        }

        void OnGUI()
        {
            if (onSettingsGUI != null)
            {
                onSettingsGUI();
            }
            else if (Event.current.type == EventType.Repaint)
            {
                EditorApplication.delayCall += CloseAll;
                GUIUtility.ExitGUI();
            }
        }*/
    }
}
