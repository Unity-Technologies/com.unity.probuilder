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

        static Func<object> s_ProGridsInstanceDelegate = null;
        static Func<bool> s_ProGridsActiveDelegate = null;
        static Func<bool> s_SceneToolbarIsExtendedDelegate = null;
        static Func<bool> s_UseAxisConstraintDelegate = null;
        static Func<bool> s_SnapEnabledDelegate = null;
        static Func<bool> s_ProGridsSnapAsGroupDelegate = null;
        static Func<bool> s_IsFullGridEnabledDelegate = null;
        static Func<float> s_GetActiveGridOffsetDelegate = null;
        static Func<float> s_SnapValueDelegate = null;
        static Func<Vector3> s_GetPivotDelegate = null;
        static FieldInfo s_GridVisibleField = null;

        static Action<Action<float>> s_SubscribePushToGridEventDelegate = null;
        static Action<Action<float>> s_UnsubscribePushToGridEventDelegate = null;
        static Action<Action<bool>> s_SubscribeToolbarEventDelegate = null;
        static Action<Action<bool>> s_UnsubscribeToolbarEventDelegate = null;

        static Action<Vector3> s_OnHandleMoveDelegate = null;

        static FieldInfo s_GetActiveGridAxisDelegate = null;

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

            if (s_ProGridsInstanceDelegate == null)
                s_ProGridsInstanceDelegate = (Func<object>)ReflectionUtility.GetOpenDelegateOnProperty<Func<object>>(GetProGridsType(), "Instance", BindingFlags.NonPublic | BindingFlags.Static);

            if (s_ProGridsInstanceDelegate != null)
                return s_ProGridsInstanceDelegate();

            return null;
        }

        public static bool GetProGridsSnapAsGroup()
        {
            if (GetProGridsType() == null)
                return false;

            if (s_ProGridsSnapAsGroupDelegate == null)
                s_ProGridsSnapAsGroupDelegate = (Func<bool>) ReflectionUtility.GetClosedDelegateOnProperty<Func<bool>>(
                    GetProGridsType(),
                    GetProGridsInstance(),
                    "SnapAsGroupEnabled",
                    BindingFlags.Instance | BindingFlags.NonPublic);

            if (s_ProGridsSnapAsGroupDelegate != null)
                return s_ProGridsSnapAsGroupDelegate();

            return false;
        }

        /// <summary>
        /// True if ProGrids is open in scene.
        /// </summary>
        /// <returns></returns>
        public static bool ProGridsActive()
        {
            if (GetProGridsType() == null)
                return false;

            if (s_ProGridsActiveDelegate == null)
                s_ProGridsActiveDelegate = (Func<bool>)ReflectionUtility.GetOpenDelegate<Func<bool>>(GetProGridsType(), "SceneToolbarActive");

            if (s_ProGridsActiveDelegate != null)
                return s_ProGridsActiveDelegate();

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

            if (s_SceneToolbarIsExtendedDelegate == null)
                s_SceneToolbarIsExtendedDelegate = (Func<bool>)ReflectionUtility.GetOpenDelegate<Func<bool>>(GetProGridsType(), "SceneToolbarIsExtended");

            if (s_SceneToolbarIsExtendedDelegate != null)
                return s_SceneToolbarIsExtendedDelegate();

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

            if (s_UseAxisConstraintDelegate == null)
                s_UseAxisConstraintDelegate = (Func<bool>)ReflectionUtility.GetOpenDelegate<Func<bool>>(GetProGridsType(), "UseAxisConstraints");

            if (s_UseAxisConstraintDelegate != null)
                return s_UseAxisConstraintDelegate();

            return false;
        }

        /// <summary>
        /// If ProGrids is open and snap enabled, return true.  False otherwise.
        /// </summary>
        /// <returns></returns>
        public static bool SnapEnabled()
        {
            if (GetProGridsType() == null || !ProGridsActive())
                return false;

            if (s_SnapEnabledDelegate == null)
                s_SnapEnabledDelegate = (Func<bool>)ReflectionUtility.GetOpenDelegate<Func<bool>>(GetProGridsType(), "SnapEnabled");

            if (s_SnapEnabledDelegate != null)
                return s_SnapEnabledDelegate();

            return false;
        }

        /// <summary>
        /// Is the grid visible?
        /// </summary>
        public static bool GridVisible()
        {
            if (GetProGridsType() == null || GetProGridsInstance() == null)
                return false;

            if (s_GridVisibleField == null)
                s_GridVisibleField = GetProGridsType().GetField("m_DrawGrid", BindingFlags.NonPublic | BindingFlags.Instance);

            if (s_GridVisibleField != null)
                return (bool) s_GridVisibleField.GetValue(GetProGridsInstance());

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

            if (s_SnapValueDelegate == null)
                s_SnapValueDelegate = (Func<float>)ReflectionUtility.GetOpenDelegate<Func<float>>(GetProGridsType(), "SnapValue");

            if (s_SnapValueDelegate != null)
                return s_SnapValueDelegate();

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

            if (s_GetPivotDelegate == null)
                s_GetPivotDelegate = (Func<Vector3>)ReflectionUtility.GetOpenDelegate<Func<Vector3>>(GetProGridsType(), "GetPivot");

            if (s_GetPivotDelegate != null)
            {
                pivot = s_GetPivotDelegate();

                // earlier version of progrids return a non-snapped pivot point
                pivot = Snapping.SnapValue(pivot, SnapValue());
                return true;
            }

            return false;
        }

        public static bool IsFullGridEnabled()
        {
            if (s_IsFullGridEnabledDelegate == null)
                s_IsFullGridEnabledDelegate = (Func<bool>)ReflectionUtility.GetClosedDelegateOnProperty<Func<bool>>(
                    GetProGridsType(), GetProGridsInstance(), "FullGridEnabled", BindingFlags.Instance | BindingFlags.NonPublic);

            if (s_IsFullGridEnabledDelegate != null)
                return s_IsFullGridEnabledDelegate();

            return false;
        }

        public static HandleAxis GetActiveGridAxis()
        {
            if (s_GetActiveGridAxisDelegate == null)
                s_GetActiveGridAxisDelegate = ReflectionUtility.GetFieldInfo(GetProGridsType(), "m_RenderPlane", (BindingFlags.Instance | BindingFlags.NonPublic));

            if (s_GetActiveGridAxisDelegate != null)
            {
                var value = (int) s_GetActiveGridAxisDelegate.GetValue(GetProGridsInstance());

                // note - the hex notation that doesn't align to bit masks is intentional. long ago these values were
                // defined in progrids, and now we're stuck with random values in the render plane enum.
                if(value == 0x1 || value == 0x8)
                    return HandleAxis.X;
                if(value == 0x2 || value == 0x16)
                    return HandleAxis.Y;
                if(value == 0x4 || value == 0x32)
                    return HandleAxis.Z;
            }

            return HandleAxis.Free;
        }

        public static float GetActiveGridOffset()
        {
            if (s_GetActiveGridOffsetDelegate == null)
                s_GetActiveGridOffsetDelegate = (Func<float>)ReflectionUtility.GetClosedDelegateOnProperty<Func<float>>(
                    GetProGridsType(), GetProGridsInstance(), "GridRenderOffset", (BindingFlags.Instance | BindingFlags.Public));

            if (s_GetActiveGridOffsetDelegate != null)
                return s_GetActiveGridOffsetDelegate();

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

            if (s_SubscribePushToGridEventDelegate == null)
                s_SubscribePushToGridEventDelegate = (Action<Action<float>>)ReflectionUtility.GetOpenDelegate<Action<Action<float>>>(GetProGridsType(), "AddPushToGridListener");

            if (s_SubscribePushToGridEventDelegate != null)
                s_SubscribePushToGridEventDelegate(listener);
        }

        /// <summary>
        /// Remove subscription from PushToGrid events.
        /// </summary>
        /// <param name="listener"></param>
        public static void UnsubscribePushToGridEvent(Action<float> listener)
        {
            if (GetProGridsType() == null)
                return;

            if (s_UnsubscribePushToGridEventDelegate == null)
                s_UnsubscribePushToGridEventDelegate = (Action<Action<float>>)ReflectionUtility.GetOpenDelegate<Action<Action<float>>>(GetProGridsType(), "RemovePushToGridListener");

            if (s_UnsubscribePushToGridEventDelegate != null)
                s_UnsubscribePushToGridEventDelegate(listener);
        }

        /// <summary>
        /// Tell ProGrids that a non-Unity handle has moved in some direction (in world space).
        /// </summary>
        /// <param name="worldDirection"></param>
        public static void OnHandleMove(Vector3 worldDirection)
        {
            if (GetProGridsType() == null)
                return;

            if (s_OnHandleMoveDelegate == null)
                s_OnHandleMoveDelegate = (Action<Vector3>)ReflectionUtility.GetOpenDelegate<Action<Vector3>>(GetProGridsType(), "OnHandleMove");

            if (s_OnHandleMoveDelegate != null)
                s_OnHandleMoveDelegate(worldDirection);
        }

        /// <summary>
        /// Subscribe to toolbar extendo/retracto events.  Delegates are called with bool paramater Listener(bool menuOpen);
        /// </summary>
        /// <param name="listener"></param>
        public static void SubscribeToolbarEvent(Action<bool> listener)
        {
            if (GetProGridsType() == null)
                return;

            if (s_SubscribeToolbarEventDelegate == null)
                s_SubscribeToolbarEventDelegate = (Action<Action<bool>>)ReflectionUtility.GetOpenDelegate<Action<Action<bool>>>(GetProGridsType(), "AddToolbarEventSubscriber");

            if (s_SubscribeToolbarEventDelegate != null)
                s_SubscribeToolbarEventDelegate(listener);
        }

        /// <summary>
        /// Remove subscription from extendo/retracto tooblar events.
        /// </summary>
        /// <param name="listener"></param>
        public static void UnsubscribeToolbarEvent(Action<bool> listener)
        {
            if (GetProGridsType() == null)
                return;

            if (s_UnsubscribeToolbarEventDelegate == null)
                s_UnsubscribeToolbarEventDelegate = (Action<Action<bool>>)ReflectionUtility.GetOpenDelegate<Action<Action<bool>>>(GetProGridsType(), "RemoveToolbarEventSubscriber");

            if (s_UnsubscribeToolbarEventDelegate != null)
                s_UnsubscribeToolbarEventDelegate(listener);
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

            if (SnapEnabled())
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
