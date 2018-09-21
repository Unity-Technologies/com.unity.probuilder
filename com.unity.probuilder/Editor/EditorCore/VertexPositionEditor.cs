using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// A simple line-item editor for vertex positions.
	/// </summary>
	sealed class VertexPositionEditor : EditorWindow
	{
		const int MAX_SCENE_LABELS = 100;

		class VertexEditorSelection
		{
			public bool isVisible = false;
			public IEnumerable<int> common;

			public VertexEditorSelection(ProBuilderMesh mesh, bool visible, IEnumerable<int> indexes)
			{
				this.isVisible = visible;
				this.common = mesh.GetCoincidentVertices(indexes);
			}
		}

		Dictionary<ProBuilderMesh, VertexEditorSelection> selection = new Dictionary<ProBuilderMesh, VertexEditorSelection>();

		static Color EVEN;
		static Color ODD;

		Vector2 scroll = Vector2.zero;
		bool moving = false;
		public bool worldSpace = true;

		public static void MenuOpenVertexEditor()
		{
			EditorWindow.GetWindow<VertexPositionEditor>(true, "Positions Editor", true);
		}

		void OnEnable()
		{
			EVEN = EditorGUIUtility.isProSkin ? new Color(.18f, .18f, .18f, 1f) : new Color(.85f, .85f, .85f, 1f);
			ODD = EditorGUIUtility.isProSkin ? new Color(.15f, .15f, .15f, 1f) : new Color(.80f, .80f, .80f, 1f);

			ProBuilderEditor.selectionUpdated += OnSelectionUpdate;
			SceneView.onSceneGUIDelegate += OnSceneGUI;

			if(ProBuilderEditor.instance != null)
				OnSelectionUpdate(ProBuilderEditor.instance.selection);
		}

		void OnDisable()
		{
			ProBuilderEditor.selectionUpdated -= OnSelectionUpdate;
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
		}

		void OnSelectionUpdate(ProBuilderMesh[] newSelection)
		{
			if(newSelection == null)
			{
				if(selection != null)
					selection.Clear();

				return;
			}

			var res = new Dictionary<ProBuilderMesh, VertexEditorSelection>();

			foreach(var mesh in newSelection)
			{
				VertexEditorSelection sel;

				if(selection.TryGetValue(mesh, out sel))
				{
					sel.common = mesh.GetCoincidentVertices(mesh.selectedIndexesInternal);
					res.Add(mesh, sel);
				}
				else
				{
					res.Add(mesh, new VertexEditorSelection(mesh, true, mesh.selectedIndexesInternal));
				}
			}

			selection = res;

			this.Repaint();
		}

		void OnVertexMovementBegin(ProBuilderMesh pb)
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

				GUIStyle style = worldSpace ? EditorStyles.toolbarButton : UI.EditorGUIUtility.GetOnStyle(EditorStyles.toolbarButton);

				if( GUILayout.Button(worldSpace ? "World Space" : "Model Space", style) )
					worldSpace = !worldSpace;

			GUILayout.EndHorizontal();

			if(selection == null || selection.Count < 1 || !selection.Any(x => x.Key.selectedVertexCount > 0))
			{
				GUILayout.FlexibleSpace();
				GUILayout.Label("Select a ProBuilder Mesh", UI.EditorGUIUtility.CenteredGreyMiniLabel);
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
				ProBuilderMesh pb = kvp.Key;
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
						GUILayout.BeginHorizontal(UI.EditorGUIUtility.solidBackgroundStyle);
						GUI.backgroundColor = background;

							GUILayout.Label(u.ToString(), GUILayout.MinWidth(32), GUILayout.MaxWidth(32));

							Vector3 v = pb.positionsInternal[pb.sharedVerticesInternal[u][0]];

							if(worldSpace) v = transform.TransformPoint(v);

							EditorGUI.BeginChangeCheck();

								v = EditorGUILayout.Vector3Field("", v);

							if(EditorGUI.EndChangeCheck())
							{
								if(!moving)
									OnVertexMovementBegin(pb);

								UndoUtility.RecordObject(pb, "Set Vertex Postion");

								pb.SetSharedVertexPosition(u, worldSpace ? transform.InverseTransformPoint(v) : v);

								if(ProBuilderEditor.instance != null)
								{
									pb.RefreshUV( MeshSelection.selectedFacesInEditZone[pb] );
									pb.Refresh(RefreshMask.Normals);
									pb.mesh.RecalculateBounds();
									ProBuilderEditor.Refresh();
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
			foreach(KeyValuePair<ProBuilderMesh, VertexEditorSelection> selected in selection)
			{
				ProBuilderMesh pb = selected.Key;
				VertexEditorSelection sel = selected.Value;

				if(!sel.isVisible)
					continue;

				Vector3[] positions = pb.positionsInternal;

				foreach(int i in sel.common)
				{
					var indexes = pb.sharedVerticesInternal[i];

					Vector3 point = pb.transform.TransformPoint(positions[indexes[0]]);

					Vector2 cen = HandleUtility.WorldToGUIPoint(point);

					UI.EditorGUIUtility.SceneLabel(i.ToString(), cen);

					if(++labelCount > MAX_SCENE_LABELS) break;
				}
			}
			Handles.EndGUI();
		}
	}
}
