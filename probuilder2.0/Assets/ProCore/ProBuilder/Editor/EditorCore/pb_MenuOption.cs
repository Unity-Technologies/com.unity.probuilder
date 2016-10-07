using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Options menu window container. 
	 */
	public class pb_MenuOption : EditorWindow
	{
		[SerializeField] pb_MenuAction.SettingsDelegate onSettingsGUI = null;

		public static pb_MenuOption Show(pb_MenuAction.SettingsDelegate onSettingsGUI)
		{
			pb_MenuOption win = EditorWindow.GetWindow<pb_MenuOption>(true, "Options", true);
			win.hideFlags = HideFlags.HideAndDontSave;
			win.onSettingsGUI = onSettingsGUI;
			
			// don't let window hang around after a script reload nukes the pb_MenuAction instances
			object parent = pb_Reflection.GetValue(win, typeof(EditorWindow), "m_Parent");
			object window = pb_Reflection.GetValue(parent, typeof(EditorWindow), "window");
			pb_Reflection.SetValue(parent, "mouseRayInvisible", true);
			pb_Reflection.SetValue(window, "m_DontSaveToLayout", true);

			win.Show();

			return win;
		}

		public static void CloseAll()
		{
			foreach(pb_MenuOption win in Resources.FindObjectsOfTypeAll<pb_MenuOption>())
				win.Close();
		}

		void OnEnable()
		{
			this.autoRepaintOnSceneChange = true;
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
			if(onSettingsGUI != null)
			{
				onSettingsGUI();
			}
			else if(Event.current.type == EventType.Repaint)
			{
				EditorApplication.delayCall += () => { pb_MenuOption.CloseAll(); };
				GUIUtility.ExitGUI();
			}
		}
	}
}
