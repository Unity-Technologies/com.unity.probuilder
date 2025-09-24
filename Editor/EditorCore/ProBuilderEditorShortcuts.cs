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

		[Shortcut("ProBuilder/Editor/Toggle Edit Mode", typeof(PositionToolContext.ProBuilderShortcutContext), KeyCode.H)]
		static void Toggle_SelectMode()
		{
			if(ProBuilderEditor.instance != null)
				ProBuilderEditor.instance.ToggleSelectionMode();
		}

		[Shortcut("ProBuilder/Editor/Delete Selection", typeof(SceneViewMotion.SceneViewContext), KeyCode.Backspace)]
		static void DeleteSelection_Shortcut()
		{
			if (!CheckAndEnterPBContextIfNeeded())
				return;

			if (ProBuilderEditor.selectMode == SelectMode.Vertex)
			{
				// Invoke the DeleteVertices toolbar action.
				EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.DeleteVertices>().PerformAction());
			}
			else
			{
				EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.DeleteFaces>().PerformAction());
			}
		}

		static bool CheckAndEnterPBContextIfNeeded()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext))
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
