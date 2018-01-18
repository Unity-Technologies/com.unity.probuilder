using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;

namespace ProBuilder.Interface
{
	/// <summary>
	/// Collection of commonly used styles in the editor.
	/// </summary>
	static class pb_EditorStyles
	{
		static readonly Color k_TextColorWhiteNormal = new Color(0.7f, 0.7f, 0.7f, 1f);
		static readonly Color k_TextColorWhiteHover = new Color(0.7f, 0.7f, 0.7f, 1f);
		static readonly Color k_TextColorWhiteActive = new Color(0.5f, 0.5f, 0.5f, 1f);

		static GUIStyle m_ButtonStyle = null;
		static GUIStyle m_ToolbarHelpIcon = null;
		static GUIStyle m_SettingsGroupStyle = null;
		static GUIStyle m_RowStyle = null;
		static GUIStyle m_HeaderLabel = null;
		static GUIStyle m_SceneTextBox = null;

		/// <summary>
		/// A generic menu button with no accent bar.
		/// </summary>
		public static GUIStyle buttonStyle
		{
			get
			{
				if(m_ButtonStyle == null)
				{
					m_ButtonStyle = new GUIStyle();
					m_ButtonStyle.normal.background = pb_IconUtility.GetIcon("Toolbar/Background/RoundedRect_Normal");
					m_ButtonStyle.normal.textColor = EditorGUIUtility.isProSkin ? k_TextColorWhiteNormal : Color.black;
					m_ButtonStyle.hover.background = pb_IconUtility.GetIcon("Toolbar/Background/RoundedRect_Hover");
					m_ButtonStyle.hover.textColor = EditorGUIUtility.isProSkin ? k_TextColorWhiteHover : Color.black;
					m_ButtonStyle.active.background = pb_IconUtility.GetIcon("Toolbar/Background/RoundedRect_Pressed");
					m_ButtonStyle.active.textColor = EditorGUIUtility.isProSkin ? k_TextColorWhiteActive : Color.black;
					m_ButtonStyle.alignment = pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI) ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
					m_ButtonStyle.border = new RectOffset(3, 3, 3, 3);
					m_ButtonStyle.stretchWidth = true;
					m_ButtonStyle.stretchHeight = false;
					m_ButtonStyle.margin = new RectOffset(4, 4, 4, 4);
					m_ButtonStyle.padding = new RectOffset(4, 4, 4, 4);
				}
				return m_ButtonStyle;
			}
		}

		public static GUIStyle toolbarHelpIcon
		{
			get
			{
				if (m_ToolbarHelpIcon == null)
				{
					m_ToolbarHelpIcon = new GUIStyle();
					m_ToolbarHelpIcon.margin = new RectOffset(0,0,0,0);
					m_ToolbarHelpIcon.padding = new RectOffset(0,0,0,0);
					m_ToolbarHelpIcon.alignment = TextAnchor.MiddleCenter;
					m_ToolbarHelpIcon.fixedWidth = 18;
					m_ToolbarHelpIcon.fixedHeight = 18;
				}
				return m_ToolbarHelpIcon;
			}
		}

		/// <summary>
		/// Box outline for a settings group.
		/// </summary>
		public static GUIStyle settingsGroup
		{
			get
			{
				if (m_SettingsGroupStyle == null)
				{
					m_SettingsGroupStyle = new GUIStyle();

					m_SettingsGroupStyle.normal.background 	= pb_IconUtility.GetIcon("Toolbar/RoundedBorder");
					m_SettingsGroupStyle.hover.background 	= pb_IconUtility.GetIcon("Toolbar/RoundedBorder");
					m_SettingsGroupStyle.active.background 	= pb_IconUtility.GetIcon("Toolbar/RoundedBorder");
					m_SettingsGroupStyle.border 			= new RectOffset(3,3,3,3);
					m_SettingsGroupStyle.stretchWidth 		= true;
					m_SettingsGroupStyle.stretchHeight 		= false;
					m_SettingsGroupStyle.margin 			= new RectOffset(4,4,4,4);
					m_SettingsGroupStyle.padding 			= new RectOffset(4,4,4,6);
				}

				return m_SettingsGroupStyle;
			}
		}

		public static GUIStyle rowStyle
		{
			get
			{
				if (m_RowStyle == null)
				{
					m_RowStyle = new GUIStyle();
					m_RowStyle.normal.background = EditorGUIUtility.whiteTexture;
					m_RowStyle.stretchWidth = true;
					m_RowStyle.stretchHeight = false;
					m_RowStyle.margin = new RectOffset(4,4,4,4);
					m_RowStyle.padding = new RectOffset(4,4,4,4);
				}
				return m_RowStyle;
			}
		}

		public static GUIStyle headerLabel
		{
			get
			{
				if (m_HeaderLabel == null)
				{
					m_HeaderLabel = new GUIStyle(EditorStyles.boldLabel);
					Font asap = pb_FileUtil.LoadInternalAsset<Font>("About/Font/Asap-Regular.otf");
					if(asap != null)
						m_HeaderLabel.font = asap;
					m_HeaderLabel.alignment = TextAnchor.LowerLeft;
					m_HeaderLabel.fontSize = 18;
					m_HeaderLabel.stretchWidth = true;
					m_HeaderLabel.stretchHeight = false;
				}

				return m_HeaderLabel;
			}
		}

		public static GUIStyle sceneTextBox
		{
			get
			{
				if (m_SceneTextBox == null)
				{
					m_SceneTextBox = new GUIStyle(GUI.skin.box);
					m_SceneTextBox.wordWrap = false;
					m_SceneTextBox.richText = true;
					m_SceneTextBox.stretchWidth = false;
					m_SceneTextBox.stretchHeight = false;
					m_SceneTextBox.border = new RectOffset(2,2,2,2);
					m_SceneTextBox.padding = new RectOffset(4,4,4,4);
					m_SceneTextBox.normal.textColor = k_TextColorWhiteNormal;
					m_SceneTextBox.alignment = TextAnchor.UpperLeft;
					m_SceneTextBox.normal.background = pb_IconUtility.GetIcon("Scene/TextBackground");
				}

				return m_SceneTextBox;
			}
		}
	}
}