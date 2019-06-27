using System;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.OpenSubdiv
{
    public class OpenSubdivEditor : Editor
    {
        SerializedProperty m_SubdivisionSettings;
        SerializedProperty m_SubdivisionEnabled;
        SerializedProperty m_SubdivisionMethod;
        SerializedProperty m_GenerateBoundaryVertexWeights;
        SerializedProperty m_GeneratedBoundaryVertexWeight;
        SerializedProperty m_SubdivisionLevel;

        void OnEnable()
        {
            m_SubdivisionSettings = serializedObject.FindProperty("m_SubdivisionSettings");
            m_SubdivisionEnabled = serializedObject.FindProperty("m_SubdivisionEnabled");
            m_SubdivisionMethod = m_SubdivisionSettings.FindPropertyRelative("m_SubdivisionMethod");
            m_GenerateBoundaryVertexWeights = m_SubdivisionSettings.FindPropertyRelative("m_GenerateBoundaryVertexWeights");
            m_GeneratedBoundaryVertexWeight = m_SubdivisionSettings.FindPropertyRelative("m_GeneratedBoundaryVertexWeight");
            m_SubdivisionLevel = m_SubdivisionSettings.FindPropertyRelative("m_SubdivisionLevel");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_SubdivisionEnabled);

            EditorGUILayout.PropertyField(m_SubdivisionMethod);

            EditorGUILayout.IntSlider(m_SubdivisionLevel, 0, 5, GUILayout.MinWidth(300));

            EditorGUILayout.PropertyField(m_GenerateBoundaryVertexWeights);
            EditorGUI.indentLevel++;
            using (new EditorGUI.DisabledScope(!m_GenerateBoundaryVertexWeights.boolValue))
                EditorGUILayout.Slider(m_GeneratedBoundaryVertexWeight, 0f, 10f);
            EditorGUI.indentLevel--;

            var rebuild = serializedObject.hasModifiedProperties;

            serializedObject.ApplyModifiedProperties();

            if(rebuild)
            {
                foreach (var mesh in targets)
                {
                    if (mesh is ProBuilderMesh)
                        ((ProBuilderMesh)mesh).Rebuild();
                }
            }

        }
    }
}
