using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Provides functions for picking mesh elements in a view. This allows the Editor to either render a
    /// texture to test, or cast a ray.
    /// </summary>
    public static class SelectionPicker
    {
        /// <summary>
        /// Picks the vertex indices contained within a rect (rectangular selection).
        /// </summary>
        /// <param name="cam">Use this camera to evaluate whether any vertices are inside the `rect`.</param>
        /// <param name="rect">Rect is in GUI space, where 0,0 is the top left of the screen, and `width = cam.pixelWidth / pointsPerPixel`.</param>
        /// <param name="selectable">The collection of objects to check for selectability. ProBuilder verifies whether these objects fall inside the `rect`.</param>
        /// <param name="options">The options that define whether to include mesh elements that are hidden and whether to consider elements that only partially fall inside the `rect`.</param>
        /// <param name="pixelsPerPoint">
        /// Scale the render texture to match `rect` coordinates. By default, ProBuilder doesn't scale the texture (the default value is set to 1.0),
        /// but you can specify <see cref="UnityEditor.EditorGUIUtility.pixelsPerPoint" /> if you need to use a different number of screen pixels per point.</param>
        /// <returns>A dictionary of ProBuilderMesh and sharedIndexes that are in the selection `rect`. To get triangle indexes, use the <see cref="ProBuilderMesh.sharedVertices">sharedVertices</see> array.</returns>
        public static Dictionary<ProBuilderMesh, HashSet<int>> PickVerticesInRect(
            Camera cam,
            Rect rect,
            IList<ProBuilderMesh> selectable,
            PickerOptions options,
            float pixelsPerPoint = 1f)
        {
            if (options.depthTest)
            {
                return SelectionPickerRenderer.PickVerticesInRect(
                    cam,
                    rect,
                    selectable,
                    true,
                    (int)(cam.pixelWidth / pixelsPerPoint),
                    (int)(cam.pixelHeight / pixelsPerPoint));
            }

            // while the selectionpicker render path supports no depth test picking, it's usually faster to skip
            // the render. also avoids issues with vertex billboards obscuring one another.
            var selected = new Dictionary<ProBuilderMesh, HashSet<int>>();

            foreach (var pb in selectable)
            {
                if (!pb.selectable)
                    continue;

                SharedVertex[] sharedIndexes = pb.sharedVerticesInternal;
                HashSet<int> inRect = new HashSet<int>();
                Vector3[] positions = pb.positionsInternal;
                var trs = pb.transform;
                float pixelHeight = cam.pixelHeight;

                for (int n = 0; n < sharedIndexes.Length; n++)
                {
                    Vector3 v = trs.TransformPoint(positions[sharedIndexes[n][0]]);
                    Vector3 p = cam.WorldToScreenPoint(v);

                    if (p.z < cam.nearClipPlane)
                        continue;

                    p.x /= pixelsPerPoint;
                    p.y = (pixelHeight - p.y) / pixelsPerPoint;

                    if (rect.Contains(p))
                        inRect.Add(n);
                }

                selected.Add(pb, inRect);
            }

            return selected;
        }

        /// <summary>
        /// Picks the <see cref="Face" /> objects contained within a rect (rectangular selection).
        /// </summary>
        /// <param name="cam">Use this camera to evaluate whether any face object(s) are inside the `rect`.</param>
        /// <param name="rect">Rect is in GUI space, where 0,0 is the top left of the screen, and `width = cam.pixelWidth / pointsPerPixel`.</param>
        /// <param name="selectable">The collection of objects to check for selectability. ProBuilder verifies whether these objects fall inside the `rect`.</param>
        /// <param name="options">The options that define whether to include mesh elements that are hidden and whether to consider elements that only partially fall inside the `rect`.</param>
        /// <param name="pixelsPerPoint">
        /// Scale the render texture to match `rect` coordinates. By default, ProBuilder doesn't scale the texture (the default value is set to 1.0),
        /// but you can specify <see cref="UnityEditor.EditorGUIUtility.pixelsPerPoint" /> if you need to use a different number of screen pixels per point.</param>
        /// <returns>A dictionary of ProBuilderMesh and Face objects that are in the selection `rect`. </returns>
        public static Dictionary<ProBuilderMesh, HashSet<Face>> PickFacesInRect(
            Camera cam,
            Rect rect,
            IList<ProBuilderMesh> selectable,
            PickerOptions options,
            float pixelsPerPoint = 1f)
        {
            if (options.depthTest && options.rectSelectMode == RectSelectMode.Partial)
            {
                return SelectionPickerRenderer.PickFacesInRect(
                    cam,
                    rect,
                    selectable,
                    (int)(cam.pixelWidth / pixelsPerPoint),
                    (int)(cam.pixelHeight / pixelsPerPoint));
            }

            var selected = new Dictionary<ProBuilderMesh, HashSet<Face>>();

            foreach (var pb in selectable)
            {
                if (!pb.selectable)
                    continue;

                HashSet<Face> selectedFaces = new HashSet<Face>();
                Transform trs = pb.transform;
                Vector3[] positions = pb.positionsInternal;
                Vector3[] screenPoints = new Vector3[pb.vertexCount];

                for (int nn = 0; nn < pb.vertexCount; nn++)
                    screenPoints[nn] = cam.ScreenToGuiPoint(cam.WorldToScreenPoint(trs.TransformPoint(positions[nn])), pixelsPerPoint);

                for (int n = 0; n < pb.facesInternal.Length; n++)
                {
                    Face face = pb.facesInternal[n];

                    // rect select = complete
                    if (options.rectSelectMode == RectSelectMode.Complete)
                    {
                        // face is behind the camera
                        if (screenPoints[face.indexesInternal[0]].z < cam.nearClipPlane)
                            continue;

                        // only check the first index per quad, and if it checks out, then check every other point
                        if (rect.Contains(screenPoints[face.indexesInternal[0]]))
                        {
                            bool nope = false;

                            for (int q = 1; q < face.distinctIndexesInternal.Length; q++)
                            {
                                int index = face.distinctIndexesInternal[q];

                                if (screenPoints[index].z < cam.nearClipPlane || !rect.Contains(screenPoints[index]))
                                {
                                    nope = true;
                                    break;
                                }
                            }

                            if (!nope)
                            {
                                if (!options.depthTest ||
                                    !HandleUtility.PointIsOccluded(cam, pb, trs.TransformPoint(Math.Average(positions, face.distinctIndexesInternal))))
                                {
                                    selectedFaces.Add(face);
                                }
                            }
                        }
                    }
                    // rect select = partial
                    else
                    {
                        Bounds2D poly = new Bounds2D(screenPoints, face.edgesInternal);
                        bool overlaps = false;

                        if (poly.Intersects(rect))
                        {
                            // if rect contains one point of polygon, it overlaps
                            for (int nn = 0; nn < face.distinctIndexesInternal.Length && !overlaps; nn++)
                            {
                                Vector3 p = screenPoints[face.distinctIndexesInternal[nn]];
                                overlaps = p.z > cam.nearClipPlane && rect.Contains(p);
                            }

                            // if polygon contains one point of rect, it overlaps. otherwise check for edge intersections
                            if (!overlaps)
                            {
                                Vector2 tl = new Vector2(rect.xMin, rect.yMax);
                                Vector2 tr = new Vector2(rect.xMax, rect.yMax);
                                Vector2 bl = new Vector2(rect.xMin, rect.yMin);
                                Vector2 br = new Vector2(rect.xMax, rect.yMin);

                                overlaps = Math.PointInPolygon(screenPoints, poly, face.edgesInternal, tl);
                                if (!overlaps) overlaps = Math.PointInPolygon(screenPoints, poly, face.edgesInternal, tr);
                                if (!overlaps) overlaps = Math.PointInPolygon(screenPoints, poly, face.edgesInternal, br);
                                if (!overlaps) overlaps = Math.PointInPolygon(screenPoints, poly, face.edgesInternal, bl);

                                // if any polygon edge intersects rect
                                for (int nn = 0; nn < face.edgesInternal.Length && !overlaps; nn++)
                                {
                                    if (Math.GetLineSegmentIntersect(tr, tl, screenPoints[face.edgesInternal[nn].a], screenPoints[face.edgesInternal[nn].b]))
                                        overlaps = true;
                                    else if (Math.GetLineSegmentIntersect(tl, bl, screenPoints[face.edgesInternal[nn].a], screenPoints[face.edgesInternal[nn].b]))
                                        overlaps = true;
                                    else if (Math.GetLineSegmentIntersect(bl, br, screenPoints[face.edgesInternal[nn].a], screenPoints[face.edgesInternal[nn].b]))
                                        overlaps = true;
                                    else if (Math.GetLineSegmentIntersect(br, tl, screenPoints[face.edgesInternal[nn].a], screenPoints[face.edgesInternal[nn].b]))
                                        overlaps = true;
                                }
                            }
                        }

                        // don't test occlusion since that case is handled special
                        if (overlaps)
                            selectedFaces.Add(face);
                    }
                }

                selected.Add(pb, selectedFaces);
            }

            return selected;
        }

        /// <summary>
        /// Picks the <see cref="Edge" /> objects contained within a rect (rectangular selection).
        /// </summary>
        /// <param name="cam">Use this camera to evaluate whether any edge object(s) are inside the `rect`.</param>
        /// <param name="rect">Rect is in GUI space, where 0,0 is the top left of the screen, and `width = cam.pixelWidth / pointsPerPixel`.</param>
        /// <param name="selectable">The collection of objects to check for selectability. ProBuilder verifies whether these objects fall inside the `rect`.</param>
        /// <param name="options">The options that define whether to include mesh elements that are hidden and whether to consider elements that only partially fall inside the `rect`.</param>
        /// <param name="pixelsPerPoint">
        /// Scale the render texture to match `rect` coordinates. By default, ProBuilder doesn't scale the texture (the default value is set to 1.0),
        /// but you can specify <see cref="UnityEditor.EditorGUIUtility.pixelsPerPoint" /> if you need to use a different number of screen pixels per point.</param>
        /// <returns>A dictionary of ProBuilderMesh and Edge objects that are in the selection `rect`. </returns>
        public static Dictionary<ProBuilderMesh, HashSet<Edge>> PickEdgesInRect(
            Camera cam,
            Rect rect,
            IList<ProBuilderMesh> selectable,
            PickerOptions options,
            float pixelsPerPoint = 1f)
        {
            if (options.depthTest && options.rectSelectMode == RectSelectMode.Partial)
            {
                return SelectionPickerRenderer.PickEdgesInRect(
                    cam,
                    rect,
                    selectable,
                    true,
                    (int)(cam.pixelWidth / pixelsPerPoint),
                    (int)(cam.pixelHeight / pixelsPerPoint));
            }

            var selected = new Dictionary<ProBuilderMesh, HashSet<Edge>>();

            foreach (var pb in selectable)
            {
                if (!pb.selectable)
                    continue;

                Transform trs = pb.transform;
                var selectedEdges = new HashSet<Edge>();

                for (int i = 0, fc = pb.faceCount; i < fc; i++)
                {
                    var edges = pb.facesInternal[i].edgesInternal;

                    for (int n = 0, ec = edges.Length; n < ec; n++)
                    {
                        var edge = edges[n];

                        var posA = trs.TransformPoint(pb.positionsInternal[edge.a]);
                        var posB = trs.TransformPoint(pb.positionsInternal[edge.b]);

                        Vector3 a = cam.ScreenToGuiPoint(cam.WorldToScreenPoint(posA), pixelsPerPoint);
                        Vector3 b = cam.ScreenToGuiPoint(cam.WorldToScreenPoint(posB), pixelsPerPoint);

                        switch (options.rectSelectMode)
                        {
                            case RectSelectMode.Complete:
                            {
                                // if either of the positions are clipped by the camera we cannot possibly select both, skip it
                                if ((a.z < cam.nearClipPlane || b.z < cam.nearClipPlane))
                                    continue;

                                if (rect.Contains(a) && rect.Contains(b))
                                {
                                    if (!options.depthTest || !HandleUtility.PointIsOccluded(cam, pb, (posA + posB) * .5f))
                                        selectedEdges.Add(edge);
                                }

                                break;
                            }

                            case RectSelectMode.Partial:
                            {
                                // partial + depth test is covered earlier
                                if (Math.RectIntersectsLineSegment(rect, a, b))
                                    selectedEdges.Add(edge);

                                break;
                            }
                        }
                    }
                }

                selected.Add(pb, selectedEdges);
            }

            return selected;
        }

        /// <summary>
        /// Returns the first face on a ProBuilder mesh that collides with the ray cast by ProBuilder, given a specific screen position and camera.
        /// </summary>
        /// <param name="camera">The camera to use when calculating the raycast.</param>
        /// <param name="mousePosition">The screen position to use when calculating the raycast.</param>
        /// <param name="pickable">The ProBuilderMesh to test for the ray/face intersection.</param>
        /// <returns>The intersected face if successful; null if the ray failed to hit a face.</returns>
        public static Face PickFace(Camera camera, Vector3 mousePosition, ProBuilderMesh pickable)
        {
            Ray ray = camera.ScreenPointToRay(mousePosition);

            RaycastHit hit;

            if (HandleUtility.FaceRaycast(ray, pickable, out hit, Mathf.Infinity, CullingMode.Back))
                return pickable.facesInternal[hit.face];

            return null;
        }
    }
}
