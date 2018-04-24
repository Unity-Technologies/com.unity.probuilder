// #define GENERATE_DESATURATED_ICONS

using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Connects a GUI button to an action.
	/// </summary>
	public abstract class MenuAction
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

		protected const char CMD_SUPER 	= PreferenceKeys.CMD_SUPER;
		protected const char CMD_SHIFT 	= PreferenceKeys.CMD_SHIFT;
		protected const char CMD_OPTION = PreferenceKeys.CMD_OPTION;
		protected const char CMD_ALT 	= PreferenceKeys.CMD_ALT;
		protected const char CMD_DELETE = PreferenceKeys.CMD_DELETE;

		static readonly GUIContent AltButtonContent = new GUIContent("+", "");
		public virtual bool isProOnly { get { return false; } }

		protected MenuAction()
		{
			isIconMode = PreferencesInternal.GetBool(PreferenceKeys.pbIconGUI);
		}

		public static int CompareActionsByGroupAndPriority(MenuAction left, MenuAction right)
		{
			if(left == null)
			{
				if(right == null)
					return 0;
				else
					return -1;
			}
			else
			{
				if(right == null)
				{
					return 1;
				}
				else
				{
					int l = (int) left.group, r = (int) right.group;

					if(l < r)
						return -1;
					else if(l > r)
						return 1;
					else
					{
						int lp = left.toolbarPriority < 0 ? int.MaxValue : left.toolbarPriority,
							rp = right.toolbarPriority < 0 ? int.MaxValue : right.toolbarPriority;

						return lp.CompareTo(rp);
					}
				}
			}
		}

		public delegate void SettingsDelegate();

		protected static pb_Object[] selection { get { return MeshSelection.All(); } }

		protected static EditLevel editLevel { get { return ProBuilderEditor.instance ? ProBuilderEditor.instance.editLevel : EditLevel.Top; } }
		protected static SelectMode selectionMode { get { return ProBuilderEditor.instance ? ProBuilderEditor.instance.selectionMode : SelectMode.Face; } }

		public static GUIStyle textButtonStyleVertical 		{ get { return MenuActionStyles.buttonStyleVertical; } }
		public static GUIStyle buttonStyleHorizontal 	{ get { return MenuActionStyles.buttonStyleHorizontal; } }

		public static GUIStyle rowStyleVertical 		{ get { return MenuActionStyles.rowStyleVertical; } }
		public static GUIStyle rowStyleHorizontal 		{ get { return MenuActionStyles.rowStyleHorizontal; } }
		public static GUIStyle altButtonStyle 			{ get { return MenuActionStyles.altButtonStyle; } }

		Texture2D m_DesaturatedIcon = null;

		/**
		 * By default this function will look for an image named `${icon}_disabled`. If your disabled icon is somewhere
		 * else override this function.
		 *
		 * Note that unlike `pb_MenuAction.icon` this function caches the result.
		 */
		public virtual Texture2D desaturatedIcon
		{
			get
			{
				if(m_DesaturatedIcon == null)
				{
					if(icon == null)
						return null;

					m_DesaturatedIcon = IconUtility.GetIcon(string.Format("Toolbar/{0}_disabled", icon.name));

#if GENERATE_DESATURATED_ICONS
					if(!m_DesaturatedIcon)
						m_DesaturatedIcon = ProBuilder2.EditorCommon.DebugUtilities.pb_GenerateDesaturatedImage.CreateDesaturedImage(icon);
#endif
				}

				return m_DesaturatedIcon;
			}
		}

		// What category this action belongs in.  See pb_ToolbarGroup.
		public abstract ToolbarGroup group { get; }
		// Optional value influences where in the toolbar this menu item will be placed.
		// 0 is first, 1 is second, -1 is no preference.
		public virtual int toolbarPriority { get { return -1; } }
		public abstract Texture2D icon { get; }
		public abstract TooltipContent tooltip { get; }

		/**
		 *	Optional override for the action title displayed in the toolbar button.  If unimplemented the tooltip title
		 *	is used.
		 */
		public virtual string menuTitle { get { return tooltip.title; } }

		/**
		 *	True if this class should have an entry built into the hardware menu.
		 */
		public virtual bool hasFileMenuEntry { get { return true; } }

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
		public abstract ActionResult DoAction();

		/**
		 *	The 'Alt' button has been pressed.  The default action is to
		 *	open a new Options window with the OnSettingsGUI delegate.
		 */
		public virtual void DoAlt()
		{
			MenuOption.Show(OnSettingsGUI, OnSettingsEnable, OnSettingsDisable);
		}

		public virtual void OnSettingsGUI() {}

		/**
		 *	Called when the settings window is opened.
		 *	Only used when AltState is MenuActionState.VisibleAndEnabled.
		 */
		public virtual void OnSettingsEnable() {}

		/**
		 *	Called when the settings window is closed.
		 *	Only used when AltState is MenuActionState.VisibleAndEnabled.
		 */
		public virtual void OnSettingsDisable() {}

		protected bool isIconMode;

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

			GUI.backgroundColor = Color.white;

			if(isIconMode)
			{
				if( GUILayout.Button(buttonEnabled || !desaturatedIcon ? icon : desaturatedIcon, ToolbarGroupUtility.GetStyle(group, isHorizontal), layoutOptions) )
				{
					if(showOptions && (AltState() & MenuActionState.VisibleAndEnabled) == MenuActionState.VisibleAndEnabled)
					{
						DoAlt();
					}
					else
					{
						ActionResult result = DoAction();
						EditorUtility.ShowNotification(result.notification);
					}
				}

#if PROTOTYPE
				if(isProOnly || (AltState() & MenuActionState.VisibleAndEnabled) == MenuActionState.VisibleAndEnabled)
#else
				if((AltState() & MenuActionState.VisibleAndEnabled) == MenuActionState.VisibleAndEnabled)
#endif
				{
					Rect r = GUILayoutUtility.GetLastRect();
#if PROTOTYPE
					if(isProOnly)
					{
						r.x = r.x + r.width - 12;
						r.y -= 4;
						r.width = 16;
						r.height = 16;

						GUI.backgroundColor = ProOnlyTint;
						GUI.Label(r, GUIContent.none, proOnlyStyle);
						GUI.backgroundColor = Color.white;
					}
					else
#endif
					{
						r.x = r.x + r.width - 16;
						r.y += 0;
						r.width = 14;
						r.height = 14;
						GUI.Label(r, IconUtility.GetIcon("Toolbar/Options", IconSkin.Pro), GUIStyle.none);
					}

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
				GUI.backgroundColor = ToolbarGroupUtility.GetColor(group);

				// in text mode always use the vertical layout.
				isHorizontal = false;
				GUILayout.BeginHorizontal(rowStyleVertical, layoutOptions);
					if(GUILayout.Button(menuTitle, textButtonStyleVertical))
					{
						ActionResult res = DoAction();
						EditorUtility.ShowNotification(res.notification);
					}

#if PROTOTYPE
					if( isProOnly )
					{
						GUILayout.Label(ProOnlyContent, advancedOnlyStyle, GUILayout.MaxWidth(21), GUILayout.MaxHeight(16));
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

				GUI.backgroundColor = Color.white;

				GUI.enabled = wasEnabled;

				return false;
			}
		}

		protected virtual bool DoAltButton(params GUILayoutOption[] options)
		{
			return GUILayout.Button(AltButtonContent, altButtonStyle, options);
		}

		public static readonly Vector2 AltButtonSize = new Vector2(21, 0);

		private Vector2 m_LastCalculatedSize = Vector2.zero;

		/**
		 *	Get the rendered width of this GUI item.
		 */
		public Vector2 GetSize(bool isHorizontal)
		{
			if(isIconMode)
			{
				m_LastCalculatedSize = ToolbarGroupUtility.GetStyle(ToolbarGroup.Object, isHorizontal).CalcSize(UI.EditorGUIUtility.TempGUIContent(null, null, icon));
			}
			else
			{
				// in text mode always use the vertical layout.
				isHorizontal = false;
				m_LastCalculatedSize = textButtonStyleVertical.CalcSize(UI.EditorGUIUtility.TempGUIContent(menuTitle)) + AltButtonSize;
			}
			return m_LastCalculatedSize;
		}
	}
}
