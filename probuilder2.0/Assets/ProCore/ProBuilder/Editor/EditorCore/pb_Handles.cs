#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
#define UNITY_5_4_OR_LOWER
#else
#define UNITY_5_5_OR_HIGHER
#endif

#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
#define UNITY_5_5_OR_LOWER
#else
#define UNITY_5_6_OR_HIGHER
#endif

using UnityEngine;
using UnityEditor;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Wrap UnityEngine.Handles class functions for backwards compatibility.
	/// </summary>
	static class pb_Handles
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

		public static void DotCap(int id, Vector3 position, Quaternion rotation, float size, EventType type)
		{
#if UNITY_5_4_OR_LOWER
			Handles.DotCap(id, position, rotation, size);
#else
			Handles.DotHandleCap(id, position, rotation, size, type);
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

		/**
		 *	A free move handle with a dot cap.
		 */
		public static Vector3 FreeMoveDotHandle(Vector3 position, Quaternion rotation, float size, Vector3 snap)
		{
#if UNITY_5_5_OR_LOWER
			return Handles.FreeMoveHandle(position, rotation, size, snap, Handles.DotCap);
#else
			return Handles.FreeMoveHandle(position, rotation, size, snap, Handles.DotHandleCap);
#endif
		}

		public static Vector3 DotSlider2D(Vector3 position, Vector3 up, Vector3 right, Vector3 forward, float size, Vector3 snap, bool drawHelper)
		{
#if UNITY_5_4_OR_LOWER
			return Handles.Slider2D(position, up, right, forward, size, Handles.DotCap, snap, drawHelper);
#else
			return Handles.Slider2D(position, up, right, forward, size, Handles.DotHandleCap, snap, drawHelper);
#endif
		}

		public static Vector3 DotSlider(Vector3 position, Vector3 up, float size, float snap)
		{
#if UNITY_5_4_OR_LOWER
			return Handles.Slider(position, up, size, Handles.DotCap, snap);
#else
			return Handles.Slider(position, up, size, Handles.DotHandleCap, snap);
#endif
		}

	}
}
