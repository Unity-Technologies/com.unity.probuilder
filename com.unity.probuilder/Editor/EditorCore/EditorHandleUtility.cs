using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Utilities for creating and manipulating Handles and points in GUI space.
    /// </summary>
    static class EditorHandleUtility
    {
        static Stack<Matrix4x4> s_HandleMatrix = new Stack<Matrix4x4>();

        public static bool SceneViewInUse(Event e)
        {
            return e.alt
                || Tools.current == Tool.View
                || GUIUtility.hotControl > 0
                || (e.isMouse && e.button > 0)
                || Tools.viewTool == ViewTool.FPS
                || Tools.viewTool == ViewTool.Orbit;
        }

        public static bool IsAppendModifier(EventModifiers em)
        {
            return (em & EventModifiers.Shift) == EventModifiers.Shift ||
                (em & EventModifiers.Control) == EventModifiers.Control ||
                (em & EventModifiers.Alt) == EventModifiers.Alt ||
                (em & EventModifiers.Command) == EventModifiers.Command;
        }

        const int HANDLE_PADDING = 8;
        const int LEFT_MOUSE_BUTTON = 0;
        const int MIDDLE_MOUSE_BUTTON = 2;

        static readonly Quaternion QuaternionUp = Quaternion.Euler(Vector3.right * 90f);
        static readonly Quaternion QuaternionRight = Quaternion.Euler(Vector3.up * 90f);
        static readonly Vector3 ConeDepth = new Vector3(0f, 0f, 16f);

        static readonly Color k_HandleColorUp = new Color(0f, .7f, 0f, .8f);
        static readonly Color k_HandleColorRight = new Color(0f, 0f, .7f, .8f);
        static readonly Color k_HandleColorRotate = new Color(0f, .7f, 0f, .8f);
        static readonly Color k_HandleColorScale = new Color(.7f, .7f, .7f, .8f);

        static Material s_HandleMaterial = null;

        public static Material handleMaterial
        {
            get
            {
                if (s_HandleMaterial == null)
                    s_HandleMaterial = (Material)EditorGUIUtility.LoadRequired("SceneView/2DHandleLines.mat");

                return s_HandleMaterial;
            }
        }

        public static int CurrentID { get { return currentId; } }
        static int currentId = -1;

        static Vector2 handleOffset = Vector2.zero;
        static Vector2 initialMousePosition = Vector2.zero;

        static HandleConstraint2D axisConstraint = new HandleConstraint2D(0, 0);    // Multiply this value by input to mask axis movement.
        public static HandleConstraint2D CurrentAxisConstraint { get { return axisConstraint; } }

        public static bool limitToLeftButton = true;

        /**
         * A 2D GUI view position handle.
         * @param id The Handle id.
         * @param position The position in GUI coordinates.
         * @param size How large in pixels to draw this handle.
         */
        public static Vector2 PositionHandle2d(int id, Vector2 position, int size)
        {
            int width = size / 4;
            var evt = Event.current;

            Rect handleRectUp = new Rect(position.x - width / 2, position.y - size - HANDLE_PADDING, width, size + HANDLE_PADDING);
            Rect handleRectRight = new Rect(position.x, position.y - width / 2, size, width + HANDLE_PADDING);

            Handles.color = Color.yellow;
            Handles.CircleHandleCap(-1, position, Quaternion.identity, width / 2f, Event.current.type);
            Handles.color = k_HandleColorUp;

            // Y Line
            Handles.DrawLine(position, position - Vector2.up * size);

            // Y Cone
            if (position.y - size > 0f)
                Handles.ConeHandleCap(0, ((Vector3)((position - Vector2.up * size))) - ConeDepth, QuaternionUp, width / 2, evt.type);

            Handles.color = k_HandleColorRight;

            // X Line
            Handles.DrawLine(position, position + Vector2.right * size);

            // X Cap
            if (position.y > 0f)
                Handles.ConeHandleCap(0, ((Vector3)((position + Vector2.right * size))) - ConeDepth, QuaternionRight, width / 2, evt.type);

            // If a Tool already is engaged and it's not this one, bail.
            if (currentId >= 0 && currentId != id)
                return position;

            Vector2 mousePosition = evt.mousePosition;
            Vector2 newPosition = position;

            if (currentId == id)
            {
                switch (evt.type)
                {
                    case EventType.MouseDrag:
                        newPosition = axisConstraint.Mask(mousePosition + handleOffset) + axisConstraint.InverseMask(position);
                        break;

                    case EventType.MouseUp:
                    case EventType.Ignore:
                        currentId = -1;
                        break;
                }
            }
            else
            {
                if (evt.type == EventType.MouseDown && ((!limitToLeftButton && evt.button != MIDDLE_MOUSE_BUTTON) || evt.button == LEFT_MOUSE_BUTTON))
                {
                    if (Vector2.Distance(mousePosition, position) < width / 2f)
                    {
                        currentId = id;
                        handleOffset = position - mousePosition;
                        axisConstraint = new HandleConstraint2D(1, 1);
                    }
                    else if (handleRectRight.Contains(mousePosition))
                    {
                        currentId = id;
                        handleOffset = position - mousePosition;
                        axisConstraint = new HandleConstraint2D(1, 0);
                    }
                    else if (handleRectUp.Contains(mousePosition))
                    {
                        currentId = id;
                        handleOffset = position - mousePosition;
                        axisConstraint = new HandleConstraint2D(0, 1);
                    }
                }
            }

            return newPosition;
        }

        static Vector2 s_InitialDirection;

        /// <summary>
        /// A 2D rotation handle. Behaves like HandleUtility.RotationHandle
        /// </summary>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static float RotationHandle2d(int id, Vector2 position, float rotation, int radius)
        {
            Event evt = Event.current;
            Vector2 mousePosition = evt.mousePosition;
            float newRotation = rotation;

            Vector2 currentDirection = (mousePosition - position).normalized;

            // Draw gizmos
            Handles.color = k_HandleColorRotate;
            Handles.CircleHandleCap(-1, position, Quaternion.identity, radius, evt.type);

            if (currentId == id)
            {
                Handles.color = Color.gray;
                Handles.DrawLine(position, position + (mousePosition - position).normalized * radius);
                GUI.Label(new Rect(position.x, position.y, 90f, 30f), newRotation.ToString("F2") + PreferenceKeys.DEGREE_SYMBOL);
            }

            // If a Tool already is engaged and it's not this one, bail.
            if (currentId >= 0 && currentId != id)
                return rotation;

            if (currentId == id)
            {
                switch (evt.type)
                {
                    case EventType.MouseDrag:

                        newRotation = Vector2.Angle(s_InitialDirection, currentDirection);

                        if (Vector2.Dot(new Vector2(-s_InitialDirection.y, s_InitialDirection.x), currentDirection) < 0)
                            newRotation = 360f - newRotation;
                        break;

                    case EventType.MouseUp:
                    case EventType.Ignore:
                        currentId = -1;
                        break;
                }
            }
            else
            {
                if (evt.type == EventType.MouseDown && ((!limitToLeftButton && evt.button != MIDDLE_MOUSE_BUTTON) || evt.button == LEFT_MOUSE_BUTTON))
                {
                    if (Mathf.Abs(Vector2.Distance(mousePosition, position) - radius) < 8)
                    {
                        currentId = id;
                        initialMousePosition = mousePosition;
                        s_InitialDirection = (initialMousePosition - position).normalized;
                        handleOffset = position - mousePosition;
                    }
                }
            }

            return newRotation;
        }

        /// <summary>
        /// Scale handle in 2d space.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Vector2 ScaleHandle2d(int id, Vector2 position, Vector2 scale, int size)
        {
            Event evt = Event.current;
            Vector2 mousePosition = evt.mousePosition;
            int width = size / 4;

            Handles.color = k_HandleColorUp;
            Handles.DrawLine(position, position - Vector2.up * size * scale.y);

            if (position.y - size > 0f)
                Handles.CubeHandleCap(0, ((Vector3)((position - Vector2.up * scale.y * size))) - Vector3.forward * 16, QuaternionUp, width / 3, evt.type);

            Handles.color = k_HandleColorRight;
            Handles.DrawLine(position, position + Vector2.right * size * scale.x);

            if (position.y > 0f)
                Handles.CubeHandleCap(0,
                    ((Vector3)((position + Vector2.right * scale.x * size))) - Vector3.forward * 16,
                    Quaternion.Euler(Vector3.up * 90f),
                    width / 3f,
                    evt.type);

            Handles.color = k_HandleColorScale;

            Handles.CubeHandleCap(0,
                ((Vector3)position) - Vector3.forward * 16,
                QuaternionUp,
                width / 2f,
                evt.type);

            // If a Tool already is engaged and it's not this one, bail.
            if (currentId >= 0 && currentId != id)
                return scale;

            Rect handleRectUp = new Rect(position.x - width / 2f, position.y - size - HANDLE_PADDING, width, size + HANDLE_PADDING);
            Rect handleRectRight = new Rect(position.x, position.y - width / 2f, size + 8, width);
            Rect handleRectCenter = new Rect(position.x - width / 2f, position.y - width / 2f, width, width);

            if (currentId == id)
            {
                switch (evt.type)
                {
                    case EventType.MouseDrag:
                        Vector2 diff = axisConstraint.Mask(mousePosition - initialMousePosition);
                        diff.x += size;
                        diff.y = -diff.y;   // gui space Y is opposite-world
                        diff.y += size;
                        scale = diff / size;
                        if (axisConstraint == HandleConstraint2D.None)
                        {
                            scale.x = Mathf.Min(scale.x, scale.y);
                            scale.y = Mathf.Min(scale.x, scale.y);
                        }
                        break;

                    case EventType.MouseUp:
                    case EventType.Ignore:
                        currentId = -1;
                        break;
                }
            }
            else
            {
                if (evt.type == EventType.MouseDown && ((!limitToLeftButton && evt.button != MIDDLE_MOUSE_BUTTON) || evt.button == LEFT_MOUSE_BUTTON))
                {
                    if (handleRectCenter.Contains(mousePosition))
                    {
                        currentId = id;
                        handleOffset = position - mousePosition;
                        initialMousePosition = mousePosition;
                        axisConstraint = new HandleConstraint2D(1, 1);
                    }
                    else if (handleRectRight.Contains(mousePosition))
                    {
                        currentId = id;
                        handleOffset = position - mousePosition;
                        initialMousePosition = mousePosition;
                        axisConstraint = new HandleConstraint2D(1, 0);
                    }
                    else if (handleRectUp.Contains(mousePosition))
                    {
                        currentId = id;
                        handleOffset = position - mousePosition;
                        initialMousePosition = mousePosition;
                        axisConstraint = new HandleConstraint2D(0, 1);
                    }
                }
            }

            return scale;
        }

        internal static void GetAllOverlapping(Vector2 mousePosition, List<GameObject> list)
        {
            list.Clear();

            GameObject nearestGameObject = null;

            do
            {
                nearestGameObject = HandleUtility.PickGameObject(mousePosition, false, list.ToArray());

                if (nearestGameObject != null)
                    list.Add(nearestGameObject);
                else
                    break;
            }
            while (nearestGameObject != null);
        }

        internal static void GetHovered(Vector2 mousePosition, List<GameObject> list)
        {
            list.Clear();
            var go = HandleUtility.PickGameObject(mousePosition, false);
            if (go != null)
                list.Add(go);
        }

        /**
         * Given two Vector2[] arrays, find the nearest two points within maxDelta and return the difference in offset.
         * @param points First Vector2[] array.
         * @param compare The Vector2[] array to compare @c points againts.
         * @mask If mask is not null, any index in mask will not be used in the compare array.
         * @param maxDelta The maximum distance for two points to be apart to be considered for nearness.
         * @notes This should probably use a divide and conquer algorithm instead of the O(n^2) approach (http://www.geeksforgeeks.org/closest-pair-of-points/)

         */
        public static bool NearestPointDelta(Vector2[] points, Vector2[] compare, int[] mask, float maxDelta, out Vector2 offset)
        {
            float dist = 0f;
            float minDist = maxDelta;
            bool foundMatch = false;
            offset = Vector2.zero;

            for (int i = 0; i < points.Length; i++)
            {
                for (int n = 0; n < compare.Length; n++)
                {
                    if (points[i] == compare[n]) continue;

                    dist = Vector2.Distance(points[i], compare[n]);

                    if (dist < minDist)
                    {
                        if (mask != null && System.Array.IndexOf(mask, n) > -1)
                            continue;

                        minDist = dist;
                        offset = compare[n] - points[i];
                        foundMatch = true;
                    }
                }
            }

            return foundMatch;
        }

        /**
         * Returns the index of the nearest point in the points array, or -1 if no point is within maxDelta range.
         */
        public static int NearestPoint(Vector2 point, Vector2[] points, float maxDelta)
        {
            float dist = 0f;
            float minDist = maxDelta;
            int index = -1;

            for (int i = 0; i < points.Length; i++)
            {
                if (point == points[i]) continue;

                dist = Vector2.Distance(point, points[i]);

                if (dist < minDist)
                {
                    minDist = dist;
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// Pick the closest point on a world space set of line segments.
        /// Similar to UnityEditor.HandleUtility version except this also
        /// returns the index of the segment that best matched (source modified
        /// from UnityEngine.HandleUtility class).
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="index"></param>
        /// <param name="distanceToLine"></param>
        /// <param name="closeLoop"></param>
        /// <param name="trs"></param>
        /// <returns></returns>
        public static Vector3 ClosestPointToPolyLine(List<Vector3> vertices, out int index, out float distanceToLine, bool closeLoop = false, Transform trs = null)
        {
            distanceToLine = Mathf.Infinity;

            if (trs != null)
                distanceToLine = HandleUtility.DistanceToLine(trs.TransformPoint(vertices[0]), trs.TransformPoint(vertices[1]));
            else
                distanceToLine = HandleUtility.DistanceToLine(vertices[0], vertices[1]);

            index = 0;
            int count = vertices.Count;

            for (int i = 2; i < (closeLoop ? count + 1 : count); i++)
            {
                var distance = 0f;

                if (trs != null)
                    distance = HandleUtility.DistanceToLine(trs.TransformPoint(vertices[i - 1]), trs.TransformPoint(vertices[i % count]));
                else
                    distance = HandleUtility.DistanceToLine(vertices[i - 1], vertices[i % count]);

                if (distance < distanceToLine)
                {
                    distanceToLine = distance;
                    index = i - 1;
                }
            }

            Vector3 point_a = trs != null ? trs.TransformPoint(vertices[index]) : vertices[index];
            Vector3 point_b = trs != null ? trs.TransformPoint(vertices[(index + 1) % count]) : vertices[index + 1];

            index++;

            Vector2 gui_a = Event.current.mousePosition - HandleUtility.WorldToGUIPoint(point_a);
            Vector2 gui_b = HandleUtility.WorldToGUIPoint(point_b) - HandleUtility.WorldToGUIPoint(point_a);

            float magnitude = gui_b.magnitude;
            float travel = Vector3.Dot(gui_b, gui_a);

            if (magnitude > 1E-06f)
                travel /= magnitude * magnitude;

            Vector3 p = Vector3.Lerp(point_a, point_b, Mathf.Clamp01(travel));

            return trs != null ? trs.InverseTransformPoint(p) : p;
        }

        internal static void PushMatrix()
        {
            s_HandleMatrix.Push(Handles.matrix);
        }

        internal static void PopMatrix()
        {
            Handles.matrix = s_HandleMatrix.Pop();
        }

        /// <summary>
        /// Collects coincident vertices and returns a rotation calculated from the average normal and bitangent.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        internal static Quaternion GetRotation(ProBuilderMesh mesh, IEnumerable<int> indices)
        {
            if (!mesh.HasArrays(MeshArrays.Normal))
                Normals.CalculateNormals(mesh);

            if (!mesh.HasArrays(MeshArrays.Tangent))
                Normals.CalculateTangents(mesh);

            var normals = mesh.normalsInternal;
            var tangents = mesh.tangentsInternal;

            var nrm = Vector3.zero;
            var tan = Vector4.zero;
            float count = 0;

            foreach (var index in indices)
            {
                var n = normals[index];
                var t = tangents[index];

                nrm.x += n.x;
                nrm.y += n.y;
                nrm.z += n.z;

                tan.x += t.x;
                tan.y += t.y;
                tan.z += t.z;
                tan.w += t.w;

                count++;
            }

            nrm.x /= count;
            nrm.y /= count;
            nrm.z /= count;

            tan.x /= count;
            tan.y /= count;
            tan.z /= count;
            tan.w /= count;

            if (nrm == Vector3.zero || tan == Vector4.zero)
                return mesh.transform.rotation;

            var bit = Vector3.Cross(nrm, tan * tan.w);

            return mesh.transform.rotation * Quaternion.LookRotation(nrm, bit);
        }

        internal static Quaternion GetFaceRotation(ProBuilderMesh mesh, HandleOrientation orientation, IEnumerable<Face> faces)
        {
            if (mesh == null)
                return Quaternion.identity;

            switch (orientation)
            {
                case HandleOrientation.ActiveElement:

                    if (mesh.selectedFaceCount < 1)
                        goto case HandleOrientation.ActiveObject;

                    var face = faces.First();

                    // Intentionally not using coincident vertices here. We want the normal of just the face, not an
                    // average of it's neighbors.
                    return GetRotation(mesh, face.distinctIndexesInternal);

                case HandleOrientation.ActiveObject:
                    return mesh.transform.rotation;

                default:
                    return Quaternion.identity;
            }
        }

        internal static Quaternion GetEdgeRotation(ProBuilderMesh mesh, HandleOrientation orientation, IEnumerable<Edge> edges)
        {
            if (mesh == null)
                return Quaternion.identity;

            switch (orientation)
            {
                case HandleOrientation.ActiveElement:
                    if (mesh.selectedEdgeCount < 1)
                        goto case HandleOrientation.ActiveObject;

                    // Getting an average of the edge normals isn't very helpful in real world uses, so we just use the
                    // first selected edge for orientation.
                    // This function accepts an enumerable because in the future we may want to do something more
                    // sophisticated, and it's convenient because selections are stored as collections.
                    var face = EdgeUtility.GetFace(mesh, edges.First());

                    if (face == null)
                        goto case HandleOrientation.ActiveObject;

                    Normal nrm = Math.NormalTangentBitangent(mesh, face);

                    if (nrm.normal == Vector3.zero || nrm.bitangent == Vector3.zero)
                        goto case HandleOrientation.ActiveObject;

                    return mesh.transform.rotation * Quaternion.LookRotation(nrm.normal, nrm.bitangent);

                case HandleOrientation.ActiveObject:
                    return mesh.transform.rotation;

                default:
                    return Quaternion.identity;
            }
        }

        internal static Quaternion GetVertexRotation(ProBuilderMesh mesh, HandleOrientation orientation, IEnumerable<int> vertices)
        {
            if (mesh == null)
                return Quaternion.identity;

            switch (orientation)
            {
                case HandleOrientation.ActiveElement:
                    if (mesh.selectedVertexCount < 1)
                        goto case HandleOrientation.ActiveObject;

                    return GetRotation(mesh, vertices);

                case HandleOrientation.ActiveObject:
                    return mesh.transform.rotation;

                default:
                    return Quaternion.identity;
            }
        }
    }
}
