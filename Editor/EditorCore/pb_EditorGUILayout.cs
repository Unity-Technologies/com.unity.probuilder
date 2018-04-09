using UnityEngine;
using UnityEditor;

namespace ProBuilder.Interface
{
	/// <summary>
	/// Additional GUI functions for Editor use.
	/// </summary>
	static class pb_EditorGUILayout
	{
		private static bool m_RowToggle = true;
		private static readonly Color RowOddColor = new Color(.45f, .45f, .45f, .2f);
		private static readonly Color RowEvenColor = new Color(.30f, .30f, .30f, .2f);

		public static void BeginRow(int index = -1)
		{
			if(index > -1)
				m_RowToggle = index % 2 == 0;

			pb_EditorGUIUtility.PushBackgroundColor(m_RowToggle ? RowEvenColor : RowOddColor);
			GUILayout.BeginHorizontal(pb_EditorStyles.rowStyle);
			m_RowToggle = !m_RowToggle;
			pb_EditorGUIUtility.PopBackgroundColor();
		}

		public static void EndRow()
		{
			GUILayout.EndHorizontal();
		}

		/**
		 *	An automatically laid out toolbar that returns the index of the selected button. Optionally allows no selection.
		 */
		public static int Toolbar(int index, GUIContent[] content, GUIStyle style, bool allowNoSelection = false, params GUILayoutOption[] addlParams)
		{
			return Toolbar(index, content, style, style, style, allowNoSelection, addlParams);
		}

		public static int Toolbar(int index, GUIContent[] content, GUIStyle left, GUIStyle mid, GUIStyle right, bool allowNoSelection = false, params GUILayoutOption[] addlParams)
		{
			GUILayout.BeginHorizontal();

			for(int i = 0; i < content.Length; i++)
			{
				GUIStyle m_Style = i < 1 ? left : (i >= content.Length - 1 ? right : mid);

				if(index == i)
					m_Style = pb_EditorGUIUtility.GetOnStyle(m_Style);

				if(GUILayout.Button(content[i], m_Style, addlParams))
				{
					if(index == i && allowNoSelection)
						index = -1;
					else
						index = i;
				}
			}

			GUILayout.EndHorizontal();

			return index;
		}

		/**
		 *	An automatically laid out toolbar that toggles flags. Content corresponds to the bits starting at 1 - ex:
		 *		- content[0] = 0x1
		 *		- content[1] = 0x2
		 *		- content[2] = 0x4
		 */
		public static int FlagToolbar(int index, GUIContent[] content, GUIStyle style, bool allowNoSelection = false, bool allowMultipleSelected = true, params GUILayoutOption[] addlParams)
		{
			return FlagToolbar(index, content, style, style, style, allowNoSelection, allowMultipleSelected, addlParams);
		}

		public static int FlagToolbar(int index, GUIContent[] content, bool allowNoSelection = false, bool allowMultipleSelected = true, params GUILayoutOption[] addlParams)
		{
			return FlagToolbar(index, content, EditorStyles.miniButtonLeft, EditorStyles.miniButtonMid, EditorStyles.miniButtonRight, allowNoSelection, allowMultipleSelected, addlParams);
		}

		public static int FlagToolbar(int index, GUIContent[] content, GUIStyle left, GUIStyle mid, GUIStyle right, bool allowNoSelection = false, bool allowMultipleSelected = true, params GUILayoutOption[] addlParams)
		{
			GUILayout.BeginHorizontal();

			for(int i = 0; i < content.Length; i++)
			{
				GUIStyle m_Style = i < 1 ? left : (i >= content.Length - 1 ? right : mid);

				if( (index & (0x1 << i)) > 0 )
					m_Style = pb_EditorGUIUtility.GetOnStyle(m_Style);

				if(GUILayout.Button(content[i], m_Style, addlParams))
				{
					if(!allowMultipleSelected)
						index = (index & (0x1 << i));

					index ^= (0x1 << i);

					if(!allowNoSelection && index == 0x0)
						index = 0x1 << i;
				}
			}

			GUILayout.EndHorizontal();

			return index;
		}
	}
}