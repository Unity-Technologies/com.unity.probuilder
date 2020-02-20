using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine;

namespace SlideyDebuggey
{
    public enum ProjectionMethod
    {
        Legacy,
        CameraAligned,
        DualWorldAxis
    }

    public static class HandleUtility2
    {
        const float k_HandleSize = .15f;
        const float k_HandleOpacity = .33f;

        static SavedInt s_ProjectionMethod = new SavedInt("HandleUtility.projectionMethod", (int) ProjectionMethod.CameraAligned);

        public static ProjectionMethod projectionMethod
        {
            get { return (ProjectionMethod) s_ProjectionMethod.value; }
            set { s_ProjectionMethod.value = (int) value; }
        }
        public static bool s_VisualizeHitTesting;
        public static bool s_VisualizeHitPlane = true;

        static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        internal static Vector3 CalcPositionWithConstraint(Camera camera, Vector2 gui, Vector3 constraintOrigin, Vector3 constraintDir)
        {
            switch (projectionMethod)
            {
                case ProjectionMethod.DualWorldAxis:
                    return CalcMousePositionOnConstraintAlignAxis(Camera.current, gui, constraintOrigin, constraintDir);

                case ProjectionMethod.CameraAligned:
                    return CalcMousePositionOnConstraint(Camera.current, gui, constraintOrigin, constraintDir);

                default:
                    return Vector3.zero;
            }
        }

        static bool PlaneRayPointOnLineParam(Ray ray, Vector3 constraintOrigin, Vector3 constraintDir, Vector3 constraintNrm, out float param)
        {
            var plane = new Plane(constraintNrm, constraintOrigin);
            param = Mathf.Infinity;
            if (!plane.Raycast(ray, out float distance))
                return false;
            param = HandleUtility.PointOnLineParameter(ray.GetPoint(distance), constraintOrigin, constraintDir);
            return true;
        }

        public static Vector3 CalcMousePositionOnConstraint(Camera camera, Vector2 gui, Vector3 constraintOrigin, Vector3 constraintDir)
        {
            Vector3 constraintToCameraTangent = Vector3.Cross(constraintDir, camera.transform.position - constraintOrigin).normalized;
            Vector3 constraintPlaneNormal = Vector3.Cross(constraintDir, constraintToCameraTangent).normalized;
            Plane plane = new Plane(constraintPlaneNormal, constraintOrigin);
            var ray = HandleUtility.GUIPointToWorldRay(gui);

            if (s_VisualizeHitPlane)
                EditorMeshHandles.DrawPlane(plane, constraintOrigin, Color.cyan);

            if (plane.Raycast(ray, out float distance))
            {
                var pointOnPlane = ray.GetPoint(distance);
                var res = constraintOrigin + constraintDir * HandleUtility.PointOnLineParameter(pointOnPlane, constraintOrigin, constraintDir);

                if (s_VisualizeHitTesting)
                {
                    Handles.color = new Color(0f,1f,1f, k_HandleOpacity);
                    Handles.DrawLine(pointOnPlane, res);
                    Handles.CubeHandleCap(-1, res, Quaternion.identity, HandleUtility.GetHandleSize(res) * k_HandleSize, Event.current.type);
                    Handles.CubeHandleCap(-1, pointOnPlane, Quaternion.identity, HandleUtility.GetHandleSize(res) * k_HandleSize, Event.current.type);
                }

                return res;
            }

            return Vector3.zero;
        }

