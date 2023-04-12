using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Tests.Framework;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

class MeshSyncTests : TemporaryAssetTest
{
    static readonly string copyPasteTestScene = $"{TestUtility.testsRootDirectory}/Scenes/CopyPasteDuplicate.unity";

    [SetUp]
    public void Setup()
    {
        var window = EditorWindow.GetWindow<SceneView>();
        window.Show(false);
        window.Repaint();
        window.Focus();

        OpenScene(copyPasteTestScene);
    }

    [Test]
    public void Duplicate_CreatesNewMesh()
    {
        var parent = new GameObject().transform;
        var cube = ShapeGenerator.CreateShape(ShapeType.Cube, PivotLocation.FirstCorner);
        cube.transform.parent = parent;
        Assume.That(parent.childCount, Is.EqualTo(1));
        int originalMeshId = cube.GetComponent<MeshFilter>().sharedMesh.GetInstanceID();

        Selection.objects = new[] { cube.gameObject };
        Assume.That(EditorApplication.ExecuteMenuItem("Edit/Duplicate"), Is.True);
        Assume.That(parent.transform.childCount, Is.EqualTo(2));

        var copy = parent.GetChild(1).GetComponent<ProBuilderMesh>();
        // EditorUtility.SynchronizeWithMeshFilter(copy);

        Assume.That(copy, Is.Not.EqualTo(cube));
        Assert.That(copy.GetComponent<MeshFilter>().sharedMesh.GetInstanceID(), Is.Not.EqualTo(originalMeshId));
    }

    [Test]
    public void CopyPaste_CreatesNewMesh()
    {
        var parent = new GameObject().transform;
        var cube = ShapeGenerator.CreateShape(ShapeType.Cube, PivotLocation.FirstCorner);
        cube.transform.parent = parent;
        Assume.That(parent.childCount, Is.EqualTo(1));
        var source = cube.GetComponent<MeshFilter>().sharedMesh;

        Selection.objects = new[] { cube.gameObject };
        Assume.That(EditorApplication.ExecuteMenuItem("Edit/Copy"), Is.True);
        Assume.That(EditorApplication.ExecuteMenuItem("Edit/Paste"), Is.True);
        Assume.That(parent.transform.childCount, Is.EqualTo(2));

        var copy = parent.GetChild(1).GetComponent<ProBuilderMesh>();

        // this is called by ObjectChangeKind.CreateGameObjectHierarchy in HierarchyListener. for the sake of the test
        // we'll assume that the callback is working as intended. this way we avoid waiting til end of frame for the
        // events to flush.
        EditorUtility.SynchronizeWithMeshFilter(copy);

        Assume.That(copy, Is.Not.EqualTo(cube));
        Assert.That(copy.GetComponent<MeshFilter>().sharedMesh, Is.Not.EqualTo(source));
    }

    [Test]
    public void OpenScene_DoesNot_DirtyMeshSync()
    {
        var cube = GameObject.Find("Cube");
        Assume.That(cube, Is.Not.Null);
        var mesh = cube.GetComponent<ProBuilderMesh>();
        var id = mesh.id;
        OpenScene(copyPasteTestScene);

        var second = GameObject.Find("Cube");
        Assume.That(second, Is.Not.Null);
        Assert.That(second.GetComponent<ProBuilderMesh>().id, Is.EqualTo(id));
    }
}
