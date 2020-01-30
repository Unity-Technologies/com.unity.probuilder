using UnityEditor.SettingsManagement;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    enum SnapAxis
    {
        /// <summary>
        /// When an <see cref="UnityEditor.EditorTool"/> is modifying vertices, snap vertex positions only to the axis that is currently moving.
        /// </summary>
        ActiveAxis,
        /// <summary>
        /// When an <see cref="UnityEditor.EditorTool"/> is modifying vertices, snap vertex positions in all axis directions.
        /// </summary>
        AllAxes
    }

    enum SnapMode
    {
        None,
        Relative,
        World
    }

    // Snap settings for ProBuilder tools. If ProGrids is active, ProGrids settings will take priority.
    static class ProBuilderSnapSettings
    {
#if UNITY_2019_3_OR_NEWER
        [UserSetting("Snap Settings", "Snap as Group", "When enabled, selected mesh elements will keep their relative offsets when snapping to the grid. When disabled, every element in the selection is snapped to grid independently.")]
#endif
        static Pref<bool> s_SnapAsGroup = new Pref<bool>("ProBuilder.SnapSettings.s_SnapAsGroup", true, SettingsScope.User);

#if UNITY_2019_3_OR_NEWER
        [UserSetting("Snap Settings", "Snap Axis", "When an Editor Tool is modifying vertices, this setting determines in which directions the affected vertices will snap.")]
#endif
        static Pref<SnapAxis> s_SnapAxis = new Pref<SnapAxis>("ProBuilder.SnapSettings.s_SnapAxis", SnapAxis.ActiveAxis, SettingsScope.User);

        public static SnapMode snapMode
        {
            get
            {
#if UNITY_2019_3_OR_NEWER
                if (EditorSnapSettings.incrementalSnapActive)
                    return SnapMode.Relative;
                if (ProGridsInterface.IsActive() || EditorSnapSettings.gridSnapEnabled)
                    return SnapMode.World;
                return SnapMode.None;
#else
                if (Event.current != null && EditorGUI.actionKey)
                    return SnapMode.Relative;
                if (ProGridsInterface.IsActive())
                    return SnapMode.World;
                return SnapMode.None;
#endif
            }
        }

        public static bool snapAsGroup
        {
            get { return ProGridsInterface.IsActive() ? ProGridsInterface.GetSnapAsGroup() : s_SnapAsGroup.value; }
        }

        public static SnapAxis snapMethod
        {
            get { return ProGridsInterface.IsActive() ? ProGridsInterface.GetSnapMethod() : s_SnapAxis.value; }
        }

        public static Vector3 incrementalSnapMoveValue
        {
#if UNITY_2019_3_OR_NEWER
            get { return EditorSnapSettings.move; }
#else
            get { return new Vector3(relativeSnapX, relativeSnapY, relativeSnapZ); }
#endif
        }

        public static Vector3 worldSnapMoveValue
        {
            get
            {
#if UNITY_2019_3_OR_NEWER
                return ProGridsInterface.IsActive() ? ProGridsInterface.SnapValue() * Vector3.one : GridSettings.size;
#else
                return ProGridsInterface.IsActive() ? ProGridsInterface.SnapValue() * Vector3.one : Vector3.zero;
#endif
            }
        }

        public static float incrementalSnapRotateValue
        {
            get
            {
#if UNITY_2019_3_OR_NEWER
                return EditorSnapSettings.rotate;
#else
                return relativeSnapRotation;
#endif
            }
        }

        public static float incrementalSnapScaleValue
        {
            get
            {
#if UNITY_2019_3_OR_NEWER
                return EditorSnapSettings.scale;
#else
                return relativeSnapScale;
#endif
            }
        }
#if !UNITY_2019_3_OR_NEWER
        const string UnityMoveSnapX = "MoveSnapX";
        const string UnityMoveSnapY = "MoveSnapY";
        const string UnityMoveSnapZ = "MoveSnapZ";
        const string UnityScaleSnap = "ScaleSnap";
        const string UnityRotateSnap = "RotationSnap";

        internal static float relativeSnapX
        {
            get { return EditorPrefs.GetFloat(UnityMoveSnapX, 1f); }
        }

        internal static float relativeSnapY
        {
            get { return EditorPrefs.GetFloat(UnityMoveSnapY, 1f); }
        }

        internal static float relativeSnapZ
        {
            get { return EditorPrefs.GetFloat(UnityMoveSnapZ, 1f); }
        }

        internal static float relativeSnapScale
        {
            get { return EditorPrefs.GetFloat(UnityScaleSnap, .1f); }
        }

        internal static float relativeSnapRotation
        {
            get { return EditorPrefs.GetFloat(UnityRotateSnap, 15f); }
        }
#endif
    }
}
