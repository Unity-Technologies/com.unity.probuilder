using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProBuilder2.UpgradeKit
{
	/**
	 * Inspector for pb_SerializedComponent
	 */
	[CustomEditor(typeof(pb_SerializedComponent))]
	public class pb_SerializedComponentEditor : Editor
	{
		bool showObject = false;
		bool showEntity = false;

		pb_SerializedComponent ser;

		Vector2 entScroll = Vector2.zero, objScroll = Vector2.zero;

		void OnEnable()
		{
			ser = (pb_SerializedComponent)target;
		}

		public override void OnInspectorGUI()
		{
			showObject = EditorGUILayout.Foldout(showObject, "pb_Object");

			if(showObject)
			{
				objScroll = EditorGUILayout.BeginScrollView(objScroll);
					GUILayout.Label(ser.GetObjectData(), EditorStyles.wordWrappedLabel);
				EditorGUILayout.EndScrollView();
			}

			showEntity = EditorGUILayout.Foldout(showEntity, "pb_Entity");

			if(showEntity)
			{
				entScroll = EditorGUILayout.BeginScrollView(entScroll);
					GUILayout.Label(ser.GetEntityData(), EditorStyles.wordWrappedLabel);
				EditorGUILayout.EndScrollView();
			}
		}
	}
}