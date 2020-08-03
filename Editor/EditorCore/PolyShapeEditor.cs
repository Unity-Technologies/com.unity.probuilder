using UnityEngine;
//using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder;

#if !UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.EditorTools;
#else
using ToolManager = UnityEditor.EditorTools.ToolManager;
#endif


namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(PolyShape))]
    sealed class PolyShapeEditor : Editor
    {
         PolyShape polygon
         {
             get { return target as PolyShape; }
         }

         public override void OnInspectorGUI()
         {
             switch (polygon.polyEditMode)
             {
                 case PolyShape.PolyEditMode.None:
                 {
                     if(GUILayout.Button("Edit Poly Shape"))
                     {
                         polygon.polyEditMode = PolyShape.PolyEditMode.Edit;
                         PolyShapeTool tool = ScriptableObject.CreateInstance<PolyShapeTool>();
                         tool.polygon = polygon;
                         ToolManager.SetActiveTool(tool);
                     }

                     EditorGUILayout.HelpBox(
                         "Editing a poly shape will erase any modifications made to the mesh!\n\nIf you accidentally enter Edit Mode you can Undo to get your changes back.",
                         MessageType.Warning);

                     break;
                 }

                 case PolyShape.PolyEditMode.Path:
                 {
                     EditorGUILayout.HelpBox("\nClick To Add Points\n\nPress 'Enter' or 'Space' to Set Height\n", MessageType.Info);
                     break;
                 }

                 case PolyShape.PolyEditMode.Height:
                 {
                     EditorGUILayout.HelpBox("\nMove Mouse to Set Height\n\nPress 'Enter' or 'Space' to Finalize\n", MessageType.Info);
                     break;
                 }

                 case PolyShape.PolyEditMode.Edit:
                 {
                     EditorGUILayout.HelpBox("\nMove Poly Shape points to update the shape\n\nPress 'Enter' or 'Space' to Finalize\n", MessageType.Info);
                     break;
                 }
             }
         }

    }
}
