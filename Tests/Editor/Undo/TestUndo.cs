using UnityEngine;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.ProBuilder.Tests.Framework;

static class UndoTests
{
    [Test]
    public static void RegisterComplete()
    {
        var cube = ShapeFactory.Instantiate<Cube>();
        var duplicate = UnityEngine.Object.Instantiate(cube.gameObject).GetComponent<ProBuilderMesh>();
        duplicate.MakeUnique();

        TestUtility.AssertAreEqual(cube.mesh, duplicate.mesh);

        UnityEditor.Undo.RegisterCompleteObjectUndo(new[] { cube }, "Merge Vertices");
        cube.MergeVertices(new int[] { 0, 1 }, true);
        cube.ToMesh();
        cube.Refresh();

        Assert.IsFalse(TestUtility.MeshesAreEqual(cube.mesh, duplicate.mesh));

        UnityEditor.Undo.PerformUndo();

        // this is usually caught by UndoUtility
        cube.InvalidateCaches();

        cube.ToMesh();
        cube.Refresh();

        TestUtility.AssertAreEqual(duplicate.mesh, cube.mesh);

        UnityEngine.Object.DestroyImmediate(cube.gameObject);
        UnityEngine.Object.DestroyImmediate(duplicate.gameObject);
    }

    [Test]
    public static void DetachFaceUndoTest()
    {
        var cube = ShapeFactory.Instantiate<Cube>();
        var duplicate = UnityEngine.Object.Instantiate(cube.gameObject).GetComponent<ProBuilderMesh>();
        duplicate.MakeUnique();

        // Select the mesh
        MeshSelection.SetSelection(cube.gameObject);
        MeshSelection.OnObjectSelectionChanged();
        Assume.That(MeshSelection.selectedObjectCount, Is.EqualTo(1));

        // Select a face
        cube.SetSelectedFaces(new Face[]{cube.facesInternal[0]});
        Assume.That(cube.selectedFacesInternal.Length, Is.EqualTo(1));

        // Perform `Detach Faces` action
        var detachAction = new DetachFaces();
        var result = detachAction.PerformAction();
        Assume.That(result.status, Is.EqualTo(ActionResult.Status.Success));

        UnityEditor.Undo.PerformUndo();

        // this is usually caught by UndoUtility
        cube.InvalidateCaches();

        cube.ToMesh();
        cube.Refresh();

        // After undo, previously edited mesh should match the duplicate
        TestUtility.AssertAreEqual(duplicate.mesh, cube.mesh);

        UnityEngine.Object.DestroyImmediate(cube.gameObject);
        UnityEngine.Object.DestroyImmediate(duplicate.gameObject);
    }
}
