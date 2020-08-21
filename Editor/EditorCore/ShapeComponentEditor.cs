using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(ShapeComponent))]
    public class ShapeComponentEditor : Editor
    {
        ShapeComponent m_ShapeComponent;
        IMGUIContainer m_ShapeField;

        SerializedProperty m_shape;
        static string[] s_ShapeTypes;
        static TypeCache.TypeCollection s_AvailableShapeTypes;

        static int s_ActiveShapeIndex = 0;

        static ShapeComponentEditor()
        {
            s_AvailableShapeTypes = TypeCache.GetTypesDerivedFrom<Shape>();
            s_ShapeTypes = s_AvailableShapeTypes.Select(x => x.ToString()).ToArray();
        }

        private void OnEnable()
        {
            m_ShapeComponent = target as ShapeComponent;
            m_shape = serializedObject.FindProperty("m_Shape");
            s_ActiveShapeIndex = s_AvailableShapeTypes.IndexOf(m_ShapeComponent.shape.GetType());
        }

        public override void OnInspectorGUI()
        {
            DrawShapeGUI((ShapeComponent)target, serializedObject);
        }

        public static void DrawShapeGUI(ShapeComponent shapeComp, SerializedObject obj)
        {
            if (shapeComp == null || obj == null)
                return;

            var shape = shapeComp.shape; 
            obj.Update();
            EditorGUI.BeginChangeCheck();

            var shapeProperty = obj.FindProperty("m_Shape");
            s_ActiveShapeIndex = Mathf.Max(0, s_AvailableShapeTypes.IndexOf(shape.GetType()));
            s_ActiveShapeIndex = EditorGUILayout.Popup(s_ActiveShapeIndex, s_ShapeTypes);

            if (EditorGUI.EndChangeCheck())
            {
                UndoUtility.RegisterCompleteObjectUndo(shapeComp, "Change Shape");
                var type = s_AvailableShapeTypes[s_ActiveShapeIndex];
                shapeComp.SetShape(EditorShapeUtility.CreateShape(type));
                ProBuilderEditor.Refresh();
            }

            EditorGUI.BeginChangeCheck();
            shapeComp.size = EditorGUILayout.Vector3Field("Size", shapeComp.size);
            shapeComp.SetRotation(Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", shapeComp.rotationQuaternion.eulerAngles)));
            if (EditorGUI.EndChangeCheck())
            {
                shapeComp.Rebuild();
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
