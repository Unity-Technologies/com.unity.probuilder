// #define GENERATE_DESATURATED_ICONS

using ProBuilder.Core;
using UnityEngine;
using UnityEditor;

namespace ProBuilder.EditorCore
{
	/**
	 *	Styles used in pb_MenuAction.
	 */
	public static class pb_MenuActionStyles
	{
		internal static readonly Color TEXT_COLOR_WHITE_NORMAL = new Color(0.82f, 0.82f, 0.82f, 1f);
		internal static readonly Color TEXT_COLOR_WHITE_HOVER = new Color(0.7f, 0.7f, 0.7f, 1f);
		internal static readonly Color TEXT_COLOR_WHITE_ACTIVE = new Color(0.5f, 0.5f, 0.5f, 1f);

#if PROTOTYPE
		private static Color _proOnlyTintLight = new Color(0f, .5f, 1f, 1f);
		private static Color _proOnlyTintDark = new Color(.25f, 1f, 1f, 1f);
		private static Color ProOnlyTint
		{
			get { return EditorGUIUtility.isProSkin ? _proOnlyTintDark : _proOnlyTintLight; }
		}
#endif

		/**
		 *	Reset static GUIStyle objects so that they will be re-initialized the next time used.
		 */
		public static void ResetStyles()
		{
			m_ButtonStyleVertical = null;
			m_ButtonStyleHorizontal = null;
			m_RowStyleVertical = null;
			m_AltButtonStyle = null;
			m_ProOnlyStyle = null;
#if PROTOTYPE
			m_AdvancedOnlyStyle = null;
#endif
		}

		private static GUIStyle m_ButtonStyleVertical = null;

		public static GUIStyle buttonStyleVertical
		{
			get
			{
				if(m_ButtonStyleVertical == null)
				{
					m_ButtonStyleVertical = new GUIStyle();
					m_ButtonStyleVertical.normal.background = pb_IconUtility.GetIcon("Toolbar/Button_Normal", IconSkin.Pro);
					m_ButtonStyleVertical.normal.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_NORMAL : Color.black;
					m_ButtonStyleVertical.hover.background = pb_IconUtility.GetIcon("Toolbar/Button_Hover", IconSkin.Pro);
					m_ButtonStyleVertical.hover.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_HOVER : Color.black;
					m_ButtonStyleVertical.active.background = pb_IconUtility.GetIcon("Toolbar/Button_Pressed", IconSkin.Pro);
					m_ButtonStyleVertical.active.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_ACTIVE : Color.black;
					m_ButtonStyleVertical.alignment = pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI) ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
					m_ButtonStyleVertical.border = new RectOffset(4,0,0,0);
					m_ButtonStyleVertical.stretchWidth = true;
					m_ButtonStyleVertical.stretchHeight = false;
					m_ButtonStyleVertical.margin = new RectOffset(4,5,4,4);
					m_ButtonStyleVertical.padding = new RectOffset(8,0,2,2);
				}
				return m_ButtonStyleVertical;
			}
		}

		private static GUIStyle m_ButtonStyleHorizontal = null;

		public static GUIStyle buttonStyleHorizontal
		{
			get
			{
				if(m_ButtonStyleHorizontal == null)
				{
					m_ButtonStyleHorizontal = new GUIStyle();

					m_ButtonStyleHorizontal.normal.textColor 	= EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_NORMAL : Color.black;
					m_ButtonStyleHorizontal.normal.background 	= pb_IconUtility.GetIcon("Toolbar/Button_Normal_Horizontal", IconSkin.Pro);
					m_ButtonStyleHorizontal.hover.background 	= pb_IconUtility.GetIcon("Toolbar/Button_Hover_Horizontal", IconSkin.Pro);
					m_ButtonStyleHorizontal.hover.textColor 		= EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_HOVER : Color.black;
					m_ButtonStyleHorizontal.active.background 	= pb_IconUtility.GetIcon("Toolbar/Button_Pressed_Horizontal", IconSkin.Pro);
					m_ButtonStyleHorizontal.active.textColor 	= EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_ACTIVE : Color.black;
					m_ButtonStyleHorizontal.alignment 			= TextAnchor.MiddleCenter;
					m_ButtonStyleHorizontal.border 				= new RectOffset(0,0,4,0);
					m_ButtonStyleHorizontal.stretchWidth 		= true;
					m_ButtonStyleHorizontal.stretchHeight 		= false;
					m_ButtonStyleHorizontal.margin 				= new RectOffset(4,4,4,5);
					m_ButtonStyleHorizontal.padding 				= new RectOffset(2,2,8,0);
				}
				return m_ButtonStyleHorizontal;
			}
		}

		private static GUIStyle m_RowStyleVertical = null;

