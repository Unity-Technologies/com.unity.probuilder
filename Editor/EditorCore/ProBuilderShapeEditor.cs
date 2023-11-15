using System;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using ToolManager = UnityEditor.EditorTools.ToolManager;

namespace UnityEditor.ProBuilder
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ProBuilderShape))]
    class ProBuilderShapeEditor : Editor
    {
        SerializedProperty m_ShapeProperty = null;
        SerializedProperty shapeProperty
        {
            get
            {
                if(m_ShapeProperty == null)
                    m_ShapeProperty = serializedObject.FindProperty("m_Shape");
                return m_ShapeProperty;
            }
        }

        SerializedProperty m_ShapeSizeProperty = null;
        SerializedProperty shapeSizeProperty
        {
            get
            {
                if(m_ShapeSizeProperty == null)
                    m_ShapeSizeProperty = serializedObject.FindProperty("m_Size");
                return m_ShapeSizeProperty;
            }
        }

        int m_ActiveShapeIndex = 0;

        static bool s_foldoutEnabled = true;

        public GUIContent m_ShapePropertyLabel = new GUIContent("Shape Properties");
        readonly GUIContent k_ShapePivotLabel = new GUIContent("Pivot");

        bool HasMultipleShapeTypes
        {
            get
            {
                m_CurrentShapeType = null;
                foreach(var comp in targets)
                {
                    if(m_CurrentShapeType == null)
                        m_CurrentShapeType = ( (ProBuilderShape) comp ).shape.GetType();
                    else if( m_CurrentShapeType != ( (ProBuilderShape) comp ).shape.GetType() )
                        return true;
                }

                return false;
            }
        }

        Type m_CurrentShapeType;

        public override void OnInspectorGUI()
        {
            DrawShapeGUI();
            DrawShapeParametersGUI();

            if(ToolManager.activeToolType != typeof(DrawShapeTool)
               && ToolManager.activeToolType != typeof(EditShapeTool))
            {
                if(GUILayout.Button("Edit Shape"))
                    ToolManager.SetActiveTool<EditShapeTool>();
            }
        }

        public void DrawShapeGUI()
        {
            if(target == null)
                return;

            serializedObject.Update();

            int editedShapesCount = 0;
            foreach(var comp in targets)
                editedShapesCount += ( (ProBuilderShape) comp ).isEditable ? 0 : 1;

            if(editedShapesCount > 0)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.HelpBox(
                    L10n.Tr(
                        "You have manually modified the selected shape(s). Reset the shape to remove all manual changes in order to proceed."),
                    MessageType.Info);

                if(GUILayout.Button("Reset Shape"))
                {
                    foreach(var comp in targets)
                    {
                        var shapeComponent = comp as ProBuilderShape;
                        UndoUtility.RecordComponents<Transform, ProBuilderMesh, ProBuilderShape>(shapeComponent.GetComponents(typeof(Component)),"Reset Shape");
                        shapeComponent.UpdateShape();
                        ProBuilderEditor.Refresh();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if(editedShapesCount == targets.Length)
                GUI.enabled = false;
        }

        public void DrawShapeParametersGUI(DrawShapeTool tool = null)
        {
            if(target == null || serializedObject == null)
                return;

            serializedObject.Update();

            var foldoutEnabled = tool == null ? s_foldoutEnabled : DrawShapeTool.s_SettingsEnabled.value;
            foldoutEnabled = EditorGUILayout.Foldout(foldoutEnabled, m_ShapePropertyLabel, true);

            if(tool == null)
                s_foldoutEnabled = foldoutEnabled;
            else
                DrawShapeTool.s_SettingsEnabled.value = foldoutEnabled;

            if(foldoutEnabled)
            {
                EditorGUI.indentLevel++;
                EditorGUIUtility.labelWidth = 90;

                if (tool)
                    tool.pivotLocation = (PivotLocation)EditorGUILayout.EnumPopup(k_ShapePivotLabel, tool.pivotLocation);
                else
                {
                    EditorGUI.BeginChangeCheck();
                    m_ActiveShapeIndex = HasMultipleShapeTypes
                        ? -1
                        : Mathf.Max(-1, Array.IndexOf(EditorShapeUtility.availableShapeTypes, m_CurrentShapeType));
                    m_ActiveShapeIndex = EditorGUILayout.Popup("Shape", m_ActiveShapeIndex, EditorShapeUtility.shapeTypes);

                    if(EditorGUI.EndChangeCheck())
                    {
                        var type = EditorShapeUtility.availableShapeTypes[m_ActiveShapeIndex];
                        foreach(var comp in targets)
                        {
                            ProBuilderShape proBuilderShape = ( (ProBuilderShape) comp );
                            Shape shape = proBuilderShape.shape;
                            if(shape.GetType() != type)
                            {
                                if(tool != null)
                                    DrawShapeTool.s_ActiveShapeIndex.value = m_ActiveShapeIndex;

                                UndoUtility.RecordComponents<Transform, ProBuilderMesh, ProBuilderShape>(new [] { proBuilderShape },"Change Shape");
                                proBuilderShape.SetShape(EditorShapeUtility.CreateShape(type));
                                ProBuilderEditor.Refresh();
                            }
                        }
                    }
                }


                EditorGUILayout.PropertyField(shapeSizeProperty);
                EditorGUI.indentLevel--;
            }

            if(!HasMultipleShapeTypes)
                EditorGUILayout.PropertyField(shapeProperty, new GUIContent("Shape Properties"), true);

            if (serializedObject.ApplyModifiedProperties())
            {
                foreach(var comp in targets)
                {
                    if(comp is ProBuilderShape shapeComponent && shapeComponent.isEditable)
                    {
                        UndoUtility.RecordComponents<Transform, ProBuilderMesh, ProBuilderShape>(shapeComponent.GetComponents(typeof(Component)),"Resize Shape");
                        shapeComponent.UpdateShape();
                        if(tool != null)
                        {
                            tool.SetBounds(shapeComponent.size);
                            tool.SaveShapeParams(shapeComponent);
                        }
                        ProBuilderEditor.Refresh();
                    }
                }
            }

            GUI.enabled = true;
        }
    }
}
