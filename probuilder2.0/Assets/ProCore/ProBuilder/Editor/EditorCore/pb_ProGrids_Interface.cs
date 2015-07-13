using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace ProBuilder2.Common
{
	/**
	 * Acts as a bridge between ProGrids and ProBuilder.
	 * Provides a delegate for push to grid events, and 
	 * allows access to snap enabled, axis preference,
	 * and grid size values.
	 */
	public static class pb_ProGrids_Interface
	{
		/**
		 * Get a pg_Editor type.
		 */
		public static Type GetProGridsType()
		{
			Assembly editorAssembly = Assembly.Load("Assembly-CSharp-Editor");
			Type type = editorAssembly.GetType("ProGrids.pg_Editor");
			if( type == null ) type = editorAssembly.GetType("pg_Editor");
			return type;
		}

		/**
		 * True if ProGrids is open in scene.
		 */
		public static bool ProGridsActive()
		{
			Type type = GetProGridsType();

			return type != null && (bool) type.GetMethod("SceneToolbarActive").Invoke(null, null);
		}

		/**
		 * Returns the current UseAxisConstraints value from ProGrids.
		 */
		public static bool UseAxisConstraints()
		{
			Type type = GetProGridsType();

			if( type != null )
				return (bool) type.GetMethod("UseAxisConstraints").Invoke(null, null);
			else
				return false;
		}

		/**
		 * If ProGrids is open and snap enabled, return true.  False otherwise.
		 */
		public static bool SnapEnabled()
		{
			Type type = GetProGridsType();

			if( type != null )
				return (bool) type.GetMethod("SnapEnabled").Invoke(null, null);
			else
				return false;
		}

		/**
		 * Return the last known snap value setting from ProGrids.
		 */
		public static float SnapValue()
		{
			Type type = GetProGridsType();

			if( type != null )
				return (float) type.GetMethod("SnapValue").Invoke(null, null);
			else
				return 0f;
		}
		
		/**
		 * Subscribe to PushToGrid events.
		 */
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

		/**
		 * Remove subscription from PushToGrid events.
		 */
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

		/**
		 * Tell ProGrids that a non-Unity handle has moved in some direction (in world space).
		 */
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

		/**
		 * Subscribe to toolbar extendo/retracto events.  Delegates are called with bool paramater Listener(bool menuOpen);
		 */
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

		/**
		 * Remove subscription from extendo/retracto tooblar events.
		 */
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

	}
}