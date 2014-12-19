#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5 || UNITY_5_0
#define UNITY_4_3
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;

/**
 * A general purpose replacement for UnityEngine.Undo class with support for Unity 3.5.6 and 4.3+.
 * Wraps your Undo calls with the proper non-deprecated function.  Does not support the old 
 * `SetSnapshot` stuff - you'll still need to do that on your own.  Sorry.
 */
public class pbUndo : Editor 
{
	/**
	 * If UNITY_4_3 or above, uses UnityEngine.RecordObject.  Otherwise, use RegisterUndo.
	 */
	public static void RecordObject(Object obj, string msg)
	{
		#if UNITY_4_3
		Undo.RecordObject(obj, msg);
		#else
		Undo.RegisterUndo(obj, msg);
		#endif
	}

	/**
	 * If UNITY_4_3 or above, this calls RecordObjects - else RegisterUndo.
	 */
	public static void RecordObjects(Object[] objs, string msg)
	{
		#if !UNITY_4_3
		Undo.RegisterUndo(objs, msg);
		#else
		Undo.RecordObjects(objs, msg);
		#endif
	}

	/**
	 * If UNITY_4_3 or above, calls Undo.DestroyObjectImmediate.  Else wraps
	 * a DestroyImmediate call in Undo.RegisterUndo.
	 */
	public static void DestroyImmediate(Object obj, string msg)
	{
		#if UNITY_4_3
		Undo.DestroyObjectImmediate(obj);
		#else
		Undo.RegisterSceneUndo(msg);
		GameObject.DestroyImmediate(obj);
		#endif
	}
}
