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
	public static class pb_ProGridsInterface
	{
		/**
		 * Returns the current UseAxisConstraints value from ProGrids.
		 */
		public static bool UseAxisConstraints()
		{
			Type type = Type.GetType("pg_Editor");

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
			Type type = Type.GetType("pg_Editor");

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
			Type type = Type.GetType("pg_Editor");

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
			Type type = Type.GetType("pg_Editor");

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
			Type type = Type.GetType("pg_Editor");

			if( type != null )
			{
				MethodInfo mi = type.GetMethod("RemovePushToGridListener");
				if(mi != null)
					mi.Invoke(null, new object[] { listener } );
			}
		}
	}
}