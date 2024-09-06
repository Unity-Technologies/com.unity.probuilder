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
		[Shortcut("ProBuilder/Editor/Edit Vertices", typeof(SceneViewMotion.SceneViewContext))]
		static void SetSelectMode_Vertex()
		{
			if (!CheckAndEnterPBContextIfNeeded())
				return;

			ProBuilderEditor.selectMode = SelectMode.Vertex;
		}
		
		[Shortcut("ProBuilder/Editor/Edit Edges", typeof(SceneViewMotion.SceneViewContext))]
		static void SetSelectMode_Edge()
		{			
			if (!CheckAndEnterPBContextIfNeeded())
				return;
			
			ProBuilderEditor.selectMode = SelectMode.Edge;
		}
		
		[Shortcut("ProBuilder/Editor/Edit Faces", typeof(SceneViewMotion.SceneViewContext))]
		static void SetSelectMode_Faces()
		{
			if (!CheckAndEnterPBContextIfNeeded())
				return;
			
			ProBuilderEditor.selectMode = SelectMode.Face;
		}

        [Shortcut("ProBuilder/Editor/Toggle ProBuilder Context", typeof(SceneViewMotion.SceneViewContext), KeyCode.G)]
        static void Toggle_ObjectElementMode()
        {
            if (ToolManager.activeContextType == typeof(GameObjectToolContext))
                ToolManager.SetActiveContext<PositionToolContext>();
            else if(ToolManager.activeContextType == typeof(PositionToolContext))
                ToolManager.SetActiveContext<GameObjectToolContext>();
        }

		[Shortcut("ProBuilder/Editor/Toggle Edit Mode", typeof(PositionToolContext.ProBuilderShortcutContext), KeyCode.H)]
		static void Toggle_SelectMode()
		{
			if(ProBuilderEditor.instance != null)
				ProBuilderEditor.instance.ToggleSelectionMode();
		}

		static bool CheckAndEnterPBContextIfNeeded()
		{
			if (ToolManager.activeContextType == typeof(GameObjectToolContext))
			{
				// Check if PositionToolContext can actually be entered
				if (EditorToolManager.GetComponentContext(typeof(PositionToolContext), true) == null)
					return false;
				
				ToolManager.SetActiveContext<PositionToolContext>();
			}

			return true;
		}
	}
}
