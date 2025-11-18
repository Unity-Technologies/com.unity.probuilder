using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.TestTools;

[TestFixture]
public class FacePickerTests
{
    private ProBuilderMesh m_Mesh;
    private Camera m_Camera;
    private ScenePickerPreferences m_PickerPreferences;

    [SetUp]
    public void Setup()
    {
        // Ensure SceneView is open and focused for HandleUtility to work correctly
        var window = EditorWindow.GetWindow<SceneView>();
        window.Show(false);
        window.Repaint();
        window.Focus();

        m_Camera = new GameObject("TestCamera", typeof(Camera)).GetComponent<Camera>();
        m_Camera.transform.position = new Vector3(0, 3, 0);
        m_Camera.transform.LookAt(Vector3.zero);
        m_Camera.orthographic = false;
        m_Camera.cullingMask = ~0;
        m_Camera.fieldOfView = 60f;
        MatchSceneViewToCamera(m_Camera);

        m_Mesh = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Cube>();
        m_Mesh.name = "TestCube";
        m_Mesh.transform.position = Vector3.zero;
        m_Mesh.transform.rotation = Quaternion.identity;
        m_Mesh.Refresh();

        var meshCollider = m_Mesh.gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = m_Mesh.gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = m_Mesh.mesh;
        meshCollider.enabled = true;

        Camera.SetupCurrent(m_Camera);

        m_PickerPreferences = new ScenePickerPreferences()
        {
            cullMode = CullingMode.Back,
            rectSelectMode = RectSelectMode.Partial
        };

        MeshSelection.ClearElementSelection();
        MeshSelection.AddToSelection(m_Mesh.gameObject);

        ActiveEditorTracker.sharedTracker.ForceRebuild();
        ToolManager.SetActiveContext<PositionToolContext>();
    }

    [TearDown]
    public void Cleanup()
    {
        MeshSelection.ClearElementSelection();
        EditorSceneViewPicker.selection.Clear();

        if (m_Mesh != null)
            UObject.DestroyImmediate(m_Mesh.gameObject);
        if (m_Camera != null)
            UObject.DestroyImmediate(m_Camera.gameObject);

        Camera.SetupCurrent(null);
        LogAssert.NoUnexpectedReceived();
    }

    // Helper to create a mouse event
    private Event CreateMouseEvent(Vector3 mousePosition, EventType type = EventType.MouseDown, EventModifiers modifiers = EventModifiers.None)
    {
        Event evt = new Event
        {
            type = type,
            mousePosition = mousePosition,
            modifiers = modifiers
        };
        return evt;
    }

