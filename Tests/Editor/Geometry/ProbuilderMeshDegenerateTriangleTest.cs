using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using System.Linq;

public class ProbuilderMeshDegenerateTriangleTest
{
    [Test]
    public void TestNormalsWithDegenerateTriangles()
    {
        // Create a Probuilder cube
        var mesh = ShapeFactory.Instantiate(typeof(Cube));
        try
        {
            // Collapse two of the shared vertices together.
            var positions = new List<Vector3>(mesh.positions);
            Vector3 svPosition = positions[mesh.sharedVertices[0][0]];
            for (int v = 0; v < mesh.sharedVertices[1].Count; ++v)
            {
                positions[mesh.sharedVertices[1][v]] = svPosition;
            }

            mesh.positions = positions.ToList();
            mesh.Rebuild();

            // Check all the normals in use by faces to ensure none have invalid values.
            foreach (int index in mesh.mesh.triangles)
            {
                var normal = mesh.normals[index];
                Assert.IsFalse(float.IsNaN(normal.x) || float.IsNaN(normal.y) || float.IsNaN(normal.z), "Normals should not contain NaN values.");
                Assert.IsFalse(float.IsInfinity(normal.x) || float.IsInfinity(normal.y) || float.IsInfinity(normal.z), "Normals should not contain Infinite values.");
                Assert.IsFalse(normal == Vector3.zero, "Normals should not be zero vectors.");
            }
        }
        finally
        {
            // Clean up the created GameObject
            GameObject.DestroyImmediate(mesh.gameObject);
        }
    }

    [Test]
    [Description("PBLD-251 : IndexOutOfRangeException appears in the Console when exporting certain ProBuilder meshes")]
    public void TestOriginalDoorBugScenario()
    {
        // Recreate the specific Door shape bug scenario
        var door = ShapeFactory.Instantiate(typeof(Door));

        try
        {
            Assert.DoesNotThrow(() => door.ToMesh(MeshTopology.Quads));

            Assert.IsNotNull(door.mesh);
            Assert.Greater(door.mesh.vertexCount, 0);
        }
        finally
        {
            GameObject.DestroyImmediate(door.gameObject);
        }
    }
}
