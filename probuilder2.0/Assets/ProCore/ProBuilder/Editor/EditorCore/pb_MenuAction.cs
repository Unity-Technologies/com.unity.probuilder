using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Interface;

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

		// If this action has special extra settings, override this property to enable DoSettings().
		public virtual bool HasSettings { get { return false; } }

		public abstract pb_ActionResult DoAction();

		// Is this action valid based on the current selection and context?
		public abstract bool IsEnabled();

		public virtual void DoSettings() {}

		public void DoButton()
		{
			bool wasEnabled = GUI.enabled;
			
			GUI.enabled = IsEnabled();

			if( GUILayout.Button(icon, buttonStyle) )
				DoAction();

			GUI.enabled = wasEnabled;
		}

		/**
		 *	Get the rendered width of this GUI item.
		 */
		public Vector2 GetSize()
		{
			return buttonStyle.CalcSize( pb_GUI_Utility.TempGUIContent(null, icon) );
		}
	}
}
