using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

namespace ProBuilder2.EditorCommon
{
	/**
	 * A wrapper around Unity Undo calls.  Used for debugging and (previously) version compatibility.
	 */
	public class pbUndo
	{
		/**
		 * Since Undo calls can potentially hang the main thread, store states when the diff
		 * will large.
		 */
		public static void RecordSelection(pb_Object pb, string msg)
		{
			RecordSelection(new pb_Object[] { pb }, msg);
		}

		/**
		 * @todo - Remove this and implement a pb_Selection class that can easily be recorded for undo without
		 * doing this weird hcak.
		 */
		public static void RecordSelection(pb_Object[] pb, string msg)
		{
			if( pb.Sum(x => x.SelectedTriangleCount) > 256 )
				RegisterCompleteObjectUndo(pb, msg);
			else
				RecordObjects(pb, msg);
		}

		/**
		 * Record an object for Undo.
		 */
		public static void RecordObject(Object obj, string msg)
		{
			#if PB_DEBUG
			Debug.Log("RecordObject()  ->  " + msg);
			#endif
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
		 * Undo.RegisterCompleteObjectUndo
		 */
		public static void RegisterCompleteObjectUndo(Object[] objs, string msg)
		{
			Undo.RegisterCompleteObjectUndo(objs, msg);
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
}
