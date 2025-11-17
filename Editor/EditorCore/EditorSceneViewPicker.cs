using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using PHandleUtility = UnityEngine.ProBuilder.HandleUtility;
using UHandleUtility = UnityEditor.HandleUtility;
using RaycastHit = UnityEngine.ProBuilder.RaycastHit;

namespace UnityEditor.ProBuilder
{
    struct ScenePickerPreferences
    {
        public const float maxPointerDistance = 20f;
        public const float offPointerMultiplier = 1.2f;

        public CullingMode cullMode;
        public RectSelectMode rectSelectMode;
    }

    static class EditorSceneViewPicker
    {
        static int s_DeepSelectionPrevious = 0x0;
        static bool s_AppendModifierPreviousState = false;
        static SceneSelection s_Selection = new SceneSelection();
        internal static SceneSelection selection => s_Selection;
        static List<VertexPickerEntry> s_NearestVertices = new List<VertexPickerEntry>();
        static List<GameObject> s_OverlappingGameObjects = new List<GameObject>();
        static readonly List<int> s_IndexBuffer = new List<int>(16);
        static List<Edge> s_EdgeBuffer = new List<Edge>(32);

        // When enabled, a mouse click on an unselected mesh will select both the GameObject and the mesh element picked.
        const bool k_AllowUnselected = true;

        public static void DoMouseHover(SceneSelection selection)
        {
            if (selection.faces.Count == 0)
                return;
            var mesh = selection.mesh;
            var face = selection.faces[0];
            var activeFace = mesh.GetActiveFace();
            if (activeFace == null || activeFace == face)
                return;

            var faces = mesh.facesInternal;
            var pathFaces = SelectPathFaces.GetPath(mesh, Array.IndexOf<Face>(faces, activeFace), Array.IndexOf<Face>(faces, face));

            if (pathFaces != null)
            {
                foreach (var path in pathFaces)
                    selection.faces.Add(faces[path]);
            }
        }

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

            var isProBuilderMesh = s_Selection.gameObject != null && s_Selection.gameObject.GetComponent<ProBuilderMesh>() != null;

            if (!appendModifier)
            {
                if(s_Selection.mesh != null)
                    s_Selection.mesh.ClearSelection();
                if (s_Selection.gameObject == null || !isProBuilderMesh)
                {
                    // Don't clear object selection if we are in the PB Context, just clear sub-elements selection
                    MeshSelection.ClearElementSelection();
                }
                else
                    MeshSelection.SetSelection((GameObject)null);
            }

            if (!isProBuilderMesh)
                return null;

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

                foreach (var face in s_Selection.faces)
                {
                    var faces = mesh.faces as Face[] ?? mesh.faces.ToArray();
                    var ind = Array.IndexOf<Face>(faces, face);
                    var sel = mesh.selectedFaceIndexes.IndexOf(ind);

                    UndoUtility.RecordSelection(mesh, "Select Face");
                    if (sel > -1)
                    {
                        if (!appendModifier || addOrRemoveIfPresentFromSelectionModifier ||
                            (addToSelectionModifier && face == mesh.GetActiveFace() && !activeObjectSelectionChanged))
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

                foreach(var edge in s_Selection.edges)
                {
                    int ind = mesh.IndexOf(mesh.selectedEdges, edge);

                    UndoUtility.RecordSelection(mesh, "Select Edge");

                    if (ind > -1)
                    {
                        if (!appendModifier || addOrRemoveIfPresentFromSelectionModifier ||
                            (addToSelectionModifier && edge == mesh.GetActiveEdge() && !activeObjectSelectionChanged))
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
                            mesh.selectedEdgesInternal = mesh.selectedEdgesInternal.Remove(edge);
                            mesh.SetSelectedEdges(mesh.selectedEdgesInternal.Add(edge));
                        }
                    }
                    else
                        mesh.SetSelectedEdges(mesh.selectedEdges.ToArray().Add(edge));
                }
                foreach(var vertex in s_Selection.vertexes)
                {
                    int ind = Array.IndexOf(mesh.selectedIndexesInternal, vertex);

                    UndoUtility.RecordSelection(mesh, "Select Vertex");

                    if (ind > -1)
                    {
                        var sharedIndex = mesh.sharedVertexLookup[vertex];
                        var sharedVertex = mesh.sharedVerticesInternal[sharedIndex];
                        s_IndexBuffer.Clear();
                        foreach (var sVertex in sharedVertex)
                        {
                            var index = Array.IndexOf(mesh.selectedIndexesInternal, sVertex);
                            if (index < 0)
                                continue;

                            s_IndexBuffer.Add(index);
                        }
                        s_IndexBuffer.Sort();

                        if (!appendModifier || addOrRemoveIfPresentFromSelectionModifier ||
                           (addToSelectionModifier && vertex == mesh.GetActiveVertex() && !activeObjectSelectionChanged))
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
                            mesh.SetSelectedVertices(mesh.selectedIndexesInternal.Add(vertex));
                        }

                    }
                    else
                        mesh.SetSelectedVertices(mesh.selectedIndexesInternal.Add(vertex));
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
            bool isSelectionAddModifier = EditorHandleUtility.IsSelectionAddModifier(Event.current.modifiers);
            bool isSelectionRemoveModifier = EditorHandleUtility.IsSelectionAppendOrRemoveIfPresentModifier(Event.current.modifiers);
            bool isAppendModifier = isSelectionAddModifier || isSelectionRemoveModifier;

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

                            if(isSelectionAddModifier)
                                common.UnionWith(kvp.Value);
                            else if(isSelectionRemoveModifier)
                                common.RemoveWhere(x => kvp.Value.Contains(x));
                        }
                        else
                        {
                            common = kvp.Value;
                        }

