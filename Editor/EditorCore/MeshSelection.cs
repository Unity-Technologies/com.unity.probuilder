using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

#if UNITY_2020_2_OR_NEWER
using EditorToolManager = UnityEditor.EditorTools.EditorToolManager;
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using EditorToolManager = UnityEditor.EditorTools.EditorToolContext;
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Provides helper functions for selecting Unity objects and ProBuilder mesh elements.
    /// </summary>
    [InitializeOnLoad]
    public static class MeshSelection
    {
        static List<ProBuilderMesh> s_TopSelection = new List<ProBuilderMesh>();
        static ProBuilderMesh s_ActiveMesh;
        static List<MeshAndElementSelection> s_ElementSelection = new List<MeshAndElementSelection>();

        static bool s_ElementCountsDirty = true;
        static bool s_SelectedElementGroupsDirty = true;
        static bool s_SelectedFacesInEditAreaDirty = true;
        static bool s_SelectionBoundsDirty = true;

        static Bounds s_SelectionBounds = new Bounds();
        static Dictionary<ProBuilderMesh, List<Face>> s_SelectedFacesInEditArea = new Dictionary<ProBuilderMesh, List<Face>>();

        /// <summary>
        /// Gets the axis-aligned bounding box encompassing the selected elements.
        /// </summary>
        public static Bounds bounds
        {
            get
            {
                if(s_SelectionBoundsDirty)
                    RecalculateSelectionBounds();
                return s_SelectionBounds;
            }
        }

        static int s_SelectedObjectCount;
        static int s_SelectedVertexCount;
        static int s_SelectedSharedVertexCount;
        static int s_SelectedFaceCount;
        static int s_SelectedEdgeCount;

        static int s_SelectedFaceCountObjectMax;
        static int s_SelectedEdgeCountObjectMax;
        static int s_SelectedVertexCountObjectMax;
        static int s_SelectedSharedVertexCountObjectMax;
        static int s_SelectedCoincidentVertexCountMax;

        static int s_TotalVertexCount;
        static int s_TotalFaceCount;
        static int s_TotalEdgeCount;
        static int s_TotalCommonVertexCount;
        static int s_TotalVertexCountCompiled;
        static int s_TotalTriangleCountCompiled;

        /// <summary>
        /// Gets how many ProBuilderMesh components are currently selected.
        /// </summary>
        public static int selectedObjectCount { get { CacheElementCounts(); return s_SelectedObjectCount; } }

        /// <summary>
        /// Gets the sum of all currently selected vertices on all currently selected ProBuilderMesh objects.
        /// </summary>
        /// <seealso cref="selectedSharedVertexCount"/>
        public static int selectedVertexCount { get { CacheElementCounts(); return s_SelectedVertexCount; } }

        /// <summary>
        /// Gets the sum of all currently selected shared vertices on all currently selected ProBuilderMesh objects.
        /// </summary>
        /// <seealso cref="selectedVertexCount"/>
        public static int selectedSharedVertexCount { get { CacheElementCounts(); return s_SelectedSharedVertexCount; } }

        /// <summary>
        /// Gets the sum of all currently selected faces on all currently selected ProBuilderMesh objects.
        /// </summary>
        public static int selectedFaceCount { get { CacheElementCounts(); return s_SelectedFaceCount; } }

        /// <summary>
        /// Gets the sum of all currently selected edges on all currently selected ProBuilderMesh objects.
        /// </summary>
        public static int selectedEdgeCount { get { CacheElementCounts(); return s_SelectedEdgeCount; } }

        // per-object selected element maxes
        internal static int selectedFaceCountObjectMax { get { CacheElementCounts(); return s_SelectedFaceCountObjectMax; } }
        internal static int selectedEdgeCountObjectMax { get { CacheElementCounts(); return s_SelectedEdgeCountObjectMax; } }
        internal static int selectedVertexCountObjectMax { get { CacheElementCounts(); return s_SelectedVertexCountObjectMax; } }
        internal static int selectedSharedVertexCountObjectMax { get { CacheElementCounts(); return s_SelectedSharedVertexCountObjectMax; } }
        internal static int selectedCoincidentVertexCountMax { get { CacheElementCounts(); return s_SelectedCoincidentVertexCountMax; } }

        // Faces that need to be refreshed when moving or modifying the actual selection
        internal static Dictionary<ProBuilderMesh, List<Face>> selectedFacesInEditZone
        {
            get
            {
                if(s_SelectedFacesInEditAreaDirty)
                    RecalculateFacesInEditableArea();
                return s_SelectedFacesInEditArea;
            }
        }

        internal static void InvalidateCaches()
        {
            s_ElementCountsDirty = true;
            s_SelectedElementGroupsDirty = true;
            s_SelectedFacesInEditAreaDirty = true;
            s_SelectionBoundsDirty = true;
        }

        internal static IEnumerable<MeshAndElementSelection> elementSelection
        {
            get
            {
                RecalculateSelectedElementGroups();
                return s_ElementSelection;
            }
        }

        static MeshSelection()
        {
            Selection.selectionChanged += OnObjectSelectionChanged;
            Undo.undoRedoPerformed += UndoRedoPerformed;
            ProBuilderMesh.elementSelectionChanged += ElementSelectionChanged;
            EditorMeshUtility.meshOptimized += (x, y) => { s_ElementCountsDirty = true; };
            ProBuilderMesh.componentWillBeDestroyed += RemoveMeshFromSelectionInternal;
            ProBuilderMesh.componentHasBeenReset += RefreshSelectionAfterComponentReset;
            ProBuilderEditor.selectModeChanged += SelectModeChanged;
            ToolManager.activeToolChanged += ActiveToolChanged;
#if UNITY_2020_2_OR_NEWER
            ToolManager.activeContextChanged += ActiveToolChanged;
#endif
            VertexManipulationTool.afterMeshModification += AfterMeshModification;
            OnObjectSelectionChanged();
        }

        /// <summary>
        /// Gets the ProBuilder mesh on the active selected GameObject.
        /// </summary>
        public static ProBuilderMesh activeMesh
        {
            get
            {
                // If shift selecting between objects already selected there won't be an OnObjectSelectionChanged
                // triggered which might lead to Selection.activeGameObject and s_ActiveMesh to be out of sync.
                // This check below is to handle this situation.
                GameObject activeGo = (s_ActiveMesh ? s_ActiveMesh.gameObject : null);

                if (activeGo != Selection.activeGameObject)
                    s_ActiveMesh = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<ProBuilderMesh>() : null;

                return s_ActiveMesh;
            }
        }

        internal static Face activeFace
        {
            get { return activeMesh != null ? activeMesh.selectedFacesInternal.LastOrDefault() : null; }
        }

        /// <summary>
        /// Raised when the object selection changes.
        /// </summary>
        public static event System.Action objectSelectionChanged;

        static HashSet<ProBuilderMesh> s_UnitySelectionChangeMeshes = new HashSet<ProBuilderMesh>();

        internal static void OnObjectSelectionChanged()
        {
            // GameObjects returns both parent and child when both are selected, where transforms only returns the
            // top-most transform.
            s_UnitySelectionChangeMeshes.Clear();
            s_ElementSelection.Clear();
            s_ActiveMesh = null;

            var gameObjects = Selection.gameObjects;

            for (int i = 0, c = gameObjects.Length; i < c; i++)
            {
#if UNITY_2019_3_OR_NEWER
                ProBuilderMesh mesh;
                if(gameObjects[i].TryGetComponent<ProBuilderMesh>(out mesh))
#else
                var mesh = gameObjects[i].GetComponent<ProBuilderMesh>();
                if (mesh != null)
#endif
                {
                    if (gameObjects[i] == Selection.activeGameObject)
                        s_ActiveMesh = mesh;

                    s_UnitySelectionChangeMeshes.Add(mesh);
                }
            }

            for (int i = 0, c = s_TopSelection.Count; i < c; i++)
            {
                if (!s_UnitySelectionChangeMeshes.Contains(s_TopSelection[i]))
                {
                    if(s_TopSelection[i] != null)
                        UndoUtility.RecordSelection(s_TopSelection[i], "Selection Change");
                    s_TopSelection[i].ClearSelection();
                }
            }

            s_TopSelection.Clear();

            foreach (var mesh in s_UnitySelectionChangeMeshes)
            {
                // don't add prefabs or assets to the mesh selection
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(mesh.gameObject)))
                {
                    EditorUtility.SynchronizeWithMeshFilter(mesh);
                    s_TopSelection.Add(mesh);
                }
            }

            InvalidateCaches();

            if (objectSelectionChanged != null)
                objectSelectionChanged();

            s_UnitySelectionChangeMeshes.Clear();
        }

        static void SelectModeChanged(SelectMode mode)
        {
            InvalidateCaches();
        }

        static void ActiveToolChanged()
        {
            InvalidateCaches();
        }

        static void AfterMeshModification(IEnumerable<ProBuilderMesh> selection)
        {
            InvalidateCaches();
        }

        /// <summary>
        /// Ensure the mesh selection matches the current Unity selection. Called after Undo/Redo, as adding or removing
        /// mesh components can cause the selection to de-sync without emitting a selection changed event.
        /// </summary>
        internal static void UndoRedoPerformed()
        {
            var willInvokeChangeCallback = false;
            for (int i = 0; i < topInternal.Count; i++)
            {
                if (topInternal[i] == null || !Selection.Contains(topInternal[i].gameObject))
                {
                    EditorApplication.delayCall += OnObjectSelectionChanged;
                    willInvokeChangeCallback = true;
                    break;
                }
            }

            if (!willInvokeChangeCallback)
            {
                var activeMesh = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<ProBuilderMesh>() : null;
                if (activeMesh != null && !topInternal.Contains(activeMesh) || (activeMesh == null && topInternal.Count == 0))
                    EditorApplication.delayCall += OnObjectSelectionChanged;
            }

            InvalidateCaches();
        }

        static void ElementSelectionChanged(ProBuilderMesh mesh)
        {
            InvalidateCaches();
        }

        internal static void RecalculateSelectedElementGroups()
        {
            if (!s_SelectedElementGroupsDirty)
                return;

            s_SelectedElementGroupsDirty = false;
            s_ElementSelection.Clear();

#if UNITY_2020_2_OR_NEWER
            VertexManipulationTool activeTool = null;
            var editorTool = EditorToolManager.activeTool;
            if(editorTool is VertexManipulationTool)
                activeTool = (VertexManipulationTool)editorTool;
#else
            var activeTool = ProBuilderEditor.activeTool;
#endif

            if (activeTool != null)
            {
                foreach (var mesh in s_TopSelection)
                    s_ElementSelection.Add(activeTool.GetElementSelection(mesh, VertexManipulationTool.pivotPoint));
            }
        }

        internal static void CacheElementCounts(bool forceRebuild = false)
        {
            if (!s_ElementCountsDirty && !forceRebuild)
                return;

            s_SelectedFaceCount = 0;
            s_SelectedEdgeCount = 0;
            s_SelectedVertexCount = 0;
            s_SelectedSharedVertexCount = 0;

            s_SelectedVertexCountObjectMax = 0;
            s_SelectedSharedVertexCountObjectMax = 0;
            s_SelectedCoincidentVertexCountMax = 0;
            s_SelectedFaceCountObjectMax = 0;
            s_SelectedEdgeCountObjectMax = 0;

            s_TotalVertexCount = 0;
            s_TotalFaceCount = 0;
            s_TotalEdgeCount = 0;
            s_TotalCommonVertexCount = 0;
            s_TotalVertexCountCompiled = 0;
            s_TotalTriangleCountCompiled = 0;

            s_SelectedObjectCount = topInternal.Count;

            for (var i = 0; i < s_SelectedObjectCount; i++)
            {
                var mesh = topInternal[i];

                s_SelectedFaceCount += mesh.selectedFaceCount;
                s_SelectedEdgeCount += mesh.selectedEdgeCount;
                s_SelectedVertexCount += mesh.selectedIndexesInternal.Length;
                s_SelectedSharedVertexCount += mesh.selectedSharedVerticesCount;

                s_SelectedVertexCountObjectMax = System.Math.Max(s_SelectedVertexCountObjectMax, mesh.selectedIndexesInternal.Length);
                s_SelectedSharedVertexCountObjectMax = System.Math.Max(s_SelectedSharedVertexCountObjectMax, mesh.selectedSharedVerticesCount);
                s_SelectedCoincidentVertexCountMax = System.Math.Max(s_SelectedCoincidentVertexCountMax, mesh.selectedCoincidentVertexCount);
                s_SelectedFaceCountObjectMax = System.Math.Max(s_SelectedFaceCountObjectMax, mesh.selectedFaceCount);
                s_SelectedEdgeCountObjectMax = System.Math.Max(s_SelectedEdgeCountObjectMax, mesh.selectedEdgeCount);

                // element total counts
                s_TotalVertexCount += mesh.vertexCount;
                s_TotalFaceCount += mesh.faceCount;
                s_TotalEdgeCount += mesh.edgeCount;
                s_TotalCommonVertexCount += mesh.sharedVerticesInternal.Length;
                s_TotalVertexCountCompiled += mesh.mesh == null ? 0 : mesh.mesh.vertexCount;
                s_TotalTriangleCountCompiled += (int) UnityEngine.ProBuilder.MeshUtility.GetPrimitiveCount(mesh.mesh);
            }

            s_ElementCountsDirty = false;
        }

        internal static void RecalculateSelectionBounds()
        {
            s_SelectionBoundsDirty = false;
            s_SelectionBounds = new Bounds();
            var boundsInitialized = false;

            for (int i = 0, c = topInternal.Count; i < c; i++)
            {
                var mesh = topInternal[i];

                // Undo causes this state
                if (mesh == null)
                    return;

                if (!boundsInitialized && mesh.selectedVertexCount > 0)
                {
                    boundsInitialized = true;
                    s_SelectionBounds = new Bounds(mesh.transform.TransformPoint(mesh.positionsInternal[mesh.selectedIndexesInternal[0]]), Vector3.zero);
                }

                if (mesh.selectedVertexCount > 0)
                {
                    var shared = mesh.sharedVerticesInternal;

                    foreach (var sharedVertex in mesh.selectedSharedVertices)
                        s_SelectionBounds.Encapsulate(mesh.transform.TransformPoint(mesh.positionsInternal[shared[sharedVertex][0]]));
                }
            }
        }

        static void RecalculateFacesInEditableArea()
        {
            s_SelectedFacesInEditAreaDirty = false;
            s_SelectedFacesInEditArea.Clear();

            foreach (var mesh in topInternal)
            {
                s_SelectedFacesInEditArea.Add(mesh, ElementSelection.GetNeighborFaces(mesh, mesh.selectedIndexesInternal));
            }
        }

        /// <summary>
        /// Get all selected ProBuilderMesh components. Corresponds to <![CDATA[Selection.gameObjects.Select(x => x.GetComponent<ProBuilderMesh>().Where(y => y != null);]]>.
        /// </summary>
        /// <summary>An array of the currently selected ProBuilderMesh components. Does not include children of selected objects.</summary>
        public static IEnumerable<ProBuilderMesh> top
        {
            get { return new ReadOnlyCollection<ProBuilderMesh>(s_TopSelection); }
        }

        internal static List<ProBuilderMesh> topInternal
        {
            get { return s_TopSelection; }
        }

        /// <summary>
        /// Gets all selected ProBuilderMesh components, including those on the children of selected objects.
        /// </summary>
        /// <returns>All selected ProBuilderMesh components, including those on the children of selected objects.</returns>
        public static IEnumerable<ProBuilderMesh> deep
        {
            get { return Selection.gameObjects.SelectMany(x => x.GetComponentsInChildren<ProBuilderMesh>()); }
        }

        internal static bool Contains(ProBuilderMesh mesh)
        {
            return s_TopSelection.Contains(mesh);
        }

        /// <summary>
        /// Gets the sum of the vertices across all selected meshes.
        /// </summary>
        /// <remarks>
        /// This is <see cref="ProBuilderMesh.vertexCount"/>, not <see cref="UnityEngine.Mesh.vertexCount"/>.
        /// </remarks>
        public static int totalVertexCount { get { CacheElementCounts(); return s_TotalVertexCount; } }

        /// <summary>
        /// Gets the number of all selected vertices across the selected ProBuilder meshes, excluding coincident duplicates.
        /// </summary>
        public static int totalCommonVertexCount { get { CacheElementCounts(); return s_TotalCommonVertexCount; } }

        internal static int totalVertexCountOptimized { get { CacheElementCounts(); return s_TotalVertexCountCompiled; } }

        /// <summary>
        /// Gets the sum of all selected ProBuilderMesh faces.
        /// </summary>
        public static int totalFaceCount { get { CacheElementCounts(); return s_TotalFaceCount; } }

        /// <summary>
        /// Gets the sum of all selected ProBuilderMesh edges.
        /// </summary>
        public static int totalEdgeCount { get { CacheElementCounts(); return s_TotalEdgeCount; } }

        /// <summary>
        /// Gets the sum of all selected ProBuilder compiled mesh triangles (3 indices make up a triangle, or 4 indices if topology is quad).
        /// </summary>
        public static int totalTriangleCountCompiled { get { CacheElementCounts(); return s_TotalTriangleCountCompiled; } }

        internal static void AddToSelection(GameObject t)
        {
            if (t == null || Selection.objects.Contains(t))
                return;

            int len = Selection.objects.Length;

            Object[] temp = new Object[len + 1];

            for (int i = 0; i < len; i++)
                temp[i] = Selection.objects[i];

            temp[len] = t;

            Selection.activeObject = t;
            Selection.objects = temp;
            OnObjectSelectionChanged();
        }

        internal static void RemoveFromSelection(GameObject t)
        {
            int ind = System.Array.IndexOf(Selection.objects, t);
            if (ind < 0)
                return;

            Object[] temp = new Object[Selection.objects.Length - 1];

            for (int i = 0; i < temp.Length; i++)
            {
                if (i != ind)
                    temp[i] = Selection.objects[i];
            }

            Selection.objects = temp;

            if (Selection.activeGameObject == t)
                Selection.activeObject = temp.FirstOrDefault();

            OnObjectSelectionChanged();
        }

        internal static void MakeActiveObject(GameObject t)
        {
            if (t == null || !Selection.objects.Contains(t))
                return;

            int ind = System.Array.IndexOf(Selection.objects, t);
            int len = Selection.objects.Length;

            Object[] temp = new Object[len];

            for (int i = 0; i < len - 1 ; i++)
            {
                if(i == ind)
                {
                    temp[i] = Selection.objects[len - 1];
                }
                else
                {
                    temp[i] = Selection.objects[i];
                }
            }

            temp[len - 1] = t;

            Selection.activeObject = t;
            Selection.objects = temp;
            OnObjectSelectionChanged();
        }

        internal static void RemoveMeshFromSelectionInternal(ProBuilderMesh mesh)
        {
            if (s_TopSelection.Contains(mesh))
                s_TopSelection.Remove(mesh);
        }

        internal static void RefreshSelectionAfterComponentReset(ProBuilderMesh mesh)
        {
            ProBuilderEditor.Refresh(true);
        }

        internal static void SetSelection(IList<GameObject> newSelection)
        {
            UndoUtility.RecordSelection(topInternal.ToArray(), "Change Selection");
            ClearElementAndObjectSelection();

            // if the previous tool was set to none, use Tool.Move
            if (Tools.current == Tool.None)
                Tools.current = Tool.Move;

            var newCount = newSelection != null ? newSelection.Count : 0;

            if (newCount > 0)
            {
                Selection.activeTransform = newSelection[newCount - 1].transform;
                Selection.objects = newSelection.ToArray();
            }
            else
            {
                Selection.activeTransform = null;
            }

            OnObjectSelectionChanged();
        }

        internal static void SetSelection(GameObject go)
        {
            UndoUtility.RecordSelection(topInternal.ToArray(), "Change Selection");
            ClearElementAndObjectSelection();
            AddToSelection(go);
        }

        /// <summary>
        /// Clears all selected mesh attributes in the current selection. This means triangles, faces, and edges, but not objects.
        /// </summary>
        /// <seealso cref="ClearElementAndObjectSelection"/>
        public static void ClearElementSelection()
        {
            if (ProBuilderEditor.instance)
                ProBuilderEditor.instance.ClearElementSelection();
            InvalidateCaches();
            if (objectSelectionChanged != null)
                objectSelectionChanged();
        }

        /// <summary>
        /// Clears both the <see cref="UnityEditor.Selection.objects"/> and ProBuilder mesh attribute selections.
        /// </summary>
        /// <seealso cref="ClearElementSelection"/>
        public static void ClearElementAndObjectSelection()
        {
            if (ProBuilderEditor.instance)
                ProBuilderEditor.instance.ClearElementSelection();
            Selection.objects = new Object[0];
        }

        internal static Vector3 GetHandlePosition()
        {
            var active = GetActiveSelectionGroup();

            if(active == null || active.mesh == null)
                return Vector3.zero;

            switch (VertexManipulationTool.pivotPoint)
            {
                case PivotPoint.ActiveElement:
                case PivotPoint.IndividualOrigins:
                    if (active.elementGroups.Count == 0)
                        goto case default;
                    return active.elementGroups.Last().position;

                case PivotPoint.Center:
                default:
                    return bounds.center;
            }
        }

        internal static Quaternion GetHandleRotation()
        {
            var active = GetActiveSelectionGroup();

            if(active == null || active.mesh == null)
                return Quaternion.identity;

            switch (VertexManipulationTool.handleOrientation)
            {
                case HandleOrientation.ActiveObject:
                    return active.mesh.transform.rotation;

                case HandleOrientation.ActiveElement:
                    if (active.elementGroups.Count == 0)
                        goto case HandleOrientation.ActiveObject;
                    return active.elementGroups.Last().rotation;

                default:
                    return Quaternion.identity;
            }
        }

        internal static MeshAndElementSelection GetActiveSelectionGroup()
        {
            foreach (var pair in elementSelection)
            {
                if (pair.mesh == activeMesh)
                    return pair;
            }

            return null;
       }
    }
}
