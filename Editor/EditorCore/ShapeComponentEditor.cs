using System;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(ShapeComponent))]
    public class ShapeComponentEditor : Editor
    {
        SerializedProperty m_shape;
        static int s_CurrentIndex = 0;
        string[] m_ShapeTypes;
        TypeCache.TypeCollection m_AvailableShapeTypes;

        private void OnEnable()
        {
            m_AvailableShapeTypes = TypeCache.GetTypesDerivedFrom<Shape>();
            m_ShapeTypes = m_AvailableShapeTypes.Select(x => x.ToString()).ToArray();

            m_shape = serializedObject.FindProperty("m_shape");
            var fullName = m_shape.managedReferenceFullTypename;
            var typeName = fullName.Substring(fullName.LastIndexOf(' ') + 1);

            Type type = null;
            foreach (var shapeType in m_AvailableShapeTypes)
            {
                if (shapeType.ToString() == typeName)
                {
                    type = shapeType;
                    break;
                }
            }

            if (type != null)
            {
                s_CurrentIndex = m_AvailableShapeTypes.IndexOf(type);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            s_CurrentIndex = EditorGUILayout.Popup(s_CurrentIndex, m_ShapeTypes);
            if (EditorGUI.EndChangeCheck())
            {
                ((ShapeComponent)target).SetShape(m_AvailableShapeTypes[s_CurrentIndex]);
                ProBuilderEditor.Refresh(false);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_shape, true);
            if (EditorGUI.EndChangeCheck())
            {
                ((ShapeComponent)target).Rebuild();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
