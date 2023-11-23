using UnityEditor.EditorTools;
using UnityEditor.SettingsManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    enum SnapAxis
    {
        /// <summary>
        /// When an <see cref="EditorTool"/> is modifying vertices, snap vertex positions only to the axis that is currently moving.
        /// </summary>
        ActiveAxis,
        /// <summary>
        /// When an <see cref="EditorTool"/> is modifying vertices, snap vertex positions in all axis directions.
        /// </summary>
        AllAxes
    }

    enum SnapMode
    {
        None,
        Relative,
        World
    }

    // Snapping for ProBuilder tools. If ProGrids is active, ProGrids settings will take priority. Otherwise the Unity
    // EditorSnap state is used.
    static class EditorSnapping
    {
        [UserSetting("Snap Settings", "Snap as Group", "When enabled, selected mesh elements will keep their relative offsets when snapping to the grid. When disabled, every element in the selection is snapped to grid independently.")]
        static Pref<bool> s_SnapAsGroup = new Pref<bool>("ProBuilder.SnapSettings.s_SnapAsGroup", true, SettingsScope.User);


        [UserSetting("Snap Settings", "Snap Axis", "When an Editor Tool is modifying vertices, this setting determines in which directions the affected vertices will snap.")]
        static Pref<SnapAxis> s_SnapAxis = new Pref<SnapAxis>("ProBuilder.SnapSettings.s_SnapAxis", SnapAxis.ActiveAxis, SettingsScope.User);

        public static SnapMode snapMode
        {
            get
            {
                if (EditorSnapSettings.incrementalSnapActive)
                    return SnapMode.Relative;
                if (ProGridsInterface.SnapEnabled() || EditorSnapSettings.gridSnapActive && Tools.pivotRotation == PivotRotation.Global)
                    return SnapMode.World;
                return SnapMode.None;
            }
        }

        public static bool snapAsGroup
        {
            get { return ProGridsInterface.IsActive() ? ProGridsInterface.GetSnapAsGroup() : s_SnapAsGroup.value; }
        }

        internal static SnapAxis snapMethod
        {
            get { return ProGridsInterface.IsActive() ? ProGridsInterface.GetSnapMethod() : s_SnapAxis.value; }
        }

        internal static Vector3 activeMoveSnapValue
        {
            get
            {
                switch (snapMode)
                {
                    case SnapMode.Relative:
                        return incrementalSnapMoveValue;
                    case SnapMode.World:
                        return worldSnapMoveValue;
                    default:
                        return Vector3.zero;
                }
            }
        }

        internal static Vector3 activeScaleSnapValue
        {
            get
            {
                switch (snapMode)
                {
                    case SnapMode.Relative:
                    case SnapMode.World:
                        return Vector3.one * incrementalSnapScaleValue;
                    default:
                        return Vector3.zero;
                }
            }
        }

        public static float MoveSnap(float value)
        {
            return ProBuilderSnapping.Snap(value, activeMoveSnapValue.x);
        }

        public static Vector3 MoveSnap(Vector3 value)
        {
            return ProBuilderSnapping.Snap(value, activeMoveSnapValue);
        }

        public static float RotateSnap(float value)
        {
            return ProBuilderSnapping.Snap(value, activeRotateSnapValue);
        }

        public static Vector3 ScaleSnap(Vector3 value)
        {
            return ProBuilderSnapping.Snap(value, Vector3.one * activeRotateSnapValue);
        }

        internal static float activeRotateSnapValue
        {
            get
            {
                switch (snapMode)
                {
                    case SnapMode.Relative:
                    case SnapMode.World:
                        return incrementalSnapRotateValue;
                    default:
                        return 0f;
                }
            }
        }

        public static Vector3 incrementalSnapMoveValue => EditorSnapSettings.move;

        public static Vector3 worldSnapMoveValue
            => ProGridsInterface.IsActive() ? ProGridsInterface.SnapValue() * Vector3.one : GridSettings.size;

        public static float incrementalSnapRotateValue => EditorSnapSettings.rotate;

        public static float incrementalSnapScaleValue => EditorSnapSettings.scale;
    }
}
