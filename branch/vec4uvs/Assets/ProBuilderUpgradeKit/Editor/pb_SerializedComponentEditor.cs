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
		const int MAX_STRING_LENGTH = 2048;

		bool showObject = false;
		bool showEntity = false;

		pb_SerializedComponent ser;

		Vector2 entScroll = Vector2.zero, objScroll = Vector2.zero;

		void OnEnable()
		{
			ser = (pb_SerializedComponent)target;
		}

		string Truncate(string str)
		{
			if( str.Length > MAX_STRING_LENGTH)
				return str.Substring(0, MAX_STRING_LENGTH - 7) + "\n...etc";
			else
				return str;
		}

		public override void OnInspectorGUI()
		{
			showObject = EditorGUILayout.Foldout(showObject, "pb_Object");

			if(showObject)
			{
				objScroll = EditorGUILayout.BeginScrollView(objScroll);
					GUILayout.Label(Truncate(ser.GetObjectData()), EditorStyles.wordWrappedLabel);
				EditorGUILayout.EndScrollView();
			}

			showEntity = EditorGUILayout.Foldout(showEntity, "pb_Entity");

			if(showEntity)
			{
				entScroll = EditorGUILayout.BeginScrollView(entScroll);
					GUILayout.Label(ser.GetEntityData(), EditorStyles.wordWrappedLabel);
				EditorGUILayout.EndScrollView();
			}

			GUILayout.Label("Is Prefab Instance: " + ser.isPrefabInstance);
		}
	}
}