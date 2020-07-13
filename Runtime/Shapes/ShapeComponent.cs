using System;

namespace UnityEngine.ProBuilder
{
    [RequireComponent(typeof(ProBuilderMesh))]
    public class ShapeComponent : MonoBehaviour
    {
        [SerializeReference]
        Shape m_shape = new Cube();

        public Shape m_Shape => m_shape;

        ProBuilderMesh m_Mesh;

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
            RotateBy(m_RotationMatrix);
            FitToSize();
        }

        public void SetShape(Type type)
        {
            if (type.IsAssignableFrom(typeof(Shape)))
                throw new ArgumentException("Type needs to derive from Shape");

            m_shape = Activator.CreateInstance(type) as Shape;
            m_shape.SetToLastParams();
            Rebuild();
        }

        public void SetShape(Shape shape)
        {
            m_shape = shape;
            m_shape.SetToLastParams();
            Rebuild();
        }

        public void SetShape<T>() where T : Shape, new()
        {
            SetShape(typeof(T));
        }

        void FitToSize()
        {
            if (mesh.vertexCount < 1)
                return;

            var scale = size.DivideBy(mesh.mesh.bounds.size);
            var positions = mesh.positionsInternal;

            if (System.Math.Abs(mesh.mesh.bounds.size.x) < 0.01f)
                scale.x = 0;
            if (System.Math.Abs(mesh.mesh.bounds.size.y) < 0.01f)
                scale.y = 0;
            if (System.Math.Abs(mesh.mesh.bounds.size.z) < 0.01f)
                scale.z = 0;

            for (int i = 0, c = mesh.vertexCount; i < c; i++)
            {
                positions[i] -= mesh.mesh.bounds.center;
                positions[i].Scale(scale);
            }

            mesh.ToMesh();
            mesh.Rebuild();
        }

        /// <summary>
        /// Rotates the Shape by a given set of eular angles
        /// </summary>
        /// <param name="eulerAngles">The angles to rotate by</param>
        public void RotateBy(Vector3 eulerAngles, bool reset = false)
        {
            if (reset)
            {
                m_RotationMatrix = m_RotationMatrix.inverse * m_RotationMatrix;
            }
            Quaternion rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
            var matrix = Matrix4x4.Rotate(rotation);
            m_RotationMatrix = matrix * m_RotationMatrix;

            RotateBy(matrix);
            FitToSize();
        }

        void RotateBy(Matrix4x4 matrix)
        {
            if (matrix == Matrix4x4.identity)
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
                newVerts[i] = matrix.MultiplyPoint3x4(origVerts[i]);
                i++;
            }
            mesh.mesh.vertices = newVerts;
            mesh.ReplaceVertices(newVerts);
        }
    }
}
