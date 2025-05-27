using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using System.Linq;

public class ProbuilderMeshDegenerateTriangleTest
{
    private ProBuilderMesh m_Mesh;

    [SetUp]
    public void SetUp()
    {
        // Create a Probuilder cube
        m_Mesh = ShapeFactory.Instantiate(typeof(Cube));
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the created GameObject
        GameObject.DestroyImmediate(m_Mesh.gameObject);
    }

    [Test]
    public void TestNormalsWithDegenerateTriangles()
    {
        // Collapse two of the shared vertices together.
        var positions = new List<Vector3>(m_Mesh.positions);
        Vector3 svPosition = positions[m_Mesh.sharedVertices[0][0]];
        for (int v = 0; v < m_Mesh.sharedVertices[1].Count; ++v)
        {
            positions[m_Mesh.sharedVertices[1][v]] = svPosition;
        }
        m_Mesh.positions = positions.ToList();
        m_Mesh.Rebuild();

        // Check all the normals in use by faces to ensure none have invalid values.
        foreach (int index in m_Mesh.mesh.triangles)
        {
            var normal = m_Mesh.normals[index];
            Assert.IsFalse(float.IsNaN(normal.x) || float.IsNaN(normal.y) || float.IsNaN(normal.z), "Normals should not contain NaN values.");
            Assert.IsFalse(float.IsInfinity(normal.x) || float.IsInfinity(normal.y) || float.IsInfinity(normal.z), "Normals should not contain Infinite values.");
            Assert.IsFalse(normal == Vector3.zero, "Normals should not be zero vectors.");
        }
    }
}
