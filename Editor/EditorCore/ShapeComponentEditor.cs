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
    [CustomEditor(typeof(ShapeComponent))]
    class ShapeComponentEditor : Editor
    {
        IMGUIContainer m_ShapeField;

        SerializedProperty m_ShapeProperty;
        SerializedProperty m_ShapePivotProperty;
        SerializedProperty m_ShapeWidthProperty;
        SerializedProperty m_ShapeLengthProperty;
        SerializedProperty m_ShapeHeightProperty;

        int m_ActiveShapeIndex = 0;

        static bool s_foldoutEnabled = true;

        public GUIContent m_ShapePropertyLabel = new GUIContent("Shape Properties");
        readonly GUIContent k_ShapePivotLabel = new GUIContent("Pivot");
        readonly GUIContent k_ShapeWidthLabel = new GUIContent("Width");
        readonly GUIContent k_ShapeLengthLabel = new GUIContent("Length");
        readonly GUIContent k_ShapeHeightLabel = new GUIContent("Height");

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
                        m_CurrentShapeType = ( (ShapeComponent) comp ).shape.GetType();
                    else if( m_CurrentShapeType != ( (ShapeComponent) comp ).shape.GetType() )
                        return true;
                }

                return false;
            }
        }

        Type m_CurrentShapeType;

        void OnEnable()
        {
            m_ShapeProperty = serializedObject.FindProperty("m_Shape");
            m_ShapePivotProperty = serializedObject.FindProperty("m_PivotLocation");
            m_ShapeWidthProperty = serializedObject.FindProperty("m_Properties.m_Width");
            m_ShapeLengthProperty = serializedObject.FindProperty("m_Properties.m_Length");
            m_ShapeHeightProperty = serializedObject.FindProperty("m_Properties.m_Height");
        }

        public override void OnInspectorGUI()
        {
            DrawShapeGUI();
            DrawShapeParametersGUI();
        }

        public void DrawShapeGUI(DrawShapeTool tool = null)
        {
            if(target == null || serializedObject == null)
                return;

            serializedObject.Update();

            int editedShapesCount = 0;
            foreach(var comp in targets)
                editedShapesCount += ( (ShapeComponent) comp ).edited ? 1 : 0;

            if(editedShapesCount > 0)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.HelpBox(
                    L10n.Tr(
                        "You have manually modified Shape(s). Revert manual changes to access to procedural parameters"),
                    MessageType.Info);

                if(GUILayout.Button("Reset Shape"))
                {
                    foreach(var comp in targets)
                    {
                        var shapeComponent = comp as ShapeComponent;
                        UndoUtility.RecordComponents<Transform, ProBuilderMesh, ShapeComponent>(shapeComponent.GetComponents(typeof(Component)),"Reset Shape");
                        if( shapeComponent.edited )
                        {
                            shapeComponent.edited = false;
                            shapeComponent.UpdateComponent();
                            ProBuilderEditor.Refresh();
                        }
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

            serializedObject.Update ();

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
                        ShapeComponent shapeComponent = ( (ShapeComponent) comp );
                        Shape shape = shapeComponent.shape;
                        if(shape.GetType() != type)
                        {
                            if(tool != null)
                                DrawShapeTool.s_ActiveShapeIndex.value = m_ActiveShapeIndex;
                            UndoUtility.RecordComponents<Transform, ProBuilderMesh, ShapeComponent>(shapeComponent.GetComponents(typeof(Component)),"Change Shape");
                            shapeComponent.SetShape(EditorShapeUtility.CreateShape(type, shape));
                            ProBuilderEditor.Refresh();
                        }
                    }
                }

                EditorGUILayout.PropertyField(m_ShapePivotProperty, k_ShapePivotLabel);

                EditorGUILayout.PropertyField(m_ShapeWidthProperty, k_ShapeWidthLabel);
                EditorGUILayout.PropertyField(m_ShapeLengthProperty, k_ShapeLengthLabel);
                if(HasMultipleShapeTypes || (m_CurrentShapeType != typeof(Plane) &&  m_CurrentShapeType != typeof(Sprite)))
                    EditorGUILayout.PropertyField(m_ShapeHeightProperty, k_ShapeHeightLabel);

                EditorGUI.indentLevel--;
            }

            if(!HasMultipleShapeTypes)
                EditorGUILayout.PropertyField(m_ShapeProperty, new GUIContent("Shape Properties"), true);

            if (serializedObject.ApplyModifiedProperties())
            {
                foreach(var comp in targets)
                {
                    var shapeComponent = comp as ShapeComponent;
                    if(!shapeComponent.edited)
                    {
                        UndoUtility.RecordComponents<Transform, ProBuilderMesh, ShapeComponent>(shapeComponent.GetComponents(typeof(Component)),"Resize Shape");
                        shapeComponent.UpdateComponent();
                        if(tool != null)
                            tool.SetBounds(shapeComponent.size);
                        EditorShapeUtility.SaveParams(shapeComponent.shape);
                        ProBuilderEditor.Refresh();
                    }
                }
            }

            GUI.enabled = true;
        }
    }
}
