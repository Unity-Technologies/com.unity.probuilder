using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    // This should not be public until there is something meaningful that can be done with it. However it has been
    // public in the past, so we can't change it until the next major version increment.
    /// <summary>
    /// Manages object and element selection in the scene.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SceneSelection : IEquatable<SceneSelection>
    {
        /// <summary>The Unity GameObject</summary>
        public GameObject gameObject;
        /// <summary>The ProBuilder mesh</summary>
        public ProBuilderMesh mesh;

        List<int> m_Vertices;
        List<Edge> m_Edges;
        List<Face> m_Faces;

        /// <summary>
        /// Gets or sets the list of vertex indices for the selected mesh.
        /// </summary>
        public List<int> vertexes
        {
            get { return m_Vertices; }
            set { m_Vertices = value; }
        }

        /// <summary>
        /// Gets or sets the list of edges for the selected mesh.
        /// </summary>
        public List<Edge> edges
        {
            get { return m_Edges; }
            set { m_Edges = value; }
        }

        /// <summary>
        /// Gets or sets the list of faces for the selected mesh.
        /// </summary>
        public List<Face> faces
        {
            get { return m_Faces; }
            set { m_Faces = value; }
        }

        /// <summary>Obsolete. Use `SetSingleVertex` instead.</summary>
        [Obsolete("Use SetSingleVertex")]
        public int vertex;

        /// <summary>Obsolete. Use `SetSingleEdge` instead.</summary>
        [Obsolete("Use SetSingleEdge")]
        public Edge edge;

        /// <summary>Obsolete. Use `SetSingleFace` instead.</summary>
        [Obsolete("Use SetSingleFace")]
        public Face face;

        /// <summary>
        /// Creates a SceneSelection object in the [Object editing mode](../manual/modes.html) from the
        /// specified GameObject. If the GameObject is not specified it creates an empty selection.
        /// </summary>
        /// <param name="gameObject">The optional GameObject to set as the SceneSelection.</param>
        public SceneSelection(GameObject gameObject = null)
        {
            this.gameObject = gameObject;
            m_Vertices = new List<int>();
            m_Edges = new List<Edge>();
            m_Faces = new List<Face>();
        }

        /// <summary>
        /// Creates a SceneSelection object in the [Vertex editing mode](../manual/modes.html) from the specified mesh.
        /// </summary>
        /// <param name="mesh">The ProBuilderMesh containing the vertex to select.</param>
        /// <param name="vertex">The index of the vertex to set as the SceneSelection.</param>
        public SceneSelection(ProBuilderMesh mesh, int vertex) : this(mesh, new List<int>() { vertex }) { }

        /// <summary>
        /// Creates a SceneSelection object in the [Edge editing mode](../manual/modes.html) from the specified mesh.
        /// </summary>
        /// <param name="mesh">The ProBuilderMesh containing the edge to select.</param>
        /// <param name="edge">The Edge to set as the SceneSelection.</param>
        public SceneSelection(ProBuilderMesh mesh, Edge edge) : this(mesh, new List<Edge>() { edge }) { }

        /// <summary>
        /// Creates a SceneSelection object in the [Face editing mode](../manual/modes.html) from the specified mesh.
        /// </summary>
        /// <param name="mesh">The ProBuilderMesh containing the face to select.</param>
        /// <param name="face">The Face to set as the SceneSelection.</param>
        public SceneSelection(ProBuilderMesh mesh, Face face) : this(mesh, new List<Face>() { face }) { }

        internal SceneSelection(ProBuilderMesh mesh, List<int> vertexes) : this(mesh != null ? mesh.gameObject : null)
        {
            this.mesh = mesh;
            m_Vertices = vertexes;
            m_Edges = new List<Edge>();
            m_Faces = new List<Face>();
        }

        internal SceneSelection(ProBuilderMesh mesh, List<Edge> edges) : this(mesh != null ? mesh.gameObject : null)
        {
            this.mesh = mesh;
            vertexes = new List<int>();
            this.edges = edges;
            faces = new List<Face>();
        }

        internal SceneSelection(ProBuilderMesh mesh, List<Face> faces) : this(mesh != null ? mesh.gameObject : null)
        {
            this.mesh = mesh;
            vertexes = new List<int>();
            edges = new List<Edge>();
            this.faces = faces;
        }

        /// <summary>
        /// Resets the selection to the specified face.
        /// </summary>
        /// <param name="face">The face to select</param>
        public void SetSingleFace(Face face)
        {
            faces.Clear();
            faces.Add(face);
        }

        /// <summary>
        /// Resets the selection to the specified vertex.
        /// </summary>
        /// <param name="vertex">The index of the vertex to select</param>
        public void SetSingleVertex(int vertex)
        {
            vertexes.Clear();
            vertexes.Add(vertex);
        }

        /// <summary>
        /// Resets the selection to the specified edge.
        /// </summary>
        /// <param name="edge">The edge to select</param>
        public void SetSingleEdge(Edge edge)
        {
            edges.Clear();
            edges.Add(edge);
        }

        /// <summary>
        /// Empties the selection.
        /// </summary>
        public void Clear()
        {
            gameObject = null;
            mesh = null;
            faces.Clear();
            edges.Clear();
            vertexes.Clear();
        }

        /// <summary>
        /// Copies the list of selected object(s) and element(s) to match this SceneSelection object.
        /// </summary>
        /// <param name="dst">The SceneSelection object to copy this object to.</param>
        public void CopyTo(SceneSelection dst)
        {
            dst.gameObject = gameObject;
            dst.mesh = mesh;
            dst.faces.Clear();
            dst.edges.Clear();
            dst.vertexes.Clear();
            dst.faces.AddRange(faces);
            dst.edges.AddRange(edges);
            dst.vertexes.AddRange(vertexes);
        }

        /// <summary>
        /// Returns a string that represents this SceneSelection.
        /// </summary>
        /// <returns>A multi-line string containing the names of the GameObject and ProBuilderMesh objects, and a string representation of the lists of faces, edges, and vertex indices.</returns>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("GameObject: " + (gameObject != null ? gameObject.name : null));
            sb.AppendLine("ProBuilderMesh: " + (mesh != null ? mesh.name : null));
            sb.AppendLine("Face: " + (faces != null ? faces.ToString() : null));
            sb.AppendLine("Edge: " + edges.ToString());
            sb.AppendLine("Vertex: " + vertexes);
            return sb.ToString();
        }

        /// <summary>
        /// Evaluates whether the specified SceneSelection is equivalent to this one.
        /// </summary>
        /// <param name="other">The SceneSelection object to compare to this object.</param>
        /// <returns>True if the objects are equivalent; false otherwise.</returns>
        public bool Equals(SceneSelection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(gameObject, other.gameObject)
                && Equals(mesh, other.mesh)
                && Enumerable.SequenceEqual(vertexes, other.vertexes)
                && Enumerable.SequenceEqual(edges, other.edges)
                && Enumerable.SequenceEqual(faces, other.faces);
        }

        /// <summary>
        /// Evaluates whether the specified object is equivalent to this one.
        /// </summary>
        /// <param name="obj">The object to compare to this object.</param>
        /// <returns>True if the objects are equivalent; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SceneSelection)obj);
        }

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (gameObject != null ? gameObject.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (mesh != null ? mesh.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (vertexes != null ? vertexes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (edges != null ? edges.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (faces != null ? faces.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Returns true if the two SceneSelection objects are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if the objects are equal; false otherwise.</returns>
        public static bool operator==(SceneSelection left, SceneSelection right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns true if the two SceneSelection objects are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if the objects are not equal; false otherwise.</returns>
        public static bool operator!=(SceneSelection left, SceneSelection right)
        {
            return !Equals(left, right);
        }
    }

    struct VertexPickerEntry
    {
        public ProBuilderMesh mesh;
        public int vertex;
        public float screenDistance;
        public Vector3 worldPosition;
    }
}