                        elementsInDragRect |= kvp.Value.Count > 0;
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

                            if(isSelectionAddModifier)
                                current.UnionWith(kvp.Value);
                            else if(isSelectionRemoveModifier)
                                current.RemoveWhere(x => kvp.Value.Contains(x));
                        }
                        else
                        {
                            current = kvp.Value;
                        }

                        elementsInDragRect |= kvp.Value.Count > 0;
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

                            if(isSelectionAddModifier)
                                current.UnionWith(selectedEdges);
                            else if(isSelectionRemoveModifier)
                                current.RemoveWhere(x => selectedEdges.Contains(x));
                        }
                        else
                        {
                            current = selectedEdges;
                        }

                        elementsInDragRect |= kvp.Value.Count > 0;
                        mesh.SetSelectedEdges(current.Select(x => x.local));
                    }

                    break;
                }
            }

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

        static List<(ProBuilderMesh mesh, Face face, float dist, int hash)> s_PbHits = new List<(ProBuilderMesh, Face, float, int)>();

        static float FaceRaycast(Vector3 mousePosition,
            ScenePickerPreferences pickerOptions,
            bool allowUnselected,
            SceneSelection selection,
            int deepClickOffset = 0,
            bool isPreview = true)
        {
            GameObject candidateGo = null;
            ProBuilderMesh candidatePb = null;
            Face        candidateFace = null;
            float       candidateDistance = Mathf.Infinity;
            float       resultDistance = Mathf.Infinity;

            // If any event modifiers are engaged don't cycle the deep click
            EventModifiers em = EventModifiers.None;
            if (Event.current != null)
                em = Event.current.modifiers;

            // Reset cycle if we used an event modifier previously.
            // Move state back to single selection.
            if ((em != EventModifiers.None) != s_AppendModifierPreviousState)
            {
                s_AppendModifierPreviousState = (em != EventModifiers.None);
                s_DeepSelectionPrevious = 0;
            }

            if (isPreview || em != EventModifiers.None)
                EditorHandleUtility.GetHovered(mousePosition, s_OverlappingGameObjects);
            else
                EditorHandleUtility.GetAllOverlapping(mousePosition, s_OverlappingGameObjects);

            selection.Clear();

            // First, find all ProBuilder meshes and their hit faces under the mouse
            for (int i = 0, pickedCount = s_OverlappingGameObjects.Count; i < pickedCount; i++)
            {
                var go = s_OverlappingGameObjects[i];
                var mesh = go.GetComponent<ProBuilderMesh>();

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
                        Face face = mesh.facesInternal[hit.face];
                        float dist = Vector2.SqrMagnitude(((Vector2)mousePosition) - HandleUtility.WorldToGUIPoint(mesh.transform.TransformPoint(hit.point)));
                        s_PbHits.Add((mesh, face, dist, face.GetHashCode()));
                    }
                }
            }

            if (s_PbHits.Count > 0)
            {
                int chosenIndex = 0;

                // Apply deep click cycling logic only if it's an actual click and a previous selection exists
                if (!isPreview && s_DeepSelectionPrevious != 0)
                {
                    int currentSelectionIndex = -1;
                    // Find the index of the previously selected item (if it's still in the list)
                    for (int i = 0; i < s_PbHits.Count; i++)
                    {
                        if (s_PbHits[i].hash == s_DeepSelectionPrevious)
                        {
                            currentSelectionIndex = i;
                            break;
                        }
                    }

                    if (currentSelectionIndex != -1)
                    {
                        // Calculate the next index for cycling
                        chosenIndex = (currentSelectionIndex + (1 + deepClickOffset)) % s_PbHits.Count;
                        // Handle negative result from modulo for deepClickOffset = -1 if currentSelectionIndex is 0
                        if (chosenIndex < 0) chosenIndex += s_PbHits.Count;
                    }
                    else
                    {
                        // If the mouse moved enough that none of the hit faces correspond to the previous selection, we reset
                        s_DeepSelectionPrevious = 0;
                    }
                    // If s_DeepSelectionPrevious was set but no matching PB hit is found in current list,
                    // fall back to the closest one (chosenIndex remains 0)
                }
                // else for first click or if s_DeepSelectionPrevious is 0, chosenIndex remains 0 (selects closest)

                var selectedHit = s_PbHits[chosenIndex];
                candidateGo = selectedHit.mesh.gameObject;
                candidatePb = selectedHit.mesh;
                candidateFace = selectedHit.face;
                candidateDistance = selectedHit.dist;

                // Update s_DeepSelectionPrevious only for actual clicks after cycling/filtering (not hovers)
                if (!isPreview)
                {
                    s_DeepSelectionPrevious = selectedHit.hash;
                }
            }

            // Final selection update
            if (candidateGo != null)
            {
                selection.gameObject = candidateGo;
                resultDistance = Mathf.Sqrt(candidateDistance);

                if (candidatePb != null)
                {
                    if (candidatePb.selectable)
                    {
                        selection.mesh = candidatePb;
                        selection.SetSingleFace(candidateFace);
                    }
                }
            }
            else if (candidateGo == null && !isPreview)
            {
                // if we click somewhere with no PB, we reset the deep cycle
                s_DeepSelectionPrevious = 0;
            }

            s_PbHits.Clear();
            return resultDistance;
        }

        static float VertexRaycast(Vector3 mousePosition, ScenePickerPreferences pickerOptions, bool allowUnselected, SceneSelection selection)
        {
            Camera cam = SceneView.lastActiveSceneView.camera;
            selection.Clear();
            s_NearestVertices.Clear();
            bool highlightedVertexExists = ProBuilderEditor.instance.hovering.vertexes.Count > 0;

            // if there is a vertex already highlighted, we don't want to select a different game object
            if (!highlightedVertexExists)
            {
                selection.gameObject = HandleUtility.PickGameObject(mousePosition, false);
            }
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
                    selection.SetSingleVertex(s_NearestVertices[i].vertex);

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
            bool highlightedEdgeExists = ProBuilderEditor.instance.hovering.edges.Count > 0;

            // if there is an edge already highlighted, we don't want to select a different game object
            if (highlightedEdgeExists)
            {
                ProBuilderMesh hoveredMeshFromHighlight = ProBuilderEditor.instance.hovering.mesh;
                if (hoveredMeshFromHighlight != null && ProBuilderEditor.instance.hovering.edges.Count > 0)
                {
                    Edge highlightedEdge = ProBuilderEditor.instance.hovering.edges[0]; // Assuming single edge hover

                    // Calculate the screen distance for this specific highlighted edge.
                    Vector3[] positions = hoveredMeshFromHighlight.positionsInternal;
                    Vector3 worldPosA = hoveredMeshFromHighlight.transform.TransformPoint(positions[highlightedEdge.a]);
                    Vector3 worldPosB = hoveredMeshFromHighlight.transform.TransformPoint(positions[highlightedEdge.b]);

                    if (ProcessEdgePoints(Camera.current, worldPosA, worldPosB, out Vector3 guiPointA, out Vector3 guiPointB))
                    {
                        float distToHighlightedEdge = HandleUtility.DistancePointLine(mousePosition, guiPointA, guiPointB);

                        // If the highlighted edge is within the maximum picking distance, select it and return early.
                        if (distToHighlightedEdge <= ScenePickerPreferences.maxPointerDistance)
                        {
                            selection.gameObject = hoveredMeshFromHighlight.gameObject;
                            selection.mesh = hoveredMeshFromHighlight;
                            selection.SetSingleEdge(highlightedEdge);
                            return distToHighlightedEdge;
                        }
                    }
                }
            }

            float bestDistance = Mathf.Infinity;
            selection.gameObject = HandleUtility.PickGameObject(mousePosition, false);
            var hoveredMesh = selection.gameObject != null ? selection.gameObject.GetComponent<ProBuilderMesh>() : null;

            bool hoveredIsInSelection = MeshSelection.topInternal.Contains(hoveredMesh);

            if (hoveredMesh != null && (allowUnselected || hoveredIsInSelection))
            {
                var tup = GetNearestEdgeOnMesh(hoveredMesh, mousePosition);

                if (tup.edge.IsValid())
                {
                    selection.gameObject = hoveredMesh.gameObject;
                    selection.mesh = hoveredMesh;
                    selection.SetSingleEdge(tup.edge);
                    bestDistance = tup.distance;

                    // If the nearest edge was acquired by a raycast on an already selected mesh,
                    // return early to prioritize it.
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

                        Vector3 worldPosA = trs.TransformPoint(positions[x]);
                        Vector3 worldPosB = trs.TransformPoint(positions[y]);

                        if (ProcessEdgePoints(Camera.current, worldPosA, worldPosB, out Vector3 guiPointA, out Vector3 guiPointB))
                        {
                            // Calculate distance from the mouse position to the line
                            float d = HandleUtility.DistancePointLine(mousePosition, guiPointA, guiPointB);
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
                                selection.SetSingleEdge(new Edge(x, y));
                                bestDistance = d;
                            }
                        }
                    }
                }

                // If more than 1 edge is closest, the closest is one of the vertex.
                // Get closest edge to the camera.
                if (s_EdgeBuffer.Count > 1)
                    selection.SetSingleEdge(GetClosestEdgeToCamera(positions, s_EdgeBuffer));
            }

            return selection.gameObject != null ? bestDistance : Mathf.Infinity;
        }


        /// <summary>
        /// Function to clip a point to the near plane.
        /// </summary>
        /// <param name="pointBehind"> the point behind the plane</param>
        /// <param name="pointInFront"> the point in front of te plane</param>
        /// <param name="nearPlaneZ"> the camera near plane z</param>
        /// <returns>The intersection point between the line and the near plane</returns>
        static Vector3 ClipPointToNearPlane(Vector3 pointBehind, Vector3 pointInFront, float nearPlaneZ)
        {
            float t = (nearPlaneZ - pointBehind.z) / (pointInFront.z - pointBehind.z);
            return pointBehind + t * (pointInFront - pointBehind);
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
                {
                    Vector3 worldPosA = mesh.transform.TransformPoint(v[res.edge.a]);
                    Vector3 worldPosB = mesh.transform.TransformPoint(v[res.edge.b]);

                    if (ProcessEdgePoints(Camera.current, worldPosA, worldPosB, out Vector3 guiPointA, out Vector3 guiPointB))
                    {
                        // Calculate distance from the mouse position to the line
                        res.distance = HandleUtility.DistancePointLine(mousePosition, guiPointA, guiPointB);
                    }
                    else
                    {
                        // If both points are behind the camera, skip this edge
                        res.distance = Mathf.Infinity;
                    }
                }
            }

            return res;
        }

        static bool ProcessEdgePoints(Camera camera, Vector3 worldPosA, Vector3 worldPosB, out Vector3 guiPointA, out Vector3 guiPointB)
        {
            guiPointA = Vector3.zero;
            guiPointB = Vector3.zero;

            // Check if the points are in front of the camera
            bool pointAInFront = Vector3.Dot(camera.transform.forward, worldPosA - camera.transform.position) > 0;
            bool pointBInFront = Vector3.Dot(camera.transform.forward, worldPosB - camera.transform.position) > 0;

            // If both points are behind the camera, skip this edge
            if (!pointAInFront && !pointBInFront)
            {
                return false;
            }

            // Check if either point is behind the camera
            if (!pointAInFront || !pointBInFront)
            {
                // Clip points against the near plane
                Vector3 cameraSpacePointA = camera.transform.InverseTransformPoint(worldPosA);
                Vector3 cameraSpacePointB = camera.transform.InverseTransformPoint(worldPosB);

                float nearPlaneZ = camera.nearClipPlane;

                if (cameraSpacePointA.z < nearPlaneZ)
                {
                    cameraSpacePointA = ClipPointToNearPlane(cameraSpacePointA, cameraSpacePointB, nearPlaneZ);
                }

                if (cameraSpacePointB.z < nearPlaneZ)
                {
                    cameraSpacePointB = ClipPointToNearPlane(cameraSpacePointB, cameraSpacePointA, nearPlaneZ);
                }

                // Transform clipped points back to screen space
                guiPointA = HandleUtility.WorldToGUIPoint(camera.transform.TransformPoint(cameraSpacePointA));
                guiPointB = HandleUtility.WorldToGUIPoint(camera.transform.TransformPoint(cameraSpacePointB));
            }
            else // Both points are in front of the camera
            {
                guiPointA = HandleUtility.WorldToGUIPoint(worldPosA);
                guiPointB = HandleUtility.WorldToGUIPoint(worldPosB);
            }

            return true;
        }
    }
}
