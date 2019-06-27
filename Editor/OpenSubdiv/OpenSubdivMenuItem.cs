#if OPEN_SUBDIV_ENABLED

using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.OSD;

namespace UnityEditor.ProBuilder.OpenSubdiv
{
    [ProBuilderMenuAction]
    class OpenSubdivMenuItem : MenuAction
    {
		internal const int k_MinSubdivLevel = 0;
		internal const int k_MaxSubdivLevel = 5;

        static Pref<bool> s_ShowSubdivSettings = new Pref<bool>("OpenSubdivMenuItem.s_ShowSubdivSettings", false);

        static bool showSubdivSettings
        {
            get { return s_ShowSubdivSettings.value; }
            set
            {
                if (value != s_ShowSubdivSettings.value)
                    s_ShowSubdivSettings.SetValue(value, true);
            }
        }

        Editor m_Editor;

        public OpenSubdivMenuItem()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        ~OpenSubdivMenuItem()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        public override ToolbarGroup @group
        {
            get { return ToolbarGroup.Tool; }
        }

        public override Texture2D icon
        {
            get { return null; }
        }

        public override TooltipContent tooltip
        {
            get
            {
                if(showSubdivSettings)
                    return new TooltipContent("Open Subdiv: On", "Show the Open Subdiv settings window");

                return new TooltipContent("Open Subdiv: Off", "Show the Open Subdiv settings window");
            }
        }

        public override ActionResult DoAction()
        {
            showSubdivSettings = !showSubdivSettings;
            SceneView.RepaintAll();
            return new ActionResult(ActionResult.Status.Success, "Open Subdiv Settings");
        }

        void OnSceneGUI(SceneView view)
        {
            if(showSubdivSettings)
                SceneViewOverlay.Window(new GUIContent("Open Subdiv"), SettingsGUI, 10000, null, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle);
        }

        void SettingsGUI(Object target, SceneView view)
        {
            const int k_SettingsIconPad = 2;
            Vector2 settingsSize = EditorStyles.iconButton.CalcSize(EditorGUI.GUIContents.titleSettingsIcon);
            Rect settingsRect = new Rect(300 - 4 - k_SettingsIconPad - settingsSize.x, 4 + k_SettingsIconPad, settingsSize.x, settingsSize.y);

            if (GUI.Button(settingsRect, EditorGUI.GUIContents.titleSettingsIcon, EditorStyles.iconButton))
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Reset"), false, Reset);
                menu.AddItem(new GUIContent("Set Default"), false, SetDefault);
            }

            // this is terrible
            GUILayout.Label(GUIContent.none, GUIStyle.none, GUILayout.MinWidth(300));

            // this is less terrible, but we should avoid the ToArray
            Editor.CreateCachedEditor(MeshSelection.topInternal.ToArray(), typeof(OpenSubdivEditor), ref m_Editor);
            if(m_Editor)
                m_Editor.OnInspectorGUI();
        }

        static void Reset()
        {

        }

        static void SetDefault() { }
    }
}
#endif
