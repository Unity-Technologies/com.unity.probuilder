using System;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder
{
    [RequireComponent(typeof(ProBuilderMesh))]
    public class ShapeComponent : MonoBehaviour
    {
        [SerializeReference]
        Shape m_shape = new Cube();

        ProBuilderMesh m_Mesh;

        [HideInInspector]
        [SerializeField]
        Vector3 m_Size;

        [HideInInspector]
        [SerializeField]
        Matrix4x4 m_RotationMatrix = Matrix4x4.identity;

        public Vector3 size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

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
            size = Math.Abs(bounds.size);
            transform.position = bounds.center;
            transform.rotation = rotation;
            Rebuild();
        }

        public void Rebuild()
        {
            m_shape.RebuildMesh(mesh, size);
            Rotate();
            FitToSize();
         
        }

        public void SetShape(Type type)
        {
            if (type.IsAssignableFrom(typeof(Shape)))
                throw new ArgumentException("Type needs to derive from Shape");

            m_shape = Activator.CreateInstance(type) as Shape;
            Rebuild();
        }

        public void SetShape<T>() where T : Shape, new()
        {
            SetShape(typeof(T));
        }

        // Assumes that mesh origin is {0,0,0}
        void FitToSize()
        {
            if (mesh.vertexCount < 1)
                return;

            var scale = size.DivideBy(mesh.mesh.bounds.size);
            var positions = mesh.positionsInternal;

            if (System.Math.Abs(size.x) < 0.01f)
                scale.x = 0;
            if (System.Math.Abs(size.y) < 0.01f)
                scale.y = 0;
            if (System.Math.Abs(size.z) < 0.01f)
                scale.z = 0;

            for (int i = 0, c = mesh.vertexCount; i < c; i++)
            {
                positions[i] -= mesh.mesh.bounds.center;
                positions[i].Scale(scale);
            }

            mesh.ToMesh();
            mesh.Rebuild();
        }

        public void Rotate(Vector3 eulerAngles)
        {
            Quaternion rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
            m_RotationMatrix = Matrix4x4.Rotate(rotation);

            Rotate();
            FitToSize();
        }

        private void Rotate()
        {
            if (m_RotationMatrix == Matrix4x4.identity)
            {
                return;
            }
            Vector3[] origVerts;
            Vector3[] newVerts;

            origVerts = mesh.mesh.vertices;
            newVerts = new Vector3[origVerts.Length];

            int i = 0;
            while (i < origVerts.Length)
            {
                newVerts[i] = m_RotationMatrix.MultiplyPoint3x4(origVerts[i]);
                i++;
            }
            mesh.mesh.vertices = newVerts;
            mesh.GeometryWithPoints(newVerts);
        }
    }
}
