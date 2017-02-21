using UnityEngine;
using UnityEditor;

namespace ProBuilder2.EditorCommon
{
	class pb_UpdateAvailable : EditorWindow
	{
		public static void Init(pb_VersionInfo newVersion, string changelog)
		{
			pb_UpdateAvailable win = EditorWindow.GetWindow<pb_UpdateAvailable>(true, "ProBuilder Update Available", true);
			win.m_NewVersion = newVersion;
			win.m_NewChangelog = changelog;
		}

		[SerializeField] pb_VersionInfo m_NewVersion;
		[SerializeField] string m_NewChangelog;
		Vector2 scroll = Vector2.zero;

		void OnEnable()
		{
			pb_AboutWindow.InitGuiStyles();
		}

		void OnGUI()
		{
			GUILayout.Label("A new version of ProBuilder is available");

			// always bold the first line (cause it's the version info stuff)
			scroll = EditorGUILayout.BeginScrollView(scroll, pb_AboutWindow.changelogStyle);
			GUILayout.Label(string.Format("Version: {0}", m_NewVersion.text), pb_AboutWindow.versionInfoStyle);
			GUILayout.Label("\n" + m_NewChangelog, pb_AboutWindow.changelogTextStyle);
			EditorGUILayout.EndScrollView();
		}
	}
}
