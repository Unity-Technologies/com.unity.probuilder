using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    [Serializable]
    public class ElementCollection
    {
        // todo Selection shouldn't need a reference to the mesh. This will require a refactor of how selection works
        [SerializeField]
        ProBuilderMesh m_Mesh;

        [SerializeField]
        int[] m_SelectedFaces;
        [SerializeField]
        Edge[] m_SelectedEdges;
        [SerializeField]
        int[] m_SelectedVertices;

        bool m_SelectedCacheDirty;
        int m_SelectedSharedVerticesCount = 0;
        int m_SelectedCoincidentVertexCount = 0;
        HashSet<int> m_SelectedSharedVertices;
        List<int> m_SelectedCoincidentVertices;

        ElementCollection()
        {
            m_SelectedFaces = new int[0];
            m_SelectedEdges = new Edge[0];
            m_SelectedVertices = new int[0];
            m_SelectedSharedVertices = new HashSet<int>();
            m_SelectedCoincidentVertices = new List<int>();
        }

        public ElementCollection(ProBuilderMesh mesh) : this()
        {
            if(mesh == null)
                throw new ArgumentNullException("mesh");
            m_Mesh = mesh;
        }

        ProBuilderMesh mesh
        {
            get { return m_Mesh; }
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
                var lookup = mesh.sharedVertexLookup;
                m_SelectedSharedVerticesCount = 0;
                m_SelectedCoincidentVertexCount = 0;

                try
                {
                    foreach (var i in m_SelectedVertices)
                    {
                        if (m_SelectedSharedVertices.Add(lookup[i]))
                        {
                            var coincident = mesh.sharedVerticesInternal[lookup[i]];

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
        /// Get a copy of the selected face array.
        /// </summary>
        public Face[] GetSelectedFaces()
        {
            int len = m_SelectedFaces.Length;
            var selected = new Face[len];
            for (var i = 0; i < len; i++)
                selected[i] = mesh.facesInternal[m_SelectedFaces[i]];
            return selected;
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

        internal Face[] selectedFacesInternal
        {
            get { return GetSelectedFaces(); }
            set { m_SelectedFaces = value.Select(x => Array.IndexOf(mesh.facesInternal, x)).ToArray(); }
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
            return mesh.facesInternal[selectedFaceIndicesInternal[selectedFaceCount - 1]];
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
            SetSelectedFaces(selected != null ? selected.Select(x => Array.IndexOf(mesh.facesInternal, x)) : null);
        }

        // todo It shouldn't be necessary to pass in Mesh to set an attribute selection. However because of the way we
        // currently store elements (in a hierarchical structure where setting face will affect edges & vertices) it is
        // necessary.
        internal void SetSelectedFaces(IEnumerable<int> selected)
        {
            if (selected == null)
            {
                ClearSelection();
            }
            else
            {
                m_SelectedFaces = selected.ToArray();
                m_SelectedVertices = m_SelectedFaces.SelectMany(x => mesh.facesInternal[x].distinctIndexesInternal).ToArray();
                m_SelectedEdges = m_SelectedFaces.SelectMany(x => mesh.facesInternal[x].edges).ToArray();
            }

            m_SelectedCacheDirty = true;
            mesh.InvokeElementSelectionChanged();
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

            mesh.InvokeElementSelectionChanged();
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
            mesh.InvokeElementSelectionChanged();
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

        public void InvalidateCache()
        {
            m_SelectedCacheDirty = true;
        }
    }
}
