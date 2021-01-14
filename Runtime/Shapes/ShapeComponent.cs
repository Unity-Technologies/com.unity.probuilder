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
        ShapeBoxProperties m_Properties = new ShapeBoxProperties();

        ProBuilderMesh m_Mesh;

        [SerializeField]
        PivotLocation m_PivotLocation;

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
        }

        public Vector3 size
        {
            get => m_Shape.size;
            set => m_Shape.size = value;
        }

        public Quaternion rotation
        {
            get => m_Shape.rotation;
            set => m_Shape.rotation = value;
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
                m_EditionBounds.center = m_Shape.shapeBox.center;
                m_EditionBounds.size = m_Shape.size;
                if(Mathf.Abs(m_Shape.shapeBox.size.y) < Mathf.Epsilon)
                    m_EditionBounds.size = new Vector3(m_Shape.size.x, 0f, m_Shape.size.z);

                return m_EditionBounds;
            }
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

        public void SetPivotPosition(Vector3 position)
        {
            m_Shape.pivotLocalPosition = m_Mesh.transform.InverseTransformPoint(position);
        }

        void UpdateProperties()
        {
            m_Properties.m_SizeX = size.x;
            m_Properties.m_SizeY = size.y;
            m_Properties.m_SizeZ = size.z;
        }

        public void UpdateComponent()
        {
            if(m_Shape.pivotLocation != m_PivotLocation)
            {
                m_Shape.pivotLocation = m_PivotLocation;
                m_Shape.RebuildPivot(mesh);
            }else
            //If pivot is located at first corner, then take this position as a reference when changing size properties
            if(m_Shape.pivotLocation == PivotLocation.FirstCorner)
            {
                var center = m_Shape.size / 2f;
                var newCenter = new Vector3(
                                        m_Properties.m_SizeX / 2f,
                                        m_Properties.m_SizeY / 2f,
                                        m_Properties.m_SizeZ / 2f);

                Bounds shapeBB = m_Shape.shapeBox;
                shapeBB.center += (newCenter - center);
                m_Shape.shapeBox = shapeBB;
            }

            //Recenter shape
            m_Shape.ResetPivot(mesh);
            size = new Vector3(m_Properties.m_SizeX, m_Properties.m_SizeY, m_Properties.m_SizeZ);
            Rebuild();
        }

        public void UpdateBounds(Bounds bounds)
        {
            var centerLocalPos = m_Mesh.transform.InverseTransformPoint(bounds.center);
            Bounds shapeBB = m_Shape.shapeBox;
            shapeBB.center = centerLocalPos;
            m_Shape.shapeBox = shapeBB;

            //Recenter shape
            m_Shape.ResetPivot(m_Mesh);
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
            if(gameObject == null || gameObject.hideFlags != HideFlags.None)
            {
                UpdateProperties();
                return;
            }

            m_Shape.Rebuild(mesh);
            m_Edited = false;

            Bounds bounds = m_Shape.shapeBox;
            bounds.size = Math.Abs(m_Shape.shapeBox.size);
            MeshUtility.FitToSize(mesh, bounds, size);

            UpdateProperties();
        }

        public void SetShape(Shape shape, PivotLocation location)
        {
            m_PivotLocation = location;
            shape.pivotLocalPosition = m_Shape.pivotLocalPosition;
            shape.pivotLocation = location;

            m_Shape = shape;
            if(m_Shape is Plane || m_Shape is Sprite)
            {
                Bounds bounds = m_Shape.shapeBox;
                var newCenter = bounds.center;
                var newSize = bounds.size;
                newCenter.y = 0;
                newSize.y = 0;
                bounds.center = newCenter;
                bounds.size = newSize;
                m_Shape.shapeBox = bounds;
            }
            //Else if coming from a 2D-state and being back to a 3D shape
            //No changes is pivot is centered
            else if(pivotLocation == PivotLocation.FirstCorner
                    && m_Shape.shapeBox.size.y == 0 && size.y != 0)
            {
                Bounds bounds = m_Shape.shapeBox;
                var newCenter = bounds.center;
                var newSize = bounds.size;
                newCenter.y += size.y / 2f;
                newSize.y = size.y;
                bounds.center = newCenter;
                bounds.size = newSize;
                m_Shape.shapeBox = bounds;
            }
            m_Shape.ResetPivot(mesh);
            Rebuild();
        }

        /// <summary>
        /// Rotates the Shape by a given quaternion while respecting the bounds
        /// </summary>
        /// <param name="rotation">The angles to rotate by</param>
        public void RotateInsideBounds(Quaternion deltaRotation)
        {
            m_Shape.ResetPivot(mesh);
            rotation = deltaRotation * rotation;
            Rebuild();
        }
    }
}
