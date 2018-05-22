using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	/// <inheritdoc />
	/// <summary>
	/// Inspector for working with pb_Object lightmap UV generation params.
	/// </summary>
	[CanEditMultipleObjects]
	sealed class UnwrapParametersEditor : Editor
	{
		SerializedProperty p;
		GUIContent gc_unwrapParameters = new GUIContent("UV2 Generation Params", "Settings for how Unity unwraps the UV2 (lightmap) UVs");

		void OnEnable()
		{
			try {
				p = serializedObject.FindProperty("unwrapParameters");
			} catch {}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(p, gc_unwrapParameters, true);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
