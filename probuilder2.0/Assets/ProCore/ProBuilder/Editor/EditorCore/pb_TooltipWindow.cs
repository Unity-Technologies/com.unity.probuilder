using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	public class pb_TooltipWindow : EditorWindow
	{
		static readonly Color BasicBackgroundColor = new Color(.87f, .87f, .87f, 1f);

		private static pb_TooltipWindow _instance;
		private static Rect windowRect = new Rect(0,0,0,0);

		// much like highlander, there can only be one
		public static pb_TooltipWindow instance()
		{
			if(_instance == null)
			{
				_instance = ScriptableObject.CreateInstance<pb_TooltipWindow>();
				_instance.minSize = Vector2.zero;
				_instance.maxSize = Vector2.zero;
				_instance.ShowPopup();

				object parent = pb_Reflection.GetValue(_instance, "m_Parent");
				object window = pb_Reflection.GetValue(parent, "window");
				pb_Reflection.SetValue(parent, "mouseRayInvisible", true);
				pb_Reflection.SetValue(window, "m_DontSaveToLayout", true);
			}

			return _instance;
		}


		public static void Hide()
		{
			if(_instance != null)
			{
				_instance.Close();
				_instance = null;
			}
		}

		public static void Show(Vector2 position, pb_TooltipContent content)
		{
			instance().ShowInternal(position, content);
		}

		public void ShowInternal(Vector2 position, pb_TooltipContent content)
		{
			this.content = content;
			Vector2 size = content.CalcSize();

			this.minSize = size;
			this.maxSize = size;

			this.position = new Rect(
				position.x,
				position.y,
				size.x,
				size.y);

			windowRect = new Rect(0,0,size.x, size.y);
		}

		public pb_TooltipContent content = null;

		void OnGUI()
		{
			if(!EditorGUIUtility.isProSkin)
			{
				GUI.backgroundColor = BasicBackgroundColor;
				GUI.Box(windowRect, "");
				GUI.backgroundColor = Color.white;
			}

			if(content == null)
				return;

			content.Draw();
		}
	}
}
