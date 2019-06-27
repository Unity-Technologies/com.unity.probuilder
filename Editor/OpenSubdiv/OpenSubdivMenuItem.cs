#if OPEN_SUBDIV_ENABLED
using UnityEditor.ShortcutManagement;
using System.Linq;
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

        [Shortcut("ProBuilder/Toggle Object Subdivision", typeof(SceneView), KeyCode.S, ShortcutModifiers.None)]
        static void ToggleSubdivideEnabled()
        {
            var anyNotEnabled = MeshSelection.topInternal.Any(x => !x.subdivisionEnabled);

            foreach (var mesh in MeshSelection.topInternal)
            {
                mesh.subdivisionEnabled = anyNotEnabled;
                mesh.Rebuild();
            }

            SceneView.RepaintAll();
        }

        [Shortcut("ProBuilder/Toggle Subdivision Enabled", typeof(SceneView), KeyCode.S, ShortcutModifiers.Alt)]
        static void ToggleGlobalSubdivideEnabled()
        {
                SetSmoothingVisibility(!ProBuilderMesh.globalEnableSubdivide);
        }

        static void SetSmoothingVisibility(bool visible)
        {
            ProBuilderMesh.globalEnableSubdivide = visible;

            foreach (var mesh in Object.FindObjectsOfType<ProBuilderMesh>())
                mesh.Rebuild();

            SceneView.RepaintAll();
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
            using (new EditorGUI.DisabledScope(MeshSelection.selectedObjectCount < 1))
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUI.GUIContents.titleSettingsIcon, EditorStyles.iconButton))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Reset"), false, Reset);
                    menu.AddItem(new GUIContent("Set Default"), false, SetDefault);
                    menu.ShowAsContext();
                }

                GUILayout.EndHorizontal();
            }

            EditorGUI.BeginChangeCheck();
            ProBuilderMesh.globalEnableSubdivide = EditorGUILayout.Toggle("Smoothing Visibility", ProBuilderMesh.globalEnableSubdivide);
            if (EditorGUI.EndChangeCheck())
                SetSmoothingVisibility(ProBuilderMesh.globalEnableSubdivide);

            using (new EditorGUI.DisabledScope(MeshSelection.selectedObjectCount < 1))
            {
                // this is terrible
                GUILayout.Label(GUIContent.none, GUIStyle.none, GUILayout.MinWidth(300));

                // this is less terrible, but we should avoid the ToArray
                Editor.CreateCachedEditor(MeshSelection.topInternal.ToArray(), typeof(OpenSubdivEditor), ref m_Editor);
                if (m_Editor)
                    m_Editor.OnInspectorGUI();
            }
        }

        static void Reset()
        {
            foreach(var mesh in MeshSelection.topInternal)
                mesh.subdivisionSettings = SubdivisionSettings.defaultSettings;

            foreach(var mesh in MeshSelection.topInternal)
                mesh.subdivisionEnabled = false;
        }

        static void SetDefault()
        {
            EditorUtility.s_SubdivisionEnabled.SetValue(MeshSelection.activeMesh.subdivisionEnabled, true);
            EditorUtility.s_SubdivisionSettings.SetValue(MeshSelection.activeMesh.subdivisionSettings, true);
        }
    }
}
#endif
