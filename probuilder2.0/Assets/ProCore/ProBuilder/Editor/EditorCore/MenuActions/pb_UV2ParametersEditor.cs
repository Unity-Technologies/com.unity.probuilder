using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Inspector for working with pb_Object lightmap UV generation params.
	 */
	[CanEditMultipleObjects]
	public class pb_UV2ParametersEditor : Editor
	{
		SerializedProperty p;
		GUIContent gc_UV2GenParams = new GUIContent("UV2 Generation Params", "Settings for how Unity unwraps the UV2 (lightmap) UVs");

		void OnEnable()
		{
			p = serializedObject.FindProperty("uv2Parameters");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(p, gc_UV2GenParams, true);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
