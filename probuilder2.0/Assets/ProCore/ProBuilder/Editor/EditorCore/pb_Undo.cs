using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/// <summary>
	/// A wrapper around Unity Undo calls.  Used for debugging and (previously) version compatibility.
	/// </summary>
	static class pb_Undo
	{
		/**
		 * Since Undo calls can potentially hang the main thread, store states when the diff
		 * will large.
		 */
		public static void RecordSelection(pb_Object pb, string msg)
		{
			if( pb.vertexCount > 256 )
				RegisterCompleteObjectUndo(pb, msg);
			else
				Undo.RecordObject(pb, msg);
		}

		/**
		 *	Tests if any pb_Object in the selection has more than 512 vertices, and if so records the entire object
		 * 	instead of diffing the serialized object (which is very slow for large arrays).
		 */
		public static void RecordSelection(pb_Object[] pb, string msg)
		{
			if( pb.Any(x => { return x.vertexCount > 256; }) )
				RegisterCompleteObjectUndo(pb, msg);
			else
				Undo.RecordObjects(pb, msg);
		}

		/**
		 * Record an object for Undo.
		 */
		public static void RecordObject(Object obj, string msg)
		{
			if(obj is pb_Object && ((pb_Object)obj).vertexCount > 256)
			{
#if PB_DEBUG
				Debug.LogWarning("RecordObject()  ->  " + ((pb_Object)obj).vertexCount);
#endif
				RegisterCompleteObjectUndo(obj as pb_Object, msg);
			}
			else
			{
				Undo.RecordObject(obj, msg);
			}
		}

		/**
		 * Record objects for Undo.
		 */
		public static void RecordObjects(Object[] objs, string msg)
		{
			if(objs == null)
				return;

			Object[] obj = objs.Where(x => !(x is pb_Object)).ToArray();
			pb_Object[] pb = objs.Where(x => x is pb_Object).Cast<pb_Object>().ToArray();

			Undo.RecordObjects(obj, msg);
			pb_Undo.RecordSelection(pb, msg);
		}

		/**
		 * Undo.RegisterCompleteObjectUndo
		 */
		public static void RegisterCompleteObjectUndo(Object objs, string msg)
		{
			Undo.RegisterCompleteObjectUndo(objs, msg);
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
