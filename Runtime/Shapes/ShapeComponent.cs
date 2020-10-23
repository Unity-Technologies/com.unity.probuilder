using System;
using System.ComponentModel;
using System.Linq;

namespace UnityEngine.ProBuilder.Shapes
{
    [RequireComponent(typeof(ProBuilderMesh))]
    public sealed class ShapeComponent : MonoBehaviour
    {
        [SerializeReference]
        Shape m_Shape = new Cube();

        ProBuilderMesh m_Mesh;

        [SerializeField]
        Vector3 m_Size;

        [SerializeField]
        Vector3[] m_MeshOriginalVertices;

        [SerializeField]
        Quaternion m_Rotation = Quaternion.identity;

        [SerializeField]
        bool m_Edited = false;

        public Shape shape
        {
            get { return m_Shape; }
            set { m_Shape = value; }
        }

        public Quaternion rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }

        public Vector3 size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        public bool edited
        {
            get => m_Edited;
            set => m_Edited = value;
        }

        /// <summary>
        /// Reference to the <see cref="ProBuilderMesh"/> that this component is creating.
        /// </summary>
        public ProBuilderMesh mesh
        {
            get { return m_Mesh == null ? m_Mesh = GetComponent<ProBuilderMesh>() : m_Mesh; }
        }

        // Bounds where center is in world space, size is mesh.bounds.size
        internal Bounds meshFilterBounds
        {
            get
            {
                var mb = mesh.mesh.bounds;
                return new Bounds(transform.TransformPoint(mb.center), mb.size);
            }
        }

        public void Rebuild(Bounds bounds, Quaternion rotation)
        {
            m_Size = Math.Abs(bounds.size);
            transform.position = bounds.center;
            transform.rotation = rotation;
            Rebuild();
        }

        public void Rebuild()
        {
            m_Shape.RebuildMesh(mesh, m_Size);
            m_MeshOriginalVertices = new Vector3[mesh.vertexCount];
            Array.Copy(mesh.positionsInternal,m_MeshOriginalVertices, mesh.vertexCount);
            m_Edited = false;
            Quaternion rotation = m_Rotation;
            ApplyRotation(Quaternion.identity);
            ApplyRotation(rotation);
            MeshUtility.FitToSize(mesh, m_Size);
        }

        public void SetShape(Shape shape)
        {
            m_Shape = shape;
            Rebuild();
        }

        /// <summary>
        /// Set the rotation of the Shape to a given quaternion, then rotates it while respecting the bounds
        /// </summary>
        /// <param name="angles">The angles to rotate by</param>
        public void SetRotation(Quaternion angles)
        {
            ApplyRotation(angles);
            MeshUtility.FitToSize(mesh, m_Size);
        }

        /// <summary>
        /// Rotates the Shape by a given quaternion while respecting the bounds
        /// </summary>
        /// <param name="rotation">The angles to rotate by</param>
        public void Rotate(Quaternion deltaRotation)
        {
            Quaternion rotation = deltaRotation * m_Rotation;
            ApplyRotation(rotation);
            MeshUtility.FitToSize(mesh, m_Size);
        }

        private void ApplyRotation(Quaternion rotation)
        {
            if ( rotation.Equals(m_Rotation) )
                return;

            //m_Shape.RebuildMesh(mesh, m_Size);
            m_Rotation = rotation;
            m_Edited = false;

            //var origVerts = mesh.positionsInternal;

            var origVerts = new Vector3[m_MeshOriginalVertices.Length];
            Array.Copy(m_MeshOriginalVertices, origVerts, m_MeshOriginalVertices.Length);

            // The Shape is flipped, pointing downwards, the rotation must be inverted
            if(transform.up.y < 0)
                rotation = Quaternion.Inverse(rotation);

            for (int i = 0; i < origVerts.Length; ++i)
            {
                origVerts[i] = rotation * origVerts[i];
            }
            mesh.mesh.vertices = origVerts;
            mesh.ReplaceVertices(origVerts);
        }

    }
}
