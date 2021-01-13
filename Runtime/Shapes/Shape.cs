using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{
    [System.Serializable]
    public abstract class Shape
    {
        [SerializeField]
        PivotLocation m_PivotLocation;

        [SerializeField]
        Vector3 m_PivotPosition;

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

        public PivotLocation pivotLocation
        {
            get => m_PivotLocation;
            set => m_PivotLocation = value;
        }

        public Vector3 pivotLocalPosition
        {
            get => m_PivotPosition;
            set => m_PivotPosition = value;
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
                var pivotWorldPos = mesh.transform.TransformPoint(m_PivotPosition);
                mesh.SetPivot(bbCenter);
                m_PivotPosition = mesh.transform.InverseTransformPoint(pivotWorldPos);
                UpdateBounds(mesh);
            }
        }

        public virtual void RebuildPivot(ProBuilderMesh mesh)
        {
            if(mesh != null && mesh.mesh != null)
            {
                var bbCenter = mesh.transform.TransformPoint(m_ShapeBox.center);
                var pivotWorldPos = mesh.transform.TransformPoint(m_PivotPosition);
                mesh.SetPivot(m_PivotLocation, pivotWorldPos);
                m_ShapeBox.center = mesh.transform.InverseTransformPoint(bbCenter);
                m_PivotPosition = mesh.transform.InverseTransformPoint(pivotWorldPos);
                UpdateBounds(mesh);
            }
        }

        public virtual void UpdateBounds(ProBuilderMesh mesh)
        {
            m_ShapeBox = mesh.mesh.bounds;
        }

        public void Rebuild(ProBuilderMesh mesh)
        {
            RebuildMesh(mesh);
            RebuildPivot(mesh);
        }

        public abstract void RebuildMesh(ProBuilderMesh mesh);
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
