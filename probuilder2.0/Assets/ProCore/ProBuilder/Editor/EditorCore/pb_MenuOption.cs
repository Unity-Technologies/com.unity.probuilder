using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Options menu window container. Do not instantiate this yourself, the toolbar will handle opening option windows.
	/// </summary>
	public class pb_MenuOption : EditorWindow
	{
		[SerializeField] pb_MenuAction.SettingsDelegate onSettingsGUI = null;
		[SerializeField] pb_MenuAction.SettingsDelegate onSettingsDisable = null;

		internal static pb_MenuOption Show(pb_MenuAction.SettingsDelegate onSettingsGUI, pb_MenuAction.SettingsDelegate onSettingsEnable, pb_MenuAction.SettingsDelegate onSettingsDisable)
		{
			pb_MenuOption win = EditorWindow.GetWindow<pb_MenuOption>(true, "Options", true);
			win.hideFlags = HideFlags.HideAndDontSave;

			if(win.onSettingsDisable != null)
				win.onSettingsDisable();

			if(onSettingsEnable != null)
				onSettingsEnable();

			win.onSettingsDisable = onSettingsDisable;

			win.onSettingsGUI = onSettingsGUI;

			// don't let window hang around after a script reload nukes the pb_MenuAction instances
			object parent = pb_Reflection.GetValue(win, typeof(EditorWindow), "m_Parent");
			object window = pb_Reflection.GetValue(parent, typeof(EditorWindow), "window");
			pb_Reflection.SetValue(parent, "mouseRayInvisible", true);
			pb_Reflection.SetValue(window, "m_DontSaveToLayout", true);

			win.Show();

			return win;
		}

		/// <summary>
		/// Close any currently open option windows.
		/// </summary>
		public static void CloseAll()
		{
			foreach(pb_MenuOption win in Resources.FindObjectsOfTypeAll<pb_MenuOption>())
				win.Close();
		}

		void OnEnable()
		{
			autoRepaintOnSceneChange = true;
		}

		void OnDisable()
		{
			if(onSettingsDisable != null)
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
			if(onSettingsGUI != null)
			{
				onSettingsGUI();
			}
			else if(Event.current.type == EventType.Repaint)
			{
				EditorApplication.delayCall += CloseAll;
				GUIUtility.ExitGUI();
			}
		}
	}
}
