using System;

namespace UnityEngine.ProBuilder
{
    [RequireComponent(typeof(ProBuilderMesh))]
    public class ShapeComponent : MonoBehaviour
    {
        [SerializeReference]
        Shape m_shape = new Cube();

        ProBuilderMesh m_Mesh;
        Transform m_Transform;

        [HideInInspector]
        [SerializeField]
        Vector3 m_Size;

        public Vector3 size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        public ProBuilderMesh mesh
        {
            get { return m_Mesh == null ? m_Mesh = GetComponent<ProBuilderMesh>() : m_Mesh; }
        }

        public Transform transform
        {
            get { return m_Transform == null ? m_Transform = GetComponent<Transform>() : m_Transform; }
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
                positions[i].Scale(scale);

            mesh.ToMesh();
            mesh.Rebuild();
        }
    }
}
