using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
using EditorToolManager = UnityEditor.EditorTools.EditorToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
using EditorToolManager = UnityEditor.EditorTools.EditorToolContext;
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
                         ToolManager.SetActiveTool<PolyShapeTool>();
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

             EditorGUI.BeginChangeCheck();

             float extrude = polygon.extrude;
             extrude = EditorGUILayout.FloatField("Extrusion", extrude);

             bool flipNormals = polygon.flipNormals;
             flipNormals = EditorGUILayout.Toggle("Flip Normals", flipNormals);

             if (EditorGUI.EndChangeCheck())
             {
                 if (polygon.polyEditMode == PolyShape.PolyEditMode.None)
                 {
                     if (ProBuilderEditor.instance != null)
                         ProBuilderEditor.instance.ClearElementSelection();

                     UndoUtility.RecordComponents<ProBuilderMesh,PolyShape>(polygon.GetComponents(typeof(Component)), "Edit Polygon Shape");
                 }
                 else
                 {
                     UndoUtility.RecordObject(polygon, "Change Polygon Shape Settings");
                 }

                 polygon.extrude = extrude;
                 polygon.flipNormals = flipNormals;

                 RebuildPolyShapeMesh(polygon);
             }
         }

         void RebuildPolyShapeMesh(bool vertexCountChanged = false)
         {
             // If Undo is called immediately after creation this situation can occur
             if (polygon == null)
                 return;

             if(ToolManager.activeToolType == typeof(PolyShapeTool))
             {
                 PolyShapeTool tool = ((PolyShapeTool)EditorToolManager.activeTool);
                 if(tool.polygon == polygon)
                     tool.RebuildPolyShapeMesh(vertexCountChanged);
             }

             if (polygon.polyEditMode != PolyShape.PolyEditMode.Path)
             {
                 var result = polygon.CreateShapeFromPolygon();
             }

             // While the vertex count may not change, the triangle winding might. So unfortunately we can't take
             // advantage of the `vertexCountChanged = false` optimization here.
             ProBuilderEditor.Refresh();
         }
    }
}