    [UnityTest]
    public IEnumerator FacePicker_PicksVisibleFace()
    {
        // The front face should be selected
        Face faceToPick = m_Mesh.facesInternal[4];

        Vector3 centerOfFace_world = GetFaceCenterWorld(m_Mesh, faceToPick);
        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(centerOfFace_world);

        yield return null;

        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face, // Select faces
            m_PickerPreferences);
        }
        yield return null;

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNotNull(currentSelection.mesh, "A mesh should be selected.");
        Assert.AreEqual(m_Mesh, currentSelection.mesh, "The correct mesh should be selected.");
        Assert.IsNotNull(currentSelection.faces, "Faces collection should not be null.");
        Assert.AreEqual(1, currentSelection.faces.Count, "Exactly one face should be selected.");
        Assert.AreEqual(faceToPick, currentSelection.faces.First(), "The expected face should be picked.");
    }

    [UnityTest]
    public IEnumerator FacePicker_DoesNotPickWhenNotHovering()
    {
        // Click far away from any objects
        Vector2 mousePos = new Vector2(Screen.width / 2f, Screen.height / 2f + 500f);

        yield return null;

        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);
        }
        yield return null;

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNull(currentSelection.mesh, "No mesh should be selected.");
        Assert.IsEmpty(currentSelection.faces, "No faces should be selected.");
        Assert.IsNull(currentSelection.gameObject, "No GameObject should be selected.");
    }

    [UnityTest]
    public IEnumerator FacePicker_PicksClosestFaceWithMultipleOverlappingProBuilderMeshes()
    {
        // Create a second cube, closer to the camera than the first cube
        ProBuilderMesh mesh2 = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Cube>();
        mesh2.name = "TestCube2";
        mesh2.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
        mesh2.Refresh();

        var meshCollider2 = mesh2.gameObject.GetComponent<MeshCollider>();
        if (meshCollider2 == null) meshCollider2 = mesh2.gameObject.AddComponent<MeshCollider>();
        meshCollider2.sharedMesh = mesh2.mesh;
        meshCollider2.enabled = true;

        MeshSelection.AddToSelection(mesh2.gameObject); // Add to current selection for picker to consider

        Face face1 = m_Mesh.facesInternal[4]; // Face on the default cube
        Vector3 center1_world = GetFaceCenterWorld(m_Mesh, face1);
        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(center1_world);

        yield return null;

        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);
        }
        yield return null;

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNotNull(currentSelection.mesh, "A face should be picked.");
        Assert.AreEqual(mesh2, currentSelection.mesh, "The closer ProBuilder mesh (m_Mesh) should be picked.");
        Assert.IsNotNull(currentSelection.faces, "Faces collection should not be null.");
        Assert.AreEqual(1, currentSelection.faces.Count, "Exactly one face should be selected.");
        Assert.AreEqual(mesh2.facesInternal[4], currentSelection.faces.First(),
            "The expected face (from mesh2) should be picked, because the new cube is closer to the camera");

        UObject.DestroyImmediate(mesh2.gameObject);
    }

    [UnityTest]
    public IEnumerator FacePicker_PicksProBuilderFaceOverlappingNonProBuilderGameObject()
    {
        // Place a non-ProBuilder cube in front of the ProBuilder mesh
        // nonPbGo at y=1.5, m_Mesh at y=0.0. Camera at y=3. nonPbGo is CLOSER.
        GameObject nonPbGo = CreateNonProBuilderCube(new Vector3(0, 1.5f, 0), 1f, 1f);

        Face pbFace = m_Mesh.facesInternal[4];
        Vector3 centerPbFace_world = GetFaceCenterWorld(m_Mesh, pbFace); // Use helper
        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(centerPbFace_world);


        yield return null;
        using (new IgnoreAssertScope())
        {

            // As per the provided EditorSceneViewPicker.cs (second version),
            // ProBuilder faces are prioritized if any are hit.
            EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);
        }

        yield return null;

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNotNull(currentSelection.mesh, "A ProBuilder mesh should be selected.");
        Assert.AreEqual(m_Mesh, currentSelection.mesh, "The ProBuilder mesh should be picked despite the closer non-ProBuilder GameObject.");
        Assert.IsNotNull(currentSelection.faces, "Faces collection should not be null.");
        Assert.AreEqual(1, currentSelection.faces.Count, "Exactly one face should be selected.");
        Assert.AreEqual(pbFace, currentSelection.faces.First(), "The expected ProBuilder face should be picked.");

        // Explicitly assert that the non-ProBuilder object was NOT selected as the main GameObject.
        Assert.AreNotEqual(nonPbGo, currentSelection.gameObject, "The non-ProBuilder GameObject should not be the selected object.");

        UObject.DestroyImmediate(nonPbGo);
    }

    [UnityTest]
    public IEnumerator FacePicker_CyclesThroughOverlappingProBuilderFacesOnRepeatedClick()
    {
        // Create a second ProBuilder mesh, slightly in back of the first
        // m_Mesh at y=0, mesh2 at y=-1.5. Camera at y=3.
        ProBuilderMesh mesh2 = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Cube>();
        mesh2.name = "TestPlaneBehind";
        mesh2.transform.position = new Vector3(0.0f, -1.5f, 0.0f);
        mesh2.Refresh();

        var meshCollider2 = mesh2.gameObject.GetComponent<MeshCollider>();
        if (meshCollider2 == null) meshCollider2 = mesh2.gameObject.AddComponent<MeshCollider>();
        meshCollider2.sharedMesh = mesh2.mesh;
        meshCollider2.enabled = true;

        // Create a third ProBuilder mesh, slightly in back of the first
        // m_Mesh at y=0, mesh2 at y=-1.5, mesh3 at y=1.5. Camera at y=3.
        ProBuilderMesh mesh3 = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Cube>();
        mesh3.name = "TestPlaneFront";
        mesh3.transform.position = new Vector3(0.0f, 1.5f, 0.0f);
        mesh3.Refresh();

        var meshCollider3 = mesh3.gameObject.GetComponent<MeshCollider>();
        if (meshCollider3 == null) meshCollider3 = mesh3.gameObject.AddComponent<MeshCollider>();
        meshCollider3.sharedMesh = mesh3.mesh;
        meshCollider3.enabled = true;

        // Add both to selection for picker to consider
        MeshSelection.AddToSelection(mesh2.gameObject);
        MeshSelection.AddToSelection(mesh3.gameObject);

        Face face1 = m_Mesh.facesInternal[4];   // Face on the middle mesh (m_Mesh)
        Face face2 = mesh2.facesInternal[4];    // Face on the back mesh (mesh2)
        Face face3 = mesh3.facesInternal[4];    // Face on the front mesh (mesh3)

        // Calculate an overlapping point for clicking
        Face pbFace = m_Mesh.facesInternal[4];
        Vector3 centerPbFace_world = GetFaceCenterWorld(m_Mesh, pbFace); // Use helper
        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(centerPbFace_world);

        yield return null;

        // --- First click: Should pick the front mesh (mesh3)
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);
        }

        yield return null;

        SceneSelection currentSelection = EditorSceneViewPicker.selection;
        Assert.AreEqual(mesh3, currentSelection.mesh, "First click: Front ProBuilder mesh should be picked.");
        Assert.AreEqual(1, currentSelection.faces.Count, "First click: Exactly one face should be selected.");
        Assert.AreEqual(face3, currentSelection.faces.First(), "First click: The front face should be picked.");

        // --- Second click: Should pick the middle mesh due to deep cycling
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);
        }
        yield return null;

        currentSelection = EditorSceneViewPicker.selection;
        Assert.AreEqual(m_Mesh, currentSelection.mesh, "Second click: middle ProBuilder mesh should be picked (cycled).");
        Assert.AreEqual(1, currentSelection.faces.Count, "Second click: Exactly one face should be selected.");
        Assert.AreEqual(face1, currentSelection.faces.First(), "Second click: The middle face should be picked.");

        // --- Third click: Should pick the back mesh due to deep cycling
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);
        }
        yield return null;

        currentSelection = EditorSceneViewPicker.selection;
        Assert.AreEqual(mesh2, currentSelection.mesh, "Second click: middle ProBuilder mesh should be picked (cycled).");
        Assert.AreEqual(1, currentSelection.faces.Count, "Second click: Exactly one face should be selected.");
        Assert.AreEqual(face2, currentSelection.faces.First(), "Second click: The middle face should be picked.");

        // --- Fourth click: Should cycle back to the front mesh (mesh3)
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);
        }
        yield return null;

        currentSelection = EditorSceneViewPicker.selection;
        Assert.AreEqual(mesh3, currentSelection.mesh, "Third click: Front ProBuilder mesh should be picked again (cycled).");
        Assert.AreEqual(1, currentSelection.faces.Count, "Third click: Exactly one face should be selected.");
        Assert.AreEqual(face3, currentSelection.faces.First(), "Third click: The front face should be picked again.");

        UObject.DestroyImmediate(mesh2.gameObject);
        UObject.DestroyImmediate(mesh3.gameObject);
    }

    [UnityTest]
    public IEnumerator FacePicker_DoesNotPickFace_BehindCamera()
    {
        // Move the entire mesh behind the camera's near clip plane
        // m_Mesh at y=11
        m_Mesh.transform.position = new Vector3(0, 11, 0);
        m_Mesh.Refresh();

        m_Mesh.GetComponent<MeshCollider>().sharedMesh = m_Mesh.mesh;
        m_Mesh.GetComponent<MeshCollider>().enabled = true;

        Face faceToTest = m_Mesh.facesInternal[0];
        Vector3 centerOfFace_world = GetFaceCenterWorld(m_Mesh, faceToTest);
        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(centerOfFace_world);
        yield return null;

        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(
                CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
                SelectMode.Face,
                m_PickerPreferences);
        }
        yield return null;

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNull(currentSelection.mesh, "No mesh should be selected.");
        Assert.IsEmpty(currentSelection.faces, "No faces should be selected.");
        Assert.IsNull(currentSelection.gameObject, "No GameObject should be selected.");
    }

    [UnityTest]
    public IEnumerator FacePicker_CyclesThroughOffsetOverlappingProBuilderFaces()
    {
        // Create base mesh
        Face baseFace = m_Mesh.facesInternal[4];
        Vector3 baseCenterWorld = GetFaceCenterWorld(m_Mesh, baseFace);

        // Mesh in front but offset in X
        ProBuilderMesh frontMesh = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Cube>();
        frontMesh.transform.position = m_Mesh.transform.position + new Vector3(0.25f, 1.5f, 0.25f);
        frontMesh.Refresh();
        var colliderFront = frontMesh.gameObject.GetComponent<MeshCollider>();
        if (colliderFront == null) colliderFront = frontMesh.gameObject.AddComponent<MeshCollider>();
        colliderFront.sharedMesh = frontMesh.mesh;
        colliderFront.enabled = true;

        // Mesh behind but offset in Z
        ProBuilderMesh backMesh = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Cube>();
        backMesh.transform.position = m_Mesh.transform.position + new Vector3(-0.25f, -1.5f, -0.25f);
        backMesh.Refresh();
        var colliderBack = frontMesh.gameObject.GetComponent<MeshCollider>();
        if (colliderBack == null) colliderBack = backMesh.gameObject.AddComponent<MeshCollider>();
        colliderBack.sharedMesh = backMesh.mesh;
        colliderBack.enabled = true;

        MeshSelection.AddToSelection(frontMesh.gameObject);
        MeshSelection.AddToSelection(backMesh.gameObject);

        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(baseCenterWorld);
        yield return null;

        // --- First click: front mesh
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(CreateMouseEvent(mousePos), SelectMode.Face, m_PickerPreferences);
        }
        yield return null;
        SceneSelection selection = EditorSceneViewPicker.selection;
        Assert.AreEqual(frontMesh, selection.mesh, "First click: Front offset ProBuilder mesh should be picked.");
        Assert.AreEqual(1, selection.faces.Count, "First click: Exactly one face should be selected.");
        Assert.AreEqual(frontMesh.facesInternal[4], selection.faces.First(), "First click: The front offset face should be picked.");

        // --- Second click: middle mesh
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(CreateMouseEvent(mousePos), SelectMode.Face, m_PickerPreferences);
        }
        yield return null;
        selection = EditorSceneViewPicker.selection;
        Assert.AreEqual(m_Mesh, selection.mesh, "Second click: Middle ProBuilder mesh should be picked.");
        Assert.AreEqual(1, selection.faces.Count, "Second click: Exactly one face should be selected.");
        Assert.AreEqual(m_Mesh.facesInternal[4], selection.faces.First(), "Second click: The middle face should be picked.");

        // --- Third click: back mesh
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(CreateMouseEvent(mousePos), SelectMode.Face, m_PickerPreferences);
        }
        yield return null;
        selection = EditorSceneViewPicker.selection;
        Assert.AreEqual(backMesh, selection.mesh, "Third click: Back offset ProBuilder mesh should be picked.");
        Assert.AreEqual(1, selection.faces.Count, "Third click: Exactly one face should be selected.");
        Assert.AreEqual(backMesh.facesInternal[4], selection.faces.First(), "Third click: The back offset face should be picked.");

        // --- Fourth click: front mesh
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(CreateMouseEvent(mousePos), SelectMode.Face, m_PickerPreferences);
        }
        yield return null;
        selection = EditorSceneViewPicker.selection;
        Assert.AreEqual(frontMesh, selection.mesh, "Fourth click: Front offset ProBuilder mesh should be picked.");
        Assert.AreEqual(1, selection.faces.Count, "Fourth click: Exactly one face should be selected.");
        Assert.AreEqual(frontMesh.facesInternal[4], selection.faces.First(), "Fourth click: The front offset face should be picked.");

        UObject.DestroyImmediate(frontMesh.gameObject);
        UObject.DestroyImmediate(backMesh.gameObject);
    }

    [UnityTest]
    public IEnumerator FacePicker_CycleResetsWhenMouseMovesAndClickOutsideSelectedFace()
    {
        // Create front mesh
        ProBuilderMesh frontMesh = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Cube>();
        frontMesh.name = "FrontMesh";
        frontMesh.transform.position = m_Mesh.transform.position + new Vector3(0, 1.5f, 0);
        frontMesh.Refresh();
        var colliderFront = frontMesh.gameObject.GetComponent<MeshCollider>();
        if (colliderFront == null) colliderFront = frontMesh.gameObject.AddComponent<MeshCollider>();
        colliderFront.sharedMesh = frontMesh.mesh;
        colliderFront.enabled = true;

        // Create back mesh
        ProBuilderMesh backMesh = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Cube>();
        backMesh.name = "BackMesh";
        backMesh.transform.position = m_Mesh.transform.position + new Vector3(0, -1.5f, 0);
        backMesh.Refresh();
        var colliderBack = backMesh.gameObject.GetComponent<MeshCollider>();
        if (colliderBack == null) colliderBack = backMesh.gameObject.AddComponent<MeshCollider>();
        colliderBack.sharedMesh = backMesh.mesh;
        colliderBack.enabled = true;

        // Add all to selection for cycling
        MeshSelection.AddToSelection(frontMesh.gameObject);
        MeshSelection.AddToSelection(backMesh.gameObject);

        Face baseFace = m_Mesh.facesInternal[4];
        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(GetFaceCenterWorld(m_Mesh, baseFace));
        yield return null;

        // --- First click: front mesh
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(CreateMouseEvent(mousePos), SelectMode.Face, m_PickerPreferences);
        }
        yield return null;
        SceneSelection selection = EditorSceneViewPicker.selection;
        Assert.AreEqual(frontMesh, selection.mesh, "First click: Front ProBuilder mesh should be picked.");
        Assert.AreEqual(1, selection.faces.Count, "First click: Exactly one face should be selected.");
        Assert.AreEqual(frontMesh.facesInternal[4], selection.faces.First(), "First click: The front face should be picked.");

        // --- Second click: middle mesh
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(CreateMouseEvent(mousePos), SelectMode.Face, m_PickerPreferences);
        }
        yield return null;
        selection = EditorSceneViewPicker.selection;
        Assert.AreEqual(m_Mesh, selection.mesh, "Second click: Middle ProBuilder mesh should be picked.");
        Assert.AreEqual(1, selection.faces.Count, "Second click: Exactly one face should be selected.");
        Assert.AreEqual(m_Mesh.facesInternal[4], selection.faces.First(), "Second click: The middle face should be picked.");

        // --- Simulate moving mouse far away to reset before cycling to back mesh
        Vector2 farMousePos = mousePos + new Vector2(1000, 1000);

        // Perform a click at the far position (no object hit) to trigger cycle reset
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(CreateMouseEvent(farMousePos), SelectMode.Face, m_PickerPreferences);
        }
        yield return null; // Let the frame advance

        selection = EditorSceneViewPicker.selection;
        Assert.IsNull(selection.mesh, "Cycle reset: No mesh should be selected.");
        Assert.IsEmpty(selection.faces, "Cycle reset: No faces should be selected.");
        Assert.IsNull(selection.gameObject, "Cycle reset: No GameObject should be selected.");

        // --- Click again on original position: cycle should restart from front mesh
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(CreateMouseEvent(mousePos), SelectMode.Face, m_PickerPreferences);
        }
        yield return null;
        selection = EditorSceneViewPicker.selection;
        Assert.AreEqual(frontMesh, selection.mesh, "Cycle restart: Front ProBuilder mesh should be picked again.");
        Assert.AreEqual(1, selection.faces.Count, "Cycle restart: Exactly one face should be selected.");
        Assert.AreEqual(frontMesh.facesInternal[4], selection.faces.First(), "Cycle restart: The front face should be picked again.");

        // --- Second click after reset: should pick middle mesh again, NOT back mesh
        using (new IgnoreAssertScope())
        {
            EditorSceneViewPicker.DoMouseClick(CreateMouseEvent(mousePos), SelectMode.Face, m_PickerPreferences);
        }
        yield return null;
        selection = EditorSceneViewPicker.selection;
        Assert.AreEqual(m_Mesh, selection.mesh, "After reset second click: Middle ProBuilder mesh should be picked.");
        Assert.AreEqual(1, selection.faces.Count, "After reset second click: Exactly one face should be selected.");
        Assert.AreEqual(m_Mesh.facesInternal[4], selection.faces.First(), "After reset second click: The middle face should be picked.");

        UObject.DestroyImmediate(frontMesh.gameObject);
        UObject.DestroyImmediate(backMesh.gameObject);
    }

    private void MatchSceneViewToCamera(Camera cam)
    {
        var sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null)
            sceneView = EditorWindow.GetWindow<SceneView>();

        sceneView.in2DMode = false;
        sceneView.orthographic = cam.orthographic;
        sceneView.size = cam.orthographicSize;
        sceneView.pivot = cam.transform.position + cam.transform.forward * 10f;
        sceneView.rotation = cam.transform.rotation;
        sceneView.cameraSettings.fieldOfView = cam.fieldOfView;

        sceneView.Repaint();
    }

    // Helper to calculate the world-space center of a face
    private Vector3 GetFaceCenterWorld(ProBuilderMesh mesh, Face face)
    {
        Vector3 centerLocal = Vector3.zero;
        Assume.That(face.indexesInternal != null);
        Assume.That(face.indexesInternal.Length != 0);

        Vector3[] positions = mesh.positionsInternal;
        foreach (int index in face.indexesInternal)
        {
            Assume.That(index >= 0 && index < positions.Length);
            centerLocal += positions[index];
        }
        centerLocal /= face.indexesInternal.Length;

        return mesh.transform.TransformPoint(centerLocal);
    }

    // Helper to create a non-ProBuilder GameObject with a MeshCollider
    private GameObject CreateNonProBuilderCube(Vector3 position, float sizeX, float sizeZ)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "NonProBuilderCube";
        go.transform.position = position;
        go.transform.localScale = new Vector3(sizeX, 1, sizeZ);
        // Ensure it has a collider for picking
        MeshCollider collider = go.GetComponent<MeshCollider>();
        if (collider == null)
            collider = go.AddComponent<MeshCollider>();
        collider.enabled = true;
        return go;
    }
}
