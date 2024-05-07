using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

class MeshSyncTests : TemporaryAssetTest
{
    static readonly string copyPasteTestScene = $"{TestUtility.testsRootDirectory}/Scenes/CopyPasteDuplicate.unity";

    private Scene m_Scene;

    [SetUp]
    public void Setup()
    {
        var window = EditorWindow.GetWindow<SceneView>();
        window.Show(false);
        window.Repaint();
        window.Focus();

        m_Scene = OpenScene(copyPasteTestScene);
    }

    [TearDown]
    public void TearDown()
    {
        CloseScene(m_Scene);
    }

    #if UNITY_2020_2_OR_NEWER
    static IEnumerable CopyPasteDuplicate
    {
        get
        {
            yield return new TestCaseData(new object[]{new [] { "Edit/Duplicate" }}) { TestName = "Duplicate"};
            yield return new TestCaseData(new object[]{new [] { "Edit/Copy", "Edit/Paste" }}) { TestName = "Copy/Paste"};
        }
    }

    [Test]
    [TestCaseSource(nameof(CopyPasteDuplicate))]
    public void ExecuteCopyPasteDuplicate_CreatesUniqueMesh(string[] commands)
    {
        var parent = new GameObject().transform;
        var cube = ShapeGenerator.CreateShape(ShapeType.Cube, PivotLocation.FirstCorner);
        cube.transform.parent = parent;
        Assume.That(parent.childCount, Is.EqualTo(1));
        int originalMeshId = cube.GetComponent<MeshFilter>().sharedMesh.GetInstanceID();

        Selection.objects = new[] { cube.gameObject };

        foreach(var command in commands)
            Assume.That(EditorApplication.ExecuteMenuItem(command), Is.True);

        Assume.That(parent.transform.childCount, Is.EqualTo(2));

        var copy = parent.GetChild(1).GetComponent<ProBuilderMesh>();

        // this is called by ObjectChangeKind.CreateGameObjectHierarchy in HierarchyListener. for the sake of the test
        // we'll assume that the callback is working as intended. this way we avoid waiting til end of frame for the
        // events to flush.
        HierarchyListener.OnObjectCreated(copy);

        Assume.That(copy, Is.Not.EqualTo(cube));
        Assert.That(copy.GetComponent<MeshFilter>().sharedMesh.GetInstanceID(), Is.Not.EqualTo(originalMeshId));
    }

    //[PBLD-75] Sending the event to the scene view is needed as just calling HierarchyListener.OnObjectCreated
    // is not reproducing the actual bug.
    [UnityTest]
    public IEnumerator ExecuteCopyPasteDuplicateOnParent_CreatesUniqueMesh()
    {
        string[] commands = new[] { "Duplicate" };
        var parent = new GameObject().transform;
        var emptyGO = new GameObject().transform;
        var cube = ShapeGenerator.CreateShape(ShapeType.Cube, PivotLocation.FirstCorner);

        emptyGO.parent = parent;
        cube.transform.parent = emptyGO;

        Assume.That(parent.childCount, Is.EqualTo(1));
        int originalMeshId = cube.GetComponent<MeshFilter>().sharedMesh.GetInstanceID();

        Selection.objects = new[] { emptyGO.gameObject };
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        foreach (var command in commands)
        {
            var evt = new Event(){type = EventType.ExecuteCommand, commandName = command};
            SceneView.lastActiveSceneView.SendEvent(evt);
        }
        //    Assume.That(EditorApplication.ExecuteMenuItem(command), Is.True);

        var count = 0;
        while (parent.transform.childCount < 2 && count < 100)
        {
            count++;
            yield return null;
        }
        Assert.That(count, Is.Not.EqualTo(100), "Exiting from the duplicate loop after too many attempts.");

        //parent.GetChild(1) is the duplicated emptyGO
        var copy = parent.GetChild(1).GetChild(0).GetComponent<ProBuilderMesh>();

        // this is called by ObjectChangeKind.CreateGameObjectHierarchy in HierarchyListener. for the sake of the test
        // we'll assume that the callback is working as intended. this way we avoid waiting til end of frame for the
        // events to flush.
        HierarchyListener.OnObjectCreated(copy);

        Assume.That(copy, Is.Not.EqualTo(cube));
        Assert.That(copy.GetComponent<MeshFilter>().sharedMesh.GetInstanceID(), Is.Not.EqualTo(originalMeshId));
    }
    #endif

    [Test]
    public void OpenSceneDoesNotDirtyScene()
    {
        var cube = GameObject.Find("Cube");
        Assume.That(cube, Is.Not.Null);
        var mesh = cube.GetComponent<ProBuilderMesh>();
        EditorUtility.SynchronizeWithMeshFilter(mesh);
        Assert.That(EditorSceneManager.GetActiveScene().isDirty, Is.False);
    }

    // Instantiating a ProBuilderMesh component should not automatically create a new mesh asset. In the editor, this
    // is handled by HierarchyListener. At runtime, it is the responsibility of the caller to invoke
    // ProBuilderMesh.MakeUnique if editing is required.
    [Test]
    public static void InstantiateReferencesOriginalMesh()
    {
        var original = ShapeFactory.Instantiate<Cube>();
        var copy = Object.Instantiate(original);

        try
        {
            Assert.AreNotEqual(copy, original, "GameObject references are equal");
            Assert.IsTrue(ReferenceEquals(copy.mesh, original.mesh), "Mesh references are equal");
        }
        finally
        {
            Object.DestroyImmediate(original.gameObject);
            Object.DestroyImmediate(copy.gameObject);
        }
    }
}
