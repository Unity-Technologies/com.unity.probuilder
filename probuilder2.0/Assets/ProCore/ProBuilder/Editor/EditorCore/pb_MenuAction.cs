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
		static GUIStyle _buttonStyle = null;
		static GUIStyle buttonStyle
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

		public string tooltip;

		public virtual bool HasSettings { get { return false; } }

		public abstract pb_ActionResult DoAction();

		public virtual void DoSettings() {}

		public void DoButton()
		{
			if( GUILayout.Button(icon, buttonStyle) )
				Debug.Log(DoAction().notification);
		}

		public static void DoSpace(Vector2 size)
		{
			GUILayout.Label("", buttonStyle, 
				GUILayout.MinWidth(size.x),
				GUILayout.MinHeight(size.y)
				);
		}

		/**
		 *	Get the rendered width of this GUI item.
		 */
		public Vector2 GetSize()
		{
			return buttonStyle.CalcSize( pb_GUI_Utility.TempGUIContent(null, icon) );
		}
	}

	public class pb_MenuAction_Simple : pb_MenuAction
	{
		public System.Func<pb_Object[], pb_ActionResult> action;

		public pb_MenuAction_Simple(Texture2D icon, System.Func<pb_Object[], pb_ActionResult> action)
		{
			this.icon = icon;
			this.action = action;
		}

		public override pb_ActionResult DoAction()
		{
			return action(new pb_Object[] { null });
		}
	}
}
