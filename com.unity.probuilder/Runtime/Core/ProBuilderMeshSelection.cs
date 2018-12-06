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
        HashSet<int> m_SelectedSharedVertices = new HashSet<int>();
        List<int> m_SelectedCoincidentVertices = new List<int>();

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
            get { return m_SelectedFaces.Length; }
        }

        /// <value>
        /// Get the number of selected vertex indexes.
        /// </value>
        public int selectedVertexCount
        {
            get { return m_SelectedVertices.Length; }
        }

        /// <value>
        /// Get the number of selected edges.
        /// </value>
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

        internal IEnumerable<int> selectedSharedVertices
        {
            get
            {
                CacheSelection();
                return m_SelectedSharedVertices;
            }
        }

        /// <summary>
        /// All selected vertices and their coincident neighbors.
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

                foreach (var i in m_SelectedVertices)
                {
                    if (m_SelectedSharedVertices.Add(lookup[i]))
                    {
                        m_SelectedSharedVerticesCount++;

                        foreach (var n in sharedVerticesInternal[lookup[i]])
                            m_SelectedCoincidentVertices.Add(n);
                    }
                }
            }
        }

        /// <summary>
        /// Get a copy of the selected face array.
        /// </summary>
        public Face[] GetSelectedFaces()
        {
            int len = m_SelectedFaces.Length;
            var selected = new Face[len];
            for (var i = 0; i < len; i++)
                selected[i] = m_Faces[m_SelectedFaces[i]];
            return selected;
        }

        internal Face[] selectedFacesInternal
        {
            get { return GetSelectedFaces(); }
        }

        internal int[] selectedFaceIndicesInternal
        {
            get { return m_SelectedFaces; }
        }

        /// <value>
        /// A collection of the currently selected faces by their index in the @"UnityEngine.ProBuilder.ProBuilderMesh.faces" array.
        /// </value>
        public ReadOnlyCollection<int> selectedFaceIndexes
        {
            get { return new ReadOnlyCollection<int>(m_SelectedFaces); }
        }

        /// <value>
        /// A collection of the currently selected vertices by their index in the @"UnityEngine.ProBuilder.ProBuilderMesh.positions" array.
        /// </value>
        public ReadOnlyCollection<int> selectedVertices
        {
            get { return new ReadOnlyCollection<int>(m_SelectedVertices); }
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
            get { return m_SelectedVertices; }
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
                m_SelectedFaces = selected.ToArray();
                m_SelectedVertices = m_SelectedFaces.SelectMany(x => facesInternal[x].distinctIndexesInternal).ToArray();
                m_SelectedEdges = m_SelectedFaces.SelectMany(x => facesInternal[x].edges).ToArray();
            }

            m_SelectedCacheDirty = true;

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
                m_SelectedFaces = new int[0];
                m_SelectedEdges = edges.ToArray();
                m_SelectedVertices = m_SelectedEdges.AllTriangles();
            }

            m_SelectedCacheDirty = true;

            if (elementSelectionChanged != null)
                elementSelectionChanged(this);
        }

        /// <summary>
        /// Sets the selected vertices array. Clears SelectedFaces and SelectedEdges arrays.
        /// </summary>
        /// <param name="vertices">The new vertex selection.</param>
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
        /// Removes face at index in SelectedFaces array, and updates the SelectedTriangles and SelectedEdges arrays to match.
        /// </summary>
        /// <param name="index"></param>
        internal void RemoveFromFaceSelectionAtIndex(int index)
        {
            SetSelectedFaces(m_SelectedFaces.RemoveAt(index));
        }

        /// <summary>
        /// Clears selected face, edge, and vertex arrays. You do not need to call this when setting an individual array, as the setter methods will handle updating the associated caches.
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
