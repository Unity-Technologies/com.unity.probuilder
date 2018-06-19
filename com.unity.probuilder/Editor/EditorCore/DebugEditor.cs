using System.Collections.Generic;
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

		[MenuItem("Tools/ProBuilder/Debug/Debug Window")]
		static void Init()
		{
			GetWindow<DebugEditor>(utilityWindow, "ProBuilder Debug", true);
		}

		void OnEnable()
		{
			MeshSelection.objectSelectionChanged += Repaint;
		}

		void OnDisable()
		{
			MeshSelection.objectSelectionChanged -= Repaint;
		}

		void OnGUI()
		{
			var evt = Event.current;

			if (evt.type == EventType.ContextClick)
				DoContextClick();

			foreach (var mesh in MeshSelection.Top())
			{
				DoMeshInfo(mesh);
			}
		}

		void DoMeshInfo(ProBuilderMesh mesh)
		{

			DoSharedVertexesInfo(mesh);
		}

		void DoSharedVertexesInfo(ProBuilderMesh mesh)
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			EditorGUILayout.Foldout(true, "SharedVertex[] m_SharedVertexes");
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Invalidate Cache", EditorStyles.toolbarButton))
				mesh.InvalidateSharedVertexLookup();
			GUILayout.EndHorizontal();

			var sharedVertexes = mesh.sharedVertexesInternal;

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
