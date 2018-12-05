using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Acts as a bridge between ProGrids and ProBuilder. Provides a delegate for push to grid events, and allows access
    /// to snap enabled, axis preference, and grid size values.
    /// </summary>
    [InitializeOnLoad]
    static class ProGridsInterface
    {
        static Type s_ProGridsType = null;

        static readonly string[] ProGridsEditorTypeNames = new string[]
        {
            "UnityEditor.ProGrids.ProGridsEditor",
            "ProGrids.Editor.ProGridsEditor",
            "ProGrids.Editor.pg_Editor",
            "ProGrids.pg_Editor",
            "pg_Editor",
        };

        static Func<object> m_ProGridsInstanceDelegate = null;
        static Func<bool> m_ProGridsActiveDelegate = null;
        static Func<bool> m_SceneToolbarIsExtendedDelegate = null;
        static Func<bool> m_UseAxisConstraintDelegate = null;
        static Func<bool> m_SnapEnabledDelegate = null;
        static Func<bool> m_IsFullGridEnabledDelegate = null;
        static Func<float> m_GetActiveGridOffsetDelegate = null;
        static Func<float> m_SnapValueDelegate = null;
        static Func<Vector3> m_GetPivotDelegate = null;

        static Action<Action<float>> m_SubscribePushToGridEventDelegate = null;
        static Action<Action<float>> m_UnsubscribePushToGridEventDelegate = null;
        static Action<Action<bool>> m_SubscribeToolbarEventDelegate = null;
        static Action<Action<bool>> m_UnsubscribeToolbarEventDelegate = null;

        static Action<Vector3> m_OnHandleMoveDelegate = null;

        static FieldInfo m_GetActiveGridAxisDelegate = null;

        static ProGridsInterface()
        {
            // Current release
            for (int i = 0, c = ProGridsEditorTypeNames.Length; i < c && s_ProGridsType == null; i++)
                s_ProGridsType = ReflectionUtility.GetType(ProGridsEditorTypeNames[i]);
        }

        /// <summary>
        /// Get a pg_Editor type.
        /// </summary>
        /// <returns></returns>
        public static Type GetProGridsType()
        {
            return s_ProGridsType;
        }

        public static object GetProGridsInstance()
        {
            if (GetProGridsType() == null)
                return null;

            if (m_ProGridsInstanceDelegate == null)
                m_ProGridsInstanceDelegate = (Func<object>)ReflectionUtility.GetOpenDelegateOnProperty<Func<object>>(GetProGridsType(), "Instance", BindingFlags.NonPublic | BindingFlags.Static);

            if (m_ProGridsInstanceDelegate != null)
                return m_ProGridsInstanceDelegate();

            return null;
        }

        /// <summary>
        /// True if ProGrids is open in scene.
        /// </summary>
        /// <returns></returns>
        public static bool ProGridsActive()
        {
            if (GetProGridsType() == null)
                return false;

            if (m_ProGridsActiveDelegate == null)
                m_ProGridsActiveDelegate = (Func<bool>)ReflectionUtility.GetOpenDelegate<Func<bool>>(GetProGridsType(), "SceneToolbarActive");

            if (m_ProGridsActiveDelegate != null)
                return m_ProGridsActiveDelegate();

            return false;
        }

        /// <summary>
        /// Is the scene toolbar extended or collapsed? Also check ProGridsActive to see if ProGrids is open in the first place.
        /// </summary>
        /// <returns>True if ProGrids scene toolbar is open and extended, false if not extended or not active in scene.</returns>
        public static bool SceneToolbarIsExtended()
        {
            if (GetProGridsType() == null)
                return false;

            if (m_SceneToolbarIsExtendedDelegate == null)
                m_SceneToolbarIsExtendedDelegate = (Func<bool>)ReflectionUtility.GetOpenDelegate<Func<bool>>(GetProGridsType(), "SceneToolbarIsExtended");

            if (m_SceneToolbarIsExtendedDelegate != null)
                return m_SceneToolbarIsExtendedDelegate();

            return false;
        }

        /// <summary>
        /// Returns the current UseAxisConstraints value from ProGrids.
        /// </summary>
        /// <returns></returns>
        public static bool UseAxisConstraints()
        {
            if (GetProGridsType() == null)
                return false;

            if (m_UseAxisConstraintDelegate == null)
                m_UseAxisConstraintDelegate = (Func<bool>)ReflectionUtility.GetOpenDelegate<Func<bool>>(GetProGridsType(), "UseAxisConstraints");
                
            if (m_UseAxisConstraintDelegate != null)
                return m_UseAxisConstraintDelegate();

            return false;
        }

        /// <summary>
        /// If ProGrids is open and snap enabled, return true.  False otherwise.
        /// </summary>
        /// <returns></returns>
        public static bool SnapEnabled()
        {
            if (GetProGridsType() == null)
                return false;

            if (m_SnapEnabledDelegate == null)
                m_SnapEnabledDelegate = (Func<bool>)ReflectionUtility.GetOpenDelegate<Func<bool>>(GetProGridsType(), "SnapEnabled");

            if (m_SnapEnabledDelegate != null)
                return m_SnapEnabledDelegate();

            return false;
        }

        /// <summary>
        /// Return the last known snap value setting from ProGrids.
        /// </summary>
        /// <returns></returns>
        public static float SnapValue()
        {
            if (GetProGridsType() == null)
                return 0f;

            if (m_SnapValueDelegate == null)
                m_SnapValueDelegate = (Func<float>)ReflectionUtility.GetOpenDelegate<Func<float>>(GetProGridsType(), "SnapValue");

            if (m_SnapValueDelegate != null)
                return m_SnapValueDelegate();

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

            if (m_GetPivotDelegate == null)
                m_GetPivotDelegate = (Func<Vector3>)ReflectionUtility.GetOpenDelegate<Func<Vector3>>(GetProGridsType(), "GetPivot");

            if (m_GetPivotDelegate != null)
            {
                pivot = m_GetPivotDelegate();
                return true;
            }

            return false;
        }

        public static bool IsFullGridEnabled()
        {
            if (m_IsFullGridEnabledDelegate == null)
                m_IsFullGridEnabledDelegate = (Func<bool>)ReflectionUtility.GetClosedDelegateOnProperty<Func<bool>>(
                    GetProGridsType(), GetProGridsInstance(), "FullGridEnabled", BindingFlags.Instance | BindingFlags.NonPublic);

            if (m_IsFullGridEnabledDelegate != null)
                return m_IsFullGridEnabledDelegate();

            return false;
        }

        public static int GetActiveGridAxis()
        {
            if (m_GetActiveGridAxisDelegate == null)
                m_GetActiveGridAxisDelegate = ReflectionUtility.GetFieldInfo(GetProGridsType(), "m_RenderPlane", (BindingFlags.Instance | BindingFlags.NonPublic));

            if (m_GetActiveGridAxisDelegate != null)
                return (int)m_GetActiveGridAxisDelegate.GetValue(GetProGridsInstance());
            
            return -1;
        }

        public static float GetActiveGridOffset()
        {
            if (m_GetActiveGridOffsetDelegate == null)
                m_GetActiveGridOffsetDelegate = (Func<float>)ReflectionUtility.GetClosedDelegateOnProperty<Func<float>>(
                    GetProGridsType(), GetProGridsInstance(), "GridRenderOffset", (BindingFlags.Instance | BindingFlags.Public));

            if (m_GetActiveGridOffsetDelegate != null)
                return m_GetActiveGridOffsetDelegate();
            
            return 0f;
        }

        /// <summary>
        /// Subscribe to PushToGrid events.
        /// </summary>
        /// <param name="listener"></param>
        public static void SubscribePushToGridEvent(Action<float> listener)
        {
            if (GetProGridsType() == null)
                return;

            if (m_SubscribePushToGridEventDelegate == null)
                m_SubscribePushToGridEventDelegate = (Action<Action<float>>)ReflectionUtility.GetOpenDelegate<Action<Action<float>>>(GetProGridsType(), "AddPushToGridListener");

            if (m_SubscribePushToGridEventDelegate != null)
                m_SubscribePushToGridEventDelegate(listener);
        }

        /// <summary>
        /// Remove subscription from PushToGrid events.
        /// </summary>
        /// <param name="listener"></param>
        public static void UnsubscribePushToGridEvent(Action<float> listener)
        {
            if (GetProGridsType() == null)
                return;

            if (m_UnsubscribePushToGridEventDelegate == null)
                m_UnsubscribePushToGridEventDelegate = (Action<Action<float>>)ReflectionUtility.GetOpenDelegate<Action<Action<float>>>(GetProGridsType(), "RemovePushToGridListener");

            if (m_UnsubscribePushToGridEventDelegate != null)
                m_UnsubscribePushToGridEventDelegate(listener);
        }

        /// <summary>
        /// Tell ProGrids that a non-Unity handle has moved in some direction (in world space).
        /// </summary>
        /// <param name="worldDirection"></param>
        public static void OnHandleMove(Vector3 worldDirection)
        {
            if (GetProGridsType() == null)
                return;

            if (m_OnHandleMoveDelegate == null)
                m_OnHandleMoveDelegate = (Action<Vector3>)ReflectionUtility.GetOpenDelegate<Action<Vector3>>(GetProGridsType(), "OnHandleMove");

            if (m_OnHandleMoveDelegate != null)
                m_OnHandleMoveDelegate(worldDirection);
        }

        /// <summary>
        /// Subscribe to toolbar extendo/retracto events.  Delegates are called with bool paramater Listener(bool menuOpen);
        /// </summary>
        /// <param name="listener"></param>
        public static void SubscribeToolbarEvent(Action<bool> listener)
        {
            if (GetProGridsType() == null)
                return;

            if (m_SubscribeToolbarEventDelegate == null)
                m_SubscribeToolbarEventDelegate = (Action<Action<bool>>)ReflectionUtility.GetOpenDelegate<Action<Action<bool>>>(GetProGridsType(), "AddToolbarEventSubscriber");

            if (m_SubscribeToolbarEventDelegate != null)
                m_SubscribeToolbarEventDelegate(listener);
        }

        /// <summary>
        /// Remove subscription from extendo/retracto tooblar events.
        /// </summary>
        /// <param name="listener"></param>
        public static void UnsubscribeToolbarEvent(Action<bool> listener)
        {
            if (GetProGridsType() == null)
                return;

            if (m_UnsubscribeToolbarEventDelegate == null)
                m_UnsubscribeToolbarEventDelegate = (Action<Action<bool>>)ReflectionUtility.GetOpenDelegate<Action<Action<bool>>>(GetProGridsType(), "RemoveToolbarEventSubscriber");

            if (m_UnsubscribeToolbarEventDelegate != null)
                m_UnsubscribeToolbarEventDelegate(listener);
        }

        /// <summary>
        /// Snap a Vector3 to the nearest point on the current ProGrids grid if ProGrids is enabled.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float ProGridsSnap(float point)
        {
            if (GetProGridsType() == null)
                return point;

            if (ProGridsInterface.SnapEnabled())
                return Snapping.SnapValue(point, ProGridsInterface.SnapValue());

            return point;
        }

        /// <summary>
        /// Snap a Vector3 to the nearest point on the current ProGrids grid if ProGrids is enabled.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ProGridsSnap(Vector3 point)
        {
            if (GetProGridsType() == null)
                return point;

            if (ProGridsInterface.SnapEnabled())
            {
                float snap = ProGridsInterface.SnapValue();
                return Snapping.SnapValue(point, snap);
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
            if (GetProGridsType() == null)
                return point;

            if (ProGridsInterface.SnapEnabled())
            {
                float snap = ProGridsInterface.SnapValue();
                return Snapping.SnapValue(point, mask * snap);
            }

            return point;
        }
    }
}
