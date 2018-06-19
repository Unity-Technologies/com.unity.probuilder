using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	sealed class DebugEditor : EditorWindow
	{
		static bool utilityWindow
		{
			get { return PreferencesInternal.GetBool("ProBuilderDebugEditor::utilityWindow", false); }
			set { PreferencesInternal.SetBool("ProBuilderDebugEditor::utilityWindow", value); }
		}

		static Dictionary<string, bool> s_Expanded = new Dictionary<string, bool>();
		Vector2 m_Scroll = Vector2.zero;

		[MenuItem("Tools/ProBuilder/Debug/Debug Window")]
		static void Init()
		{
			GetWindow<DebugEditor>(utilityWindow, "ProBuilder Debug", true);
		}

		void OnEnable()
		{
			MeshSelection.objectSelectionChanged += OnSelectionChanged;
		}

		void OnDisable()
		{
			MeshSelection.objectSelectionChanged -= OnSelectionChanged;
		}

		void OnSelectionChanged()
		{
			Repaint();
		}

		void OnGUI()
		{
			var evt = Event.current;

			if (evt.type == EventType.ContextClick)
				DoContextClick();

			m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

			foreach (var mesh in MeshSelection.Top())
			{
				DoMeshInfo(mesh);
			}

			EditorGUILayout.EndScrollView();
		}

		void DoMeshInfo(ProBuilderMesh mesh)
		{
			DoSharedVertexesInfo(mesh);
			DoSharedTexturesInfo(mesh);
		}

		static void BeginSectionHeader(ProBuilderMesh mesh, string field)
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			var mi = typeof(ProBuilderMesh).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
			var id = GetPropertyId(mesh, mi.Name);
			if (!s_Expanded.ContainsKey(id))
				s_Expanded.Add(id, true);
			s_Expanded[id] = EditorGUILayout.Foldout(s_Expanded[id], mi.MemberType + " " + mi.Name);
			GUILayout.FlexibleSpace();
		}

		static void EndSectionHeader()
		{
			GUILayout.EndHorizontal();
		}

		static string GetPropertyId(ProBuilderMesh mesh, string property)
		{
			return string.Format("{0}.{1}", mesh.GetInstanceID(), property);
		}

		void DoSharedVertexesInfo(ProBuilderMesh mesh)
		{
			BeginSectionHeader(mesh, "m_SharedVertexes");
			if(GUILayout.Button("Invalidate Cache", EditorStyles.toolbarButton))
				mesh.InvalidateSharedVertexLookup();
			GUILayout.EndHorizontal();

			var sharedVertexes = mesh.sharedVertexesInternal;

			for (int i = 0; i < sharedVertexes.Length; i++)
				GUILayout.Label(sharedVertexes[i].ToString(", "));
		}

		void DoSharedTexturesInfo(ProBuilderMesh mesh)
		{
			BeginSectionHeader(mesh, "m_SharedTextures");
			if(GUILayout.Button("Invalidate Cache", EditorStyles.toolbarButton))
				mesh.InvalidateSharedTextureLookup();
			GUILayout.EndHorizontal();

			var sharedVertexes = mesh.sharedTextures;

			for (int i = 0; i < sharedVertexes.Length; i++)
				GUILayout.Label(sharedVertexes[i].ToString(", "));
		}

		void DoContextClick()
		{
			GenericMenu menu = new GenericMenu();

			menu.AddItem(new GUIContent("Window/Floating Window", ""), utilityWindow, () =>
			{
				utilityWindow = true;
				Close();
				Init();
			});

			menu.AddItem(new GUIContent("Window/Dockable Window", ""), !utilityWindow, () =>
			{
				utilityWindow = false;
				Close();
				Init();
			});

			menu.ShowAsContext();
		}
	}
}
