using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
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
		static ProBuilderMesh[] s_TopSelection = new ProBuilderMesh[0];
		static ProBuilderMesh[] s_DeepSelection = new ProBuilderMesh[0];

		static bool s_ElementCountCacheIsDirty = true;

		static Bounds s_SelectionBounds = new Bounds();

		public static Bounds bounds
		{
			get { return s_SelectionBounds; }
		}

		static int s_TotalVertexCount;
		static int s_TotalFaceCount;
		static int s_TotalEdgeCount;
		static int s_TotalCommonVertexCount;
		static int s_TotalVertexCountCompiled;
		static int s_TotalTriangleCountCompiled;

		internal static int selectedObjectCount { get; private set; }
		internal static int selectedVertexCount { get; private set; }
		internal static int selectedSharedVertexCount { get; private set; }
		internal static int selectedFaceCount { get; private set; }
		internal static int selectedEdgeCount { get; private set; }

		// per-object selected element maxes
		internal static int selectedFaceCountObjectMax { get; private set; }
		internal static int selectedEdgeCountObjectMax { get; private set; }
		internal static int selectedVertexCountObjectMax { get; private set; }
		internal static int selectedSharedVertexCountObjectMax { get; private set; }

		// Faces that need to be refreshed when moving or modifying the actual selection
		internal static Dictionary<ProBuilderMesh, List<Face>> selectedFacesInEditZone { get; private set; }

		static ProBuilderMesh[] selection
		{
			get
			{
				return ProBuilderEditor.instance != null
					? ProBuilderEditor.instance.selection
					: InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms);
			}
		}

		static MeshSelection()
		{
			Selection.selectionChanged += OnObjectSelectionChanged;

			EditorMeshUtility.meshOptimized += (x, y) =>
			{
				s_ElementCountCacheIsDirty = true;
			};

			OnObjectSelectionChanged();
		}

		/// <value>
		/// Receive notifications when the object selection changes.
		/// </value>
		public static event System.Action objectSelectionChanged;

		internal static void OnObjectSelectionChanged()
		{
			// GameObjects returns both parent and child when both are selected, where transforms only returns the top-most
			// transform.
			s_TopSelection = Selection.gameObjects.Select(x => x.GetComponent<ProBuilderMesh>()).Where(x => x != null).ToArray();
			s_DeepSelection = Selection.gameObjects.SelectMany(x => x.GetComponentsInChildren<ProBuilderMesh>()).ToArray();

			selectedObjectCount = s_TopSelection.Length;
			s_ElementCountCacheIsDirty = true;

			if (objectSelectionChanged != null)
				objectSelectionChanged();
		}

		internal static void OnComponentSelectionChanged()
		{
			selectedVertexCount = 0;
			selectedFaceCount = 0;
			selectedEdgeCount = 0;
			selectedSharedVertexCount = 0;

			selectedFaceCountObjectMax = 0;
			selectedVertexCountObjectMax = 0;
			selectedSharedVertexCountObjectMax = 0;
			selectedEdgeCountObjectMax = 0;

			RecalculateSelectedComponentCounts();
			RecalculateFacesInEditableArea();
			RecalculateSelectionBounds();
		}

		internal static void RecalculateSelectedComponentCounts()
		{
			for (var i = 0; i < selection.Length; i++)
			{
				var mesh = selection[i];

				selectedFaceCount += mesh.selectedFaceCount;
				selectedEdgeCount += mesh.selectedEdgeCount;
				selectedVertexCount += mesh.selectedIndexesInternal.Length;
				selectedSharedVertexCount += mesh.selectedSharedVerticesCount;

				selectedVertexCountObjectMax = System.Math.Max(selectedVertexCountObjectMax, mesh.selectedIndexesInternal.Length);
				selectedSharedVertexCountObjectMax = System.Math.Max(selectedSharedVertexCountObjectMax, mesh.selectedSharedVerticesCount);
				selectedFaceCountObjectMax = System.Math.Max(selectedFaceCountObjectMax, mesh.selectedFaceCount);
				selectedEdgeCountObjectMax = System.Math.Max(selectedEdgeCountObjectMax, mesh.selectedEdgeCount);
			}
		}

		internal static void RecalculateSelectionBounds()
		{
			s_SelectionBounds = new Bounds();
			var boundsInitialized = false;

			for (var i = 0; i < selection.Length; i++)
			{
				var mesh = selection[i];

				if (!boundsInitialized && mesh.selectedVertexCount > 0)
				{
					boundsInitialized = true;
					s_SelectionBounds = new Bounds(mesh.transform.TransformPoint(mesh.positionsInternal[mesh.selectedIndexesInternal[0]]), Vector3.zero);
				}

				if (mesh.selectedVertexCount > 0)
				{
					var shared = mesh.sharedVerticesInternal;

					foreach(var sharedVertex in mesh.selectedSharedVertices)
						s_SelectionBounds.Encapsulate(mesh.transform.TransformPoint(mesh.positionsInternal[shared[sharedVertex][0]]));
				}
			}
		}

		internal static void RecalculateFacesInEditableArea()
		{
			if (selectedFacesInEditZone != null)
				selectedFacesInEditZone.Clear();
			else
				selectedFacesInEditZone = new Dictionary<ProBuilderMesh, List<Face>>();

			foreach (var mesh in selection)
			{
				selectedFacesInEditZone.Add(mesh, ElementSelection.GetNeighborFaces(mesh, mesh.selectedIndexesInternal));
			}
		}

		/// <summary>
		/// Get all selected ProBuilderMesh components. Corresponds to <![CDATA[Selection.gameObjects.Select(x => x.GetComponent<ProBuilderMesh>().Where(y => y != null);]]>.
		/// </summary>
		/// <returns>An array of the currently selected ProBuilderMesh components. Does not include children of selected objects.</returns>
		public static IEnumerable<ProBuilderMesh> Top()
		{
			return s_TopSelection;
		}

		internal static ProBuilderMesh[] TopInternal()
		{
			return s_TopSelection;
		}

		/// <summary>
		/// Get all selected ProBuilderMesh components, including those in children of selected objects.
		/// </summary>
		/// <returns>All selected ProBuilderMesh components, including those in children of selected objects.</returns>
		public static IEnumerable<ProBuilderMesh> All()
		{
			return s_DeepSelection;
		}

		/// <value>
		/// How many ProBuilderMesh components are currently selected. Corresponds to the length of Top.
		/// </value>
		public static int count
		{
			get { return TopInternal().Length; }
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
			if (!s_ElementCountCacheIsDirty)
				return;

			try
			{
				s_TotalVertexCount = TopInternal().Sum(x => x.vertexCount);
				s_TotalFaceCount = TopInternal().Sum(x => x.faceCount);
				s_TotalEdgeCount = TopInternal().Sum(x => x.edgeCount);
				s_TotalCommonVertexCount = TopInternal().Sum(x => x.sharedVerticesInternal.Length);
				s_TotalVertexCountCompiled = TopInternal().Sum(x => x.mesh == null ? 0 : x.mesh.vertexCount);
				s_TotalTriangleCountCompiled = TopInternal().Sum(x => (int) UnityEngine.ProBuilder.MeshUtility.GetPrimitiveCount(x.mesh));
				s_ElementCountCacheIsDirty = false;
			}
			catch
			{
				// expected when UndoRedo is called
			}
		}

		internal static void AddToSelection(GameObject t)
		{
			if(t == null || Selection.objects.Contains(t))
				return;
			Object[] temp = new Object[Selection.objects.Length + 1];
			temp[0] = t;
			for(int i = 1; i < temp.Length; i++)
				temp[i] = Selection.objects[i-1];
			Selection.objects = temp;
		}

		internal static void RemoveFromSelection(GameObject t)
		{
			int ind = System.Array.IndexOf(Selection.objects, t);
			if(ind < 0)
				return;

			Object[] temp = new Object[Selection.objects.Length - 1];

			for(int i = 1; i < temp.Length; i++) {
				if(i != ind)
					temp[i] = Selection.objects[i];
			}

			Selection.objects = temp;
		}

		internal static void SetSelection(IList<GameObject> newSelection)
		{
			UndoUtility.RecordSelection(selection, "Change Selection");
			ClearElementAndObjectSelection();

			// if the previous tool was set to none, use Tool.Move
			if(Tools.current == Tool.None)
				Tools.current = Tool.Move;

			if(newSelection != null && newSelection.Count > 0) {
				Selection.activeTransform = newSelection[0].transform;
				Selection.objects = newSelection.ToArray();
			}
			else
			{
				Selection.activeTransform = null;
			}
		}

		internal static void SetSelection(GameObject go)
		{
			UndoUtility.RecordSelection(selection, "Change Selection");
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
			s_ElementCountCacheIsDirty = true;
			if (objectSelectionChanged != null)
				objectSelectionChanged();
		}

		/// <summary>
		/// Clear both the Selection.objects and ProBuilder mesh attribute selections.
		/// </summary>
		public static void ClearElementAndObjectSelection()
		{
			if(ProBuilderEditor.instance)
				ProBuilderEditor.instance.ClearElementSelection();
			Selection.objects = new Object[0];
		}
	}
}
