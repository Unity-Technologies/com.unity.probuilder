using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

/**
 * Extends MonoBehaviour Inspector, automatically fixing missing script
 * references caused by the upgrade process.
 */
// [CustomEditor(typeof(MonoBehaviour))]
public class pb_MissingScriptEditor : Editor
{
	[MenuItem("Tools/ProBuilder/FIND BROKED SHIT")]
	public static void nimsif()
	{
		MethodInfo loadFromCache = typeof(SerializedObject).GetMethod("LoadFromCache", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

		foreach(GameObject go in FindObjectsOfType(typeof(GameObject)))
		{
			SerializedObject so = new SerializedObject( go );

			// object[] parameters = new object[] { (int)go.GetInstanceID() };
			// SerializedObject so; 
			// object result = loadFromCache.Invoke(null, parameters);
			// so = (SerializedObject)result;//(SerializedObject)parameters[1];

			Debug.Log("serializedObject: " + so);

			SerializedProperty iterator = so.GetIterator();

			iterator.Next(true);

			string txt = go.name + "\n";

			while( iterator.Next(true) )
			{
				txt += iterator.name + "  : (" + iterator.type + " / " + iterator.propertyType + ")";

				if(iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.type == "PPtr<Component>")
				{
					txt += (iterator.objectReferenceValue == null ? " is null\n" : "\n");
					
					SerializedObject iso = new SerializedObject(iterator.objectReferenceValue);

					SerializedProperty it = iso.GetIterator();
					it.Next(true);

					while( it.Next(true) ){
						txt += "\t" + it.name + "\n";
					}
				}
				else
				{
					txt += "\n";
				}
			}

			SerializedProperty sp = so.FindProperty("m_Script");
			Debug.Log(txt);

			if(sp == null || sp.objectReferenceValue != null)
				continue;

			Debug.Log(go.name + " contains a null component");
		}
	}

	// /**
	//  * SerializedProperty names found in pb_Entity.
	//  */
	// List<string> PB_OBJECT_SCRIPT_PROPERTIES = new List<string>()
	// {
	// 	"_sharedIndices",
	// 	"_vertices",
	// 	"_uv",
	// 	"_sharedIndicesUV",
	// 	"_quads"
	// };

	// /**
	//  * SerializedProperty names found in pb_Object.
	//  */
	// List<string> PB_ENTITY_SCRIPT_PROPERTIES = new List<string>()
	// {
	// 	"pb",
	// 	"userSetDimensions",
	// 	"_entityType"
	// };

	// public override void OnInspectorGUI()
	// {
	// 	SerializedProperty scriptProperty = this.serializedObject.FindProperty("m_Script");

	// 	if(scriptProperty == null || scriptProperty.objectReferenceValue != null)
	// 	{
	// 		base.OnInspectorGUI();
	// 		return;
	// 	}

	// 	GUILayout.Label(scriptProperty.type);

	// 	int pbObjectMatches = 0, pbEntityMatches = 0;

	// 	SerializedProperty iterator = this.serializedObject.GetIterator();

	// 	iterator.Next(true);

	// 	while( iterator.Next(false) )
	// 	{
	// 		GUILayout.Label(iterator.name);

	// 		if( PB_OBJECT_SCRIPT_PROPERTIES.Contains(iterator.name) )
	// 			pbObjectMatches++;

	// 		if( PB_ENTITY_SCRIPT_PROPERTIES.Contains(iterator.name) )
	// 			pbEntityMatches++;
	// 	}

	// 	if(pbObjectMatches >= 3)
	// 	{
	// 		Debug.Log("Matched pb_Object script to: " + target.name);
	// 		GUILayout.Label("SCRIPT MATCHES PB_OBJECT", EditorStyles.boldLabel);

	// 		GameObject go = new GameObject();
	// 		pb_Object pb = go.AddComponent<pb_Object>();
	// 		MonoScript ms = MonoScript.FromMonoBehaviour( pb );
			
	// 		Debug.Log(ms.name);
			
	// 		scriptProperty.objectReferenceValue = MonoScript.FromMonoBehaviour( pb );
	// 		scriptProperty.serializedObject.ApplyModifiedProperties();
	// 		scriptProperty.serializedObject.Update();

	// 		DestroyImmediate(go);
	// 	}

	// 	if(pbEntityMatches >= 3)
	// 	{
	// 		Debug.Log("Matched pb_Object script to: " + target.name);
	// 		GUILayout.Label("SCRIPT MATCHES PB_ENTITY", EditorStyles.boldLabel);

	// 		GameObject go = new GameObject();

	// 		pb_Object pb = go.AddComponent<pb_Object>();
	// 		pb_Entity pe = go.AddComponent<pb_Entity>();

	// 		MonoScript ms = MonoScript.FromMonoBehaviour( pe );

	// 		Debug.Log(ms.name);

	// 		scriptProperty.objectReferenceValue = MonoScript.FromMonoBehaviour( pe );
	// 		scriptProperty.serializedObject.ApplyModifiedProperties();
	// 		scriptProperty.serializedObject.Update();

	// 		DestroyImmediate(go);
	// 	}
	// }
}
