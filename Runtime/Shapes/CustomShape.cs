using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes
{

    public class CustomShape : Shape
    {
        /// <summary>
        /// A set of 8 vertices forming the template for a custom mesh.
        /// </summary>
        [SerializeField]
        Vector3[] m_Vertices;

        /// <summary>
        /// A set of triangles forming the shape.
        /// </summary>
        [SerializeField]
        List<Face> m_Faces;

        public void SetGeometry(ProBuilderMesh mesh)
        {
            m_Vertices = mesh.positionsInternal;
            m_Faces = new List<Face>(mesh.faces);
        }

        public override void RebuildMesh(ProBuilderMesh mesh, Vector3 size)
        {
            mesh.Clear();

            Vector3[] points = new Vector3[m_Vertices.Length];

            for(int i = 0; i < m_Vertices.Length; i++)
                points[i] = Vector3.Scale(m_Vertices[i], size);

            mesh.RebuildWithPositionsAndFaces(points, m_Faces);
        }
    }
}
