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
		public const string PROBUILDER_MENU_PATH = "Tools/ProBuilder/";

		protected const char CMD_SUPER 	= pb_Constant.CMD_SUPER;
		protected const char CMD_SHIFT 	= pb_Constant.CMD_SHIFT;
		protected const char CMD_OPTION = pb_Constant.CMD_OPTION;
		protected const char CMD_ALT 	= pb_Constant.CMD_ALT;
		protected const char CMD_DELETE = pb_Constant.CMD_DELETE;

		private static readonly Color TEXT_COLOR_WHITE_NORMAL = new Color(0.82f, 0.82f, 0.82f, 1f);
		private static readonly Color TEXT_COLOR_WHITE_HOVER = new Color(0.7f, 0.7f, 0.7f, 1f);
		private static readonly Color TEXT_COLOR_WHITE_ACTIVE = new Color(0.5f, 0.5f, 0.5f, 1f);

		public delegate void SettingsDelegate();

		public static pb_Object[] selection 
		{
			get
			{
				return pbUtil.GetComponents<pb_Object>(Selection.transforms);
			}
		}

		protected static GUIStyle _buttonStyleVertical = null;
		protected static GUIStyle buttonStyleVertical
		{
			get
			{
				if(_buttonStyleVertical == null)
				{
					_buttonStyleVertical = new GUIStyle();
					_buttonStyleVertical.border = new RectOffset(4,0,0,0);
					_buttonStyleVertical.alignment = TextAnchor.MiddleCenter;
					_buttonStyleVertical.normal.background = pb_IconUtility.GetIcon("Button_Normal");
					_buttonStyleVertical.normal.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_NORMAL : Color.black;
					_buttonStyleVertical.hover.background = pb_IconUtility.GetIcon("Button_Hover");
					_buttonStyleVertical.hover.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_HOVER : Color.black;
					_buttonStyleVertical.active.background = pb_IconUtility.GetIcon("Button_Pressed");
					_buttonStyleVertical.active.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_ACTIVE : Color.black;
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
					_buttonStyleHorizontal.border = new RectOffset(0,0,4,0);
					_buttonStyleHorizontal.alignment = TextAnchor.MiddleCenter;
					_buttonStyleHorizontal.normal.background = pb_IconUtility.GetIcon("Button_Normal_Horizontal");
					_buttonStyleHorizontal.normal.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_NORMAL : Color.black;
					_buttonStyleHorizontal.hover.background = pb_IconUtility.GetIcon("Button_Hover_Horizontal");
					_buttonStyleHorizontal.hover.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_HOVER : Color.black;
					_buttonStyleHorizontal.active.background = pb_IconUtility.GetIcon("Button_Pressed_Horizontal");
					_buttonStyleHorizontal.active.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_ACTIVE : Color.black;
					_buttonStyleHorizontal.stretchWidth = true;
					_buttonStyleHorizontal.stretchHeight = true;
					_buttonStyleHorizontal.margin = new RectOffset(4,4,4,5);
					_buttonStyleHorizontal.padding = new RectOffset(2,2,8,0);
				}
				return _buttonStyleHorizontal;
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

					_desaturatedIcon = pb_IconUtility.GetIcon(icon.name + "_disabled");

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

		public abstract pb_IconGroup group { get; }
		public abstract Texture2D icon { get; }
		public abstract pb_TooltipContent tooltip { get; }
		// Optional override for the action title displayed in the toolbar button.  If unimplemented the tooltip title is used.
		public virtual string MenuTitle { get { return tooltip.title; } }

		public virtual bool IsHidden() { return false; }
		public abstract bool IsEnabled();
		public virtual bool SettingsEnabled() { return false; }
		
		public abstract pb_ActionResult DoAction();
		public virtual void OnSettingsGUI() {}

		protected bool isIconMode = true;

		public pb_MenuAction()
		{
			isIconMode = false; // pb_Preferences_Internal.GetBool(pb_Constant.pbIconGUI);
		}

		/**
		 *	Draw a menu button.  Returns true if the button is active and settings are enabled, false if settings are not enabled.
		 */
		public bool DoButton(bool isHorizontal, bool showOptions, ref Rect optionsRect, params GUILayoutOption[] layoutOptions)
		{
			bool wasEnabled = GUI.enabled;
			bool buttonEnabled = IsEnabled();
			
			GUI.enabled = buttonEnabled;

			GUI.backgroundColor = pb_IconGroupUtility.GetColor(group);

			if(isIconMode)
			{
				if( GUILayout.Button(buttonEnabled || !desaturatedIcon ? icon : desaturatedIcon, isHorizontal ? buttonStyleHorizontal : buttonStyleVertical) )
				{
					if(showOptions && SettingsEnabled())
						pb_MenuOption.Show(OnSettingsGUI);
					else
					{
						pb_ActionResult result = DoAction();
						pb_Editor_Utility.ShowNotification(result.notification);
					}
				}

				GUI.backgroundColor = Color.white;

				if(SettingsEnabled())
				{
					Rect r = GUILayoutUtility.GetLastRect();
					// options icon is 16x16
					r.x = r.x + r.width - 14;
					r.y -= 2;
					r.width = 17;
					r.height = 17;
					GUI.Label(r, pb_IconUtility.GetIcon("Options"));
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
				if(GUILayout.Button(MenuTitle, isHorizontal ? buttonStyleHorizontal : buttonStyleVertical, layoutOptions))
				{
					pb_ActionResult res = DoAction();
					pb_Editor_Utility.ShowNotification(res.notification);
				}

				return false;
			}
		}

		/**
		 *	Get the rendered width of this GUI item.
		 */
		public Vector2 GetSize(bool isHorizontal)
		{
			if(isIconMode)
			{
				return isHorizontal ?
					buttonStyleHorizontal.CalcSize( pb_GUI_Utility.TempGUIContent(null, null, icon) ) :
					buttonStyleVertical.CalcSize( pb_GUI_Utility.TempGUIContent(null, null, icon) );			
			}
			else
			{
				return isHorizontal ?
					buttonStyleHorizontal.CalcSize( pb_GUI_Utility.TempGUIContent(MenuTitle) ) :
					buttonStyleVertical.CalcSize( pb_GUI_Utility.TempGUIContent(MenuTitle) );			
			}
		}
	}
}
