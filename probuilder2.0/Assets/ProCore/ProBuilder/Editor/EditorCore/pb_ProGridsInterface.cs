using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Linq;
using System.Reflection;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Acts as a bridge between ProGrids and ProBuilder. Provides a delegate for push to grid events, and allows access
	/// to snap enabled, axis preference, and grid size values.
	/// </summary>
	[InitializeOnLoad]
	static class pb_ProGridsInterface
	{
		static Type s_ProGridsType = null;

		static pb_ProGridsInterface()
		{
			// Current release
			s_ProGridsType = pb_Reflection.GetType("ProGrids.Editor.pg_Editor");

			// 2016/17ish
			if(s_ProGridsType == null)
				s_ProGridsType = pb_Reflection.GetType("ProGrids.pg_Editor");

			// earlier
			if(s_ProGridsType == null)
				s_ProGridsType = pb_Reflection.GetType("pg_Editor");
		}

		/// <summary>
		/// Get a pg_Editor type.
		/// </summary>
		/// <returns></returns>
		public static Type GetProGridsType()
		{
			return s_ProGridsType;
		}

		public static ScriptableObject GetProGridsInstance()
		{
			return Resources.FindObjectsOfTypeAll<ScriptableObject>().FirstOrDefault(x => x.GetType().ToString().Contains("pg_Editor"));
		}

		/// <summary>
		/// True if ProGrids is open in scene.
		/// </summary>
		/// <returns></returns>
		public static bool ProGridsActive()
		{
			Type type = GetProGridsType();
			return type != null && (bool) type.GetMethod("SceneToolbarActive").Invoke(null, null);
		}

		/// <summary>
		/// Returns the current UseAxisConstraints value from ProGrids.
		/// </summary>
		/// <returns></returns>
		public static bool UseAxisConstraints()
		{
			Type type = GetProGridsType();

			if( type != null )
				return (bool) type.GetMethod("UseAxisConstraints").Invoke(null, null);
			else
				return false;
		}

		/// <summary>
		/// If ProGrids is open and snap enabled, return true.  False otherwise.
		/// </summary>
		/// <returns></returns>
		public static bool SnapEnabled()
		{
			Type type = GetProGridsType();

			if( type != null )
				return (bool) type.GetMethod("SnapEnabled").Invoke(null, null);
			else
				return false;
		}

		/// <summary>
		/// Return the last known snap value setting from ProGrids.
		/// </summary>
		/// <returns></returns>
		public static float SnapValue()
		{
			Type type = GetProGridsType();

			if( type != null )
				return (float) type.GetMethod("SnapValue").Invoke(null, null);
			else
				return 0f;
		}

		/// <summary>
		/// Return the last known grid pivot point.
		/// </summary>
		/// <param name="pivot"></param>
		/// <returns></returns>
		public static bool GetPivot(out Vector3 pivot)
		{
			pivot = Vector3.zero;

			if(!pb_ProGridsInterface.SnapEnabled())
				return false;

			Type type = GetProGridsType();

			if(type == null)
				return false;

			ScriptableObject pg = GetProGridsInstance();

			if( pg != null )
			{
				object o = pb_Reflection.GetValue(pg, type, "pivot");

				if(o != null)
				{
					pivot = (Vector3) o;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Subscribe to PushToGrid events.
		/// </summary>
		/// <param name="listener"></param>
		public static void SubscribePushToGridEvent(System.Action<float> listener)
		{
			Type type = GetProGridsType();

			if( type != null )
			{
				MethodInfo mi = type.GetMethod("AddPushToGridListener");
				if(mi != null)
					mi.Invoke(null, new object[] { listener } );
			}
		}

		/// <summary>
		/// Remove subscription from PushToGrid events.
		/// </summary>
		/// <param name="listener"></param>
		public static void UnsubscribePushToGridEvent(System.Action<float> listener)
		{
			Type type = GetProGridsType();

			if( type != null )
			{
				MethodInfo mi = type.GetMethod("RemovePushToGridListener");
				if(mi != null)
					mi.Invoke(null, new object[] { listener } );
			}
		}

		/// <summary>
		/// Tell ProGrids that a non-Unity handle has moved in some direction (in world space).
		/// </summary>
		/// <param name="worldDirection"></param>
		public static void OnHandleMove(Vector3 worldDirection)
		{
			Type type = GetProGridsType();

			if(type != null )
			{
				MethodInfo mi = type.GetMethod("OnHandleMove");

				if(mi != null)
					mi.Invoke(null, new object[] { worldDirection });
			}
		}

		/// <summary>
		/// Subscribe to toolbar extendo/retracto events.  Delegates are called with bool paramater Listener(bool menuOpen);
		/// </summary>
		/// <param name="listener"></param>
		public static void SubscribeToolbarEvent(System.Action<bool> listener)
		{
			Type type = GetProGridsType();

			if( type != null )
			{
				MethodInfo mi = type.GetMethod("AddToolbarEventSubscriber");
				if(mi != null)
					mi.Invoke(null, new object[] { listener } );
			}
		}

		/// <summary>
		/// Remove subscription from extendo/retracto tooblar events.
		/// </summary>
		/// <param name="listener"></param>
		public static void UnsubscribeToolbarEvent(System.Action<bool> listener)
		{
			Type type = GetProGridsType();

			if( type != null )
			{
				MethodInfo mi = type.GetMethod("RemoveToolbarEventSubscriber");
				if(mi != null)
					mi.Invoke(null, new object[] { listener } );
			}
		}

		/// <summary>
		/// Snap a Vector3 to the nearest point on the current ProGrids grid if ProGrids is enabled.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public static float ProGridsSnap(float point)
		{
			if(pb_ProGridsInterface.SnapEnabled())
				return pb_Snap.SnapValue(point, pb_ProGridsInterface.SnapValue());

			return point;
		}

		/// <summary>
		/// Snap a Vector3 to the nearest point on the current ProGrids grid if ProGrids is enabled.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public static Vector3 ProGridsSnap(Vector3 point)
		{
			if(pb_ProGridsInterface.SnapEnabled())
			{
				float snap = pb_ProGridsInterface.SnapValue();
				return pb_Snap.SnapValue(point, snap);
			}

			return point;
		}

		/// <summary>
		/// Snap a Vector3 to the nearest point on the current ProGrids grid if ProGrids is enabled, with mask.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="mask"></param>
		/// <returns></returns>
		public static Vector3 ProGridsSnap(Vector3 point, Vector3 mask)
		{
			if(pb_ProGridsInterface.SnapEnabled())
			{
				float snap = pb_ProGridsInterface.SnapValue();
				return pb_Snap.SnapValue(point, mask * snap);
			}

			return point;
		}
	}
}
