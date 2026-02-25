using System.Collections;
using UnityEngine;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEditor.ProBuilder.Actions;


using UnityEngine.TestTools;

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

        Undo.PerformUndo();

        // this is usually caught by UndoUtility
        cube.InvalidateCaches();

        cube.ToMesh();
        cube.Refresh();

        TestUtility.AssertAreEqual(duplicate.mesh, cube.mesh);

        Object.DestroyImmediate(cube.gameObject);
        Object.DestroyImmediate(duplicate.gameObject);
    }

    // PBLD-127: Undoing shape creation and BezierShape leaves a PB Mesh in the scene
    [Test]
    public static void CreateShape_UndoDoesRemoveTheGameObject()
    {
        var instance = EditorToolbarLoader.GetInstance<NewBezierShape>();
        Assume.That(instance, Is.Not.Null);

        instance.PerformAction();

#if UNITY_6000_5_OR_NEWER
        var shapes = GameObject.FindObjectsByType<ProBuilderMesh>();
#else
        var shapes = GameObject.FindObjectsByType<ProBuilderMesh>(FindObjectsSortMode.None);
#endif
        Assume.That(shapes.Length, Is.EqualTo(1));

        Undo.PerformUndo();
#if UNITY_6000_5_OR_NEWER
        shapes = GameObject.FindObjectsByType<ProBuilderMesh>();
#else
        shapes = GameObject.FindObjectsByType<ProBuilderMesh>(FindObjectsSortMode.None);
#endif
        Assert.That(shapes.Length, Is.EqualTo(0));
    }

    [Test]
    public static void DetachFaceUndoTest()
    {
        var cube = ShapeFactory.Instantiate<Cube>();

        var redMat = new Material(Shader.Find("Standard"));
        redMat.color = Color.red;
        var greenMat = new Material(Shader.Find("Standard"));
        greenMat.color = Color.green;

        cube.renderer.sharedMaterials = new Material[] { redMat, greenMat };
        var materialCount = cube.renderer.sharedMaterials.Length;
        Assert.AreEqual(materialCount, 2);

        cube.facesInternal[0].submeshIndex = 1; // green
        for (int i = 1; i < cube.facesInternal.Length; i++)
            cube.facesInternal[i].submeshIndex = 0; // red

        cube.ToMesh();
        cube.Refresh();

        Assert.AreEqual(materialCount, cube.mesh.subMeshCount);

        var duplicate = UnityEngine.Object.Instantiate(cube.gameObject).GetComponent<ProBuilderMesh>();
        duplicate.MakeUnique();
        Assert.AreEqual(materialCount, duplicate.mesh.subMeshCount);

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
        Assert.AreEqual(materialCount - 1, cube.mesh.subMeshCount);
        Assert.AreEqual(materialCount - 1, cube.renderer.sharedMaterials.Length);

        UnityEditor.Undo.PerformUndo();

        // this is usually caught by UndoUtility
        cube.InvalidateCaches();

        cube.ToMesh();
        cube.Refresh();

        // After undo, previously edited mesh should match the duplicate
        TestUtility.AssertAreEqual(duplicate.mesh, cube.mesh);
        Assert.AreEqual(materialCount, cube.renderer.sharedMaterials.Length);
        Assert.AreEqual(materialCount, cube.mesh.subMeshCount);

        UnityEngine.Object.DestroyImmediate(cube.gameObject);
        UnityEngine.Object.DestroyImmediate(duplicate.gameObject);
    }

    [Test]
    public static void DeleteFaceUndoTest()
    {
        var cube = ShapeFactory.Instantiate<Cube>();

        var redMat = new Material(Shader.Find("Standard"));
        redMat.color = Color.red;
        var greenMat = new Material(Shader.Find("Standard"));
        greenMat.color = Color.green;

        cube.renderer.sharedMaterials = new Material[] { redMat, greenMat };
        var materialCount = cube.renderer.sharedMaterials.Length;
        Assert.AreEqual(materialCount, 2);

        cube.facesInternal[0].submeshIndex = 1; // green
        for (int i = 1; i < cube.facesInternal.Length; i++)
            cube.facesInternal[i].submeshIndex = 0; // red

        cube.ToMesh();
        cube.Refresh();

        Assert.AreEqual(materialCount, cube.mesh.subMeshCount);

        var duplicate = UnityEngine.Object.Instantiate(cube.gameObject).GetComponent<ProBuilderMesh>();
        duplicate.MakeUnique();
        Assert.AreEqual(materialCount, duplicate.mesh.subMeshCount);

        // Select the mesh
        MeshSelection.SetSelection(cube.gameObject);
        MeshSelection.OnObjectSelectionChanged();
        Assume.That(MeshSelection.selectedObjectCount, Is.EqualTo(1));

        // Select a face
        cube.SetSelectedFaces(new Face[] { cube.facesInternal[0] });
        Assume.That(cube.selectedFacesInternal.Length, Is.EqualTo(1));

        // Perform `Delete Faces` action
        var deleteAction = new DeleteFaces();
        var result = deleteAction.PerformAction();
        Assume.That(result.status, Is.EqualTo(ActionResult.Status.Success));
        Assert.AreEqual(materialCount - 1, cube.renderer.sharedMaterials.Length);
        Assert.AreEqual(materialCount - 1, cube.mesh.subMeshCount);

        UnityEditor.Undo.PerformUndo();

        // this is usually caught by UndoUtility
        cube.InvalidateCaches();

        cube.ToMesh();
        cube.Refresh();

        // After undo, previously edited mesh should match the duplicate
        TestUtility.AssertAreEqual(duplicate.mesh, cube.mesh);
        Assert.AreEqual(materialCount, cube.renderer.sharedMaterials.Length);
        Assert.AreEqual(materialCount, cube.mesh.subMeshCount);

        UnityEngine.Object.DestroyImmediate(cube.gameObject);
        UnityEngine.Object.DestroyImmediate(duplicate.gameObject);
    }

    [UnityTest]
    public static IEnumerator PerformUndoRedo_VersionIndexIsUpdated()
    {
        ProBuilderMesh cube = ShapeFactory.Instantiate<Cube>();

        // Select the mesh
        MeshSelection.SetSelection(cube.gameObject);
        Assume.That(MeshSelection.selectedObjectCount, Is.EqualTo(1));

        // Select a face
        cube.SetSelectedFaces(new Face[]{cube.facesInternal[0]});
        Assume.That(cube.selectedFacesInternal.Length, Is.EqualTo(1));

        var originalVersionIndex = cube.versionIndex;
        Assume.That(cube.meshSyncState, Is.EqualTo(MeshSyncState.InSync));

        Undo.IncrementCurrentGroup();

        // Perform `delete Faces` action
        var deleteAction = new DeleteFaces();
        var result = deleteAction.PerformAction();
        Assume.That(result.status, Is.EqualTo(ActionResult.Status.Success));

        var performedVersionIndex = cube.versionIndex;
        Assert.That(cube.meshSyncState, Is.EqualTo(MeshSyncState.InSync));
        Assert.That(performedVersionIndex, Is.GreaterThan(originalVersionIndex));

        Undo.PerformUndo();
        //Wait a Frame to trigger auto rebuild
        yield return null;

        // Assert that the new status of the mesh is correct and that the version index is updated after undoing
        Assert.That(cube.meshSyncState, Is.EqualTo(MeshSyncState.InSync));
        Assert.That(cube.versionIndex, Is.EqualTo(originalVersionIndex));

        Undo.PerformRedo();
        //Wait a Frame to trigger auto rebuild
        yield return null;

        // Assert that the new status of the mesh is correct and that the version index is updated after undoing
        Assert.That(cube.meshSyncState, Is.EqualTo(MeshSyncState.InSync));
        Assert.That(cube.versionIndex, Is.EqualTo(performedVersionIndex));

        Object.DestroyImmediate(cube.gameObject);
    }
}
