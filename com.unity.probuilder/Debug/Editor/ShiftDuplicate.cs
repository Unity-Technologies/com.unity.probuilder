//using UnityEngine;
//using UnityEditor;
//using ProBuilder.Core;
//using ProBuilder.EditorCore;
//
//[InitializeOnLoad]
//static class ShiftDuplicate
//{
//	static bool m_IsDragging = false;
//
//	static ShiftDuplicate()
//	{
//		SceneView.onSceneGUIDelegate += OnSceneGUI;
//	}
//
//	static void OnSceneGUI(SceneView scn)
//	{
//		Event e = Event.current;
//
//		if(!m_IsDragging && e.modifiers == EventModifiers.Shift && e.type == EventType.MouseDrag)
//		{
//			m_IsDragging = true;
//
//			// probuilder-specific
//			if(pb_Editor.instance != null && pb_Editor.instance.editLevel == EditLevel.Geometry)
//				return;
//
//			Object[] selection = Selection.GetFiltered(typeof(Transform), SelectionMode.TopLevel);
//
//			if(selection != null && selection.Length > 0)
//			{
//				Object[] duplicates = new Object[selection.Length];
//
//				for(int i = 0, c = selection.Length; i < c; i++)
//				{
//					duplicates[i] = Object.Instantiate(selection[i]);
//					Undo.RegisterCreatedObjectUndo((duplicates[i] as Transform).gameObject, "Shift Duplicate");
//				}
//			}
//		}
//
//		if(e.type == EventType.MouseUp || e.type == EventType.Ignore)
//			m_IsDragging = false;
//	}
//}
