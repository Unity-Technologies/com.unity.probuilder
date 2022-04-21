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

        // Serialized for undo in the editor
        [SerializeField]
        int[] m_SelectedFaces = new int[] {};
        [SerializeField]
        Edge[] m_SelectedEdges = new Edge[] {};
        [SerializeField]
        int[] m_SelectedVertices = new int[] {};

        bool m_SelectedCacheDirty;
        int m_SelectedSharedVerticesCount = 0;
        int m_SelectedCoincidentVertexCount = 0;
        HashSet<int> m_SelectedSharedVertices = new HashSet<int>();
        List<int> m_SelectedCoincidentVertices = new List<int>();

        /// <summary>
        /// Gets or sets whether elements can be selected.
        /// Used by <see cref="UnityEditor.ProBuilder.ProBuilderEditor" />.
        /// </summary>
        /// <returns>False if mesh elements are not selectable. </returns>
        public bool selectable
        {
            get { return m_IsSelectable; }
            set { m_IsSelectable = value; }
        }

        /// <summary>
        /// Gets the number of faces that are currently selected on this object.
        /// </summary>
        /// <returns>Number of selected faces. </returns>
        public int selectedFaceCount
        {
            get { return m_SelectedFaces.Length; }
        }

        /// <summary>
        /// Gets the number of selected vertex indices.
        /// </summary>
        /// <returns>Number of selected vertices. </returns>
        public int selectedVertexCount
        {
            get { return m_SelectedVertices.Length; }
        }

        /// <summary>
        /// Gets the number of selected edges.
        /// </summary>
        /// <returns>Number of selected edges. </returns>
        public int selectedEdgeCount
        {
            get { return m_SelectedEdges.Length; }
        }

        internal int selectedSharedVerticesCount
        {
            get
            {
                CacheSelection();
                return m_SelectedSharedVerticesCount;
            }
        }

        internal int selectedCoincidentVertexCount
        {
            get
            {
                CacheSelection();
                return m_SelectedCoincidentVertexCount;
            }
        }

        internal IEnumerable<int> selectedSharedVertices
        {
            get
            {
                CacheSelection();
                return m_SelectedSharedVertices;
            }
        }

        /// <summary>
        /// Gets all selected vertices and their coincident neighbors.
        /// </summary>
        internal IEnumerable<int> selectedCoincidentVertices
        {
            get
            {
                CacheSelection();
                return m_SelectedCoincidentVertices;
            }
        }

        void CacheSelection()
        {
            if (m_SelectedCacheDirty)
            {
                m_SelectedCacheDirty = false;
                m_SelectedSharedVertices.Clear();
                m_SelectedCoincidentVertices.Clear();
                var lookup = sharedVertexLookup;
                m_SelectedSharedVerticesCount = 0;
                m_SelectedCoincidentVertexCount = 0;

                try
                {
                    foreach (var i in m_SelectedVertices)
                    {
                        if (m_SelectedSharedVertices.Add(lookup[i]))
                        {
                            var coincident = sharedVerticesInternal[lookup[i]];

                            m_SelectedSharedVerticesCount++;
                            m_SelectedCoincidentVertexCount += coincident.Count;

                            foreach (var n in coincident)
                                m_SelectedCoincidentVertices.Add(n);
                        }
                    }
                }
                catch
                {
                    ClearSelection();
                }
            }
        }

        /// <summary>
        /// Returns a copy of the array of selected faces.
        /// </summary>
        /// <returns>Array of currently selected faces. </returns>
        public Face[] GetSelectedFaces()
        {
            int len = m_SelectedFaces.Length;
            var selected = new Face[len];
            for (var i = 0; i < len; i++)
                selected[i] = m_Faces[m_SelectedFaces[i]];
            return selected;
        }

        /// <summary>
        /// Gets a collection of the currently selected faces by their index in the <see cref="ProBuilderMesh.faces" /> array.
        /// </summary>
        /// <returns>Array of indices representing the currently selected faces. </returns>
        public ReadOnlyCollection<int> selectedFaceIndexes
        {
            get { return new ReadOnlyCollection<int>(m_SelectedFaces); }
        }

        /// <summary>
        /// Gets a collection of the currently selected vertices by their index in the <see cref="ProBuilderMesh.positions" /> array.
        /// </summary>
        /// <returns>Array of indices representing the currently selected vertices. </returns>
        public ReadOnlyCollection<int> selectedVertices
        {
            get { return new ReadOnlyCollection<int>(m_SelectedVertices); }
        }

        /// <summary>
        /// Gets a collection of the currently selected edges.
        /// </summary>
        /// <returns>Collection of <see cref="Edge" /> objects representing the currently selected edges. </returns>
        public ReadOnlyCollection<Edge> selectedEdges
        {
            get { return new ReadOnlyCollection<Edge>(m_SelectedEdges); }
        }

        internal Face[] selectedFacesInternal
        {
            get { return GetSelectedFaces(); }
            set { m_SelectedFaces = value.Select(x => Array.IndexOf(m_Faces, x)).ToArray(); }
        }

        internal int[] selectedFaceIndicesInternal
        {
            get { return m_SelectedFaces; }
            set { m_SelectedFaces = value; }
        }

        internal Edge[] selectedEdgesInternal
        {
            get { return m_SelectedEdges; }
            set { m_SelectedEdges = value; }
        }

        internal int[] selectedIndexesInternal
        {
            get { return m_SelectedVertices; }
            set { m_SelectedVertices = value; }
        }

        internal Face GetActiveFace()
        {
            if (selectedFaceCount < 1)
                return null;
            return m_Faces[selectedFaceIndicesInternal[selectedFaceCount - 1]];
        }

        internal Edge GetActiveEdge()
        {
            if (selectedEdgeCount < 1)
                return Edge.Empty;
            return m_SelectedEdges[selectedEdgeCount - 1];
        }

        internal int GetActiveVertex()
        {
            if (selectedVertexCount < 1)
                return -1;
            return m_SelectedVertices[selectedVertexCount - 1];
        }

        internal void AddToFaceSelection(int index)
        {
            if (index > -1)
                SetSelectedFaces(m_SelectedFaces.Add(index));
        }

        /// <summary>
        /// Sets the face selection for this mesh. Also sets the vertex and edge selection to match.
        /// </summary>
        /// <param name="selected">A set of faces to select.</param>
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
                m_SelectedFaces = selected.ToArray();
                m_SelectedVertices = m_SelectedFaces.SelectMany(x => facesInternal[x].distinctIndexesInternal).ToArray();
                m_SelectedEdges = m_SelectedFaces.SelectMany(x => facesInternal[x].edges).ToArray();
            }

            m_SelectedCacheDirty = true;

            if (elementSelectionChanged != null)
                elementSelectionChanged(this);
        }

        /// <summary>
        /// Sets the edge selection for this mesh. Also sets the vertex selection to match and clears the selected faces.
        /// </summary>
        /// <param name="edges">A set of edges to select.</param>
        public void SetSelectedEdges(IEnumerable<Edge> edges)
        {
            if (edges == null)
            {
                ClearSelection();
            }
            else
            {
                m_SelectedFaces = new int[0];
                m_SelectedEdges = edges.ToArray();
                m_SelectedVertices = m_SelectedEdges.AllTriangles();
            }

            m_SelectedCacheDirty = true;

            if (elementSelectionChanged != null)
                elementSelectionChanged(this);
        }

        /// <summary>
        /// Sets the selected vertices array. Clears the selected faces and selected edges arrays.
        /// </summary>
        /// <param name="vertices">The new vertices to select.</param>
        public void SetSelectedVertices(IEnumerable<int> vertices)
        {
            m_SelectedFaces = new int[0];
            m_SelectedEdges = new Edge[0];
            m_SelectedVertices = vertices != null ? vertices.Distinct().ToArray() : new int[0];

            m_SelectedCacheDirty = true;

            if (elementSelectionChanged != null)
                elementSelectionChanged(this);
        }

        /// <summary>
        /// Removes the specified face from the selection. Also updates the SelectedTriangles and SelectedEdges arrays to match.
        /// </summary>
        /// <param name="index">The index from the selected faces array that corresponds to the face to remove from the selection.</param>
        internal void RemoveFromFaceSelectionAtIndex(int index)
        {
            SetSelectedFaces(m_SelectedFaces.RemoveAt(index));
        }

        /// <summary>
        /// Clears the arrays of selected faces, edges, and vertices. You don't need to call this when setting
        /// an individual array, as the setter methods handle updating the associated caches.
        /// </summary>
        public void ClearSelection()
        {
            m_SelectedFaces = new int[0];
            m_SelectedEdges = new Edge[0];
            m_SelectedVertices = new int[0];
            m_SelectedCacheDirty = true;
        }
    }
}
