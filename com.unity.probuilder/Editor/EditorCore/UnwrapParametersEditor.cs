using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	/// <inheritdoc />
	/// <summary>
	/// Inspector for working with lightmap UV generation params.
	/// </summary>
	[CanEditMultipleObjects]
	sealed class UnwrapParametersEditor : Editor
	{
		SerializedProperty m_UnwrapParametersProperty;
		GUIContent m_UnwrapParamatersContent = new GUIContent("UV2 Generation Params", "Settings for how Unity unwraps the UV2 (lightmap) UVs");

		void OnEnable()
		{
			m_UnwrapParametersProperty = serializedObject.FindProperty("m_UnwrapParameters");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(m_UnwrapParametersProperty, m_UnwrapParamatersContent, true);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
