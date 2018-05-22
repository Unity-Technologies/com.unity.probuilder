using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.ProBuilder.UI
{
	/// <summary>
	/// Collection of commonly used styles in the editor.
	/// </summary>
	static class EditorStyles
	{
		static readonly Color k_TextColorWhiteNormal = new Color(0.7f, 0.7f, 0.7f, 1f);
		static readonly Color k_TextColorWhiteHover = new Color(0.7f, 0.7f, 0.7f, 1f);
		static readonly Color k_TextColorWhiteActive = new Color(0.5f, 0.5f, 0.5f, 1f);

		static GUIStyle s_ButtonStyle = null;
		static GUIStyle s_ToolbarHelpIcon = null;
		static GUIStyle s_SettingsGroupStyle = null;
		static GUIStyle s_RowStyle = null;
		static GUIStyle s_HeaderLabel = null;
		static GUIStyle s_SceneTextBox = null;

		/// <summary>
		/// A generic menu button with no accent bar.
		/// </summary>
		public static GUIStyle buttonStyle
		{
			get
			{
				if(s_ButtonStyle == null)
				{
					s_ButtonStyle = new GUIStyle();
					s_ButtonStyle.normal.background = IconUtility.GetIcon("Toolbar/Background/RoundedRect_Normal");
					s_ButtonStyle.normal.textColor = UnityEditor.EditorGUIUtility.isProSkin ? k_TextColorWhiteNormal : Color.black;
					s_ButtonStyle.hover.background = IconUtility.GetIcon("Toolbar/Background/RoundedRect_Hover");
					s_ButtonStyle.hover.textColor = UnityEditor.EditorGUIUtility.isProSkin ? k_TextColorWhiteHover : Color.black;
					s_ButtonStyle.active.background = IconUtility.GetIcon("Toolbar/Background/RoundedRect_Pressed");
					s_ButtonStyle.active.textColor = UnityEditor.EditorGUIUtility.isProSkin ? k_TextColorWhiteActive : Color.black;
					s_ButtonStyle.alignment = PreferencesInternal.GetBool(PreferenceKeys.pbIconGUI) ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
					s_ButtonStyle.border = new RectOffset(3, 3, 3, 3);
					s_ButtonStyle.stretchWidth = true;
					s_ButtonStyle.stretchHeight = false;
					s_ButtonStyle.margin = new RectOffset(4, 4, 4, 4);
					s_ButtonStyle.padding = new RectOffset(4, 4, 4, 4);
				}
				return s_ButtonStyle;
			}
		}

		public static GUIStyle toolbarHelpIcon
		{
			get
			{
				if (s_ToolbarHelpIcon == null)
				{
					s_ToolbarHelpIcon = new GUIStyle();
					s_ToolbarHelpIcon.margin = new RectOffset(0,0,0,0);
					s_ToolbarHelpIcon.padding = new RectOffset(0,0,0,0);
					s_ToolbarHelpIcon.alignment = TextAnchor.MiddleCenter;
					s_ToolbarHelpIcon.fixedWidth = 18;
					s_ToolbarHelpIcon.fixedHeight = 18;
				}
				return s_ToolbarHelpIcon;
			}
		}

		/// <summary>
		/// Box outline for a settings group.
		/// </summary>
		public static GUIStyle settingsGroup
		{
			get
			{
				if (s_SettingsGroupStyle == null)
				{
					s_SettingsGroupStyle = new GUIStyle();

					s_SettingsGroupStyle.normal.background 	= IconUtility.GetIcon("Toolbar/RoundedBorder");
					s_SettingsGroupStyle.hover.background 	= IconUtility.GetIcon("Toolbar/RoundedBorder");
					s_SettingsGroupStyle.active.background 	= IconUtility.GetIcon("Toolbar/RoundedBorder");
					s_SettingsGroupStyle.border 			= new RectOffset(3,3,3,3);
					s_SettingsGroupStyle.stretchWidth 		= true;
					s_SettingsGroupStyle.stretchHeight 		= false;
					s_SettingsGroupStyle.margin 			= new RectOffset(4,4,4,4);
					s_SettingsGroupStyle.padding 			= new RectOffset(4,4,4,6);
				}

				return s_SettingsGroupStyle;
			}
		}

		public static GUIStyle rowStyle
		{
			get
			{
				if (s_RowStyle == null)
				{
					s_RowStyle = new GUIStyle();
					s_RowStyle.normal.background = UnityEditor.EditorGUIUtility.whiteTexture;
					s_RowStyle.stretchWidth = true;
					s_RowStyle.stretchHeight = false;
					s_RowStyle.margin = new RectOffset(4,4,4,4);
					s_RowStyle.padding = new RectOffset(4,4,4,4);
				}
				return s_RowStyle;
			}
		}

		public static GUIStyle headerLabel
		{
			get
			{
				if (s_HeaderLabel == null)
				{
					s_HeaderLabel = new GUIStyle(UnityEditor.EditorStyles.boldLabel);
					Font asap = FileUtility.LoadInternalAsset<Font>("About/Font/Asap-Regular.otf");
					if(asap != null)
						s_HeaderLabel.font = asap;
					s_HeaderLabel.alignment = TextAnchor.LowerLeft;
					s_HeaderLabel.fontSize = 18;
					s_HeaderLabel.stretchWidth = true;
					s_HeaderLabel.stretchHeight = false;
				}

				return s_HeaderLabel;
			}
		}

		public static GUIStyle sceneTextBox
		{
			get
			{
				if (s_SceneTextBox == null)
				{
					s_SceneTextBox = new GUIStyle(GUI.skin.box);
					s_SceneTextBox.wordWrap = false;
					s_SceneTextBox.richText = true;
					s_SceneTextBox.stretchWidth = false;
					s_SceneTextBox.stretchHeight = false;
					s_SceneTextBox.border = new RectOffset(2,2,2,2);
					s_SceneTextBox.padding = new RectOffset(4,4,4,4);
					s_SceneTextBox.normal.textColor = k_TextColorWhiteNormal;
					s_SceneTextBox.alignment = TextAnchor.UpperLeft;
					s_SceneTextBox.normal.background = IconUtility.GetIcon("Scene/TextBackground");
				}

				return s_SceneTextBox;
			}
		}
	}
}