        public static Vector3 CalcMousePositionOnConstraintAlignAxis(Camera camera, Vector2 gui, Vector3 constraintOrigin, Vector3 constraintDir)
        {
            Vector3 constraintTangentA = 1f - Mathf.Abs(Vector3.Dot(constraintDir, Vector3.right)) < Mathf.Epsilon
                ? Vector3.Cross(constraintDir, Vector3.forward)
                : Vector3.Cross(constraintDir, Vector3.right);
            Vector3 constraintTangentB = Vector3.Cross(constraintDir, constraintTangentA);
            Ray ray = HandleUtility.GUIPointToWorldRay(gui);

            float a, b;
            bool ra = PlaneRayPointOnLineParam(ray, constraintOrigin, constraintDir, constraintTangentA, out a);
            bool rb = PlaneRayPointOnLineParam(ray, constraintOrigin, constraintDir, constraintTangentB, out b);
            if (!(ra | rb))
                return Vector3.zero;

            if (s_VisualizeHitPlane)
            {
                Plane planeA = new Plane(constraintTangentA, constraintOrigin);
                Plane planeB = new Plane(constraintTangentB, constraintOrigin);
                EditorMeshHandles.DrawPlane(planeA, constraintOrigin, Color.red);
                EditorMeshHandles.DrawPlane(planeB, constraintOrigin, Color.green);
            }

            if (s_VisualizeHitTesting)
            {
                var position = constraintOrigin + constraintDir * Mathf.Min(a, b);
                Handles.color = new Color(1,1,0,k_HandleOpacity);
                Handles.CubeHandleCap(-1, position, Quaternion.identity, HandleUtility.GetHandleSize(position) * k_HandleSize, Event.current.type);

                // for the sake of debugging we'll replicate some code here
                var intersectingPlane = new Plane(a < b ? constraintTangentA : constraintTangentB, constraintOrigin);
                if (intersectingPlane.Raycast(ray, out float d))
                {
                    var p = ray.GetPoint(d);
                    Handles.CubeHandleCap(-1, p, Quaternion.identity, HandleUtility.GetHandleSize(p) * k_HandleSize, Event.current.type);
                    Handles.DrawLine(p, position);
                }
            }

            return constraintOrigin + constraintDir * Mathf.Min(a, b);
        }

        public static Vector3 CalcLineTranslation2D(Camera camera, Vector2 gui, Vector3 constraintOrigin, Vector3 constraintDirection)
        {
            var screenConstraintOrigin = HandleUtility.WorldToGUIPoint(constraintOrigin);
            var screenConstraintDirection = HandleUtility.WorldToGUIPoint(constraintOrigin + constraintDirection) - screenConstraintOrigin;
            Handles.BeginGUI();
            GUI.Label(new Rect(screenConstraintOrigin.x, screenConstraintOrigin.y, 32f, 32f), "o");
            GUI.Label(new Rect(screenConstraintDirection.x, screenConstraintDirection.y, 32f, 32f), "d");

            var intersect = screenConstraintOrigin + screenConstraintDirection * HandleUtility.PointOnLineParameter(gui, screenConstraintOrigin, screenConstraintDirection);
            GUI.Label(new Rect(intersect.x - 16, intersect.y - 16, 32f, 32f), "I", EditorStyles.helpBox);
            Handles.EndGUI();

            return Vector3.zero;
        }

        public static Vector3 CalcMousePositionOnConstraintAlignToRay(Camera camera, Vector2 gui, Vector3 constraintOrigin, Vector3 constraintDir)
        {
            var ray = HandleUtility.GUIPointToWorldRay(gui);
            Vector3 constraintRayTangent = Vector3.Cross(constraintDir, ray.direction);
            Vector3 constraintPlaneNormal = Vector3.Cross(constraintDir, constraintRayTangent).normalized;
            Plane plane = new Plane(constraintPlaneNormal, constraintOrigin);
            Vector3 res = Vector3.zero;

            Handles.BeginGUI();
            GUILayout.Box($"dot(constraintDir, ray): {Vector3.Dot(constraintDir, ray.direction)}", GUILayout.Width(256));
            GUILayout.Box($"dot(constraintDir, rayTangent): {Vector3.Dot(constraintDir, constraintRayTangent)}");
            GUILayout.Box($"dot(constraintDir, constraintPlaneNormal): {Vector3.Dot(constraintDir, constraintPlaneNormal)}");
            Handles.EndGUI();

            if (plane.Raycast(ray, out float distance))
            {
                var pointOnPlane = ray.GetPoint(distance);
                res = constraintDir * HandleUtility.PointOnLineParameter(pointOnPlane, constraintOrigin, constraintDir);
                Handles.color = new Color(1, 1, 0, k_HandleOpacity);
                Handles.DrawLine(pointOnPlane, res);
                Handles.CubeHandleCap(-1, res, Quaternion.identity, HandleUtility.GetHandleSize(res) * k_HandleSize, Event.current.type);
                Handles.CubeHandleCap(-1, pointOnPlane, Quaternion.identity, HandleUtility.GetHandleSize(res) * k_HandleSize, Event.current.type);
            }

            EditorMeshHandles.DrawPlane(plane, constraintOrigin, Color.yellow);
            return res;
        }

    }
}
