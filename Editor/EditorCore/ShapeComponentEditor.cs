using System;
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
        static int s_CurrentIndex = 0;
        static string[] s_ShapeTypes;
        static TypeCache.TypeCollection s_AvailableShapeTypes;


        static ShapeComponentEditor()
        {
            s_AvailableShapeTypes = TypeCache.GetTypesDerivedFrom<Shape>();
            s_ShapeTypes = s_AvailableShapeTypes.Select(x => x.ToString()).ToArray();
        }

        private void OnEnable()
        {
            m_ShapeComponent = target as ShapeComponent;
            m_shape = serializedObject.FindProperty("m_shape");
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
                s_CurrentIndex = s_AvailableShapeTypes.IndexOf(type);
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            return GetShapeVisual();
        }

        VisualElement GetShapeVisual()
        {
            var root = new VisualElement();
            var popup = new PopupField<string>(s_ShapeTypes.ToList(), s_CurrentIndex);
            var shapeField = new IMGUIContainer(OnShapeGUI);

            popup.RegisterValueChangedCallback(evt =>
            {
                s_CurrentIndex = s_ShapeTypes.ToList().IndexOf(evt.newValue);
                ((ShapeComponent)target).SetShape(s_AvailableShapeTypes[s_CurrentIndex]);
                ProBuilderEditor.Refresh(false);
            });

            var vector = new Vector3Field("Size");
            vector.BindProperty(serializedObject.FindProperty("m_Size"));
            vector.RegisterValueChangedCallback(evt =>
            {
                ((ShapeComponent)target).Rebuild();
                ProBuilderEditor.Refresh(false);
            });

            root.Add(popup);
            root.Add(vector);
            root.Add(shapeField);
            return root;
        }

        void OnShapeGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_shape, true);
            if (serializedObject.ApplyModifiedProperties())
            {
                ((ShapeComponent)target).m_Shape.SaveParams();
                ((ShapeComponent)target).Rebuild();
                ProBuilderEditor.Refresh(false);
            }
        }
    }
}
