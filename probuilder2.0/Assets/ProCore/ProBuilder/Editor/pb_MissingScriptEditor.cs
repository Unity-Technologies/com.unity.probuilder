using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/**
 * Extends MonoBehaviour Inspector, automatically fixing missing script
 * references caused by the upgrade process.
 */
[CustomEditor(typeof(MonoBehaviour))]
public class pb_MissingScriptEditor : Editor
{
#region Members

	static int index = 0;
	static float total;

	static bool doFix = false;

	static MonoScript _mono_pb;
	static MonoScript _mono_pe;

	static void LoadMonoScript()
	{
		GameObject go = new GameObject();

		pb_Object pb = go.AddComponent<pb_Object>();
		pb_Entity pe = go.AddComponent<pb_Entity>();

		_mono_pb = MonoScript.FromMonoBehaviour( pb );
		_mono_pe = MonoScript.FromMonoBehaviour( pe );

		DestroyImmediate(go);
	}

	public MonoScript pb_monoscript
	{
		get
		{
			if(_mono_pb == null) LoadMonoScript();
			return _mono_pb; 
		}
	}

	public MonoScript pe_monoscript
	{
		get
		{
			if(_mono_pe == null) LoadMonoScript();
			return _mono_pe; 
		}
	}
#endregion

	[MenuItem("Tools/ProBuilder/Repair/Repair Missing Script References")]
	public static void nimsif()
	{
		EditorApplication.ExecuteMenuItem("Window/Inspector");

		total = FindObjectsOfType(typeof(GameObject)).Where(x => ((GameObject)x).GetComponents<Component>().Any(n => n == null) ).ToList().Count;

		if(total > 1)
		{
			index = 0;
			doFix = true;

			EditorApplication.delayCall += Next;
		}
		else
		{
			EditorUtility.DisplayDialog("Success", "No missing ProBuilder script references found.", "Okay");
		}
	}

	static void Next()
	{
		Debug.Log("Next");

		EditorUtility.DisplayProgressBar("Repair ProBuilder Script References", "Fixing " + (index+1) + " out of " + total + " objects in scene.", ((float)index/total) );

		foreach(GameObject go in FindObjectsOfType(typeof(GameObject)))
		{
			if(go.GetComponents<Component>().Any(x => x == null))
			{
				Selection.activeObject = go;
				return;
			}
		}

		EditorUtility.ClearProgressBar();

		Debug.Log("Done");
		doFix = false;

		EditorUtility.DisplayDialog("Success", "Successfully repaired " + total + " ProBuilder objects.", "Okay");
	}

	/**
	 * SerializedProperty names found in pb_Entity.
	 */
	List<string> PB_OBJECT_SCRIPT_PROPERTIES = new List<string>()
	{
		"_sharedIndices",
		"_vertices",
		"_uv",
		"_sharedIndicesUV",
		"_quads"
	};

	/**
	 * SerializedProperty names found in pb_Object.
	 */
	List<string> PB_ENTITY_SCRIPT_PROPERTIES = new List<string>()
	{
		"pb",
		"userSetDimensions",
		"_entityType",
		"forceConvex"
	};

