using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(ShapeComponent))]
    class ShapeComponentEditor : Editor
    {
        ShapeComponent m_ShapeComponent;
        IMGUIContainer m_ShapeField;

        SerializedProperty m_shape;
        static string[] s_ShapeTypes;
        static Type[] s_AvailableShapeTypes;

        static int s_ActiveShapeIndex = 0;

        static ShapeComponentEditor()
        {
            s_AvailableShapeTypes =  TypeCache.GetTypesWithAttribute<ShapeAttribute>().Where(t => t.BaseType == typeof(Shape)).ToArray();
            s_ShapeTypes = s_AvailableShapeTypes.Select(
                x => ((ShapeAttribute)System.Attribute.GetCustomAttribute(x, typeof(ShapeAttribute))).name)
                .ToArray();
        }

        private void OnEnable()
        {
            m_ShapeComponent = target as ShapeComponent;
            m_shape = serializedObject.FindProperty("m_Shape");
            s_ActiveShapeIndex = Array.IndexOf( s_AvailableShapeTypes, m_ShapeComponent.shape.GetType());
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
            s_ActiveShapeIndex = Mathf.Max(0, Array.IndexOf( s_AvailableShapeTypes, shape.GetType()));
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
            shapeComp.SetRotation(Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", shapeComp.rotation.eulerAngles)));
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
