using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Interface;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Connects a GUI button to an action.
	 */
	public abstract class pb_MenuAction
	{
		[System.Flags]
		public enum MenuActionState
		{
			Hidden = 0x0,
			Visible = 0x1,
			Enabled = 0x2,
			VisibleAndEnabled = 0x3
		};

		public const string PROBUILDER_MENU_PATH = "Tools/ProBuilder/";

		protected const char CMD_SUPER 	= pb_Constant.CMD_SUPER;
		protected const char CMD_SHIFT 	= pb_Constant.CMD_SHIFT;
		protected const char CMD_OPTION = pb_Constant.CMD_OPTION;
		protected const char CMD_ALT 	= pb_Constant.CMD_ALT;
		protected const char CMD_DELETE = pb_Constant.CMD_DELETE;

		private static readonly Color TEXT_COLOR_WHITE_NORMAL = new Color(0.82f, 0.82f, 0.82f, 1f);
		private static readonly Color TEXT_COLOR_WHITE_HOVER = new Color(0.7f, 0.7f, 0.7f, 1f);
		private static readonly Color TEXT_COLOR_WHITE_ACTIVE = new Color(0.5f, 0.5f, 0.5f, 1f);

		private static readonly GUIContent AltButtonContent = new GUIContent("+", "");
		private static readonly GUIContent ProOnlyContent = new GUIContent("P", "");

		public virtual bool isProOnly { get { return false; } }

#if PROTOTYPE
		private static Color _proOnlyTintLight = new Color(0f, .5f, 1f, 1f);
		private static Color _proOnlyTintDark = new Color(.25f, 1f, 1f, 1f);
		private static Color ProOnlyTint
		{
			get { return EditorGUIUtility.isProSkin ? _proOnlyTintDark : _proOnlyTintLight; }
		}
		// private static readonly Color UpgradeTint = new Color(.5f, 1f, 1f, 1f);
#endif
	
		public pb_MenuAction()
		{
			isIconMode = pb_Preferences_Internal.GetBool(pb_Constant.pbIconGUI);
		}

		public delegate void SettingsDelegate();

		public static pb_Object[] selection 
		{
			get
			{
				return pbUtil.GetComponents<pb_Object>(Selection.transforms);
			}
		}

		/**
		 *	Reset static GUIStyle objects so that they will be re-initialized the next time used. 
		 */
		public static void ResetStyles()
		{
			_buttonStyleVertical = null;
			_buttonStyleHorizontal = null;
		}

		protected static GUIStyle _buttonStyleVertical = null;
		protected static GUIStyle buttonStyleVertical
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
					_buttonStyleVertical.alignment = pb_Preferences_Internal.GetBool(pb_Constant.pbIconGUI) ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
					_buttonStyleVertical.border = new RectOffset(4,0,0,0);
					_buttonStyleVertical.stretchWidth = true;
					_buttonStyleVertical.stretchHeight = false;
					_buttonStyleVertical.margin = new RectOffset(4,5,4,4);
					_buttonStyleVertical.padding = new RectOffset(8,0,2,2);
				}
				return _buttonStyleVertical;
			}
		}

		protected static GUIStyle _buttonStyleHorizontal = null;
		protected static GUIStyle buttonStyleHorizontal
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

		protected static GUIStyle _altButtonStyle = null;
		protected static GUIStyle altButtonStyle
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

		protected Texture2D _desaturatedIcon = null;
		protected Texture2D desaturatedIcon
		{
			get
			{
				if(_desaturatedIcon == null)
				{
					if(icon == null)
						return null;

					_desaturatedIcon = pb_IconUtility.GetIcon(string.Format("Toolbar/{0}_disabled", icon.name));

					// @todo
					// if(!_desaturatedIcon)
					// {
					// 	string path = AssetDatabase.GetAssetPath(icon);
					// 	TextureImporter imp = (TextureImporter) AssetImporter.GetAtPath( path );
					
					// 	if(!imp)
					// 	{
					// 		Debug.Log("Couldn't find importer : " + icon);
					// 		return null;
					// 	}
					
					// 	imp.isReadable = true;
					// 	imp.SaveAndReimport();
					
					// 	Color32[] px = icon.GetPixels32();
					
					// 	imp.isReadable = false;
					// 	imp.SaveAndReimport();
					
					// 	int gray = 0;
					
					// 	for(int i = 0; i < px.Length; i++)
					// 	{
					// 		gray = (System.Math.Min(px[i].r, System.Math.Min(px[i].g, px[i].b)) + System.Math.Max(px[i].r, System.Math.Max(px[i].g, px[i].b))) / 2;
					// 		px[i].r = (byte) gray;
					// 		px[i].g = (byte) gray;
					// 		px[i].b = (byte) gray;
					// 	}
					
					// 	_desaturatedIcon = new Texture2D(icon.width, icon.height);
					// 	_desaturatedIcon.hideFlags = HideFlags.HideAndDontSave;
					// 	_desaturatedIcon.SetPixels32(px);
					// 	_desaturatedIcon.Apply();
					
					// 	byte[] bytes = _desaturatedIcon.EncodeToPNG();
					// 	System.IO.File.WriteAllBytes(path.Replace(".png", "_disabled.png"), bytes);
					// }
				}

				return _desaturatedIcon;
			}
		}

		protected Texture2D _proOnlyIcon = null;
		protected Texture2D proOnlyIcon
		{
			get
			{
				if(_proOnlyIcon == null)
					_proOnlyIcon = pb_IconUtility.GetIcon("Toolbar/ProOnly");
				return _proOnlyIcon;
			}
		}

		public abstract pb_IconGroup group { get; }
		public abstract Texture2D icon { get; }
		public abstract pb_TooltipContent tooltip { get; }

		/**
		 *	Optional override for the action title displayed in the toolbar button.  If unimplemented the tooltip title
		 *	is used.
		 */
		public virtual string menuTitle { get { return tooltip.title; } }

		/**
		 *	Is the current mode and selection valid for this action?
		 */
		public MenuActionState ActionState()
		{
			if( IsHidden() )
				return MenuActionState.Hidden;
			else if( IsEnabled() )
				return MenuActionState.VisibleAndEnabled;
			else
				return MenuActionState.Visible;
		}

		/**
		 *	True if this action is valid with current selection and mode.
		 */
		public abstract bool IsEnabled();

		/**
		 *	True if this action should be shown in the toolbar with the current
		 *	mode and settings, false otherwise.  This returns false by default.
		 */
		public virtual bool IsHidden() { return false; }

		/**
		 *	True if this button should show the alternate button (which by default will open an Options window with 
		 *	OnSettingsGUI delegate).
		 */
		public virtual MenuActionState AltState() { return MenuActionState.Hidden; }

		/**
		 *	Perform whatever action this menu item is supposed to do.  Must implement undo/redo here.
		 */
		public abstract pb_ActionResult DoAction();

		/**
		 *	The 'Alt' button has been pressed.  The default action is to 
		 *	open a new Options window with the OnSettingsGUI delegate.
		 */
		public virtual void DoAlt()
		{
			pb_MenuOption.Show(OnSettingsGUI);
		}

		public virtual void OnSettingsGUI() {}

		protected bool isIconMode = true;

		/**
		 *	Draw a menu button.  Returns true if the button is active and settings are enabled, false if settings are 
		 * 	not enabled.
		 */
		public bool DoButton(bool isHorizontal, bool showOptions, ref Rect optionsRect, params GUILayoutOption[] layoutOptions)
		{
			bool wasEnabled = GUI.enabled;
#if PROTOTYPE
			bool buttonEnabled = !isProOnly && (ActionState() & MenuActionState.Enabled) == MenuActionState.Enabled;
#else
			bool buttonEnabled = (ActionState() & MenuActionState.Enabled) == MenuActionState.Enabled;
#endif
			
			GUI.enabled = buttonEnabled;

			GUI.backgroundColor = pb_IconGroupUtility.GetColor(group);

			if(isIconMode)
			{
				if( GUILayout.Button(buttonEnabled || !desaturatedIcon ? icon : desaturatedIcon, isHorizontal ? buttonStyleHorizontal : buttonStyleVertical, layoutOptions) )
				{
					if(showOptions && (AltState() & MenuActionState.VisibleAndEnabled) == MenuActionState.VisibleAndEnabled)
					{
						DoAlt();
					}	
					else
					{
						pb_ActionResult result = DoAction();
						pb_Editor_Utility.ShowNotification(result.notification);
					}
				}

				GUI.backgroundColor = Color.white;

				if((AltState() & MenuActionState.VisibleAndEnabled) == MenuActionState.VisibleAndEnabled)
				{
					Rect r = GUILayoutUtility.GetLastRect();
					// options icon is 16x16
					r.x = r.x + r.width - 14;
					r.y -= 2;
					r.width = 17;
					r.height = 17;
					GUI.Label(r, pb_IconUtility.GetIcon("Toolbar/Options", IconSkin.Pro));
					optionsRect = r;
					GUI.enabled = wasEnabled;
					return buttonEnabled;
				}
				else
				{
					GUI.enabled = wasEnabled;
					return false;
				}
			}
			else
			{
				GUILayout.BeginHorizontal(layoutOptions);
					if(GUILayout.Button(menuTitle, isHorizontal ? buttonStyleHorizontal : buttonStyleVertical))
					{
						pb_ActionResult res = DoAction();
						pb_Editor_Utility.ShowNotification(res.notification);
					}

#if PROTOTYPE
					if( isProOnly )
					{
						GUI.backgroundColor = ProOnlyTint;
						GUILayout.Label(ProOnlyContent, altButtonStyle, GUILayout.MaxWidth(21), GUILayout.MaxHeight(16));
						GUI.backgroundColor = Color.white;
					}
					else
#endif
					{
						MenuActionState altState = AltState();

						if( (altState & MenuActionState.Visible) == MenuActionState.Visible )
						{
							GUI.enabled = GUI.enabled && (altState & MenuActionState.Enabled) == MenuActionState.Enabled;

							if(DoAltButton(GUILayout.MaxWidth(21), GUILayout.MaxHeight(16)))
								DoAlt();
						}
					}

				GUILayout.EndHorizontal();

				GUI.enabled = wasEnabled;
				
				return false;
			}
		}

		protected virtual bool DoAltButton(params GUILayoutOption[] options)
		{
			return GUILayout.Button(AltButtonContent, altButtonStyle, options);
		}

		public static readonly Vector2 AltButtonSize = new Vector2(21, 0);

		/**
		 *	Get the rendered width of this GUI item.
		 */
		public Vector2 GetSize(bool isHorizontal)
		{
			if(isIconMode)
				return (isHorizontal ? buttonStyleHorizontal : buttonStyleVertical).CalcSize(pb_GUI_Utility.TempGUIContent(null, null, icon));
			else
				return (isHorizontal ? buttonStyleHorizontal : buttonStyleVertical).CalcSize(pb_GUI_Utility.TempGUIContent(menuTitle)) + AltButtonSize;
		}
	}
}
