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

        ProBuilderMesh m_Mesh;

        [SerializeField]
        bool m_Edited = false;

        public Shape shape
        {
            get { return m_Shape; }
            set { m_Shape = value; }
        }

        Vector3 m_Size
        {
            get { return m_Shape.size; }
            set { m_Shape.size = value; }
        }

        public Vector3 Size => m_Size;

        public Quaternion rotation
        {
            get { return m_Shape.rotation; }
            set { m_Shape.rotation = value; }
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
            m_Properties.m_Width = m_Size.x;
            m_Properties.m_Height = m_Size.y;
            m_Properties.m_Length = m_Size.z;
        }

        public void UpdateComponent(PivotLocation pivotLocation)
        {
            //Recenter shape
            m_Shape.ResetPivot(mesh);
            m_Size = new Vector3(m_Properties.m_Width, m_Properties.m_Height, m_Properties.m_Length);
            Rebuild(pivotLocation);
        }

        public void Rebuild(Bounds bounds, Quaternion rotation, PivotLocation pivotLocation)
        {
            m_Size = Math.Abs(bounds.size);
            transform.position = bounds.center;
            transform.rotation = rotation;

            Rebuild(pivotLocation);
        }

        public void Rebuild(PivotLocation pivotLocation, bool resetRotation = false)
        {
            if( gameObject== null ||gameObject.hideFlags != HideFlags.None )
                return;

            m_Shape.RebuildMesh(mesh, m_Size);
            m_Edited = false;

            Quaternion rot = resetRotation ? Quaternion.identity : rotation;
            ApplyRotation(rot, true);

            MeshUtility.FitToSize(mesh, GetRotatedBounds(), m_Size);
            m_Shape.UpdatePivot(mesh, pivotLocation);

            UpdateProperties();
        }

        public void SetShape(Shape shape, PivotLocation pivotLocation)
        {
            m_Shape = shape;
            m_Shape.ResetPivot(mesh);
            Rebuild(pivotLocation);
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
        public void RotateInsideBounds(Quaternion deltaRotation, PivotLocation pivotLocation)
        {
            m_Shape.ResetPivot(mesh);
            rotation = deltaRotation * rotation;
            Rebuild(pivotLocation);
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
