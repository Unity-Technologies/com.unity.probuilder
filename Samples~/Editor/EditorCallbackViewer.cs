using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace ProBuilder.EditorExamples
{
	sealed class EditorCallbackViewer : EditorWindow
	{
		List<string> m_Logs = new List<string>();
		Vector2 m_Scroll = Vector2.zero;
		bool m_Collapse = true;

		[MenuItem("Tools/ProBuilder/API Examples/Log Callbacks Window")]
		static void MenuInitEditorCallbackViewer()
		{
			GetWindow<EditorCallbackViewer>(true, "ProBuilder Callbacks", true).Show();
		}

		static Color logBackgroundColor
		{
			get { return EditorGUIUtility.isProSkin ? new Color(.15f, .15f, .15f, .5f) : new Color(.8f, .8f, .8f, 1f); }
		}

		static Color disabledColor
		{
			get { return EditorGUIUtility.isProSkin ? new Color(.3f, .3f, .3f, .5f) : new Color(.8f, .8f, .8f, 1f); }
		}

		void OnEnable()
		{
			ProBuilderEditor.selectModeChanged += SelectModeChanged;
			EditorUtility.meshCreated += MeshCreated;
			ProBuilderEditor.selectionUpdated += SelectionUpdated;
			ProBuilderEditor.beforeMeshModification += BeforeMeshModification;
			ProBuilderEditor.afterMeshModification += AfterMeshModification;
			EditorMeshUtility.meshOptimized += MeshOptimized;
		}

		void OnDisable()
		{
			ProBuilderEditor.selectModeChanged -= SelectModeChanged;
			EditorUtility.meshCreated -= MeshCreated;
			ProBuilderEditor.selectionUpdated -= SelectionUpdated;
			ProBuilderEditor.beforeMeshModification -= BeforeMeshModification;
			ProBuilderEditor.afterMeshModification -= AfterMeshModification;
			EditorMeshUtility.meshOptimized -= MeshOptimized;
		}

		void BeforeMeshModification(IEnumerable<ProBuilderMesh> selection)
		{
			AddLog("Began Moving Vertices");
		}

		void AfterMeshModification(IEnumerable<ProBuilderMesh> selection)
		{
			AddLog("Finished Moving Vertices");
		}

		void SelectModeChanged(SelectMode mode)
		{
			AddLog("Selection Mode Changed: " + mode);
		}

		void MeshCreated(ProBuilderMesh mesh)
		{
			AddLog("Instantiated new ProBuilder Object: " + mesh.name);
		}

		void SelectionUpdated(IEnumerable<ProBuilderMesh> selection)
		{
			AddLog("Selection Updated: " + string.Format("{0} objects selected.", selection != null ? selection.Count() : 0));
		}

		void MeshOptimized(ProBuilderMesh pmesh, Mesh umesh)
		{
			AddLog(string.Format("Mesh {0} rebuilt", pmesh.name));
		}

		void AddLog(string summary)
		{
			m_Logs.Add(summary);
			Repaint();
		}

		void OnGUI()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);

			GUILayout.FlexibleSpace();

			GUI.backgroundColor = m_Collapse ? disabledColor : Color.white;
			if (GUILayout.Button("Collapse", EditorStyles.toolbarButton))
				m_Collapse = !m_Collapse;
			GUI.backgroundColor = Color.white;

			if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
				m_Logs.Clear();

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Callback Log", EditorStyles.boldLabel);
			GUILayout.EndHorizontal();

			Rect r = GUILayoutUtility.GetLastRect();
			r.x = 0;
			r.y = r.y + r.height + 6;
			r.width = this.position.width;
			r.height = this.position.height;

			GUILayout.Space(4);

			m_Scroll = GUILayout.BeginScrollView(m_Scroll);

			int len = m_Logs.Count;
			int min = System.Math.Max(0, len - 1024);

			for (int i = len - 1; i >= min; i--)
			{
				if (m_Collapse &&
				    i > 0 &&
				    i < len - 1 &&
				    m_Logs[i].Equals(m_Logs[i - 1]) &&
				    m_Logs[i].Equals(m_Logs[i + 1]))
					continue;

				GUILayout.Label(string.Format("{0,3}: {1}", i, m_Logs[i]));
			}

			GUILayout.EndScrollView();
		}
	}
}
