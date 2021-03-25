using System;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;
using Plane = UnityEngine.ProBuilder.Shapes.Plane;
using Sprite = UnityEngine.ProBuilder.Shapes.Sprite;
#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ProBuilderShape))]
    class ProBuilderShapeEditor : Editor
    {
        IMGUIContainer m_ShapeField;

        SerializedProperty m_ShapeProperty;
        SerializedProperty ShapeProperty
        {
            get
            {
                if(m_ShapeProperty == null)
                    m_ShapeProperty = serializedObject.FindProperty("m_Shape");
                return m_ShapeProperty;
            }
        }

        SerializedProperty m_ShapePivotProperty;
        SerializedProperty ShapePivotProperty
        {
            get
            {
                if(m_ShapePivotProperty == null)
                    m_ShapePivotProperty = serializedObject.FindProperty("m_PivotLocation");
                return m_ShapePivotProperty;
            }
        }

        SerializedProperty m_ShapeSizeXProperty;
        SerializedProperty ShapeSizeXProperty
        {
            get
            {
                if(m_ShapeSizeXProperty == null)
                    m_ShapeSizeXProperty = serializedObject.FindProperty("m_Size.x");
                return m_ShapeSizeXProperty;
            }
        }

        SerializedProperty m_ShapeSizeYProperty;
        SerializedProperty ShapeSizeYProperty
        {
            get
            {
                if(m_ShapeSizeYProperty == null)
                    m_ShapeSizeYProperty = serializedObject.FindProperty("m_Size.y");
                return m_ShapeSizeYProperty;
            }
        }

        SerializedProperty m_ShapeSizeZProperty;
        SerializedProperty ShapeSizeZProperty
        {
            get
            {
                if(m_ShapeSizeZProperty == null)
                    m_ShapeSizeZProperty = serializedObject.FindProperty("m_Size.z");
                return m_ShapeSizeZProperty;
            }
        }

        int m_ActiveShapeIndex = 0;

        static bool s_foldoutEnabled = true;

        public GUIContent m_ShapePropertyLabel = new GUIContent("Shape Properties");
        readonly GUIContent k_ShapePivotLabel = new GUIContent("Pivot");
        readonly GUIContent k_ShapeSizeXLabel = new GUIContent("X");
        readonly GUIContent k_ShapeSizeYLabel = new GUIContent("Y");
        readonly GUIContent k_ShapeSizeZLabel = new GUIContent("Z");

        const string k_dialogTitle = "Shape reset";
        const string k_dialogText = "The current shape has been edited, you will loose all modifications.";

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

        void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            DrawShapeGUI();
            DrawShapeParametersGUI();

            if(ToolManager.activeToolType != typeof(DrawShapeTool)
               && ToolManager.activeToolType != typeof(EditShapeTool) )
            {
                if(GUILayout.Button("Edit Shape"))
                {
                    ProBuilderEditor.selectMode = SelectMode.Object;
                    ToolManager.SetActiveTool<EditShapeTool>();
                }
            }
        }

        public void DrawShapeGUI(DrawShapeTool tool = null)
        {
            if(target == null || serializedObject == null)
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
                        shapeComponent.UpdateComponent();
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

                EditorGUI.BeginChangeCheck();
                m_ActiveShapeIndex = HasMultipleShapeTypes
                    ? -1
                    : Mathf.Max(-1, Array.IndexOf(EditorShapeUtility.availableShapeTypes, m_CurrentShapeType));
                m_ActiveShapeIndex = EditorGUILayout.Popup(m_ActiveShapeIndex, EditorShapeUtility.shapeTypes);

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
                            proBuilderShape.SetShape(EditorShapeUtility.CreateShape(type), proBuilderShape.pivotLocation);
                            ProBuilderEditor.Refresh();
                        }
                    }
                }

                if(tool)
                    EditorGUILayout.PropertyField(ShapePivotProperty, k_ShapePivotLabel);

                var labelWidth = EditorGUIUtility.labelWidth;
                var fieldWidth = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.labelWidth = 30;
                EditorGUIUtility.fieldWidth = 30;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ShapeSizeXProperty, k_ShapeSizeXLabel);
                var is2D = HasMultipleShapeTypes ||
                           ( m_CurrentShapeType != typeof(Plane) && m_CurrentShapeType != typeof(Sprite) );
                using (new EditorGUI.DisabledScope(!is2D))
                    EditorGUILayout.PropertyField(ShapeSizeYProperty, k_ShapeSizeYLabel);
                EditorGUILayout.PropertyField(ShapeSizeZProperty, k_ShapeSizeZLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = labelWidth;
                EditorGUIUtility.fieldWidth = fieldWidth;

                EditorGUI.indentLevel--;
            }

            if(!HasMultipleShapeTypes)
                EditorGUILayout.PropertyField(ShapeProperty, new GUIContent("Shape Properties"), true);

            if (serializedObject.ApplyModifiedProperties())
            {
                foreach(var comp in targets)
                {
                    var shapeComponent = comp as ProBuilderShape;
                    if(shapeComponent.isEditable)
                    {
                        UndoUtility.RecordComponents<Transform, ProBuilderMesh, ProBuilderShape>(shapeComponent.GetComponents(typeof(Component)),"Resize Shape");
                        shapeComponent.UpdateComponent();
                        if(tool != null)
                        {
                            tool.SetBounds(shapeComponent.size);
                            DrawShapeTool.SaveShapeParams(shapeComponent);
                        }
                        ProBuilderEditor.Refresh();
                    }
                }
            }

            GUI.enabled = true;
        }
    }
}
