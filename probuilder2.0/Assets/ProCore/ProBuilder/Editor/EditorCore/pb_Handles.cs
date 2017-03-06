#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
#define UNITY_5_4_OR_LOWER
#else
#define UNITY_5_5_OR_HIGHER
#endif

using UnityEngine;
using UnityEditor;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Wrap UnityEngine.Handles class functions for backwards compatibility.
	 */
	public static class pb_Handles
	{
		/**
		 *	Draw a button rendered with Dot Cap.
		 */
		public static bool ButtonDotCap(Vector3 position, Quaternion rotation, float size, float pickSize)
		{
#if UNITY_5_4_OR_LOWER
			return Handles.Button(position, rotation, size, pickSize, Handles.DotCap);
#else
			return Handles.Button(position, rotation, size, pickSize, Handles.DotHandleCap);
#endif
		}

		/**
		 *	Draw a circle handle.
		 */
		public static void CircleCap(int handleId, Vector3 position, Quaternion rotation, float size)
		{
#if UNITY_5_4_OR_LOWER
			Handles.CircleCap(handleId, position, rotation, size);
#else
			Handles.CircleHandleCap(handleId, position, rotation, size, Event.current.type);
#endif
		}

		/**
		 *	Draw a cone handle.
		 */
		public static void ConeCap(int handleId, Vector3 position, Quaternion rotation, float size)
		{
#if UNITY_5_4_OR_LOWER
			Handles.ConeCap(handleId, position, rotation, size);
#else
			Handles.ConeHandleCap(handleId, position, rotation, size, Event.current.type);
#endif
		}

		/**
		 *	Draw a cube handle.
		 */
		public static void CubeCap(int handleId, Vector3 position, Quaternion rotation, float size)
		{
#if UNITY_5_4_OR_LOWER
			Handles.CubeCap(handleId, position, rotation, size); 
#else
			Handles.CubeHandleCap(handleId, position, rotation, size, Event.current.type);
#endif
		}
	}
}
