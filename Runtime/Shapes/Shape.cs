using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{
    [System.Serializable]
    public abstract class Shape
    {
        [SerializeField]
        Vector3 m_Size = Vector3.one;

        [SerializeField]
        Quaternion m_Rotation = Quaternion.identity;

        [SerializeField]
        protected Bounds m_ShapeBox = new Bounds();

        public Vector3 size
        {
            get => m_Size;
            set => m_Size = value;
        }

        public Quaternion rotation
        {
            get => m_Rotation;
            set => m_Rotation = value;
        }

        public Bounds shapeBox
        {
            get => m_ShapeBox;
            set => m_ShapeBox = value;
        }

        public void CopyShapeParameters(Shape sourceShape)
        {
            m_Size = sourceShape.m_Size;
            m_Rotation = sourceShape.m_Rotation;
            m_ShapeBox = sourceShape.m_ShapeBox;
        }

        public virtual void ResetPivot(ProBuilderMesh mesh)
        {
            if(mesh != null && mesh.mesh != null)
            {
                var bbCenter = mesh.transform.TransformPoint(m_ShapeBox.center);
                mesh.SetPivot(bbCenter);
                UpdateBounds(mesh);
            }
        }

        public virtual void UpdatePivot(ProBuilderMesh mesh, PivotLocation pivotLocation)
        {
            if(mesh != null && mesh.mesh != null)
            {
                var bbCenter = mesh.transform.TransformPoint(m_ShapeBox.center);
                mesh.SetPivot(pivotLocation);
                m_ShapeBox.center = mesh.transform.InverseTransformPoint(bbCenter);

                UpdateBounds(mesh);
            }
        }

        public virtual void UpdateBounds(ProBuilderMesh mesh)
        {
            m_ShapeBox = mesh.mesh.bounds;
        }

        public abstract void RebuildMesh(ProBuilderMesh mesh, Vector3 meshSize);
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ShapeAttribute : System.Attribute
    {
        public string name;

        public ShapeAttribute(string n)
        {
            name = n;
        }
    }
}
