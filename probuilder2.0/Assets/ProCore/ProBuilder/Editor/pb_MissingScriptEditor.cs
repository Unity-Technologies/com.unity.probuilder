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
	static int index = 0;
	static float total;

	static bool doFix = false;
	static bool doShow = true;

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


	[MenuItem("Tools/ProBuilder/FIX BROKED SHIT")]
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
	}

	static void Next()
	{
		EditorUtility.DisplayProgressBar("Repair ProBuilder Script References", "Fixing " + (index+1) + " out of " + total + " objects in scene.", ((float)index/total) );
		// {
		// 	EditorUtility.DisplayDialog("User Canceled", "Successfully repaired " + index + " out of " + total + " ProBuilder objects.", "Okay");

		// 	EditorUtility.ClearProgressBar();
		// 	doFix = false;
		// 	return;
		// }

		foreach(GameObject go in FindObjectsOfType(typeof(GameObject)))
		{
			if(go.GetComponents<Component>().Any(x => x == null))
			{
				Selection.activeObject = go;
				return;
			}
		}

		EditorUtility.ClearProgressBar();

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
		if(!doShow && !doFix)
		{
			base.OnInspectorGUI();
			return;
		}

		SerializedProperty scriptProperty = this.serializedObject.FindProperty("m_Script");

		if(scriptProperty == null || scriptProperty.objectReferenceValue != null)
		{
			if(doFix && Event.current.type == EventType.Repaint)
			{
				EditorApplication.delayCall += Next;
			}

			return;
		}

		// GUILayout.Label(scriptProperty.type);

		int pbObjectMatches = 0, pbEntityMatches = 0;

		// GUILayout.Label( SerializedObjectToString(this.serializedObject) );

		SerializedProperty iterator = this.serializedObject.GetIterator();

		iterator.Next(true);

		System.Text.StringBuilder sb = new System.Text.StringBuilder();			

		while( iterator.Next(true) )
		{
			// GUILayout.Label(iterator.name);

			// if(doShow)
			// {
			// 	string tabs = "";
			// 	for(int i = 0; i < iterator.depth; i++) tabs += "\t";

			// 	sb.AppendLine(tabs + iterator.name + (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.type.Contains("Component") && iterator.objectReferenceValue == null ? " -> NULL" : "") );
				
			// 	tabs += "  - ";
				
			// 	sb.AppendLine(tabs + "Type: (" + iterator.type + " / " + iterator.propertyType + " / " + iterator.name + ")");
			// 	sb.AppendLine(tabs + iterator.propertyPath);
			// 	sb.AppendLine(tabs + "Value: " + SerializedPropertyValue(iterator));
			// }

		
			if( PB_OBJECT_SCRIPT_PROPERTIES.Contains(iterator.name) )
				pbObjectMatches++;

			if( PB_ENTITY_SCRIPT_PROPERTIES.Contains(iterator.name) )
				pbEntityMatches++;

		}

		sb.AppendLine("OBJECT MATCHES: " + pbObjectMatches);
		sb.AppendLine("ENTITY MATCHES: " + pbEntityMatches);

		if(doShow)
		{
			GUILayout.Label(sb.ToString());	
		}

		if(doFix || GUILayout.Button("Fix"))
		{
			if(pbObjectMatches >= 3)
			{
				index++;

				scriptProperty.objectReferenceValue = pb_monoscript;
				scriptProperty.serializedObject.ApplyModifiedProperties();
				scriptProperty.serializedObject.Update();

			}

			if(pbEntityMatches >= 3)
			{
				scriptProperty.objectReferenceValue = pe_monoscript;
				scriptProperty.serializedObject.ApplyModifiedProperties();
				scriptProperty.serializedObject.Update();

			}
		}

		if(doFix && Event.current.type == EventType.Repaint)
			EditorApplication.delayCall += Next;

	}


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

		// SerializedProperty sp = serializedObject.FindProperty("m_Script");

		return sb.ToString();
	}

	static List<SerializedProperty> FindNullComponents(SerializedObject serializedObject)
	{
		List<SerializedProperty> nullComponents = new List<SerializedProperty>();

		SerializedProperty iterator = serializedObject.GetIterator();

		iterator.Next(true);

		while( iterator.Next(true) )
		{	
			if(iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.type.Contains("Component") && iterator.objectReferenceValue == null)
			{
				nullComponents.Add(iterator);

				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				sb.AppendLine( iterator.name );
				sb.AppendLine( iterator.propertyPath );

				SerializedProperty nullproperty = iterator.FindPropertyRelative( iterator.propertyPath );
				sb.AppendLine( nullproperty == null ? "couldn't find component" : " huzzah ");

				Debug.Log(sb.ToString());
			}		
		}

		return nullComponents;
	}

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
