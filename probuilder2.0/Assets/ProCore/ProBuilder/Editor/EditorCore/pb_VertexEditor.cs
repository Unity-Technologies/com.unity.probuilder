using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.Interface;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// A simple line-item editor for vertex positions.
	/// </summary>
	class pb_VertexEditor : EditorWindow
	{
		const int MAX_SCENE_LABELS = 100;

		class VertexEditorSelection
		{
			public Dictionary<int, int> lookup;
			public bool isVisible = false;
			public IEnumerable<int> common;

			public VertexEditorSelection(Dictionary<int, int> lookup, bool visible, int[] indices)
			{
				this.lookup = lookup;
				this.isVisible = visible;
				this.common = pb_IntArrayUtility.GetCommonIndices(lookup, indices);
			}
		}

		Dictionary<pb_Object, VertexEditorSelection> selection = new Dictionary<pb_Object, VertexEditorSelection>();

		static Color EVEN;
		static Color ODD;

		Vector2 scroll = Vector2.zero;
		bool moving = false;
		public bool worldSpace = true;

		public static void MenuOpenVertexEditor()
		{
			EditorWindow.GetWindow<pb_VertexEditor>(true, "Positions Editor", true);
		}

		void OnEnable()
		{
			EVEN = EditorGUIUtility.isProSkin ? new Color(.18f, .18f, .18f, 1f) : new Color(.85f, .85f, .85f, 1f);
			ODD = EditorGUIUtility.isProSkin ? new Color(.15f, .15f, .15f, 1f) : new Color(.80f, .80f, .80f, 1f);

			pb_Editor.onSelectionUpdate += OnSelectionUpdate;
			SceneView.onSceneGUIDelegate += OnSceneGUI;

			if(pb_Editor.instance != null)
				OnSelectionUpdate(pb_Editor.instance.selection);
		}

		void OnDisable()
		{
			pb_Editor.onSelectionUpdate -= OnSelectionUpdate;
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
		}

		void OnSelectionUpdate(pb_Object[] newSelection)
		{
			if(newSelection == null)
			{
				if(selection != null)
					selection.Clear();

				return;
			}

			Dictionary<pb_Object, VertexEditorSelection> res = new Dictionary<pb_Object, VertexEditorSelection>();

			foreach(pb_Object pb in newSelection)
			{
				VertexEditorSelection sel;

				if(selection.TryGetValue(pb, out sel))
				{
					sel.lookup = pb.sharedIndices.ToDictionary();
					sel.common = pb_IntArrayUtility.GetCommonIndices(sel.lookup, pb.SelectedTriangles);
					res.Add(pb, sel);
				}
				else
				{
					res.Add(pb, new VertexEditorSelection(pb.sharedIndices.ToDictionary(), true, pb.SelectedTriangles));
				}
			}

			selection = res;

			this.Repaint();
		}

		void OnVertexMovementBegin(pb_Object pb)
		{
			moving = true;
			pb.ToMesh();
			pb.Refresh();
		}

		void OnVertexMovementFinish()
		{
			moving = false;

			foreach(var kvp in selection)
			{
				kvp.Key.ToMesh();
				kvp.Key.Refresh();
				kvp.Key.Optimize();
			}
		}

		void OnGUI()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);

				GUILayout.FlexibleSpace();

				GUIStyle style = worldSpace ? EditorStyles.toolbarButton : pb_EditorGUIUtility.GetOnStyle(EditorStyles.toolbarButton);

				if( GUILayout.Button(worldSpace ? "World Space" : "Model Space", style) )
					worldSpace = !worldSpace;

			GUILayout.EndHorizontal();

			if(selection == null || selection.Count < 1 || !selection.Any(x => x.Key.SelectedTriangleCount > 0))
			{
				GUILayout.FlexibleSpace();
				GUILayout.Label("Select a ProBuilder Mesh", pb_EditorGUIUtility.CenteredGreyMiniLabel);
				GUILayout.FlexibleSpace();
				return;
			}

			Event e = Event.current;

			if(moving)
			{
				if(	e.type == EventType.Ignore ||
					e.type == EventType.MouseUp )
					OnVertexMovementFinish();
			}

			scroll = EditorGUILayout.BeginScrollView(scroll);

			foreach(var kvp in selection)
			{
				pb_Object pb = kvp.Key;
				VertexEditorSelection sel = kvp.Value;

				bool open = sel.isVisible;

				EditorGUI.BeginChangeCheck();
				open = EditorGUILayout.Foldout(open, pb.name);
				if(EditorGUI.EndChangeCheck())
					sel.isVisible = open;

				if(open)
				{
					int index = 0;

					bool wasWideMode = EditorGUIUtility.wideMode;
					EditorGUIUtility.wideMode = true;
					Color background = GUI.backgroundColor;
					Transform transform = pb.transform;

					foreach(int u in sel.common)
					{
						GUI.backgroundColor = index % 2 == 0 ? EVEN : ODD;
						GUILayout.BeginHorizontal(pb_EditorGUIUtility.solidBackgroundStyle);
						GUI.backgroundColor = background;

							GUILayout.Label(u.ToString(), GUILayout.MinWidth(32), GUILayout.MaxWidth(32));

							Vector3 v = pb.vertices[pb.sharedIndices[u][0]];

							if(worldSpace) v = transform.TransformPoint(v);

							EditorGUI.BeginChangeCheck();

								v = EditorGUILayout.Vector3Field("", v);

							if(EditorGUI.EndChangeCheck())
							{
								if(!moving)
									OnVertexMovementBegin(pb);

								pb_Undo.RecordObject(pb, "Set Vertex Postion");

								pb.SetSharedVertexPosition(u, worldSpace ? transform.InverseTransformPoint(v) : v);

								if(pb_Editor.instance != null)
								{
									pb.RefreshUV( pb_Editor.instance.SelectedFacesInEditZone[pb] );
									pb.Refresh(RefreshMask.Normals);
									pb.msh.RecalculateBounds();
									pb_Editor.instance.UpdateSelection();
								}
							}
							index++;
						GUILayout.EndHorizontal();
					}

					GUI.backgroundColor = background;
					EditorGUIUtility.wideMode = wasWideMode;
				}
			}

			EditorGUILayout.EndScrollView();
		}

		void OnSceneGUI(SceneView sceneView)
		{
			if(selection == null)
				return;

			int labelCount = 0;

			Handles.BeginGUI();

			// Only show dropped down probuilder objects.
			foreach(KeyValuePair<pb_Object, VertexEditorSelection> selected in selection)
			{
				pb_Object pb = selected.Key;
				VertexEditorSelection sel = selected.Value;

				if(!sel.isVisible)
					continue;

				Vector3[] vertices = pb.vertices;

				foreach(int i in sel.common)
				{
					int[] indices = pb.sharedIndices[i];

					Vector3 point = pb.transform.TransformPoint(vertices[indices[0]]);

					Vector2 cen = HandleUtility.WorldToGUIPoint(point);

					pb_EditorGUIUtility.SceneLabel(i.ToString(), cen);

					if(++labelCount > MAX_SCENE_LABELS) break;
				}
			}
			Handles.EndGUI();
		}
	}
}
