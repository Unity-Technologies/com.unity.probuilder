using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Linq;
using System.Reflection;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 * Acts as a bridge between ProGrids and ProBuilder.
	 * Provides a delegate for push to grid events, and 
	 * allows access to snap enabled, axis preference,
	 * and grid size values.
	 */
	[InitializeOnLoad]
	public static class pb_ProGrids_Interface
	{
		private static Type m_ProGridsType = null;

		static pb_ProGrids_Interface()
		{
			m_ProGridsType = GetProGridsType();
		}

		/**
		 * Get a pg_Editor type.
		 */
		public static Type GetProGridsType()
		{
			if( m_ProGridsType == null )
			{
				try
				{
					Assembly editorAssembly = Assembly.Load("Assembly-CSharp-Editor");

					m_ProGridsType = editorAssembly.GetType("ProGrids.pg_Editor");

					if( m_ProGridsType == null )
						m_ProGridsType = editorAssembly.GetType("pg_Editor");
				}
				catch
				{}
			}

			return m_ProGridsType;
		}

		public static ScriptableObject GetProGridsInstance()
		{
			return Resources.FindObjectsOfTypeAll<ScriptableObject>().FirstOrDefault(x => x.GetType().ToString().Contains("pg_Editor"));
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
		 * Return the last known grid pivot point.
		 */
		public static bool GetPivot(out Vector3 pivot)
		{
			pivot = Vector3.zero;

			if(!pb_ProGrids_Interface.SnapEnabled())
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

		/**
		 *	Snap a Vector3 to the nearest point on the current ProGrids grid if ProGrids is enabled.
		 */
		public static float ProGridsSnap(float point)
		{
			if(pb_ProGrids_Interface.SnapEnabled())
				return pb_Snap.SnapValue(point, pb_ProGrids_Interface.SnapValue());
				
			return point;
		}

		/**
		 *	Snap a Vector3 to the nearest point on the current ProGrids grid if ProGrids is enabled.
		 */
		public static Vector3 ProGridsSnap(Vector3 point)
		{
			if(pb_ProGrids_Interface.SnapEnabled())
			{
				float snap = pb_ProGrids_Interface.SnapValue();
				return pb_Snap.SnapValue(point, snap);
			}

			return point;
		}

		/**
		 *	Snap a Vector3 to the nearest point on the current ProGrids grid if ProGrids is enabled, with mask.
		 */
		public static Vector3 ProGridsSnap(Vector3 point, Vector3 mask)
		{
			if(pb_ProGrids_Interface.SnapEnabled())
			{
				float snap = pb_ProGrids_Interface.SnapValue();
				return pb_Snap.SnapValue(point, mask * snap);
			}

			return point;
		}
	}
}
