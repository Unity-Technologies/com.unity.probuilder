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
	[System.Serializable]
	public abstract class pb_MenuAction
	{
		protected static GUIStyle _buttonStyle = null;
		protected static GUIStyle buttonStyle
		{
			get
			{
				if(_buttonStyle == null)
				{
					_buttonStyle = new GUIStyle();
					_buttonStyle.alignment = TextAnchor.MiddleCenter;
					_buttonStyle.normal.background = pb_IconUtility.GetIcon("Button_Normal");
					_buttonStyle.hover.background = pb_IconUtility.GetIcon("Button_Hover");
					_buttonStyle.active.background = pb_IconUtility.GetIcon("Button_Pressed");
					_buttonStyle.margin = new RectOffset(4,4,4,4);
					_buttonStyle.padding = new RectOffset(4,4,4,4);
				}
				return _buttonStyle;
			}
		}

		public Texture2D icon;
		public pb_TooltipContent tooltip;

		public abstract pb_ActionResult DoAction();

		// 
		public System.Type optionsWindowType = null;
		public System.Func<EditLevel, SelectMode, pb_Object[], bool> optionsWindowEnabled;

		// Is this action valid based on the current selection and context?
		public abstract bool IsEnabled();

		public virtual bool IsHidden() { return false; }

		public bool DoButton(bool showOptions, ref Rect optionsRect)
		{
			bool wasEnabled = GUI.enabled;
			
			GUI.enabled = IsEnabled();

			pb_Object[] sel = pbUtil.GetComponents<pb_Object>(Selection.transforms);
			EditLevel el = pb_Editor.instance != null ? pb_Editor.instance.editLevel : EditLevel.Top;
			SelectMode sm = pb_Editor.instance != null ? pb_Editor.instance.selectionMode : SelectMode.Face;

			bool canShowOptions = optionsWindowType != null && (optionsWindowEnabled == null || optionsWindowEnabled(el, sm, sel));

			if( GUILayout.Button(icon, buttonStyle) )
			{
				if(showOptions && canShowOptions)
					pb_MenuOption.Show(optionsWindowType);
				else
					DoAction();
			}

			if(canShowOptions)
			{
				Rect r = GUILayoutUtility.GetLastRect();
				r.x = r.x + r.width - 18;
				r.y += 2;
				r.width = 17;
				r.height = 17;
				GUI.Label(r, pb_IconUtility.GetIcon("Options"));
				optionsRect = r;
				GUI.enabled = wasEnabled;
				return true;
			}
			else
			{
				GUI.enabled = wasEnabled;
				return false;
			}
		}

		/**
		 *	Get the rendered width of this GUI item.
		 */
		public Vector2 GetSize()
		{
			return buttonStyle.CalcSize( pb_GUI_Utility.TempGUIContent(null, null, icon) );
		}
	}
}
