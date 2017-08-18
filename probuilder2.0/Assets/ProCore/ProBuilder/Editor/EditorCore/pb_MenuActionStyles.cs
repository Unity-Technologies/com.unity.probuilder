// #define GENERATE_DESATURATED_ICONS

using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Styles used in pb_MenuAction.
	 */
	public static class pb_MenuActionStyles
	{
		private static readonly Color TEXT_COLOR_WHITE_NORMAL = new Color(0.82f, 0.82f, 0.82f, 1f);
		private static readonly Color TEXT_COLOR_WHITE_HOVER = new Color(0.7f, 0.7f, 0.7f, 1f);
		private static readonly Color TEXT_COLOR_WHITE_ACTIVE = new Color(0.5f, 0.5f, 0.5f, 1f);

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
			_buttonStyleVertical = null;
			_buttonStyleHorizontal = null;
			_rowStyleVertical = null;
			_altButtonStyle = null;
			_proOnlyStyle = null;
#if PROTOTYPE
			_advancedOnlyStyle = null;
#endif
		}

		private static GUIStyle _buttonStyleVertical = null;
		public static GUIStyle buttonStyleVertical
		{
			get
			{
				if(_buttonStyleVertical == null)
				{
					_buttonStyleVertical = new GUIStyle();
					_buttonStyleVertical.normal.background = pb_IconUtility.GetIcon("Toolbar/Button_Normal", IconSkin.Pro);
					_buttonStyleVertical.normal.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_NORMAL : Color.black;
					_buttonStyleVertical.hover.background = pb_IconUtility.GetIcon("Toolbar/Button_Hover", IconSkin.Pro);
					_buttonStyleVertical.hover.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_HOVER : Color.black;
					_buttonStyleVertical.active.background = pb_IconUtility.GetIcon("Toolbar/Button_Pressed", IconSkin.Pro);
					_buttonStyleVertical.active.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_ACTIVE : Color.black;
					_buttonStyleVertical.alignment = pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI) ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
					_buttonStyleVertical.border = new RectOffset(4,0,0,0);
					_buttonStyleVertical.stretchWidth = true;
					_buttonStyleVertical.stretchHeight = false;
					_buttonStyleVertical.margin = new RectOffset(4,5,4,4);
					_buttonStyleVertical.padding = new RectOffset(8,0,2,2);
				}
				return _buttonStyleVertical;
			}
		}

		private static GUIStyle _buttonStyleHorizontal = null;
		public static GUIStyle buttonStyleHorizontal
		{
			get
			{
				if(_buttonStyleHorizontal == null)
				{
					_buttonStyleHorizontal = new GUIStyle();

					_buttonStyleHorizontal.normal.textColor 	= EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_NORMAL : Color.black;
					_buttonStyleHorizontal.normal.background 	= pb_IconUtility.GetIcon("Toolbar/Button_Normal_Horizontal", IconSkin.Pro);
					_buttonStyleHorizontal.hover.background 	= pb_IconUtility.GetIcon("Toolbar/Button_Hover_Horizontal", IconSkin.Pro);
					_buttonStyleHorizontal.hover.textColor 		= EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_HOVER : Color.black;
					_buttonStyleHorizontal.active.background 	= pb_IconUtility.GetIcon("Toolbar/Button_Pressed_Horizontal", IconSkin.Pro);
					_buttonStyleHorizontal.active.textColor 	= EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_ACTIVE : Color.black;
					_buttonStyleHorizontal.alignment 			= TextAnchor.MiddleCenter;
					_buttonStyleHorizontal.border 				= new RectOffset(0,0,4,0);
					_buttonStyleHorizontal.stretchWidth 		= true;
					_buttonStyleHorizontal.stretchHeight 		= false;
					_buttonStyleHorizontal.margin 				= new RectOffset(4,4,4,5);
					_buttonStyleHorizontal.padding 				= new RectOffset(2,2,8,0);
				}
				return _buttonStyleHorizontal;
			}
		}

		private static GUIStyle _rowStyleVertical = null;
		public static GUIStyle rowStyleVertical
		{
			get
			{
				if(_rowStyleVertical == null)
				{
					_rowStyleVertical = new GUIStyle();
					_rowStyleVertical.alignment = TextAnchor.MiddleLeft;
					_rowStyleVertical.stretchWidth = true;
					_rowStyleVertical.stretchHeight = false;
					_rowStyleVertical.margin = new RectOffset(0,0,0,0);
					_rowStyleVertical.padding = new RectOffset(0,0,0,0);
				}
				return _rowStyleVertical;
			}
		}

		private static GUIStyle _rowStyleHorizontal = null;
		public static GUIStyle rowStyleHorizontal
		{
			get
			{
				if(_rowStyleHorizontal == null)
				{
					_rowStyleHorizontal = new GUIStyle();
					_rowStyleHorizontal.alignment = TextAnchor.MiddleCenter;
					_rowStyleHorizontal.stretchWidth = true;
					_rowStyleHorizontal.stretchHeight = false;
					_rowStyleHorizontal.margin = new RectOffset(0,0,0,0);
					_rowStyleHorizontal.padding = new RectOffset(0,0,0,0);
				}
				return _rowStyleHorizontal;
			}
		}

		private static GUIStyle _altButtonStyle = null;
		public static GUIStyle altButtonStyle
		{
			get
			{
				if(_altButtonStyle == null)
				{
					_altButtonStyle = new GUIStyle();

					_altButtonStyle.normal.background 	= pb_IconUtility.GetIcon("Toolbar/AltButton_Normal", IconSkin.Pro);
					_altButtonStyle.normal.textColor 	= EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_NORMAL : Color.black;
					_altButtonStyle.hover.background 	= pb_IconUtility.GetIcon("Toolbar/AltButton_Hover", IconSkin.Pro);
					_altButtonStyle.hover.textColor 	= EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_HOVER : Color.black;
					_altButtonStyle.active.background 	= pb_IconUtility.GetIcon("Toolbar/AltButton_Pressed", IconSkin.Pro);
					_altButtonStyle.active.textColor 	= EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_ACTIVE : Color.black;
					_altButtonStyle.alignment 			= TextAnchor.MiddleCenter;
					_altButtonStyle.border 				= new RectOffset(1,1,1,1);
					_altButtonStyle.stretchWidth 		= false;
					_altButtonStyle.stretchHeight 		= true;
					_altButtonStyle.margin 				= new RectOffset(4,4,4,4);
					_altButtonStyle.padding 			= new RectOffset(2,2,1,3);
				}
				return _altButtonStyle;
			}
		}

		private static GUIStyle _proOnlyStyle = null;
		public static GUIStyle proOnlyStyle
		{
			get
			{
				if(_proOnlyStyle == null)
				{
					_proOnlyStyle = new GUIStyle(EditorStyles.label);
					_proOnlyStyle.normal.background = pb_IconUtility.GetIcon("Toolbar/ProOnly", IconSkin.Pro);
					_proOnlyStyle.hover.background 	= pb_IconUtility.GetIcon("Toolbar/ProOnly", IconSkin.Pro);
					_proOnlyStyle.active.background = pb_IconUtility.GetIcon("Toolbar/ProOnly", IconSkin.Pro);
				}
				return _proOnlyStyle;
			}
		}

#if PROTOTYPE
		private static GUIStyle _advancedOnlyStyle = null;
		public static GUIStyle advancedOnlyStyle
		{
			get
			{
				if(_advancedOnlyStyle == null)
				{
					_advancedOnlyStyle = new GUIStyle();
					_advancedOnlyStyle.normal.textColor = ProOnlyTint;
					_advancedOnlyStyle.hover.textColor = ProOnlyTint;
					_advancedOnlyStyle.active.textColor = ProOnlyTint;
					_advancedOnlyStyle.alignment = TextAnchor.MiddleCenter;
					_advancedOnlyStyle.margin = new RectOffset(4,4,4,4);
					_advancedOnlyStyle.padding = new RectOffset(2,2,2,2);
				}
				return _advancedOnlyStyle;
			}
		}
#endif
		private static Texture2D _proOnlyIcon = null;
		public static Texture2D proOnlyIcon
		{
			get
			{
				if(_proOnlyIcon == null)
					_proOnlyIcon = pb_IconUtility.GetIcon("Toolbar/ProOnly");
				return _proOnlyIcon;
			}
		}
	}
}
