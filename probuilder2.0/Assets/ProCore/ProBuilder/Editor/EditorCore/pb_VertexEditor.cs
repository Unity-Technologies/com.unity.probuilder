using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using System.Linq;
using ProBuilder2.Interface;

namespace ProBuilder2.EditorCommon
{
	public class pb_VertexEditor : EditorWindow
	{
		pb_Object[] selection;
		Dictionary<int, int>[] sharedIndices;
		Dictionary<pb_Object, bool> foldout = new Dictionary<pb_Object, bool>();

		static Color EVEN = new Color(.18f, .18f, .18f, 1f);
		static Color ODD  = new Color(.15f, .15f, .15f, 1f);

		public static void MenuOpenVertexEditor()
		{
			EditorWindow.GetWindow<pb_VertexEditor>(true, "Positions Editor", true);
		}

		void OnEnable()
		{
			pb_Editor.OnSelectionUpdate += OnSelectionUpdate;

			if(pb_Editor.instance != null)
				OnSelectionUpdate(pb_Editor.instance.selection);
		}

		void OnDisable()
		{
			pb_Editor.OnSelectionUpdate -= OnSelectionUpdate;
		}

		void OnSelectionUpdate(pb_Object[] selection)
		{
			this.selection = selection;

			int l = selection == null ? 0 : selection.Length;
			this.sharedIndices = new Dictionary<int, int>[l];
			for(int i = 0; i < l; i++)
				this.sharedIndices[i] = selection[i].sharedIndices.ToDictionary();

			this.Repaint();
		}

		Vector2 scroll = Vector2.zero;

		void OnGUI()
		{
			if(selection == null || selection.Length < 1)
			{
				GUILayout.FlexibleSpace();
				GUILayout.Label("Select a ProBuilder Mesh", EditorStyles.centeredGreyMiniLabel);
				GUILayout.FlexibleSpace();
				return;
			}

			scroll = EditorGUILayout.BeginScrollView(scroll);

			for(int i = 0; i < selection.Length; i++)
			{
				pb_Object pb = selection[i];

				if(!foldout.ContainsKey(selection[i]))
					foldout.Add(pb, true);

				bool open = foldout[pb];

				EditorGUI.BeginChangeCheck();
				open = EditorGUILayout.Foldout(open, pb.name);
				if(EditorGUI.EndChangeCheck())
					foldout[pb] = open;

				if(open)
				{
					HashSet<int> unique = new HashSet<int>(selection.SelectMany(x => x.SelectedTriangles).Select(x => sharedIndices[i][x]));
					int index = 0;
					
					bool wasWideMode = EditorGUIUtility.wideMode;
					EditorGUIUtility.wideMode = true;
					Color background = GUI.backgroundColor;

					foreach(int u in unique)
					{
						GUI.backgroundColor = index % 2 == 0 ? EVEN : ODD;
						GUILayout.BeginHorizontal(pb_GUI_Utility.solidBackgroundStyle);
						GUI.backgroundColor = background;
						
							GUILayout.Label(u.ToString(), GUILayout.MinWidth(32), GUILayout.MaxWidth(32));
							Vector3 v = selection[i].vertices[selection[i].sharedIndices[u][0]];
							v = EditorGUILayout.Vector3Field("", v);
							index++;
						GUILayout.EndHorizontal();
					}

					GUI.backgroundColor = background;
					EditorGUIUtility.wideMode = wasWideMode;
				}
			}

			EditorGUILayout.EndScrollView();
		}
	}
}
