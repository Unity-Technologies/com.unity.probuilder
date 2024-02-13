using UnityEditor.EditorTools;
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
		[Shortcut("ProBuilder/Editor/Edit Vertices", typeof(PositionToolContext.ProBuilderShortcutContext))]
		static void SetSelectMode_Vertex()
		{
			ProBuilderEditor.selectMode = SelectMode.Vertex;
		}

		[Shortcut("ProBuilder/Editor/Edit Edges", typeof(PositionToolContext.ProBuilderShortcutContext))]
		static void SetSelectMode_Edge()
		{
			ProBuilderEditor.selectMode = SelectMode.Edge;
		}

		[Shortcut("ProBuilder/Editor/Edit Faces", typeof(PositionToolContext.ProBuilderShortcutContext))]
		static void SetSelectMode_Faces()
		{
			ProBuilderEditor.selectMode = SelectMode.Face;
		}

        [Shortcut("ProBuilder/Editor/Toggle Geometry Mode", typeof(PositionToolContext.ProBuilderShortcutContext), KeyCode.G)]
        static void Toggle_ObjectElementMode()
        {
            ToolManager.SetActiveContext<PositionToolContext>();
        }

		[Shortcut("ProBuilder/Editor/Toggle Select Mode", typeof(PositionToolContext.ProBuilderShortcutContext), KeyCode.H)]
		static void Toggle_SelectMode()
		{
			if(ProBuilderEditor.instance != null)
				ProBuilderEditor.instance.ToggleSelectionMode();
		}
	}
}
