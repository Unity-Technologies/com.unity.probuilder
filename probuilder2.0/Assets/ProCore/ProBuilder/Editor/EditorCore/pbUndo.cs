using UnityEngine;
using UnityEditor;
using System.Collections;

/**
 * A wrapper around Unity Undo calls.  Used for debugging and (previously) version compatibility.
 */
public class pbUndo : Editor 
{
	/**
	 * Record an object for Undo.
	 */
	public static void RecordObject(Object obj, string msg)
	{
		// Debug.Log("RecordObject()");	
		Undo.RecordObject(obj, msg);
	}

	/**
	 * Record objects for Undo.
	 */
	public static void RecordObjects(Object[] objs, string msg)
	{
		// Debug.Log("RecordObjects()");	
		if(objs == null) return;
		
		Undo.RecordObjects(objs, msg);
	}

	/**
	 * Undo.RegisterCompleteObjectUndo
	 */
	public static void RegisterCompleteObjectUndo(Object[] objs, string msg)
	{
		// Debug.Log("RegisterCompleteObjectUndo()");	
		Undo.RegisterCompleteObjectUndo(objs, msg);
	}

	/**
	 * Record object prior to deletion.
	 */
	public static void DestroyImmediate(Object obj, string msg)
	{
		// Debug.Log("DestroyImmediate()");	
		Undo.DestroyObjectImmediate(obj);
	}

	public static void RegisterCreatedObjectUndo(Object obj, string msg)
	{
		// Debug.Log("RegisterCreatedObjectUndo()");	
		Undo.RegisterCreatedObjectUndo(obj, msg);
	}
}
