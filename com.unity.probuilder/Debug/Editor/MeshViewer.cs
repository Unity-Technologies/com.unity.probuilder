using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Debug
{
	enum MeshViewState
	{
		None,
		Selected,
		All
	}

	abstract class MeshDebugView
	{
		ProBuilderMesh m_Mesh;
		MeshViewState m_ViewState;
		static GUIContent s_TempContent = new GUIContent();

		protected MeshDebugView()
		{
			ProBuilderMesh.elementSelectionChanged += SelectionChanged;
		}

		~MeshDebugView()
		{
			ProBuilderMesh.elementSelectionChanged -= SelectionChanged;
		}

		public ProBuilderMesh mesh
		{
			get { return m_Mesh; }
		}

		public MeshViewState viewState
		{
			get { return m_ViewState; }
		}

		public void SetMesh(ProBuilderMesh mesh)
		{
			m_Mesh = mesh;
			MeshAssigned();
			AnythingChanged();
		}

		public void SetViewState(MeshViewState state)
		{
			if (m_ViewState == state)
				return;
			m_ViewState = state;
			MeshViewStateChanged();
			AnythingChanged();
		}

		void SelectionChanged(ProBuilderMesh mesh)
		{
			if (mesh == m_Mesh)
			{
				SelectionChanged();
				AnythingChanged();
			}
		}

		protected virtual void MeshAssigned() {}
		protected virtual void MeshViewStateChanged() {}
		protected virtual void SelectionChanged() {}
		protected virtual void AnythingChanged() {}

		public virtual void OnGUI() { }

		public abstract void Draw(SceneView view);

		internal static void DrawSceneLabel(Vector3 worldPosition, string contents)
		{
			s_TempContent.text = contents;
			var rect = HandleUtility.WorldPointToSizedRect(worldPosition, s_TempContent, UI.EditorStyles.sceneTextBox);
			GUI.Label(rect, s_TempContent, UI.EditorStyles.sceneTextBox);
		}
	}

	sealed class MeshViewer : EditorWindow
	{
		[Serializable]
		class MeshViewSetting
		{
			[SerializeField]
			string m_Title;

			[SerializeField]
			MeshViewState m_ViewState;

			[SerializeField]
			bool m_Details;

			[SerializeField]
			string m_AssemblyQualifiedType;

			Type m_Type;

			public string title
			{
				get { return m_Title; }
			}

			public MeshViewState viewState
			{
				get { return m_ViewState; }
				set { m_ViewState = value; }
			}

			public bool detailsExpanded
			{
				get { return m_Details; }
				set { m_Details = value; }
			}

			public Type type
			{
				get
				{
					if (m_Type == null)
						m_Type = Type.GetType(m_AssemblyQualifiedType);
					return m_Type;
				}
			}

			public MeshViewSetting(string title, MeshViewState viewState, Type type)
			{
				if(!typeof(MeshDebugView).IsAssignableFrom(type))
					throw new ArgumentException("Type must be assignable to MeshDebugView.");

				m_Title = title;
				m_ViewState = viewState;
				m_AssemblyQualifiedType = type.AssemblyQualifiedName;
				m_Type = type;
			}

			public MeshDebugView GetDebugView(ProBuilderMesh mesh)
			{
				var view = (MeshDebugView)Activator.CreateInstance(type);
				view.SetViewState(m_ViewState);
				view.SetMesh(mesh);
				return view;
			}
		}

		List<MeshDebugView> m_MeshViews = new List<MeshDebugView>();

		[SerializeField]
		List<MeshViewSetting> m_MeshViewSettings = new List<MeshViewSetting>()
		{
			new MeshViewSetting("Indexes", MeshViewState.Selected, typeof(SharedVertexView)),
			new MeshViewSetting("Edges", MeshViewState.Selected, typeof(EdgeView)),
			new MeshViewSetting("Positions", MeshViewState.Selected, typeof(PositionView)),
			new MeshViewSetting("Vertex", MeshViewState.Selected, typeof(VertexView)),
			new MeshViewSetting("Face", MeshViewState.Selected, typeof(FaceView))
		};

		Vector2 m_Scroll = Vector2.zero;

		[MenuItem("Tools/Debug/Mesh Viewer")]
		static void Init()
		{
			GetWindow<MeshViewer>(false, "Mesh Viewer", true);
		}

		void OnEnable()
		{
			SceneView.onSceneGUIDelegate += OnSceneGUI;
			MeshSelection.objectSelectionChanged += SelectionChanged;
			ProBuilderMesh.elementSelectionChanged += SelectionChanged;
			EditorMeshUtility.meshOptimized += MeshOptimized;
			SelectionChanged();
		}

		void OnDisable()
		{
			EditorMeshUtility.meshOptimized -= MeshOptimized;
			ProBuilderMesh.elementSelectionChanged -= SelectionChanged;
			MeshSelection.objectSelectionChanged -= SelectionChanged;
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
		}

		void OnGUI()
		{
			m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

			foreach (var view in m_MeshViewSettings)
			{
				GUILayout.BeginVertical(UI.EditorStyles.settingsGroup);

				GUILayout.BeginHorizontal();
				GUILayout.Label(view.title, EditorStyles.boldLabel);
				GUILayout.FlexibleSpace();

				if (GUILayout.Button(view.viewState.ToString(), GUILayout.Width(100)))
				{
					view.viewState = (MeshViewState)((((int)view.viewState) + 1) % Enum.GetValues(typeof(MeshViewState)).Length);
					SetViewState(view);
				}

				GUILayout.EndHorizontal();

				view.detailsExpanded = EditorGUILayout.Foldout(view.detailsExpanded, "Details");

				if (view.detailsExpanded)
				{
					foreach(var v in m_MeshViews)
						if(view.type.IsInstanceOfType(v))
							v.OnGUI();

				}

				GUILayout.EndVertical();
			}

			EditorGUILayout.EndScrollView();
		}

		void SetViewState(MeshViewSetting settings)
		{
			foreach(var view in m_MeshViews)
				if(settings.type.IsInstanceOfType(view))
					view.SetViewState(settings.viewState);
			SceneView.RepaintAll();
		}

		void SelectionChanged()
		{
			m_MeshViews.Clear();

			foreach(var view in m_MeshViewSettings)
				foreach(var mesh in MeshSelection.Top())
					m_MeshViews.Add(view.GetDebugView(mesh));

			Repaint();
			SceneView.RepaintAll();
		}

		void SelectionChanged(ProBuilderMesh mesh)
		{
			SelectionChanged();
		}

		void MeshOptimized(ProBuilderMesh pmesh, Mesh umesh)
		{
			SelectionChanged();
		}

		void OnSceneGUI(SceneView view)
		{
			Handles.BeginGUI();

			foreach (var mesh in m_MeshViews)
			{
				if(mesh.viewState != MeshViewState.None)
					mesh.Draw(view);
			}

			Handles.EndGUI();
		}
	}
}
