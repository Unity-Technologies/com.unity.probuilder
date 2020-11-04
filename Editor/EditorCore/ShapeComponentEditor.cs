using System;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(ShapeComponent))]
    class ShapeComponentEditor : Editor
    {
        ShapeComponent m_ShapeComponent;
        IMGUIContainer m_ShapeField;

        SerializedProperty m_shape;
        string[] m_ShapeTypes;
        Type[] m_AvailableShapeTypes;

        int m_ActiveShapeIndex = 0;

        const string k_dialogTitle = "Shape reset";
        const string k_dialogText = "The current shape has been edited, you will loose all modifications.";

        ShapeComponentEditor()
        {
            m_AvailableShapeTypes =  TypeCache.GetTypesWithAttribute<ShapeAttribute>().Where(t => t.BaseType == typeof(Shape)).ToArray();
            m_ShapeTypes = m_AvailableShapeTypes.Select(
                x => ((ShapeAttribute)System.Attribute.GetCustomAttribute(x, typeof(ShapeAttribute))).name)
                .ToArray();
        }

        private void OnEnable()
        {
            m_ShapeComponent = target as ShapeComponent;
            m_shape = serializedObject.FindProperty("m_Shape");
            m_ActiveShapeIndex = Array.IndexOf( m_AvailableShapeTypes, m_ShapeComponent.shape.GetType());

            Undo.undoRedoPerformed += UndoRedoPerformedOnShapeEditor;
        }

        void UndoRedoPerformedOnShapeEditor()
        {
            if(m_ShapeComponent != null)
                m_ShapeComponent.Rebuild();
        }

        public override void OnInspectorGUI()
        {
            DrawShapeGUI((ShapeComponent)target, serializedObject);
        }

        public void DrawShapeGUI(DrawShapeTool tool)
        {
            DrawShapeGUI((ShapeComponent)target, serializedObject, tool);
        }

        private void DrawShapeGUI(ShapeComponent shapeComp, SerializedObject obj, DrawShapeTool tool = null)
        {
            if (shapeComp == null || obj == null)
                return;

            var shape = shapeComp.shape;
            obj.Update();
            EditorGUI.BeginChangeCheck();

            if(shape is CustomShape)
                EditorGUILayout.HelpBox(L10n.Tr("You are using a Custom Shape. Selecting another shape will lose your changes"), MessageType.Info);

            var shapeProperty = obj.FindProperty("m_Shape");
            m_ActiveShapeIndex = Mathf.Max(-1, Array.IndexOf( m_AvailableShapeTypes, shape.GetType()));
            m_ActiveShapeIndex = EditorGUILayout.Popup(m_ActiveShapeIndex, m_ShapeTypes);

            if (EditorGUI.EndChangeCheck())
            {
                UndoUtility.RegisterCompleteObjectUndo(shapeComp, "Change Shape");
                var type = m_AvailableShapeTypes[m_ActiveShapeIndex];
                shapeComp.SetShape(EditorShapeUtility.CreateShape(type));
                ProBuilderEditor.Refresh();
            }

            if(shapeComp.edited)
            {
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Reset Shape"))
                {
                    if(UnityEditor.EditorUtility.DisplayDialog(
                        k_dialogTitle, k_dialogText,
                        "Continue", "Cancel"))
                    {
                        shapeComp.edited = false;
                        shapeComp.Rebuild();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.BeginChangeCheck();
            shapeComp.size = EditorGUILayout.Vector3Field("Size", shapeComp.size);
            if (EditorGUI.EndChangeCheck())
            {
                if(tool != null)
                    tool.SetBounds(shapeComp.size);
                shapeComp.Rebuild();
                ProBuilderEditor.Refresh();
            }

            EditorGUI.BeginChangeCheck();
            Vector3 rotation = EditorGUILayout.Vector3Field("Rotation", shapeComp.rotation.eulerAngles);
            if (EditorGUI.EndChangeCheck())
            {
                shapeComp.SetInnerBoundsRotation(Quaternion.Euler(rotation));
                ProBuilderEditor.Refresh();
            }

            EditorGUILayout.PropertyField(shapeProperty, true);
            if (obj.ApplyModifiedProperties())
            {
                EditorShapeUtility.SaveParams(shapeComp.shape);
                if (shapeComp != null)
                {
                    shapeComp.Rebuild();
                    ProBuilderEditor.Refresh();
                }
            }
        }
    }
}
