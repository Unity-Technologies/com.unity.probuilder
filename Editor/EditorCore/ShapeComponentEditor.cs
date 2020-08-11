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
        private ShapeComponent m_ShapeComponent;
        private IMGUIContainer m_ShapeField;

        SerializedProperty m_shape;
        static string[] s_ShapeTypes;
        static TypeCache.TypeCollection s_AvailableShapeTypes;

        static Pref<int> s_ActiveShapeIndex = new Pref<int>("ShapeBuilder.ActiveShapeIndex", 0);

        static ShapeComponentEditor()
        {
            s_AvailableShapeTypes = TypeCache.GetTypesDerivedFrom<Shape>();
            s_ShapeTypes = s_AvailableShapeTypes.Select(x => x.ToString()).ToArray();
        }

        private void OnEnable()
        {
            m_ShapeComponent = target as ShapeComponent;
            m_shape = serializedObject.FindProperty("shape");
            var fullName = m_shape.managedReferenceFullTypename;
            var typeName = fullName.Substring(fullName.LastIndexOf(' ') + 1);

            Type type = null;
            foreach (var shapeType in s_AvailableShapeTypes)
            {
                if (shapeType.ToString() == typeName)
                {
                    type = shapeType;
                    break;
                }
            }

            if (type != null)
            {
                s_ActiveShapeIndex.value = s_AvailableShapeTypes.IndexOf(type);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawShapeGUI((ShapeComponent)target, serializedObject);
        }

        static Shape CreateShape(Type type)
        {
            var shape = Activator.CreateInstance(type) as Shape;
            ShapeParameters.SetToLastParams(ref shape);
            return shape;
        }

        public static void DrawShapeGUI(ShapeComponent shapeComp, SerializedObject obj)
        {
            if (shapeComp == null || obj == null)
                return;

            var shape = shapeComp.shape;
            obj.Update();
            EditorGUI.BeginChangeCheck();

            var shapeProperty = obj.FindProperty("shape");
            s_ActiveShapeIndex.value = Mathf.Max(0, s_AvailableShapeTypes.IndexOf(shape.GetType()));
            s_ActiveShapeIndex.value = EditorGUILayout.Popup(s_ActiveShapeIndex, s_ShapeTypes);

            if (EditorGUI.EndChangeCheck())
            {
                UndoUtility.RegisterCompleteObjectUndo(shapeComp, "Change Shape");
                var type = s_AvailableShapeTypes[s_ActiveShapeIndex];
             //   SetActiveShapeType(type);
                shapeComp.SetShape(CreateShape(type));
                ProBuilderEditor.Refresh();
            }

            EditorGUI.BeginChangeCheck();
            shapeComp.size = EditorGUILayout.Vector3Field("Size", shapeComp.size);
            //m_Size = shapeComp.size;
            shapeComp.SetRotation(Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", shapeComp.rotationQuaternion.eulerAngles)));
            if (EditorGUI.EndChangeCheck())
            {
                Debug.Log("changed");
                shapeComp.Rebuild();
                ProBuilderEditor.Refresh();
            }

            EditorGUILayout.PropertyField(shapeProperty, true);
            if (obj.ApplyModifiedProperties())
            {
                ShapeParameters.SaveParams(shapeComp.shape);
                if (shapeComp != null)
                {
                    shapeComp.Rebuild();
                    ProBuilderEditor.Refresh();
                }
            }
        }
    }
}
