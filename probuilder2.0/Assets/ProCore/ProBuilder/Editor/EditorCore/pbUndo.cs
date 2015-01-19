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
		Undo.RecordObject(obj, msg);
	}

	/**
	 * Record objects for Undo.
	 */
	public static void RecordObjects(Object[] objs, string msg)
	{
		if(objs == null) return;
		
		Undo.RecordObjects(objs, msg);
	}

	/**
	 * Record object prior to deletion.
	 */
	public static void DestroyImmediate(Object obj, string msg)
	{
		Undo.DestroyObjectImmediate(obj);
	}

	public static void RegisterCreatedObjectUndo(Object obj, string msg)
	{
		Undo.RegisterCreatedObjectUndo(obj, msg);
	}
}
