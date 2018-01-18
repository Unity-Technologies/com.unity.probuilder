using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Helper functions for working with Unity object selection & ProBuilder element selection.
	/// </summary>
	[InitializeOnLoad]
	static class pb_Selection
	{
		static pb_Object[] m_TopSelection = new pb_Object[0];
		static pb_Object[] m_DeepSelection = new pb_Object[0];

		static bool m_ElementCountCacheIsDirty = true;

		static int m_TotalVertexCount;
		static int m_TotalCommonVertexCount;
		static int m_TotalVertexCountCompiled;
		static int m_TotalFaceCount;
		static int m_TotalTriangleCountCompiled;

		static int m_SelectedVertexCount;
		static int m_SelectedEdgeCount;
		static int m_SelectedFaceCount;

		static pb_Object[] selection
		{
			get
			{
				return pb_Editor.instance != null
					? pb_Editor.instance.selection
					: pb_Util.GetComponents<pb_Object>(Selection.transforms);
			}
		}

		static pb_Selection()
		{
			Selection.selectionChanged += OnSelectionChanged;
			OnSelectionChanged();
		}

		/// <summary>
		/// Allow other scripts to forcibly reload the cached selection.
		/// </summary>
		public static void OnSelectionChanged()
		{
			m_TopSelection = Selection.transforms.Select(x => x.GetComponent<pb_Object>()).Where(x => x != null).ToArray();
			m_DeepSelection = Selection.transforms.SelectMany(x => x.GetComponentsInChildren<pb_Object>()).ToArray();
			m_ElementCountCacheIsDirty = true;
		}

		/// <summary>
		/// Get just the top level selected pb_Object components.
		/// </summary>
		/// <returns></returns>
		public static pb_Object[] Top()
		{
			return m_TopSelection;
		}

		/// <summary>
		/// Get all selected pb_Object components, including those in children of selected objects.
		/// </summary>
		/// <returns></returns>
		public static pb_Object[] All()
		{
			return m_DeepSelection;
		}

		/// <summary>
		/// Get the sum of all pb_Object vertex counts in the selection.
		/// </summary>
		/// <remarks>
		/// This is the pb_Object.vertexCount, not UnityEngine.Mesh.vertexCount. To get the optimized mesh vertex count, see `totalVertexCountCompiled` for the vertex count as is rendered in the scene.
		/// </remarks>
		public static int totalVertexCount { get { RebuildElementCounts(); return m_TotalVertexCount; } }

		/// <summary>
		/// Get the sum of all pb_Object common vertex counts in the selection.
		/// </summary>
		/// <remarks>
		/// This is the pb_Object.sharedIndices, not UnityEngine.Mesh.vertexCount. To get the optimized mesh vertex count, see `totalVertexCountCompiled` for the vertex count as is rendered in the scene.
		/// </remarks>
		public static int totalCommonVertexCount { get { RebuildElementCounts(); return m_TotalCommonVertexCount; } }

		/// <summary>
		/// Get the sum of all selected ProBuilder mesh vertex counts. This value reflects the actual vertex count per UnityEngine.Mesh.
		/// </summary>
		public static int totalVertexCountCompiled { get { RebuildElementCounts(); return m_TotalVertexCountCompiled; } }

		/// <summary>
		/// Sum of all selected ProBuilder object face counts.
		/// </summary>
		public static int totalFaceCount { get { RebuildElementCounts(); return m_TotalFaceCount; } }

		/// <summary>
		/// Get the sum of all selected ProBuilder compiled mesh triangle counts (3 indices make up a triangle, or 4 indices if topology is quad).
		/// </summary>
		public static int totalTriangleCountCompiled { get { RebuildElementCounts(); return m_TotalTriangleCountCompiled; } }

		static void RebuildElementCounts()
		{
			if (!m_ElementCountCacheIsDirty)
				return;

			m_ElementCountCacheIsDirty = false;

			m_TotalVertexCount = Top().Sum(x => x.vertexCount);
			m_TotalCommonVertexCount = Top().Sum(x => x.sharedIndices.Length);
			m_TotalVertexCountCompiled = Top().Sum(x => x.msh == null ? 0 : x.msh.vertexCount);
			m_TotalFaceCount = Top().Sum(x => x.faceCount);
			m_TotalTriangleCountCompiled = Top().Sum(x => (int) pb_MeshUtility.GetTriangleCount(x.msh));
		}

		public static void AddToSelection(GameObject t)
		{
			if(t == null || Selection.objects.Contains(t))
				return;

			Object[] temp = new Object[Selection.objects.Length + 1];

			temp[0] = t;

			for(int i = 1; i < temp.Length; i++)
				temp[i] = Selection.objects[i-1];

			Selection.objects = temp;
		}

		public static void RemoveFromSelection(GameObject t)
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

		public static void SetSelection(IList<GameObject> newSelection)
		{
			pb_Undo.RecordSelection(selection, "Change Selection");

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

		public static void SetSelection(GameObject go)
		{
			pb_Undo.RecordSelection(selection, "Change Selection");
			ClearElementAndObjectSelection();
			AddToSelection(go);
		}

		/// <summary>
		/// Clears all `selected` caches associated with each pb_Object in the current selection. This means triangles, faces, and edges, but not objects.
		/// </summary>
		public static void ClearElementSelection()
		{
			if(pb_Editor.instance)
				pb_Editor.instance.ClearElementSelection();
		}

		/// <summary>
		/// Clear both the Selection.objects and ProBuilder geometry element selections.
		/// </summary>
		public static void ClearElementAndObjectSelection()
		{
			ClearElementSelection();
			Selection.objects = new Object[0];
		}
	}
}
