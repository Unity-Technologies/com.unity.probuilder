using System;
using UnityEditor;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{
    sealed class ShapeComponent : MonoBehaviour
    {
        [Serializable]
        class ShapeBoxProperties
        {
            [SerializeField]
            internal float m_Width ;
            [SerializeField]
            internal float m_Length ;
            [SerializeField]
            internal float m_Height ;
        }

        [SerializeReference]
        Shape m_Shape = new Cube();

        [SerializeField]
        ShapeBoxProperties m_Properties = new ShapeBoxProperties();

        [SerializeField]
        PivotLocation m_PivotLocation;

        ProBuilderMesh m_Mesh;

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
            set
            {
                m_PivotLocation = value;
                Rebuild();
            }
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

        void UpdateProperties()
        {
            m_Properties.m_Width = size.x;
            m_Properties.m_Height = size.y;
            m_Properties.m_Length = size.z;
        }

        public void UpdateComponent()
        {
            //Recenter shape
            m_Shape.ResetPivot(mesh);
            size = new Vector3(m_Properties.m_Width, m_Properties.m_Height, m_Properties.m_Length);
            Rebuild();
        }

        public void Rebuild(Bounds bounds, Quaternion rotation)
        {
            size = Math.Abs(bounds.size);
            transform.position = bounds.center;
            transform.rotation = rotation;

            Rebuild();
        }

        public void Rebuild(bool resetRotation = false)
        {
            if(gameObject == null || gameObject.hideFlags != HideFlags.None)
            {
                UpdateProperties();
                return;
            }

            m_Shape.RebuildMesh(mesh, size);
            m_Edited = false;

            Quaternion rot = resetRotation ? Quaternion.identity : rotation;
            ApplyRotation(rot, true);

            MeshUtility.FitToSize(mesh, GetRotatedBounds(), size);
            m_Shape.UpdatePivot(mesh, pivotLocation);

            UpdateProperties();
        }

        public void SetShape(Shape shape)
        {
            m_Shape = shape;
            m_Shape.ResetPivot(mesh);
            Rebuild();
        }

        Bounds GetRotatedBounds()
        {
            Bounds bounds = m_Shape.shapeBox;
            bounds.size = Math.Abs(rotation * m_Shape.shapeBox.size);
            return bounds;
        }

        /// <summary>
        /// Set the rotation of the Shape to a given quaternion, then rotates it while respecting the bounds
        /// </summary>
        /// <param name="angles">The angles to rotate by</param>
        public void SetInnerBoundsRotation(Quaternion angles, PivotLocation pivotLocation)
        {
            rotation = angles;
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

        void ApplyRotation(Quaternion rot, bool forceRotation = false)
        {
            if ( !forceRotation && rot.Equals(rotation) )
                return;

            rotation = rot;
            m_Edited = false;

            var origVerts = mesh.positionsInternal;

            for(int i = 0; i < origVerts.Length; ++i)
            {
                origVerts[i] = rotation * origVerts[i];
            }

            mesh.positions = origVerts;
            mesh.ToMesh();
            mesh.Refresh();
        }
    }
}
