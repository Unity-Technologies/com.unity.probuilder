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
    static readonly string emptyScene = $"{TestUtility.testsRootDirectory}/Scenes/EmptyScene.unity";

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

    static IEnumerable CopyPasteDuplicate
    {
        get
        {
            yield return new TestCaseData(new object[]{new [] { "Duplicate" }}) { TestName = "Duplicate"};
            yield return new TestCaseData(new object[]{new [] { "Copy", "Paste" }}) { TestName = "Copy/Paste"};
        }
    }

    static ulong GetRawId(Object obj)
    {
        var id = obj.GetObjectId();

#if UNITY_6000_5_OR_NEWER
        return EntityId.ToULong(id);
#elif UNITY_6000_4
        return id.GetRawData();
#else
        return (ulong)id;
#endif
    }

    [Test]
    [TestCaseSource(nameof(CopyPasteDuplicate))]
    public void ExecuteCopyPasteDuplicate_CreatesUniqueMesh(string[] commands)
    {
        var parent = new GameObject().transform;
        var cube = ShapeGenerator.CreateShape(ShapeType.Cube, PivotLocation.FirstVertex);
        cube.transform.parent = parent;
        Assume.That(parent.childCount, Is.EqualTo(1));
        ulong originalMeshId = GetRawId(cube.GetComponent<MeshFilter>().sharedMesh);

        Selection.activeObject = cube.gameObject;

        foreach (var command in commands)
        {
            var evt = new Event(){type = EventType.ExecuteCommand, commandName = command};
            SceneView.lastActiveSceneView.SendEvent(evt);
        }

        Assume.That(parent.transform.childCount, Is.EqualTo(2));

        var copy = parent.GetChild(1).GetComponent<ProBuilderMesh>();

        // this is called by ObjectChangeKind.CreateGameObjectHierarchy in HierarchyListener. for the sake of the test
        // we'll assume that the callback is working as intended. this way we avoid waiting til end of frame for the
        // events to flush.
        HierarchyListener.OnObjectCreated(copy);

        Assume.That(copy, Is.Not.EqualTo(cube));
        Assert.That(GetRawId(copy.GetComponent<MeshFilter>().sharedMesh), Is.Not.EqualTo(originalMeshId));
    }

    //[PBLD-75] Sending the event to the scene view is needed as just calling HierarchyListener.OnObjectCreated
    // is not reproducing the actual bug.
    [UnityTest]
    public IEnumerator ExecuteCopyPasteDuplicateOnParent_CreatesUniqueMesh()
    {
        string[] commands = new[] { "Duplicate" };
        var parent = new GameObject().transform;
        var emptyGO = new GameObject().transform;
        var cube = ShapeGenerator.CreateShape(ShapeType.Cube, PivotLocation.FirstVertex);

        emptyGO.parent = parent;
        cube.transform.parent = emptyGO;

        Assume.That(parent.childCount, Is.EqualTo(1));
        ulong originalMeshId = GetRawId(cube.GetComponent<MeshFilter>().sharedMesh);

        Selection.objects = new[] { emptyGO.gameObject };
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        foreach (var command in commands)
        {
            var evt = new Event(){type = EventType.ExecuteCommand, commandName = command};
            SceneView.lastActiveSceneView.SendEvent(evt);
        }

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
        Assert.That(GetRawId(copy.GetComponent<MeshFilter>().sharedMesh), Is.Not.EqualTo(originalMeshId));
    }

    [Test]
    public void OpenSceneDoesNotDirtyScene()
    {
        var cube = GameObject.Find("Cube");
        Assume.That(cube, Is.Not.Null);
        var mesh = cube.GetComponent<ProBuilderMesh>();
        EditorUtility.SynchronizeWithMeshFilter(mesh);
        Assert.That(EditorSceneManager.GetActiveScene().isDirty, Is.False);
    }

    // [PBLD-138] Duplicating PBMesh would dirty all scenes
    [UnityTest]
    public IEnumerator GivenMultipleScenes_DuplicatePBMesh_OnlyOneSceneIsDirty()
    {
        // Load two scenes
        Scene firstScene = EditorSceneManager.GetActiveScene();
        Assume.That(firstScene, Is.Not.Null, "First scene should not be null.");
        Assume.That(firstScene.isDirty, Is.False, "First scene should not be dirty.");

        Scene secondScene = OpenScene(emptyScene, OpenSceneMode.Additive);
        Assume.That(secondScene, Is.Not.Null, "Second scene should not be null");
        Assume.That(secondScene.isDirty, Is.False, "Second scene should not be dirty.");

        Assume.That(EditorSceneManager.sceneCount, Is.EqualTo(2), "There should be two scenes loaded.");

        SceneManager.SetActiveScene(firstScene);

        // Create a cube in first scene
        var cube = GameObject.Find("Cube");
        Assume.That(cube, Is.Not.Null, "Finding Cube shape should succeed.");

        // Select cube
        Selection.activeGameObject = cube.gameObject;
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        // Duplicate cube
        var evt = new Event(){type = EventType.ExecuteCommand, commandName = "Duplicate"};
        SceneView.lastActiveSceneView.SendEvent(evt);

        // Wait until we can confirm duplication command
        var count = 0;
        while (firstScene.rootCount < 2 && count < 100)
        {
            count++;
            yield return null;
        }
        Assert.That(count, Is.Not.EqualTo(100), "Exiting from the duplicate loop after too many attempts.");

        // Wait one frame for the Undo commands to flush
        yield return null;

        // Second scene should stay untouched
        Assert.That(firstScene.isDirty, Is.True, "First scene should be dirty after cube duplication.");
        Assert.That(secondScene.isDirty, Is.False, "Second scene should remain clean after cube duplication.");

        // Clean-up
        var rootGOs = firstScene.GetRootGameObjects();
        for (int i = 0; i < rootGOs.Length; ++i)
            GameObject.DestroyImmediate(rootGOs[i]);

        CloseScene(firstScene);
        CloseScene(secondScene);
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
