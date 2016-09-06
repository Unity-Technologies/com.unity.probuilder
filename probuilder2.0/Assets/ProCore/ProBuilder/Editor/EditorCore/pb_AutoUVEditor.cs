using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Inspector for working with pb_UV objects.
	 */
	[CanEditMultipleObjects]
	public class pb_AutoUVEditor : Editor
	{
		SerializedProperty useWorldSpace;
		SerializedProperty flipU;
		SerializedProperty flipV;
		SerializedProperty swapUV;
		SerializedProperty fill;
		SerializedProperty scale;
		SerializedProperty offset;
		SerializedProperty rotation;
		SerializedProperty localPivot;
		SerializedProperty localSize;

		void OnEnable()
		{
			try {
				useWorldSpace 	= serializedObject.FindProperty("useWorldSpace");
				flipU 			= serializedObject.FindProperty("flipU");
				flipV 			= serializedObject.FindProperty("flipV");
				swapUV 			= serializedObject.FindProperty("swapUV");
				fill 			= serializedObject.FindProperty("fill");
				scale 			= serializedObject.FindProperty("scale");
				offset 			= serializedObject.FindProperty("offset");
				rotation 		= serializedObject.FindProperty("rotation");
				localPivot 		= serializedObject.FindProperty("localPivot");
				localSize 		= serializedObject.FindProperty("localSize");
			} catch {}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(useWorldSpace);
			EditorGUILayout.PropertyField(flipU);
			EditorGUILayout.PropertyField(flipV);
			EditorGUILayout.PropertyField(swapUV);
			EditorGUILayout.PropertyField(fill);
			EditorGUILayout.PropertyField(scale);
			EditorGUILayout.PropertyField(offset);
			EditorGUILayout.PropertyField(rotation);
			EditorGUILayout.PropertyField(localPivot);
			EditorGUILayout.PropertyField(localSize);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
