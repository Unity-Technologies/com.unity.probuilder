#if UNITY_4_5 || UNITY_4_5_0 || UNITY_4_5_1 || UNITY_4_5_2 || UNITY_4_5_3 || UNITY_4_5_4 || UNITY_4_5_5 || UNITY_4_5_6 || UNITY_4_5_7 || UNITY_4_5_8 || UNITY_4_5_9 || UNITY_4_6 || UNITY_4_6_0 || UNITY_4_6_1 || UNITY_4_6_2 || UNITY_4_6_3 || UNITY_4_6_4 || UNITY_4_6_5 || UNITY_4_6_6 || UNITY_4_6_7 || UNITY_4_6_8 || UNITY_4_6_9 || UNITY_4_7 || UNITY_4_7_0 || UNITY_4_7_1 || UNITY_4_7_2 || UNITY_4_7_3 || UNITY_4_7_4 || UNITY_4_7_5 || UNITY_4_7_6 || UNITY_4_7_7 || UNITY_4_7_8 || UNITY_4_7_9 || UNITY_4_8 || UNITY_4_8_0 || UNITY_4_8_1 || UNITY_4_8_2 || UNITY_4_8_3 || UNITY_4_8_4 || UNITY_4_8_5 || UNITY_4_8_6 || UNITY_4_8_7 || UNITY_4_8_8 || UNITY_4_8_9 || UNITY_5 || UNITY_5_0
#define UNITY_4_5
#define UNITY_4_3
#define UNITY_4
#endif
#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5 || UNITY_4_3_6 || UNITY_4_3_7 || UNITY_4_3_8 || UNITY_4_3_9 || UNITY_4_4 || UNITY_4_4_0 || UNITY_4_4_1 || UNITY_4_4_2 || UNITY_4_4_3 || UNITY_4_4_4 || UNITY_4_4_5 || UNITY_4_4_6 || UNITY_4_4_7 || UNITY_4_4_8 || UNITY_4_4_9 || UNITY_4_5 || UNITY_4_5_0 || UNITY_4_5_1 || UNITY_4_5_2 || UNITY_4_5_3 || UNITY_4_5_4 || UNITY_4_5_5 || UNITY_4_5_6 || UNITY_4_5_7 || UNITY_4_5_8 || UNITY_4_5_9 || UNITY_4_6 || UNITY_4_6_0 || UNITY_4_6_1 || UNITY_4_6_2 || UNITY_4_6_3 || UNITY_4_6_4 || UNITY_4_6_5 || UNITY_4_6_6 || UNITY_4_6_7 || UNITY_4_6_8 || UNITY_4_6_9 || UNITY_4_7 || UNITY_4_7_0 || UNITY_4_7_1 || UNITY_4_7_2 || UNITY_4_7_3 || UNITY_4_7_4 || UNITY_4_7_5 || UNITY_4_7_6 || UNITY_4_7_7 || UNITY_4_7_8 || UNITY_4_7_9 || UNITY_4_8 || UNITY_4_8_0 || UNITY_4_8_1 || UNITY_4_8_2 || UNITY_4_8_3 || UNITY_4_8_4 || UNITY_4_8_5 || UNITY_4_8_6 || UNITY_4_8_7 || UNITY_4_8_8 || UNITY_4_8_9
#define UNITY_4_3
#define UNITY_4
#elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_5_7 || UNITY_3_8
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
