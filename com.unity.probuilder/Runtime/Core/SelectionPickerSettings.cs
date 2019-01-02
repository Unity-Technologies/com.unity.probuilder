using System;

namespace UnityEngine.ProBuilder
{
    public class SceneSelection : IEquatable<SceneSelection>
    {
        public GameObject gameObject;
        public ProBuilderMesh mesh;
        public int vertex;
        public Edge edge;
        public Face face;

        public SceneSelection(GameObject gameObject = null)
        {
            this.gameObject = gameObject;
        }

        public SceneSelection(ProBuilderMesh mesh, int vertex) : this(mesh != null ? mesh.gameObject : null)
        {
            this.mesh = mesh;
            this.vertex = vertex;
            edge = Edge.Empty;
            face = null;
        }

        public SceneSelection(ProBuilderMesh mesh, Edge edge) : this(mesh != null ? mesh.gameObject : null)
        {
            this.mesh = mesh;
            vertex = -1;
            this.edge = edge;
            face = null;
        }

        public SceneSelection(ProBuilderMesh mesh, Face face) : this(mesh != null ? mesh.gameObject : null)
        {
            this.mesh = mesh;
            vertex = -1;
            edge = Edge.Empty;
            this.face = face;
        }

        public void Clear()
        {
            gameObject = null;
            mesh = null;
            face = null;
            edge = Edge.Empty;
            vertex = -1;
        }

        public void CopyTo(SceneSelection dst)
        {
            dst.gameObject = gameObject;
            dst.mesh = mesh;
            dst.face = face;
            dst.edge = edge;
            dst.vertex = vertex;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("GameObject: " + (gameObject != null ? gameObject.name : null));
            sb.AppendLine("ProBuilderMesh: " + (mesh != null ? mesh.name : null));
            sb.AppendLine("Face: " + (face != null ? face.ToString() : null));
            sb.AppendLine("Edge: " + edge.ToString());
            sb.AppendLine("Vertex: " + vertex);
            return sb.ToString();
        }

        public bool Equals(SceneSelection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(gameObject, other.gameObject)
                && Equals(mesh, other.mesh)
                && vertex == other.vertex
                && edge.Equals(other.edge)
                && Equals(face, other.face);
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
                hashCode = (hashCode * 397) ^ vertex;
                hashCode = (hashCode * 397) ^ edge.GetHashCode();
                hashCode = (hashCode * 397) ^ (face != null ? face.GetHashCode() : 0);
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
