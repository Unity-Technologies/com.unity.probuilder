using System;
using Unity.Collections;
using UnityEditor;
using UnityEngine.Experimental.Playables;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{
    sealed class ShapeComponent : MonoBehaviour
    {
        [Serializable]
        class ShapeBoxProperties
        {
            [SerializeField]
            internal float m_SizeX ;
            [SerializeField]
            internal float m_SizeZ ;
            [SerializeField]
            internal float m_SizeY ;
        }

        [SerializeReference]
        Shape m_Shape = new Cube();

        [SerializeField]
        Vector3 m_Size = Vector3.one;

        [SerializeField]
        Quaternion m_Rotation;

        [SerializeField]
        ShapeBoxProperties m_Properties = new ShapeBoxProperties();

        ProBuilderMesh m_Mesh;

        [SerializeField]
        PivotLocation m_PivotLocation;

        [SerializeField]
        Vector3 m_PivotPosition;

        [SerializeField]
        bool m_Edited = false;

        public Shape shape
        {
            get => m_Shape;
            set => m_Shape = value;
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

        public bool edited
        {
            get => m_Edited;
            set => m_Edited = value;
        }

        Bounds m_EditionBounds;
        public Bounds editionBounds
        {
            get
            {
                m_EditionBounds.center = m_ShapeBox.center;
                m_EditionBounds.size = m_Size;
                if(Mathf.Abs(m_ShapeBox.size.y) < Mathf.Epsilon)
                    m_EditionBounds.size = new Vector3(m_Size.x, 0f, m_Size.z);

                return m_EditionBounds;
            }
        }

        [SerializeField]
        Bounds m_ShapeBox;
        public Bounds shapeBox
        {
            get => m_ShapeBox;
            set => m_ShapeBox = value;
        }

        /// <summary>
        /// Reference to the <see cref="ProBuilderMesh"/> that this component is creating.
        /// </summary>
        public ProBuilderMesh mesh
        {
            get
            {
                if(m_Mesh == null)
                    m_Mesh = GetComponent<ProBuilderMesh>();
                if(m_Mesh == null)
                    m_Mesh = gameObject.AddComponent<ProBuilderMesh>();

                return m_Mesh;
            }
        }

        public void CopyComponent(ShapeComponent shapeComponent)
        {
            rotation = shapeComponent.rotation;
            size = shapeComponent.size;
            pivotLocation = shapeComponent.pivotLocation;
            pivotLocalPosition = shapeComponent.pivotLocalPosition;
        }

        public void SetPivotPosition(Vector3 position)
        {
            pivotLocalPosition = mesh.transform.InverseTransformPoint(position);
        }

        void UpdateProperties()
        {
            m_Properties.m_SizeX = size.x;
            m_Properties.m_SizeY = size.y;
            m_Properties.m_SizeZ = size.z;
        }

        public void UpdateComponent()
        {
            // if(pivotLocation != m_PivotLocation)
            // {
            //     m_Shape.pivotLocation = m_PivotLocation;
            //     RebuildPivot(mesh, size, rotation);
            // }else
            //If pivot is located at first corner, then take this position as a reference when changing size properties
            if(m_PivotLocation == PivotLocation.FirstCorner)
            {
                var center = m_Size / 2f;
                var newCenter = new Vector3(
                                        m_Properties.m_SizeX / 2f,
                                        m_Properties.m_SizeY / 2f,
                                        m_Properties.m_SizeZ / 2f);

                Bounds shapeBB = m_ShapeBox;
                shapeBB.center += (newCenter - center);
                m_ShapeBox = shapeBB;
            }

            //Recenter shape
            ResetPivot(mesh, size, rotation);
            size = new Vector3(m_Properties.m_SizeX, m_Properties.m_SizeY, m_Properties.m_SizeZ);
            Rebuild();
        }

        public void UpdateBounds(Bounds bounds)
        {
            var centerLocalPos = mesh.transform.InverseTransformPoint(bounds.center);
            Bounds shapeBB = m_ShapeBox;
            shapeBB.center = centerLocalPos;
            m_ShapeBox = shapeBB;

            //Recenter shape
            ResetPivot(mesh, m_Size, m_Rotation);
            size = bounds.size;
            Rebuild();
        }

        public void Rebuild(Bounds bounds, Quaternion rotation)
        {
            size = bounds.size;
            transform.position = bounds.center;
            transform.rotation = rotation;

            Rebuild();
        }

        public void Rebuild()
        {
            if(gameObject == null
            || gameObject.hideFlags == HideFlags.HideAndDontSave)
                return;

            m_ShapeBox = m_Shape.RebuildMesh(mesh, size, rotation);
            RebuildPivot(mesh, size, rotation);
            m_Edited = false;

            MeshUtility.FitToSize(mesh, m_ShapeBox, size);

            UpdateProperties();
        }

        public void SetShape(Shape shape, PivotLocation location)
        {
            m_PivotLocation = location;

            m_Shape = shape;
            if(m_Shape is Plane || m_Shape is Sprite)
            {
                Bounds bounds = m_ShapeBox;
                var newCenter = bounds.center;
                var newSize = bounds.size;
                newCenter.y = 0;
                newSize.y = 0;
                bounds.center = newCenter;
                bounds.size = newSize;
                m_ShapeBox = bounds;
            }
            //Else if coming from a 2D-state and being back to a 3D shape
            //No changes is pivot is centered
            else if(pivotLocation == PivotLocation.FirstCorner
                    && m_ShapeBox.size.y == 0 && size.y != 0)
            {
                Bounds bounds = m_ShapeBox;
                var newCenter = bounds.center;
                var newSize = bounds.size;
                newCenter.y += size.y / 2f;
                newSize.y = size.y;
                bounds.center = newCenter;
                bounds.size = newSize;
                m_ShapeBox = bounds;
            }
            ResetPivot(mesh, size, rotation);
            UpdateProperties();
            Rebuild();
        }

        /// <summary>
        /// Rotates the Shape by a given quaternion while respecting the bounds
        /// </summary>
        /// <param name="rotation">The angles to rotate by</param>
        public void RotateInsideBounds(Quaternion deltaRotation)
        {
            ResetPivot(mesh, size, rotation);
            rotation = deltaRotation * rotation;
            Rebuild();
        }

        public void ResetPivot(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            if(mesh != null && mesh.mesh != null)
            {
                var bbCenter = mesh.transform.TransformPoint(m_ShapeBox.center);
                var pivotWorldPos = mesh.transform.TransformPoint(m_PivotPosition);
                mesh.SetPivot(bbCenter);
                m_PivotPosition = mesh.transform.InverseTransformPoint(pivotWorldPos);
                m_ShapeBox = m_Shape.UpdateBounds(mesh, size, rotation, m_ShapeBox);
            }
        }

        public void RebuildPivot(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            if(mesh != null && mesh.mesh != null)
            {
                var bbCenter = mesh.transform.TransformPoint(m_ShapeBox.center);
                var pivotWorldPos = mesh.transform.TransformPoint(m_PivotPosition);
                mesh.SetPivot(m_PivotLocation, pivotWorldPos);
                m_ShapeBox.center = mesh.transform.InverseTransformPoint(bbCenter);
                m_PivotPosition = mesh.transform.InverseTransformPoint(pivotWorldPos);
                m_ShapeBox = m_Shape.UpdateBounds(mesh, size, rotation, m_ShapeBox);
            }
        }
    }
}
