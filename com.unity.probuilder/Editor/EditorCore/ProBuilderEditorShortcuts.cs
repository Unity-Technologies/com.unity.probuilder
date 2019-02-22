#if UNITY_2019_1_OR_NEWER
#define SHORTCUT_MANAGER
#endif

#if SHORTCUT_MANAGER

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
		[Shortcut("ProBuilder/Editor/Edit Objects", typeof(SceneView))] //, KeyCode.G)]
		static void SetSelectMode_Object()
		{
			ProBuilderEditor.selectMode = SelectMode.Object;
		}

		[Shortcut("ProBuilder/Editor/Edit Vertices", typeof(SceneView))] //, KeyCode.H)]
		static void SetSelectMode_Vertex()
		{
			ProBuilderEditor.selectMode = SelectMode.Object;
		}

		[Shortcut("ProBuilder/Editor/Edit Edges", typeof(SceneView))] //, KeyCode.J)]
		static void SetSelectMode_Edge()
		{
			ProBuilderEditor.selectMode = SelectMode.Object;
		}

		[Shortcut("ProBuilder/Editor/Edit Faces", typeof(SceneView))] //, KeyCode.K)]
		static void SetSelectMode_Faces()
		{
			ProBuilderEditor.selectMode = SelectMode.Object;
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

#endif
