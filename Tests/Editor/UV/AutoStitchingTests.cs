using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;

[TestFixture]
public class AutoStitchTests
{
    const float k_Tolerance = 0.0001f;
    [Test]
    public void AutoStitch_AlignsEdgesCorrectly()
    {
        // Step 1: Create a cube and deform one face
        var cube = ShapeFactory.Instantiate<Cube>();
        Assume.That(cube, Is.Not.Null);

        var f0 = cube.faces[0]; // idx : 0,1,2,3
        var f1 = cube.faces[1]; // idx : 4,5,6,7
        var vertices = cube.positionsInternal;

        // let's modify the non adjacent edge
        vertices[5] += new Vector3(0, -0.5f, 0);
        vertices[8] += new Vector3(0, -0.5f, 0);
        vertices[7] += new Vector3(0, 0.5f, 0);
        vertices[10] += new Vector3(0, 0.5f, 0);
        cube.positionsInternal = vertices;

        cube.ToMesh();
        cube.Refresh();

        // Step 2: Perform AutoStitch
        bool succeeded = UVEditing.AutoStitch(cube, f0, f1, 0);
        Assert.IsTrue(succeeded, "AutoStitch operation failed.");

        // Step 3: Verify that the edge of one face UV is the same as the other face UV
        var uvs = cube.texturesInternal;

        // Get the shared edge between f0 and f1
        var sharedEdge = WingedEdge.GetWingedEdges(cube, new[] { f0, f1 })
            .FirstOrDefault(x => x.face == f0 && x.opposite != null && x.opposite.face == f1);

        Assume.That(sharedEdge, Is.Not.Null, "No shared edge found between the two faces.");

        // Check if UVs on the shared edge are aligned
        var f0Edge = sharedEdge.opposite.edge.common;
        var f1Edge = sharedEdge.opposite.edge.local;

        // Compare UVs for each vertex in the shared edge
        AssertUVsAlmostEqual(uvs[f0Edge.a], uvs[f1Edge.a], k_Tolerance, $"UV mismatch at vertex {f0Edge.a}.");
        AssertUVsAlmostEqual(uvs[f0Edge.b], uvs[f1Edge.b], k_Tolerance, $"UV mismatch at vertex {f0Edge.b}.");

        // Cleanup
        Object.DestroyImmediate(cube.gameObject);
    }

    [Test]
    public void AutoStitch_AlignsEdgesCorrectly_WhenRotated()
    {
        // Step 1: Create a cube
        var cube = ShapeFactory.Instantiate<Cube>();
        Assume.That(cube, Is.Not.Null);

        // Step 2: Rotate all faces of the cube
        var rotation = Quaternion.Euler(45, 45, 45);
        var vertices = cube.positionsInternal;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = rotation * vertices[i];
        }

        cube.positionsInternal = vertices;
        cube.ToMesh();
        cube.Refresh();

        // Deform one face slightly to ensure edge misalignment
        var f0 = cube.faces[0]; // idx : 0,1,2,3
        var f1 = cube.faces[1]; // idx : 4,5,6,7

        // let's modify the non adjacent edge
        vertices[5] += new Vector3(0, -0.5f, 0);
        vertices[8] += new Vector3(0, -0.5f, 0);
        vertices[7] += new Vector3(0, 0.5f, 0);
        vertices[10] += new Vector3(0, 0.5f, 0);
        cube.positionsInternal = vertices;

        cube.ToMesh();
        cube.Refresh();

        // Step 3: Perform AutoStitch
        bool succeeded = UVEditing.AutoStitch(cube, f0, f1, 0);
        Assert.IsTrue(succeeded, "AutoStitch operation failed.");

        // Step 4: Verify that the edge of one face UV is the same as the other face UV
        var uvs = cube.texturesInternal;

        // Get the shared edge between f0 and f1
        var sharedEdge = WingedEdge.GetWingedEdges(cube, new[] { f0, f1 })
            .FirstOrDefault(x => x.face == f0 && x.opposite != null && x.opposite.face == f1);

        Assume.That(sharedEdge, Is.Not.Null, "No shared edge found between the two faces.");

        // Check if UVs on the shared edge are aligned
        var f0Edge = sharedEdge.opposite.edge.common;
        var f1Edge = sharedEdge.opposite.edge.local;

        // Compare UVs for each vertex in the shared edge
        AssertUVsAlmostEqual(uvs[f0Edge.a], uvs[f1Edge.a], k_Tolerance, $"UV mismatch at vertex {f0Edge.a}.");
        AssertUVsAlmostEqual(uvs[f0Edge.b], uvs[f1Edge.b], k_Tolerance, $"UV mismatch at vertex {f0Edge.b}.");

        // Cleanup
        Object.DestroyImmediate(cube.gameObject);
    }

    private void AssertUVsAlmostEqual(Vector2 uv1, Vector2 uv2, float tolerance, string message)
    {
        Assert.That(uv1.x, Is.EqualTo(uv2.x).Within(tolerance), $"{message} on X coordinate.");
        Assert.That(uv1.y, Is.EqualTo(uv2.y).Within(tolerance), $"{message} on Y coordinate.");
    }
}
