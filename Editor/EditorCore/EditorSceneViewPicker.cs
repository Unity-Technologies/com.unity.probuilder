using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using PHandleUtility = UnityEngine.ProBuilder.HandleUtility;
using UHandleUtility = UnityEditor.HandleUtility;
using RaycastHit = UnityEngine.ProBuilder.RaycastHit;

namespace UnityEditor.ProBuilder
{
    struct ScenePickerPreferences
    {
        public const float maxPointerDistance = 40f;
        public const float offPointerMultiplier = 1.2f;

        public CullingMode cullMode;
        public SelectionModifierBehavior selectionModifierBehavior;
        public RectSelectMode rectSelectMode;
    }

    static class EditorSceneViewPicker
    {
        static int s_DeepSelectionPrevious = 0x0;
        static bool s_AppendModifierPreviousState = false;
        static SceneSelection s_Selection = new SceneSelection();
        static List<VertexPickerEntry> s_NearestVertices = new List<VertexPickerEntry>();
        static List<GameObject> s_OverlappingGameObjects = new List<GameObject>();
        static readonly List<int> s_IndexBuffer = new List<int>(16);
        static List<Edge> s_EdgeBuffer = new List<Edge>(32);

        // When enabled, a mouse click on an unselected mesh will select both the GameObject and the mesh element picked.
        const bool k_AllowUnselected = true;

