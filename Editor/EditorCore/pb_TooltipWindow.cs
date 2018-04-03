using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Tooltip window implementation.
	/// </summary>
	class pb_TooltipWindow : EditorWindow
	{
		static readonly Color BasicBackgroundColor = new Color(.87f, .87f, .87f, 1f);
		const int POSITION_PADDING = 4;

		private static pb_TooltipWindow _instance;
		private static Rect windowRect = new Rect(0,0,0,0);

		static GUIStyle _proOnlyStyle = null;
		static GUIStyle proOnlyStyle
		{
			get
			{
				if(_proOnlyStyle == null)
				{
					_proOnlyStyle = new GUIStyle(EditorStyles.largeLabel);
					Color c = _proOnlyStyle.normal.textColor;
					c.a = .20f;
					_proOnlyStyle.normal.textColor = c;
					_proOnlyStyle.fontStyle = FontStyle.Bold;
					_proOnlyStyle.alignment = TextAnchor.UpperRight;
					_proOnlyStyle.fontSize += 22;
					_proOnlyStyle.padding.top += 1;
					_proOnlyStyle.padding.right += 4;
				}
				return _proOnlyStyle;
			}
		}

		// much like highlander, there can only be one
		public static pb_TooltipWindow instance()
		{
			if(_instance == null)
			{
				_instance = ScriptableObject.CreateInstance<pb_TooltipWindow>();
				_instance.minSize = Vector2.zero;
				_instance.maxSize = Vector2.zero;
				_instance.hideFlags = HideFlags.HideAndDontSave;
				_instance.ShowPopup();

				object parent = pb_Reflection.GetValue(_instance, _instance.GetType(), "m_Parent");
				object window = pb_Reflection.GetValue(parent, parent.GetType(), "window");
				pb_Reflection.SetValue(parent, "mouseRayInvisible", true);
				pb_Reflection.SetValue(window, "m_DontSaveToLayout", true);
			}

			return _instance;
		}

		// unlike highlander, this will hide
		public static void Hide()
		{
			pb_TooltipWindow[] windows = Resources.FindObjectsOfTypeAll<pb_TooltipWindow>();

			for(int i = 0; i < windows.Length; i++)
			{
				windows[i].Close();
				GameObject.DestroyImmediate(windows[i]);
				windows[i] = null;
			}
		}

		public static void Show(Rect rect, pb_TooltipContent content, bool isProOnly)
		{
			instance().ShowInternal(rect, content, isProOnly);
		}

		public void ShowInternal(Rect rect, pb_TooltipContent content, bool isProOnly)
		{
			this.content = content;
#if PROTOTYPE
			this.isProOnly = isProOnly;
#else
			this.isProOnly = false;
#endif

			Vector2 size = content.CalcSize();

			Vector2 p = new Vector2(rect.x + rect.width + POSITION_PADDING, rect.y);
			// if(p.x > Screen.width) p.x = rect.x - POSITION_PADDING - size.x;

			this.minSize = size;
			this.maxSize = size;

			this.position = new Rect(
				p.x,
				p.y,
				size.x,
				size.y);

			windowRect = new Rect(0,0,size.x, size.y);
		}

		public pb_TooltipContent content = null;
		public bool isProOnly = false;

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

#if PROTOTYPE
			if(isProOnly)
				GUI.Label(windowRect, "Advanced Only", proOnlyStyle);
#endif

			content.Draw(isProOnly);
		}
	}
}
