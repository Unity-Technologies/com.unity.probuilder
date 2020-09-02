using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Manages the ProBuilder toolbar window and tool mode.
	/// </summary>
	static class ProBuilderEditorShortcuts
	{
		[Shortcut("ProBuilder/Editor/Edit Objects", typeof(SceneView))]
		static void SetSelectMode_Object()
		{
			ProBuilderEditor.selectMode = SelectMode.Object;
		}

		[Shortcut("ProBuilder/Editor/Edit Vertices", typeof(SceneView))]
		static void SetSelectMode_Vertex()
		{
			ProBuilderEditor.selectMode = SelectMode.Vertex;
		}

		[Shortcut("ProBuilder/Editor/Edit Edges", typeof(SceneView))]
		static void SetSelectMode_Edge()
		{
			ProBuilderEditor.selectMode = SelectMode.Edge;
		}

		[Shortcut("ProBuilder/Editor/Edit Faces", typeof(SceneView))]
		static void SetSelectMode_Faces()
		{
			ProBuilderEditor.selectMode = SelectMode.Face;
		}

		[Shortcut("ProBuilder/Editor/Toggle Geometry Mode", typeof(SceneView), KeyCode.G)]
		static void Toggle_ObjectElementMode()
		{
			if (ProBuilderEditor.selectMode == SelectMode.Object)
				ProBuilderEditor.ResetToLastSelectMode();
			else
				ProBuilderEditor.selectMode = SelectMode.Object;
		}

		[Shortcut("ProBuilder/Editor/Toggle Select Mode", typeof(SceneView), KeyCode.H)]
		static void Toggle_SelectMode()
		{
			if(ProBuilderEditor.instance != null)
				ProBuilderEditor.instance.ToggleSelectionMode();
		}
	}
}
