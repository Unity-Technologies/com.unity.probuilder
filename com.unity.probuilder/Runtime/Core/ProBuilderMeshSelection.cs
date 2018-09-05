using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.ObjectModel;

namespace UnityEngine.ProBuilder
{
	public sealed partial class ProBuilderMesh
	{
		[SerializeField]
		bool m_IsSelectable = true;

		[SerializeField]
		int[] m_selectedFaces = new int[] { };

		[SerializeField]
		Edge[] m_SelectedEdges = new Edge[] { };

		[SerializeField]
		int[] m_selectedTriangles = new int[] { };

		/// <value>
		/// If false mesh elements will not be selectable. This is used by @"UnityEditor.ProBuilder.ProBuilderEditor".
		/// </value>
		public bool selectable
		{
			get { return m_IsSelectable; }
			set { m_IsSelectable = value; }
		}

		/// <value>
		/// Get the number of faces that are currently selected on this object.
		/// </value>
		public int selectedFaceCount
		{
			get { return m_selectedFaces.Length; }
		}

		/// <value>
		/// Get the number of selected vertex indexes.
		/// </value>
		public int selectedVertexCount
		{
			get { return m_selectedTriangles.Length; }
		}

		/// <value>
		/// Get the number of selected edges.
		/// </value>
		public int selectedEdgeCount
		{
			get { return m_SelectedEdges.Length; }
		}

		/// <summary>
		/// Get a copy of the selected face array.
		/// </summary>
		public Face[] GetSelectedFaces()
		{
			int len = m_selectedFaces.Length;
			var selected = new Face[len];
			for (var i = 0; i < len; i++)
				selected[i] = m_Faces[m_selectedFaces[i]];
			return selected;
		}

		internal Face[] selectedFacesInternal
		{
			get { return GetSelectedFaces(); }
		}

		/// <value>
		/// A collection of the currently selected faces by their index in the @"UnityEngine.ProBuilder.ProBuilderMesh.faces" array.
		/// </value>
		public ReadOnlyCollection<int> selectedFaceIndexes
		{
			get { return new ReadOnlyCollection<int>(m_selectedFaces); }
		}

		/// <value>
		/// A collection of the currently selected vertices by their index in the @"UnityEngine.ProBuilder.ProBuilderMesh.positions" array.
		/// </value>
		public ReadOnlyCollection<int> selectedVertices
		{
			get { return new ReadOnlyCollection<int>(m_selectedTriangles); }
		}

		/// <value>
		/// A collection of the currently selected edges.
		/// </value>
		public ReadOnlyCollection<Edge> selectedEdges
		{
			get { return new ReadOnlyCollection<Edge>(m_SelectedEdges); }
		}

		internal Edge[] selectedEdgesInternal
		{
			get { return m_SelectedEdges; }
		}

		internal int[] selectedIndexesInternal
		{
			get { return m_selectedTriangles; }
		}

		internal void AddToFaceSelection(Face face)
		{
			int index = Array.IndexOf(this.facesInternal, face);
			if (index > -1)
				SetSelectedFaces(m_selectedFaces.Add(index));
		}

		internal void AddToFaceSelection(int index)
		{
			if (index > -1)
				SetSelectedFaces(m_selectedFaces.Add(index));
		}

		/// <summary>
		/// Set the face selection for this mesh. Also sets the vertex and edge selection to match.
		/// </summary>
		/// <param name="selected">The new face selection.</param>
		public void SetSelectedFaces(IEnumerable<Face> selected)
		{
			SetSelectedFaces(selected != null ? selected.Select(x => Array.IndexOf(facesInternal, x)) : null);
		}

		internal void SetSelectedFaces(IEnumerable<int> selected)
		{
			if (selected == null)
			{
				ClearSelection();
			}
			else
			{
				m_selectedFaces = selected.ToArray();
				m_selectedTriangles = m_selectedFaces.SelectMany(x => facesInternal[x].distinctIndexesInternal).ToArray();
				m_SelectedEdges = m_selectedFaces.SelectMany(x => facesInternal[x].edges).ToArray();
			}

			if (elementSelectionChanged != null)
				elementSelectionChanged(this);
		}

		/// <summary>
		/// Set the edge selection for this mesh. Also sets the face and vertex selection to match.
		/// </summary>
		/// <param name="edges">The new edge selection.</param>
		public void SetSelectedEdges(IEnumerable<Edge> edges)
		{
			if (edges == null)
			{
				ClearSelection();
			}
			else
			{
				m_selectedFaces = new int[0];
				m_SelectedEdges = edges.ToArray();
				m_selectedTriangles = m_SelectedEdges.AllTriangles();
			}

			if (elementSelectionChanged != null)
				elementSelectionChanged(this);
		}

		/// <summary>
		/// Sets the selected vertices array. Clears SelectedFaces and SelectedEdges arrays.
		/// </summary>
		/// <param name="vertices">The new vertex selection.</param>
		public void SetSelectedVertices(IEnumerable<int> vertices)
		{
			m_selectedFaces = new int[0];
			m_SelectedEdges = new Edge[0];
			m_selectedTriangles = vertices != null ? vertices.Distinct().ToArray() : new int[0];

			if (elementSelectionChanged != null)
				elementSelectionChanged(this);
		}

		/// <summary>
		/// Removes face at index in SelectedFaces array, and updates the SelectedTriangles and SelectedEdges arrays to match.
		/// </summary>
		/// <param name="index"></param>
		internal void RemoveFromFaceSelectionAtIndex(int index)
		{
			SetSelectedFaces(m_selectedFaces.RemoveAt(index));
		}

		/// <summary>
		/// Removes face from SelectedFaces array, and updates the SelectedTriangles and SelectedEdges arrays to match.
		/// </summary>
		/// <param name="face"></param>
		internal void RemoveFromFaceSelection(Face face)
		{
			int indx = Array.IndexOf(facesInternal, face);

			if (indx > -1)
				SetSelectedFaces(m_selectedFaces.Remove(indx));
		}

		/// <summary>
		/// Clears selected face, edge, and vertex arrays. You do not need to call this when setting an individual array, as the setter methods will handle updating the associated caches.
		/// </summary>
		public void ClearSelection()
		{
			m_selectedFaces = new int[0];
			m_SelectedEdges = new Edge[0];
			m_selectedTriangles = new int[0];
		}
	}
}
