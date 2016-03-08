using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	public class pb_MenuOption : EditorWindow
	{
		[SerializeField] pb_MenuAction.SettingsDelegate onSettingsGUI = null;

		public static pb_MenuOption Show(pb_MenuAction.SettingsDelegate onSettingsGUI)
		{
			pb_MenuOption win = EditorWindow.GetWindow<pb_MenuOption>(true, "Options", true);
			win.onSettingsGUI = onSettingsGUI;
			win.Show();
			return win;
		}

		void OnGUI()
		{
			if(onSettingsGUI != null)
				onSettingsGUI();
		}
	}
}
