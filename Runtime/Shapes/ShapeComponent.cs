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

        public Vector3 size
        {
            get { return m_shape.size; }
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
            var size = Math.Abs(bounds.size);
            transform.position = bounds.center;
            transform.rotation = rotation;
            m_shape.RebuildMesh(mesh, size);
            FitToSize();
        }

        public void Rebuild()
        {
            m_shape.RebuildMesh(mesh);
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
    }
}
