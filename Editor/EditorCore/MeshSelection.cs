using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Helper functions for working with Unity object selection and ProBuilder mesh attribute selections.
    /// </summary>
    [InitializeOnLoad]
    public static class MeshSelection
    {
        static List<ProBuilderMesh> s_TopSelection = new List<ProBuilderMesh>();
        static ProBuilderMesh s_ActiveMesh;
        static List<MeshAndElementSelection> s_ElementSelection = new List<MeshAndElementSelection>();

        static bool s_TotalElementCountCacheIsDirty = true;
        static bool s_SelectedElementGroupsDirty = true;
        static bool s_SelectedFacesInEditAreaDirty = true;
        static bool s_SelectionBoundsDirty = true;

        static Bounds s_SelectionBounds = new Bounds();
        static Dictionary<ProBuilderMesh, List<Face>> s_SelectedFacesInEditArea = new Dictionary<ProBuilderMesh, List<Face>>();

        /// <value>
        /// An axis-aligned bounding box encompassing the selected elements.
        /// </value>
        public static Bounds bounds
        {
            get
            {
                if(s_SelectionBoundsDirty)
                    RecalculateSelectionBounds();
                return s_SelectionBounds;
            }
        }

        static int s_TotalVertexCount;
        static int s_TotalFaceCount;
        static int s_TotalEdgeCount;
        static int s_TotalCommonVertexCount;
        static int s_TotalVertexCountCompiled;
        static int s_TotalTriangleCountCompiled;

        /// <value>
        /// How many ProBuilderMesh components are currently selected. Corresponds to the length of Top.
        /// </value>
        public static int selectedObjectCount { get; private set; }

        /// <value>
        /// The sum of all selected ProBuilderMesh selected vertex counts.
        /// </value>
        /// <seealso cref="selectedSharedVertexCount"/>
        public static int selectedVertexCount { get; private set; }

        /// <value>
        /// The sum of all selected ProBuilderMesh selected shared vertex counts.
        /// </value>
        /// <seealso cref="selectedVertexCount"/>
        public static int selectedSharedVertexCount { get; private set; }

        /// <value>
        /// The sum of all selected ProBuilderMesh selected face counts.
        /// </value>
        public static int selectedFaceCount { get; private set; }

        /// <value>
        /// The sum of all selected ProBuilderMesh selected edge counts.
        /// </value>
        public static int selectedEdgeCount { get; private set; }

        // per-object selected element maxes
        internal static int selectedFaceCountObjectMax { get; private set; }
        internal static int selectedEdgeCountObjectMax { get; private set; }
        internal static int selectedVertexCountObjectMax { get; private set; }
        internal static int selectedSharedVertexCountObjectMax { get; private set; }
        internal static int selectedCoincidentVertexCountMax { get; private set; }

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

        internal static void InvalidateElementSelection()
        {
            s_SelectedElementGroupsDirty = true;
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
            ProBuilderMesh.elementSelectionChanged += ElementSelectionChanged;
            EditorMeshUtility.meshOptimized += (x, y) => { s_TotalElementCountCacheIsDirty = true; };
            ProBuilderMesh.componentWillBeDestroyed += RemoveMeshFromSelectionInternal;
            OnObjectSelectionChanged();
        }

        /// <value>
        /// Returns the active selected mesh.
        /// </value>
        public static ProBuilderMesh activeMesh
        {
            get { return s_ActiveMesh; }
        }

        internal static Face activeFace
        {
            get { return activeMesh != null ? activeMesh.selectedFacesInternal.LastOrDefault() : null; }
        }

        /// <value>
        /// Receive notifications when the object selection changes.
        /// </value>
        public static event System.Action objectSelectionChanged;

        internal static void OnObjectSelectionChanged()
        {
            // GameObjects returns both parent and child when both are selected, where transforms only returns the top-most
            // transform.
            s_TopSelection.Clear();
            s_ElementSelection.Clear();
            s_ActiveMesh = null;

            var gameObjects = Selection.gameObjects;

            for (int i = 0, c = gameObjects.Length; i < c; i++)
            {
                var mesh = gameObjects[i].GetComponent<ProBuilderMesh>();

                if (mesh != null)
                {
                    if (gameObjects[i] == Selection.activeGameObject)
                        s_ActiveMesh = mesh;

                    s_TopSelection.Add(mesh);
                }
            }

            selectedObjectCount = s_TopSelection.Count;
            OnComponentSelectionChanged();

            if (objectSelectionChanged != null)
                objectSelectionChanged();
        }

        internal static void OnComponentSelectionChanged()
        {
            s_TotalElementCountCacheIsDirty = true;
            s_SelectedFacesInEditAreaDirty = true;
            s_SelectedElementGroupsDirty = true;
            s_SelectionBoundsDirty = true;

            selectedVertexCount = 0;
            selectedFaceCount = 0;
            selectedEdgeCount = 0;
            selectedSharedVertexCount = 0;

            selectedFaceCountObjectMax = 0;
            selectedVertexCountObjectMax = 0;
            selectedSharedVertexCountObjectMax = 0;
            selectedCoincidentVertexCountMax = 0;
            selectedEdgeCountObjectMax = 0;

            RecalculateSelectedComponentCounts();
        }

        static void ElementSelectionChanged(ProBuilderMesh mesh)
        {
            InvalidateElementSelection();
        }

        internal static void RecalculateSelectedElementGroups()
        {
            if (!s_SelectedElementGroupsDirty)
                return;

            s_SelectedElementGroupsDirty = false;
            s_ElementSelection.Clear();

            var activeTool = ProBuilderEditor.activeTool;

            if (activeTool != null)
            {
                foreach (var mesh in s_TopSelection)
                {
                    s_ElementSelection.Add(activeTool.GetElementSelection(mesh,
                        VertexManipulationTool.pivotPoint, VertexManipulationTool.handleOrientation));
                }
            }
        }

        internal static void RecalculateSelectedComponentCounts()
        {
            for (var i = 0; i < topInternal.Count; i++)
            {
                var mesh = topInternal[i];

                selectedFaceCount += mesh.selectedFaceCount;
                selectedEdgeCount += mesh.selectedEdgeCount;
                selectedVertexCount += mesh.selectedIndexesInternal.Length;
                selectedSharedVertexCount += mesh.selectedSharedVerticesCount;

                selectedVertexCountObjectMax = System.Math.Max(selectedVertexCountObjectMax, mesh.selectedIndexesInternal.Length);
                selectedSharedVertexCountObjectMax = System.Math.Max(selectedSharedVertexCountObjectMax, mesh.selectedSharedVerticesCount);
                selectedCoincidentVertexCountMax = System.Math.Max(selectedCoincidentVertexCountMax, mesh.selectedCoincidentVertexCount);
                selectedFaceCountObjectMax = System.Math.Max(selectedFaceCountObjectMax, mesh.selectedFaceCount);
                selectedEdgeCountObjectMax = System.Math.Max(selectedEdgeCountObjectMax, mesh.selectedEdgeCount);
            }
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
        /// <value>An array of the currently selected ProBuilderMesh components. Does not include children of selected objects.</value>
        public static IEnumerable<ProBuilderMesh> top
        {
            get { return new ReadOnlyCollection<ProBuilderMesh>(s_TopSelection); }
        }

        internal static List<ProBuilderMesh> topInternal
        {
            get { return s_TopSelection; }
        }

        /// <summary>
        /// Get all selected ProBuilderMesh components, including those in children of selected objects.
        /// </summary>
        /// <returns>All selected ProBuilderMesh components, including those in children of selected objects.</returns>
        public static IEnumerable<ProBuilderMesh> deep
        {
            get { return Selection.gameObjects.SelectMany(x => x.GetComponentsInChildren<ProBuilderMesh>()); }
        }

        internal static bool Contains(ProBuilderMesh mesh)
        {
            return s_TopSelection.Contains(mesh);
        }

        /// <value>
        /// Get the number of all selected vertices across the selected ProBuilder meshes.
        /// </value>
        /// <remarks>
        /// This is the ProBuilderMesh.vertexCount, not UnityEngine.Mesh.vertexCount. To get the optimized mesh vertex count,
        /// see `totalVertexCountCompiled` for the vertex count as is rendered in the scene.
        /// </remarks>
        public static int totalVertexCount { get { RebuildElementCounts(); return s_TotalVertexCount; } }

        /// <value>
        /// Get the number of all selected vertices across the selected ProBuilder meshes, excluding coincident duplicates.
        /// </value>
        public static int totalCommonVertexCount { get { RebuildElementCounts(); return s_TotalCommonVertexCount; } }

        internal static int totalVertexCountOptimized { get { RebuildElementCounts(); return s_TotalVertexCountCompiled; } }

        /// <value>
        /// Sum of all selected ProBuilderMesh object faceCount properties.
        /// </value>
        public static int totalFaceCount { get { RebuildElementCounts(); return s_TotalFaceCount; } }

        /// <value>
        /// Sum of all selected ProBuilderMesh object edgeCount properties.
        /// </value>
        public static int totalEdgeCount { get { RebuildElementCounts(); return s_TotalEdgeCount; } }

        /// <value>
        /// Get the sum of all selected ProBuilder compiled mesh triangle counts (3 indexes make up a triangle, or 4 indexes if topology is quad).
        /// </value>
        public static int totalTriangleCountCompiled { get { RebuildElementCounts(); return s_TotalTriangleCountCompiled; } }

        static void RebuildElementCounts()
        {
            if (!s_TotalElementCountCacheIsDirty)
                return;

            try
            {
                s_TotalVertexCount = topInternal.Sum(x => x.vertexCount);
                s_TotalFaceCount = topInternal.Sum(x => x.faceCount);
                s_TotalEdgeCount = topInternal.Sum(x => x.edgeCount);
                s_TotalCommonVertexCount = topInternal.Sum(x => x.sharedVerticesInternal.Length);
                s_TotalVertexCountCompiled = topInternal.Sum(x => x.mesh == null ? 0 : x.mesh.vertexCount);
                s_TotalTriangleCountCompiled = topInternal.Sum(x => (int)UnityEngine.ProBuilder.MeshUtility.GetPrimitiveCount(x.mesh));
                s_TotalElementCountCacheIsDirty = false;
            }
            catch
            {
                // expected when UndoRedo is called
            }
        }

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
        }

        internal static void RemoveMeshFromSelectionInternal(ProBuilderMesh mesh)
        {
            if (s_TopSelection.Contains(mesh))
                s_TopSelection.Remove(mesh);
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
        public static void ClearElementSelection()
        {
            if (ProBuilderEditor.instance)
                ProBuilderEditor.instance.ClearElementSelection();
            s_TotalElementCountCacheIsDirty = true;
            if (objectSelectionChanged != null)
                objectSelectionChanged();
        }

        /// <summary>
        /// Clear both the Selection.objects and ProBuilder mesh attribute selections.
        /// </summary>
        public static void ClearElementAndObjectSelection()
        {
            if (ProBuilderEditor.instance)
                ProBuilderEditor.instance.ClearElementSelection();
            Selection.objects = new Object[0];
        }

        internal static Vector3 GetHandlePosition()
        {
            var active = GetActiveSelectionGroup();

            return active != null && active.elementGroups.Count > 0
                ? active.elementGroups.Last().position
                : Vector3.zero;
        }

        internal static Quaternion GetHandleRotation()
        {
            var active = GetActiveSelectionGroup();

            return active != null && active.elementGroups.Count > 0
                ? active.elementGroups.Last().rotation
                : Quaternion.identity;
        }

        internal static MeshAndElementSelection GetActiveSelectionGroup()
        {
            foreach (var pair in elementSelection)
            {
                if (pair.mesh == s_ActiveMesh)
                    return pair;
            }

            return null;
       }
    }
}
