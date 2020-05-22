using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    public class SceneSelection : IEquatable<SceneSelection>
    {
        public GameObject gameObject;
        public ProBuilderMesh mesh;
        public List<int> vertexes;
        public List<Edge> edges;
        public List<Face> faces;

        public SceneSelection(GameObject gameObject = null)
        {
            this.gameObject = gameObject;
            vertexes = new List<int>();
            edges = new List<Edge>();
            faces = new List<Face>();
        }

        public SceneSelection(ProBuilderMesh mesh, List<int> vertexes) : this(mesh != null ? mesh.gameObject : null)
        {
            this.mesh = mesh;
            this.vertexes = vertexes;
            edges = new List<Edge>();
            faces = new List<Face>();
        }

        public SceneSelection(ProBuilderMesh mesh, List<Edge> edges) : this(mesh != null ? mesh.gameObject : null)
        {
            this.mesh = mesh;
            vertexes = new List<int>();
            this.edges = edges;
            faces = new List<Face>();
        }

        public SceneSelection(ProBuilderMesh mesh, List<Face> faces) : this(mesh != null ? mesh.gameObject : null)
        {
            this.mesh = mesh;
            vertexes = new List<int>();
            edges = new List<Edge>();
            this.faces = faces;
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
            dst.faces = faces;
            dst.edges = edges;
            dst.vertexes = vertexes;
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
                && vertexes == other.vertexes
                && edges.Equals(other.edges)
                && Equals(faces, other.faces);
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
                hashCode = (hashCode * 397) ^ vertexes.GetHashCode();
                hashCode = (hashCode * 397) ^ edges.GetHashCode();
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
