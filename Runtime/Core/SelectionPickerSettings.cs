using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    // This should not be public until there is something meaningful that can be done with it. However it has been
    // public in the past, so we can't change it until the next major version increment.
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SceneSelection : IEquatable<SceneSelection>
    {
        public GameObject gameObject;
        public ProBuilderMesh mesh;

        List<int> m_Vertices;
        List<Edge> m_Edges;
        List<Face> m_Faces;

        public List<int> vertexes
        {
            get { return m_Vertices; }
            set { m_Vertices = value; }
        }

        public List<Edge> edges
        {
            get { return m_Edges; }
            set { m_Edges = value; }
        }

        public List<Face> faces
        {
            get { return m_Faces; }
            set { m_Faces = value; }
        }

        [Obsolete("Use SetSingleVertex")]
        public int vertex;

        [Obsolete("Use SetSingleEdge")]
        public Edge edge;
        
        [Obsolete("Use SetSingleFace")]
        public Face face;

        public SceneSelection(GameObject gameObject = null)
        {
            this.gameObject = gameObject;
            m_Vertices = new List<int>();
            m_Edges = new List<Edge>();
            m_Faces = new List<Face>();
        }

        public SceneSelection(ProBuilderMesh mesh, int vertex) : this(mesh, new List<int>() { vertex }) { }

        public SceneSelection(ProBuilderMesh mesh, Edge edge) : this(mesh, new List<Edge>() { edge }) { }

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

        public void SetSingleFace(Face face)
        {
            faces.Clear();
            faces.Add(face);
        }

        public void SetSingleVertex(int vertex)
        {
            vertexes.Clear();
            vertexes.Add(vertex);
        }

        public void SetSingleEdge(Edge edge)
        {
            edges.Clear();
            edges.Add(edge);
        }

        public void Clear()
        {
            gameObject = null;
            mesh = null;
            faces.Clear();
            edges.Clear();
            vertexes.Clear();
        }

        public void CopyTo(SceneSelection dst)
        {
            dst.gameObject = gameObject;
            dst.mesh = mesh;
            dst.faces.Clear();
            dst.edges.Clear();
            dst.vertexes.Clear();
            foreach (var x in faces)
                dst.faces.Add(x);
            foreach (var x in edges)
                dst.edges.Add(x);
            foreach (var x in vertexes)
                dst.vertexes.Add(x);
        }

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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SceneSelection)obj);
        }

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

        public static bool operator==(SceneSelection left, SceneSelection right)
        {
            return Equals(left, right);
        }

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