	public override void OnInspectorGUI()
	{
		SerializedProperty scriptProperty = this.serializedObject.FindProperty("m_Script");

		if(scriptProperty == null || scriptProperty.objectReferenceValue != null)
		{
			if(doFix)
			{
				if(Event.current.type == EventType.Repaint)
				{
					Debug.Log("Object Okay: Next()");
					EditorApplication.delayCall += Next;
				}
			}
			else
			{
				base.OnInspectorGUI();
			}

			return;
		}

		int pbObjectMatches = 0, pbEntityMatches = 0;

		// Shows a detailed tree view of all the properties in this serializedobject.
		// GUILayout.Label( SerializedObjectToString(this.serializedObject) );

		SerializedProperty iterator = this.serializedObject.GetIterator();

		iterator.Next(true);

		while( iterator.Next(true) )
		{
			if( PB_OBJECT_SCRIPT_PROPERTIES.Contains(iterator.name) )
				pbObjectMatches++;

			if( PB_ENTITY_SCRIPT_PROPERTIES.Contains(iterator.name) )
				pbEntityMatches++;
		}

		// If we can fix it, show the help box, otherwise just default inspector it up.
		if(pbObjectMatches >= 3 || pbEntityMatches >= 3)
		{
			EditorGUILayout.HelpBox("Missing Script Reference\n\nProBuilder can automatically fix this missing reference.  To fix all references in the scene, click \"Fix All in Scene\".  To fix just this one, click \"Reconnect\".", MessageType.Warning);
		}
		else
		{
			base.OnInspectorGUI();
			return;
		}

		GUI.backgroundColor = Color.green;
		if(GUILayout.Button("Fix All in Scene"))
		{
			doFix = true;
		}

		GUI.backgroundColor = Color.cyan;
		if(doFix || GUILayout.Button("Reconnect"))
		{
			if(pbObjectMatches >= 3)	// only increment for pb_Object otherwise the progress bar will fill 2x faster than it should
				index++;

			scriptProperty.objectReferenceValue = pbObjectMatches >= 3 ? pb_monoscript : pe_monoscript;
			scriptProperty.serializedObject.ApplyModifiedProperties();
			scriptProperty.serializedObject.Update();
		}

		GUI.backgroundColor = Color.white;

		if(doFix && Event.current.type == EventType.Repaint)
		{
			Debug.Log("Fixed Object: Next()");
			EditorApplication.delayCall += Next;
		}
	}

	/**
	 * Returns a formatted string with all properties in serialized object.
	 */
	static string SerializedObjectToString(SerializedObject serializedObject)
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();			

		if(serializedObject == null)
		{
			sb.Append("NULL");
			return sb.ToString();
		}

		SerializedProperty iterator = serializedObject.GetIterator();

		iterator.Next(true);


		while( iterator.Next(true) )
		{	
			string tabs = "";
			for(int i = 0; i < iterator.depth; i++) tabs += "\t";

			sb.AppendLine(tabs + iterator.name + (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.type.Contains("Component") && iterator.objectReferenceValue == null ? " -> NULL" : "") );
			
			tabs += "  - ";
			
			sb.AppendLine(tabs + "Type: (" + iterator.type + " / " + iterator.propertyType + " / " + " / " + iterator.name + ")");
			sb.AppendLine(tabs + iterator.propertyPath);
			sb.AppendLine(tabs + "Value: " + SerializedPropertyValue(iterator));
		}

		return sb.ToString();
	}

	/**
	 * Return a string from the value of a SerializedProperty.
	 */
	static string SerializedPropertyValue(SerializedProperty sp)
	{
		switch(sp.propertyType)
		{
			case SerializedPropertyType.Integer:
				return sp.intValue.ToString();

			case SerializedPropertyType.Boolean:
				return sp.boolValue.ToString();

			case SerializedPropertyType.Float:
				return sp.floatValue.ToString();

			case SerializedPropertyType.String:
				return sp.stringValue.ToString();

			case SerializedPropertyType.Color:
				return sp.colorValue.ToString();

			case SerializedPropertyType.ObjectReference:
				return (sp.objectReferenceValue == null ? "null" : sp.objectReferenceValue.name);

			case SerializedPropertyType.LayerMask:
				return sp.intValue.ToString();

			case SerializedPropertyType.Enum:
				return sp.enumValueIndex.ToString();

			case SerializedPropertyType.Vector2:
				return sp.vector2Value.ToString();

			case SerializedPropertyType.Vector3:
				return sp.vector3Value.ToString();

			// Not public api as of 4.3?
			// case SerializedPropertyType.Vector4:
			// 	return sp.vector4Value.ToString();

			case SerializedPropertyType.Rect:
				return sp.rectValue.ToString();

			case SerializedPropertyType.ArraySize:
				return sp.intValue.ToString();

			case SerializedPropertyType.Character:
				return "Character";

			case SerializedPropertyType.AnimationCurve:
				return sp.animationCurveValue.ToString();

			case SerializedPropertyType.Bounds:
				return sp.boundsValue.ToString();

			case SerializedPropertyType.Gradient:
				return "Gradient";

			default:
				return "Unknown type";
		}
	}

}
