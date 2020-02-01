using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.ObjectModel;
using UnityEditor.ProBuilder;

namespace UnityEngine.ProBuilder
{
    public sealed partial class ProBuilderMesh
    {
        internal ElementCollection selection
        {
            get { return SelectionManager.instance.GetOrCreateSelection(this); }
        }

        /// <value>
        /// Get the number of faces that are currently selected on this object.
        /// </value>
        public int selectedFaceCount
        {
            get { return selection.selectedFaceCount; }
        }

        /// <value>
        /// Get the number of selected vertex indexes.
        /// </value>
        public int selectedVertexCount
        {
            get { return selection.selectedVertexCount; }
        }

        /// <value>
        /// Get the number of selected edges.
        /// </value>
        public int selectedEdgeCount
        {
            get { return selection.selectedEdgeCount; }
        }

        internal int selectedSharedVerticesCount
        {
            get { return selection.selectedSharedVerticesCount; }
        }

        internal int selectedCoincidentVertexCount
        {
            get { return selection.selectedCoincidentVertexCount; }
        }

        internal IEnumerable<int> selectedSharedVertices
        {
            get { return selection.selectedSharedVertices; }
        }

        /// <summary>
        /// All selected vertices and their coincident neighbors.
        /// </summary>
        internal IEnumerable<int> selectedCoincidentVertices
        {
            get { return selection.selectedCoincidentVertices; }
        }

        /// <summary>
        /// Get a copy of the selected face array.
        /// </summary>
        public Face[] GetSelectedFaces()
        {
            return selection.GetSelectedFaces();
        }

        /// <value>
        /// A collection of the currently selected faces by their index in the @"UnityEngine.ProBuilder.ProBuilderMesh.faces" array.
        /// </value>
        public ReadOnlyCollection<int> selectedFaceIndexes
        {
            get { return selection.selectedFaceIndexes; }
        }

        /// <value>
        /// A collection of the currently selected vertices by their index in the @"UnityEngine.ProBuilder.ProBuilderMesh.positions" array.
        /// </value>
        public ReadOnlyCollection<int> selectedVertices
        {
            get { return selection.selectedVertices; }
        }

        /// <value>
        /// A collection of the currently selected edges.
        /// </value>
        public ReadOnlyCollection<Edge> selectedEdges
        {
            get { return selection.selectedEdges; }
        }

        internal Face[] selectedFacesInternal
        {
            get { return selection.selectedFacesInternal; }
            set { selection.selectedFacesInternal = value; }
        }

        internal int[] selectedFaceIndicesInternal
        {
            get { return selection.selectedFaceIndicesInternal; }
            set { selection.selectedFaceIndicesInternal = value; }
        }

        internal Edge[] selectedEdgesInternal
        {
            get { return selection.selectedEdgesInternal; }
            set { selection.selectedEdgesInternal = value; }
        }

        internal int[] selectedIndexesInternal
        {
            get { return selection.selectedIndexesInternal; }
            set { selection.selectedIndexesInternal = value; }
        }

        internal Face GetActiveFace()
        {
            if (selectedFaceCount < 1)
                return null;
            return m_Faces[selectedFaceIndicesInternal[selectedFaceCount - 1]];
        }

        internal Edge GetActiveEdge()
        {
            return selection.GetActiveEdge();
        }

        internal int GetActiveVertex()
        {
            return selection.GetActiveVertex();
        }

        internal void AddToFaceSelection(int index)
        {
            selection.AddToFaceSelection(index);
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
            selection.SetSelectedFaces(selected);
        }

        /// <summary>
        /// Set the edge selection for this mesh. Also sets the face and vertex selection to match.
        /// </summary>
        /// <param name="edges">The new edge selection.</param>
        public void SetSelectedEdges(IEnumerable<Edge> edges)
        {
            selection.SetSelectedEdges(edges);
        }

        /// <summary>
        /// Sets the selected vertices array. Clears SelectedFaces and SelectedEdges arrays.
        /// </summary>
        /// <param name="vertices">The new vertex selection.</param>
        public void SetSelectedVertices(IEnumerable<int> vertices)
        {
            selection.SetSelectedVertices(vertices);
        }

        /// <summary>
        /// Removes face at index in SelectedFaces array, and updates the SelectedTriangles and SelectedEdges arrays to match.
        /// </summary>
        /// <param name="index"></param>
        internal void RemoveFromFaceSelectionAtIndex(int index)
        {
            selection.RemoveFromFaceSelectionAtIndex(index);
        }

        /// <summary>
        /// Clears selected face, edge, and vertex arrays. You do not need to call this when setting an individual array, as the setter methods will handle updating the associated caches.
        /// </summary>
        public void ClearSelection()
        {
            selection.ClearSelection();
        }
    }
}