        public static ProBuilderMesh DoMouseClick(Event evt, SelectMode selectionMode, ScenePickerPreferences pickerPreferences)
        {
            bool appendModifier = EditorHandleUtility.IsAppendModifier(evt.modifiers);
            bool addToSelectionModifier = EditorHandleUtility.IsSelectionAddModifier(evt.modifiers);
            bool addOrRemoveIfPresentFromSelectionModifier = EditorHandleUtility.IsSelectionAppendOrRemoveIfPresentModifier(evt.modifiers);

            float pickedElementDistance;

            if (selectionMode.ContainsFlag(SelectMode.Edge | SelectMode.TextureEdge))
                pickedElementDistance = EdgeRaycast(evt.mousePosition, pickerPreferences, k_AllowUnselected, s_Selection);
            else if (selectionMode.ContainsFlag(SelectMode.Vertex | SelectMode.TextureVertex))
                pickedElementDistance = VertexRaycast(evt.mousePosition, pickerPreferences, k_AllowUnselected, s_Selection);
            else
                pickedElementDistance = FaceRaycast(evt.mousePosition, pickerPreferences, k_AllowUnselected, s_Selection, evt.clickCount > 1 ? -1 : 0, false);

            evt.Use();

            if (!appendModifier)
            {
                if(s_Selection.mesh != null)
                    s_Selection.mesh.ClearSelection();
                MeshSelection.SetSelection((GameObject)null);
            }

            if (pickedElementDistance > ScenePickerPreferences.maxPointerDistance)
            {
                if (appendModifier && Selection.gameObjects.Contains(s_Selection.gameObject))
                    MeshSelection.RemoveFromSelection(s_Selection.gameObject);
                else
                    MeshSelection.AddToSelection(s_Selection.gameObject);

                return null;
            }

            GameObject candidateNewActiveObject = s_Selection.gameObject;
            bool activeObjectSelectionChanged = Selection.gameObjects.Contains(s_Selection.gameObject) && s_Selection.gameObject != Selection.activeGameObject;

            if (s_Selection.mesh != null)
            {
                var mesh = s_Selection.mesh;

                if (s_Selection.face != null)
                {
                    // Check for other editor mouse shortcuts first (todo proper event handling for mouse shortcuts)
                    MaterialEditor matEditor = MaterialEditor.instance;

                    if (matEditor != null && matEditor.ClickShortcutCheck(Event.current.modifiers, mesh, s_Selection.face))
                        return null;

                    UVEditor uvEditor = UVEditor.instance;

                    if (uvEditor != null && uvEditor.ClickShortcutCheck(mesh, s_Selection.face))
                        return null;

                    var faces = mesh.faces as Face[] ?? mesh.faces.ToArray();
                    var ind = Array.IndexOf<Face>(faces, s_Selection.face);
                    var sel = mesh.selectedFaceIndexes.IndexOf(ind);

                    UndoUtility.RecordSelection(mesh, "Select Face");

                    if (sel > -1)
                    {
                        if (!appendModifier || addOrRemoveIfPresentFromSelectionModifier ||
                            (addToSelectionModifier && s_Selection.face == mesh.GetActiveFace() && !activeObjectSelectionChanged))
                        {
                            mesh.RemoveFromFaceSelectionAtIndex(sel);

                            if (addOrRemoveIfPresentFromSelectionModifier && activeObjectSelectionChanged)
                            {
                                candidateNewActiveObject = Selection.activeGameObject;
                            }
                            else if (mesh.selectedFaceCount == 0)
                            {
                                for (var i = MeshSelection.topInternal.Count - 1; i >= 0; i--)
                                {
                                    if (MeshSelection.topInternal[i].selectedFaceCount > 0)
                                    {
                                        candidateNewActiveObject = MeshSelection.topInternal[i].gameObject;
                                        activeObjectSelectionChanged = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            mesh.selectedFaceIndicesInternal = mesh.selectedFaceIndicesInternal.Remove(ind);
                            mesh.SetSelectedFaces(mesh.selectedFaceIndicesInternal.Add(ind));
                        }
                    }
                    else
                        mesh.AddToFaceSelection(ind);
                }
                else if (s_Selection.edge != Edge.Empty)
                {
                    int ind = mesh.IndexOf(mesh.selectedEdges, s_Selection.edge);

                    UndoUtility.RecordSelection(mesh, "Select Edge");

                    if (ind > -1)
                    {
                        if (!appendModifier || addOrRemoveIfPresentFromSelectionModifier ||
                            (addToSelectionModifier && s_Selection.edge == mesh.GetActiveEdge() && !activeObjectSelectionChanged))
                        {
                            mesh.SetSelectedEdges(mesh.selectedEdges.ToArray().RemoveAt(ind));

                            if (addOrRemoveIfPresentFromSelectionModifier && activeObjectSelectionChanged)
                            {
                                candidateNewActiveObject = Selection.activeGameObject;
                            }
                            else if (mesh.selectedEdgeCount == 0)
                            {
                                for (var i = MeshSelection.topInternal.Count - 1; i >= 0; i--)
                                {
                                    if (MeshSelection.topInternal[i].selectedEdgeCount > 0)
                                    {
                                        candidateNewActiveObject = MeshSelection.topInternal[i].gameObject;
                                        activeObjectSelectionChanged = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            mesh.selectedEdgesInternal = mesh.selectedEdgesInternal.Remove(s_Selection.edge);
                            mesh.SetSelectedEdges(mesh.selectedEdgesInternal.Add(s_Selection.edge));
                        }
                    }
                    else
                        mesh.SetSelectedEdges(mesh.selectedEdges.ToArray().Add(s_Selection.edge));
                }
                else if (s_Selection.vertex > -1)
                {
                    int ind = Array.IndexOf(mesh.selectedIndexesInternal, s_Selection.vertex);

                    UndoUtility.RecordSelection(mesh, "Select Vertex");

                    if (ind > -1)
                    {
                        var sharedIndex = mesh.sharedVertexLookup[s_Selection.vertex];
                        var sharedVertex = mesh.sharedVerticesInternal[sharedIndex];
                        s_IndexBuffer.Clear();
                        foreach (var vertex in sharedVertex)
                        {
                            var index = Array.IndexOf(mesh.selectedIndexesInternal, vertex);
                            if (index < 0)
                                continue;

                            s_IndexBuffer.Add(index);
                        }
                        s_IndexBuffer.Sort();

                        if (!appendModifier || addOrRemoveIfPresentFromSelectionModifier ||
                           (addToSelectionModifier && s_Selection.vertex == mesh.GetActiveVertex() && !activeObjectSelectionChanged))
                        {
                            mesh.selectedIndexesInternal = mesh.selectedIndexesInternal.SortedRemoveAt(s_IndexBuffer);
                            mesh.SetSelectedVertices(mesh.selectedIndexesInternal);

                            if (addOrRemoveIfPresentFromSelectionModifier && activeObjectSelectionChanged)
                            {
                                candidateNewActiveObject = Selection.activeGameObject;
                            }
                            else if (mesh.selectedIndexesInternal.Length == 0)
                            {
                                for (var i = MeshSelection.topInternal.Count - 1; i >= 0; i--)
                                {
                                    if (MeshSelection.topInternal[i].selectedIndexesInternal.Length > 0)
                                    {
                                        candidateNewActiveObject = MeshSelection.topInternal[i].gameObject;
                                        activeObjectSelectionChanged = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            mesh.selectedIndexesInternal = mesh.selectedIndexesInternal.SortedRemoveAt(s_IndexBuffer);
                            mesh.SetSelectedVertices(mesh.selectedIndexesInternal.Add(s_Selection.vertex));
                        }

                    }
                    else
                        mesh.SetSelectedVertices(mesh.selectedIndexesInternal.Add(s_Selection.vertex));
                }

                if(activeObjectSelectionChanged)
                    MeshSelection.MakeActiveObject(candidateNewActiveObject);
                else
                    MeshSelection.AddToSelection(candidateNewActiveObject);

                return mesh;
            }

            return null;
        }

        public static void DoMouseDrag(Rect mouseDragRect, SelectMode selectionMode, ScenePickerPreferences scenePickerPreferences)
        {
            var pickingOptions = new PickerOptions()
            {
                depthTest = scenePickerPreferences.cullMode == CullingMode.Back,
                rectSelectMode = scenePickerPreferences.rectSelectMode
            };

            UndoUtility.RecordSelection("Drag Select");
            bool isAppendModifier = EditorHandleUtility.IsAppendModifier(Event.current.modifiers);

            if (!isAppendModifier)
                MeshSelection.ClearElementSelection();

            bool elementsInDragRect = false;

            switch (selectionMode)
            {
                case SelectMode.Vertex:
                case SelectMode.TextureVertex:
                {
                    Dictionary<ProBuilderMesh, HashSet<int>> selected = SelectionPicker.PickVerticesInRect(
                            SceneView.lastActiveSceneView.camera,
                            mouseDragRect,
                            MeshSelection.topInternal,
                            pickingOptions,
                            EditorGUIUtility.pixelsPerPoint);

                    foreach (var kvp in selected)
                    {
                        var mesh = kvp.Key;
                        SharedVertex[] sharedIndexes = mesh.sharedVerticesInternal;
                        HashSet<int> common;

                        if (isAppendModifier)
                        {
                            common = mesh.GetSharedVertexHandles(mesh.selectedIndexesInternal);

                            if (scenePickerPreferences.selectionModifierBehavior  == SelectionModifierBehavior.Add)
                                common.UnionWith(kvp.Value);
                            else if (scenePickerPreferences.selectionModifierBehavior  == SelectionModifierBehavior.Subtract)
                                common.RemoveWhere(x => kvp.Value.Contains(x));
                            else if (scenePickerPreferences.selectionModifierBehavior  == SelectionModifierBehavior.Difference)
                                common.SymmetricExceptWith(kvp.Value);
                        }
                        else
                        {
                            common = kvp.Value;
                        }

                        elementsInDragRect |= kvp.Value.Any();
                        mesh.SetSelectedVertices(common.SelectMany(x => sharedIndexes[x]));
                    }

                    break;
                }

                case SelectMode.Face:
                case SelectMode.TextureFace:
                {
                    Dictionary<ProBuilderMesh, HashSet<Face>> selected = SelectionPicker.PickFacesInRect(
                            SceneView.lastActiveSceneView.camera,
                            mouseDragRect,
                            MeshSelection.topInternal,
                            pickingOptions,
                            EditorGUIUtility.pixelsPerPoint);

                    foreach (var kvp in selected)
                    {
                        HashSet<Face> current;

                        if (isAppendModifier)
                        {
                            current = new HashSet<Face>(kvp.Key.selectedFacesInternal);

                            if (scenePickerPreferences.selectionModifierBehavior == SelectionModifierBehavior.Add)
                                current.UnionWith(kvp.Value);
                            else if (scenePickerPreferences.selectionModifierBehavior == SelectionModifierBehavior.Subtract)
                                current.RemoveWhere(x => kvp.Value.Contains(x));
                            else if (scenePickerPreferences.selectionModifierBehavior == SelectionModifierBehavior.Difference)
                                current.SymmetricExceptWith(kvp.Value);
                        }
                        else
                        {
                            current = kvp.Value;
                        }

                        elementsInDragRect |= kvp.Value.Any();
                        kvp.Key.SetSelectedFaces(current);
                    }

                    break;
                }

                case SelectMode.Edge:
                case SelectMode.TextureEdge:
                {
                    var selected = SelectionPicker.PickEdgesInRect(
                            SceneView.lastActiveSceneView.camera,
                            mouseDragRect,
                            MeshSelection.topInternal,
                            pickingOptions,
                            EditorGUIUtility.pixelsPerPoint);

                    foreach (var kvp in selected)
                    {
                        ProBuilderMesh mesh = kvp.Key;
                        Dictionary<int, int> common = mesh.sharedVertexLookup;
                        HashSet<EdgeLookup> selectedEdges = EdgeLookup.GetEdgeLookupHashSet(kvp.Value, common);
                        HashSet<EdgeLookup> current;

                        if (isAppendModifier)
                        {
                            current = EdgeLookup.GetEdgeLookupHashSet(mesh.selectedEdges, common);

                            if (scenePickerPreferences.selectionModifierBehavior == SelectionModifierBehavior.Add)
                                current.UnionWith(selectedEdges);
                            else if (scenePickerPreferences.selectionModifierBehavior == SelectionModifierBehavior.Subtract)
                                current.RemoveWhere(x => selectedEdges.Contains(x));
                            else if (scenePickerPreferences.selectionModifierBehavior == SelectionModifierBehavior.Difference)
                                current.SymmetricExceptWith(selectedEdges);
                        }
                        else
                        {
                            current = selectedEdges;
                        }

                        elementsInDragRect |= kvp.Value.Any();
                        mesh.SetSelectedEdges(current.Select(x => x.local));
                    }

                    break;
                }
            }

            // if nothing was selected in the drag rect, clear the object selection too
            if (!elementsInDragRect && !isAppendModifier)
                MeshSelection.ClearElementAndObjectSelection();

            ProBuilderEditor.Refresh();
            SceneView.RepaintAll();
        }

        // Get the object & mesh selection that the mouse is currently nearest.
        // A ProBuilderMesh is returned because double click actions need to know what the last selected pb_Object was.
        // If deepClickOffset is specified, the object + deepClickOffset in the deep select stack will be returned (instead of next).
        internal static float MouseRayHitTest(
            Vector3 mousePosition,
            SelectMode selectionMode,
            ScenePickerPreferences pickerOptions,
            SceneSelection selection,
            bool allowUnselected = false)
        {
            if (selectionMode.ContainsFlag(SelectMode.Edge | SelectMode.TextureEdge))
                return EdgeRaycast(mousePosition, pickerOptions, allowUnselected, selection);

            if (selectionMode.ContainsFlag(SelectMode.Vertex | SelectMode.TextureVertex))
                return VertexRaycast(mousePosition, pickerOptions, allowUnselected, selection);

            return FaceRaycast(mousePosition, pickerOptions, allowUnselected, selection, 0, true);
        }

        static float FaceRaycast(Vector3 mousePosition,
            ScenePickerPreferences pickerOptions,
            bool allowUnselected,
            SceneSelection selection,
            int deepClickOffset = 0,
            bool isPreview = true)
        {
            GameObject pickedGo = null;
            ProBuilderMesh pickedPb = null;
            Face pickedFace = null;

            int newHash = 0;

            // If any event modifiers are engaged don't cycle the deep click
            EventModifiers em = Event.current.modifiers;

            // Reset cycle if we used an event modifier previously.
            // Move state back to single selection.
            if ((em != EventModifiers.None) != s_AppendModifierPreviousState)
            {
                s_AppendModifierPreviousState = (em != EventModifiers.None);
                s_DeepSelectionPrevious = newHash;
            }

            if (isPreview || em != EventModifiers.None)
                EditorHandleUtility.GetHovered(mousePosition, s_OverlappingGameObjects);
            else
                EditorHandleUtility.GetAllOverlapping(mousePosition, s_OverlappingGameObjects);

            selection.Clear();

            float distance = Mathf.Infinity;

            for (int i = 0, next = 0, pickedCount = s_OverlappingGameObjects.Count; i < pickedCount; i++)
            {
                var go = s_OverlappingGameObjects[i];
                var mesh = go.GetComponent<ProBuilderMesh>();
                Face face = null;

                if (mesh != null && (allowUnselected || MeshSelection.topInternal.Contains(mesh)))
                {
                    Ray ray = UHandleUtility.GUIPointToWorldRay(mousePosition);
                    RaycastHit hit;

                    if (UnityEngine.ProBuilder.HandleUtility.FaceRaycast(ray,
                            mesh,
                            out hit,
                            Mathf.Infinity,
                            pickerOptions.cullMode))
                    {
                        face = mesh.facesInternal[hit.face];
                        distance = Vector2.SqrMagnitude(((Vector2)mousePosition) - HandleUtility.WorldToGUIPoint(mesh.transform.TransformPoint(hit.point)));
                    }
                }

                // pb_Face doesn't define GetHashCode, meaning it falls to object.GetHashCode (reference comparison)
                int hash = face == null ? go.GetHashCode() : face.GetHashCode();

                if (s_DeepSelectionPrevious == hash)
                    next = (i + (1 + deepClickOffset)) % pickedCount;

                if (next == i)
                {
                    pickedGo = go;
                    pickedPb = mesh;
                    pickedFace = face;

                    newHash = hash;

                    // a prior hash was matched, this is the next. if
                    // it's just the first iteration don't break (but do
                    // set the default).
                    if (next != 0)
                        break;
                }
            }

            if (!isPreview)
                s_DeepSelectionPrevious = newHash;

            if (pickedGo != null)
            {
                Event.current.Use();

                if (pickedPb != null)
                {
                    if (pickedPb.selectable)
                    {
                        selection.gameObject = pickedGo;
                        selection.mesh = pickedPb;
                        selection.face = pickedFace;

                        return Mathf.Sqrt(distance);
                    }
                }

                // If clicked off a pb_Object but onto another gameobject, set the selection
                // and dip out.
                selection.gameObject = pickedGo;
                return Mathf.Sqrt(distance);
            }

            return distance;
        }

        static float VertexRaycast(Vector3 mousePosition, ScenePickerPreferences pickerOptions, bool allowUnselected, SceneSelection selection)
        {
            Camera cam = SceneView.lastActiveSceneView.camera;
            selection.Clear();
            s_NearestVertices.Clear();
            selection.gameObject = HandleUtility.PickGameObject(mousePosition, false);
            float maxDistance = ScenePickerPreferences.maxPointerDistance * ScenePickerPreferences.maxPointerDistance;
            ProBuilderMesh hoveredMesh = selection.gameObject != null ? selection.gameObject.GetComponent<ProBuilderMesh>() : null;

            if (allowUnselected && selection.gameObject != null)
            {
                if (hoveredMesh != null && hoveredMesh.selectable && !MeshSelection.Contains(hoveredMesh))
                {
                    GetNearestVertices(hoveredMesh, mousePosition, s_NearestVertices, maxDistance, 1);
                }
            }

            if (selection.mesh == null)
            {
                foreach (var mesh in MeshSelection.topInternal)
                {
                    if (!mesh.selectable)
                        continue;

                    GetNearestVertices(mesh, mousePosition, s_NearestVertices, maxDistance, hoveredMesh == mesh || hoveredMesh == null ? 1.0f : ScenePickerPreferences.offPointerMultiplier);
                }
            }

            s_NearestVertices.Sort((x, y) => x.screenDistance.CompareTo(y.screenDistance));

            for (int i = 0; i < s_NearestVertices.Count; i++)
            {
                if (!UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(cam, s_NearestVertices[i].mesh, s_NearestVertices[i].worldPosition))
                {
                    selection.gameObject = s_NearestVertices[i].mesh.gameObject;
                    selection.mesh = s_NearestVertices[i].mesh;
                    selection.vertex = s_NearestVertices[i].vertex;

                    return Mathf.Sqrt(s_NearestVertices[i].screenDistance);
                }
            }

            return Mathf.Infinity;
        }

        static int GetNearestVertices(ProBuilderMesh mesh, Vector3 mousePosition, List<VertexPickerEntry> list, float maxDistance, float distModifier)
        {
            var positions = mesh.positionsInternal;
            var common = mesh.sharedVerticesInternal;
            var matches = 0;

            for (int n = 0, c = common.Length; n < c; n++)
            {
                int index = common[n][0];
                Vector3 v = mesh.transform.TransformPoint(positions[index]);
                Vector3 p = UHandleUtility.WorldToGUIPoint(v);

                float dist = (p - mousePosition).sqrMagnitude * distModifier;

                if (dist < maxDistance)
                {
                    list.Add(new VertexPickerEntry()
                    {
                        mesh = mesh,
                        screenDistance = dist,
                        worldPosition = v,
                        vertex = index
                    });

                    matches++;
                }
            }

            return matches;
        }

        /// <summary>
        /// Get the nearest <see cref="Edge"/> to a screen position.
        /// </summary>
        /// <returns>
        /// Distance is returned as the screen distance to mesh, not edge.
        /// </returns>
        static float EdgeRaycast(Vector3 mousePosition, ScenePickerPreferences pickerPrefs, bool allowUnselected, SceneSelection selection)
        {
            selection.Clear();
            selection.gameObject = UHandleUtility.PickGameObject(mousePosition, false);
            var hoveredMesh = selection.gameObject != null ? selection.gameObject.GetComponent<ProBuilderMesh>() : null;

            float bestDistance = Mathf.Infinity;
            bool hoveredIsInSelection = MeshSelection.topInternal.Contains(hoveredMesh);

            if (hoveredMesh != null && (allowUnselected || hoveredIsInSelection))
            {
                var tup = GetNearestEdgeOnMesh(hoveredMesh, mousePosition);

                if (tup.edge.IsValid())
                {
                    selection.gameObject = hoveredMesh.gameObject;
                    selection.mesh = hoveredMesh;
                    selection.edge = tup.edge;
                    bestDistance = tup.distance;

                    // If the nearest edge was acquired by a raycast, then the distance to mesh is 0f.
                    if (hoveredIsInSelection)
                        return tup.distance;
                }
            }

            foreach (var mesh in MeshSelection.topInternal)
            {
                var trs = mesh.transform;
                var positions = mesh.positionsInternal;
                s_EdgeBuffer.Clear();

                // When the pointer is over another object, apply a modifier to the distance to prefer picking the
                // object hovered over the currently selected
                var distMultiplier = (hoveredMesh == mesh || hoveredMesh == null)
                    ? 1.0f
                    : ScenePickerPreferences.offPointerMultiplier;

                foreach (var face in mesh.facesInternal)
                {
                    foreach (var edge in face.edges)
                    {
                        int x = edge.a;
                        int y = edge.b;

                        float d = UHandleUtility.DistanceToLine(
                                trs.TransformPoint(positions[x]),
                                trs.TransformPoint(positions[y]));

                        d *= distMultiplier;

                        // best distance isn't set to maxPointerDistance because we want to preserve an unselected
                        // gameobject over a selected gameobject with an out of bounds edge.
                        if (d > ScenePickerPreferences.maxPointerDistance)
                            continue;

                        // account for stacked edges
                        if (Mathf.Approximately(d, bestDistance))
                        {
                            s_EdgeBuffer.Add(new Edge(x, y));
                        }
                        else if (d < bestDistance)
                        {
                            s_EdgeBuffer.Clear();
                            s_EdgeBuffer.Add(new Edge(x, y));

                            selection.gameObject = mesh.gameObject;
                            selection.mesh = mesh;
                            selection.edge = new Edge(x, y);
                            bestDistance = d;
                        }
                    }
                }

                // If more than 1 edge is closest, the closest is one of the vertex.
                // Get closest edge to the camera.
                if (s_EdgeBuffer.Count > 1)
                    selection.edge = GetClosestEdgeToCamera(positions, s_EdgeBuffer);
            }

            return selection.gameObject != null ? bestDistance : Mathf.Infinity;
        }

        static Edge GetClosestEdgeToCamera(Vector3[] positions, IEnumerable<Edge> edges)
        {
            var camPos = SceneView.lastActiveSceneView.camera.transform.position;
            var closestDistToScreen = Mathf.Infinity;
            Edge closest = default(Edge);

            foreach (var edge in edges)
            {
                var a = positions[edge.a];
                var b = positions[edge.b];
                var dir = (b - a).normalized * 0.01f;

                //Use a point that is close to the vertex on the edge but not on it,
                //otherwise we will have the same issue with every edge having the same distance to screen
                float dToScreen = Mathf.Min(
                    Vector3.Distance(camPos, a + dir),
                    Vector3.Distance(camPos, b - dir));

                if (dToScreen < closestDistToScreen)
                {
                    closestDistToScreen = dToScreen;
                    closest = edge;
                }
            }

            return closest;
        }

        struct EdgeAndDistance
        {
            public Edge edge;
            public float distance;
        }

        /// <summary>
        /// Get the nearest edge to a screen position.
        /// </summary>
        static EdgeAndDistance GetNearestEdgeOnMesh(ProBuilderMesh mesh, Vector3 mousePosition)
        {
            Ray ray = UHandleUtility.GUIPointToWorldRay(mousePosition);

            var res = new EdgeAndDistance()
            {
                edge = Edge.Empty,
                distance = Mathf.Infinity
            };

            SimpleTuple<Face, Vector3> s_DualCullModeRaycastBackFace = new SimpleTuple<Face, Vector3>();
            SimpleTuple<Face, Vector3> s_DualCullModeRaycastFrontFace = new SimpleTuple<Face, Vector3>();

            // get the nearest hit face and point for both cull mode front and back, then prefer the result that is nearest the camera.
            if (PHandleUtility.FaceRaycastBothCullModes(ray, mesh, ref s_DualCullModeRaycastBackFace, ref s_DualCullModeRaycastFrontFace))
            {
                Vector3[] v = mesh.positionsInternal;

                if (s_DualCullModeRaycastBackFace.item1 != null)
                {
                    foreach (var edge in s_DualCullModeRaycastBackFace.item1.edgesInternal)
                    {
                        float d = UHandleUtility.DistancePointLine(s_DualCullModeRaycastBackFace.item2, v[edge.a], v[edge.b]);

                        if (d < res.distance)
                        {
                            res.edge = edge;
                            res.distance = d;
                        }
                    }
                }

                if (s_DualCullModeRaycastFrontFace.item1 != null)
                {
                    var a = mesh.transform.TransformPoint(s_DualCullModeRaycastBackFace.item2);
                    var b = mesh.transform.TransformPoint(s_DualCullModeRaycastFrontFace.item2);
                    var c = SceneView.lastActiveSceneView.camera.transform.position;

                    if (Vector3.Distance(c, b) < Vector3.Distance(c, a))
                    {
                        foreach (var edge in s_DualCullModeRaycastFrontFace.item1.edgesInternal)
                        {
                            float d = UHandleUtility.DistancePointLine(s_DualCullModeRaycastFrontFace.item2, v[edge.a], v[edge.b]);

                            if (d < res.distance)
                            {
                                res.edge = edge;
                                res.distance = d;
                            }
                        }
                    }
                }

                if (res.edge.IsValid())
                    res.distance = UHandleUtility.DistanceToLine(
                            mesh.transform.TransformPoint(v[res.edge.a]),
                            mesh.transform.TransformPoint(v[res.edge.b]));
            }

            return res;
        }
    }
}