		public static GUIStyle rowStyleVertical
		{
			get
			{
				if(m_RowStyleVertical == null)
				{
					m_RowStyleVertical = new GUIStyle();
					m_RowStyleVertical.alignment = TextAnchor.MiddleLeft;
					m_RowStyleVertical.stretchWidth = true;
					m_RowStyleVertical.stretchHeight = false;
					m_RowStyleVertical.margin = new RectOffset(0,0,0,0);
					m_RowStyleVertical.padding = new RectOffset(0,0,0,0);
				}
				return m_RowStyleVertical;
			}
		}

		private static GUIStyle m_RowStyleHorizontal = null;

		public static GUIStyle rowStyleHorizontal
		{
			get
			{
				if(m_RowStyleHorizontal == null)
				{
					m_RowStyleHorizontal = new GUIStyle();
					m_RowStyleHorizontal.alignment = TextAnchor.MiddleCenter;
					m_RowStyleHorizontal.stretchWidth = true;
					m_RowStyleHorizontal.stretchHeight = false;
					m_RowStyleHorizontal.margin = new RectOffset(0,0,0,0);
					m_RowStyleHorizontal.padding = new RectOffset(0,0,0,0);
				}
				return m_RowStyleHorizontal;
			}
		}

		private static GUIStyle m_AltButtonStyle = null;

		public static GUIStyle altButtonStyle
		{
			get
			{
				if(m_AltButtonStyle == null)
				{
					m_AltButtonStyle = new GUIStyle();

					m_AltButtonStyle.normal.background 	= pb_IconUtility.GetIcon("Toolbar/AltButton_Normal", IconSkin.Pro);
					m_AltButtonStyle.normal.textColor 	= EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_NORMAL : Color.black;
					m_AltButtonStyle.hover.background 	= pb_IconUtility.GetIcon("Toolbar/AltButton_Hover", IconSkin.Pro);
					m_AltButtonStyle.hover.textColor 	= EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_HOVER : Color.black;
					m_AltButtonStyle.active.background 	= pb_IconUtility.GetIcon("Toolbar/AltButton_Pressed", IconSkin.Pro);
					m_AltButtonStyle.active.textColor 	= EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_ACTIVE : Color.black;
					m_AltButtonStyle.alignment 			= TextAnchor.MiddleCenter;
					m_AltButtonStyle.border 				= new RectOffset(1,1,1,1);
					m_AltButtonStyle.stretchWidth 		= false;
					m_AltButtonStyle.stretchHeight 		= true;
					m_AltButtonStyle.margin 				= new RectOffset(4,4,4,4);
					m_AltButtonStyle.padding 			= new RectOffset(2,2,1,3);
				}
				return m_AltButtonStyle;
			}
		}

		private static GUIStyle m_ProOnlyStyle = null;

		public static GUIStyle proOnlyStyle
		{
			get
			{
				if(m_ProOnlyStyle == null)
				{
					m_ProOnlyStyle = new GUIStyle(EditorStyles.label);
					m_ProOnlyStyle.normal.background = pb_IconUtility.GetIcon("Toolbar/ProOnly", IconSkin.Pro);
					m_ProOnlyStyle.hover.background 	= pb_IconUtility.GetIcon("Toolbar/ProOnly", IconSkin.Pro);
					m_ProOnlyStyle.active.background = pb_IconUtility.GetIcon("Toolbar/ProOnly", IconSkin.Pro);
				}
				return m_ProOnlyStyle;
			}
		}

#if PROTOTYPE
		private static GUIStyle m_AdvancedOnlyStyle = null;

		public static GUIStyle advancedOnlyStyle
		{
			get
			{
				if(m_AdvancedOnlyStyle == null)
				{
					m_AdvancedOnlyStyle = new GUIStyle();
					m_AdvancedOnlyStyle.normal.textColor = ProOnlyTint;
					m_AdvancedOnlyStyle.hover.textColor = ProOnlyTint;
					m_AdvancedOnlyStyle.active.textColor = ProOnlyTint;
					m_AdvancedOnlyStyle.alignment = TextAnchor.MiddleCenter;
					m_AdvancedOnlyStyle.margin = new RectOffset(4,4,4,4);
					m_AdvancedOnlyStyle.padding = new RectOffset(2,2,2,2);
				}
				return m_AdvancedOnlyStyle;
			}
		}
#endif

		private static Texture2D m_ProOnlyIcon = null;

		public static Texture2D proOnlyIcon
		{
			get
			{
				if(m_ProOnlyIcon == null)
					m_ProOnlyIcon = pb_IconUtility.GetIcon("Toolbar/ProOnly");
				return m_ProOnlyIcon;
			}
		}
	}
}
