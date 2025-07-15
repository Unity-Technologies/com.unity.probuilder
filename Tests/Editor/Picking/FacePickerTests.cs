using System;
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

    // Helper to calculate the world-space center of a face
    private Vector3 GetFaceCenterWorld(ProBuilderMesh mesh, Face face)
    {
        Vector3 centerLocal = Vector3.zero;
        if (face.indexesInternal == null || face.indexesInternal.Length == 0)
            return Vector3.zero; // Should not happen for valid ProBuilder faces

        Vector3[] positions = mesh.positionsInternal;
        foreach (int index in face.indexesInternal)
        {
            if (index >= 0 && index < positions.Length)
            {
                centerLocal += positions[index];
            }
            else
            {
                Debug.LogError($"Invalid vertex index {index} found in face indexes.");
                return Vector3.zero; // Or handle error appropriately
            }
        }
        centerLocal /= face.indexesInternal.Length;

        return mesh.transform.TransformPoint(centerLocal);
    }

    // Helper to create a non-ProBuilder GameObject with a MeshCollider
    private GameObject CreateNonProBuilderPlane(Vector3 position, float sizeX, float sizeZ)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "NonProBuilderPlane";
        go.transform.position = position;
        go.transform.localScale = new Vector3(sizeX, 1, sizeZ);
        // Ensure it has a collider for picking
        MeshCollider collider = go.GetComponent<MeshCollider>();
        if (collider == null)
            collider = go.AddComponent<MeshCollider>();
        collider.enabled = true;
        return go;
    }

    [SetUp]
    public void Setup()
    {
        // Ensure SceneView is open and focused for HandleUtility to work correctly
        var window = EditorWindow.GetWindow<SceneView>();
        window.Show(false);
        window.Repaint();
        window.Focus();

        m_Camera = new GameObject("TestCamera", typeof(Camera)).GetComponent<Camera>();
        m_Camera.transform.position = new Vector3(0, 10, 0);
        m_Camera.transform.LookAt(Vector3.zero);
        m_Camera.orthographic = false;
        m_Camera.cullingMask = ~0; // Render everything

        m_Mesh = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Plane>();
        m_Mesh.name = "TestPlane";
        m_Mesh.transform.position = Vector3.zero;
        m_Mesh.transform.rotation = Quaternion.identity;
        m_Mesh.Refresh(); // Ensure mesh data is up-to-date

        // Add MeshCollider for picking
        var meshCollider = m_Mesh.gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = m_Mesh.gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = m_Mesh.mesh;
        meshCollider.enabled = true;

        Camera.SetupCurrent(m_Camera); // Set this camera as the active SceneView camera

        m_PickerPreferences = new ScenePickerPreferences()
        {
            cullMode = CullingMode.Back, // Default culling mode
            rectSelectMode = RectSelectMode.Partial // Default rect select mode
        };

        MeshSelection.ClearElementSelection();
        MeshSelection.AddToSelection(m_Mesh.gameObject); // Add ProBuilder mesh to current selection context

        ActiveEditorTracker.sharedTracker.ForceRebuild();
        ToolManager.SetActiveContext<PositionToolContext>(); // Set to a tool that uses picking
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

        // Destroy any other GameObjects created specifically for tests
        // Using FindObjectsOfType is generally slow, but acceptable in editor tests cleanup.
        foreach (var go in UObject.FindObjectsOfType<GameObject>())
        {
            if (go.name.Contains("TestPlane2") || go.name.Contains("NonProBuilderPlane"))
                UObject.DestroyImmediate(go);
        }

        Camera.SetupCurrent(null); // Clear active camera
        LogAssert.NoUnexpectedReceived(); // Ensure no unexpected Unity errors/warnings occurred
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

    [Test]
    public void FacePicker_PicksVisibleFace()
    {
        // Get the first face of the plane (assuming a single-face plane)
        Face faceToPick = m_Mesh.facesInternal[0];

        // Get the world center of the face using the new helper
        Vector3 centerOfFace_world = GetFaceCenterWorld(m_Mesh, faceToPick);
        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(centerOfFace_world);

        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true; // Ignore logs due to not executing OnGUI loop
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face, // Select faces
            m_PickerPreferences);

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNotNull(currentSelection.mesh, "A mesh should be selected.");
        Assert.AreEqual(m_Mesh, currentSelection.mesh, "The correct mesh should be selected.");
        Assert.IsNotNull(currentSelection.faces, "Faces collection should not be null.");
        Assert.AreEqual(1, currentSelection.faces.Count, "Exactly one face should be selected.");
        Assert.AreEqual(faceToPick, currentSelection.faces.First(), "The expected face should be picked.");
    }

    [Test]
    public void FacePicker_DoesNotPickWhenNotHovering()
    {
        // Click far away from any objects
        Vector2 mousePos = new Vector2(Screen.width / 2f, Screen.height / 2f + 500f);

        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNull(currentSelection.mesh, "No mesh should be selected.");
        Assert.IsEmpty(currentSelection.faces, "No faces should be selected.");
        Assert.IsNull(currentSelection.gameObject, "No GameObject should be selected.");
    }

    [Test]
    public void FacePicker_PicksClosestFaceWithMultipleOverlappingProBuilderMeshes()
    {
        // Adjust camera to see both planes
        m_Camera.transform.position = new Vector3(0.5f, 0.5f, -10);
        m_Camera.transform.LookAt(Vector3.zero);

        // Create a second ProBuilder mesh, slightly behind the first (m_Mesh is at Z=0)
        ProBuilderMesh mesh2 = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Plane>();
        mesh2.name = "TestPlane2";
        mesh2.transform.position = new Vector3(0.0f, 0.0f, 0.5f); // mesh2 is further from camera
        mesh2.Refresh();

        var meshCollider2 = mesh2.gameObject.GetComponent<MeshCollider>();
        if (meshCollider2 == null) meshCollider2 = mesh2.gameObject.AddComponent<MeshCollider>();
        meshCollider2.sharedMesh = mesh2.mesh;
        meshCollider2.enabled = true;

        MeshSelection.AddToSelection(mesh2.gameObject); // Add to current selection for picker to consider

        Face face1 = m_Mesh.facesInternal[0]; // Face on the front mesh (m_Mesh)
        Vector3 center1_world = GetFaceCenterWorld(m_Mesh, face1); // Use helper

        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(center1_world);

        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNotNull(currentSelection.mesh, "A face should be picked.");
        Assert.AreEqual(m_Mesh, currentSelection.mesh, "The closer ProBuilder mesh (m_Mesh) should be picked.");
        Assert.IsNotNull(currentSelection.faces, "Faces collection should not be null.");
        Assert.AreEqual(1, currentSelection.faces.Count, "Exactly one face should be selected.");
        Assert.AreEqual(face1, currentSelection.faces.First(), "The expected face (from m_Mesh) should be picked.");

        UObject.DestroyImmediate(mesh2.gameObject);
    }

    [Test]
    public void FacePicker_PicksProBuilderFaceOverlappingNonProBuilderGameObject()
    {
        // Place a non-ProBuilder plane in front of the ProBuilder mesh
        // nonPbGo at y=1.0, m_Mesh at y=0.0. Camera at y=10. nonPbGo is CLOSER.
        GameObject nonPbGo = CreateNonProBuilderPlane(new Vector3(0, 1.0f, 0), 1f, 1f);

        Face pbFace = m_Mesh.facesInternal[0];
        Vector3 centerPbFace_world = GetFaceCenterWorld(m_Mesh, pbFace); // Use helper
        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(centerPbFace_world);

        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;

        // As per the provided EditorSceneViewPicker.cs (second version),
        // ProBuilder faces are prioritized if any are hit.
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);

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

    [Test]
    public void FacePicker_PicksNonProBuilderGameObjectWhenNoProBuilderFaceIsPresent()
    {
        // Disable the ProBuilder mesh so no PB faces can be hit by the raycast
        m_Mesh.gameObject.SetActive(false);

        // Create a non-ProBuilder plane at the origin
        GameObject nonPbGo = CreateNonProBuilderPlane(new Vector3(5, 0, 0), 1f, 1f);

        // Adjust camera to look at it
        m_Camera.transform.position = new Vector3(5, 10, 0);
        m_Camera.transform.LookAt(new Vector3(5, 0, 0));

        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(nonPbGo.transform.position);

        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face, // Still in Face select mode, but will fallback to GameObject picking
            m_PickerPreferences);

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNull(currentSelection.mesh, "No ProBuilder mesh should be selected.");
        Assert.IsEmpty(currentSelection.faces, "No faces should be selected.");
        Assert.IsNotNull(currentSelection.gameObject, "A GameObject should be selected.");
        Assert.AreEqual(nonPbGo, currentSelection.gameObject, "The non-ProBuilder GameObject should be picked as a fallback.");

        UObject.DestroyImmediate(nonPbGo);
    }

    [Test]
    public void FacePicker_CyclesThroughOverlappingProBuilderFacesOnRepeatedClick()
    {
        // Create a second ProBuilder mesh, slightly in back of the first
        // m_Mesh at y=0, mesh2 at y=-0.5. Camera at y=10.
        ProBuilderMesh mesh2 = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Plane>();
        mesh2.name = "TestPlane2";
        mesh2.transform.position = new Vector3(0.0f, -0.5f, 0.0f);
        mesh2.Refresh();

        var meshCollider2 = mesh2.gameObject.GetComponent<MeshCollider>();
        if (meshCollider2 == null) meshCollider2 = mesh2.gameObject.AddComponent<MeshCollider>();
        meshCollider2.sharedMesh = mesh2.mesh;
        meshCollider2.enabled = true;

        // Add both to selection for picker to consider
        MeshSelection.AddToSelection(m_Mesh.gameObject);
        MeshSelection.AddToSelection(mesh2.gameObject);

        Face face1 = m_Mesh.facesInternal[0];   // Face on the front mesh (m_Mesh)
        Face face2 = mesh2.facesInternal[0];    // Face on the back mesh (mesh2)

        // Calculate an overlapping point for clicking
        Vector3 overlapCenter_world_face1 = GetFaceCenterWorld(m_Mesh, face1); // Use helper
        Vector3 overlapCenter_world_face2 = GetFaceCenterWorld(mesh2, face2); // Use helper
        Vector3 overlapCenter_world = (overlapCenter_world_face1 + overlapCenter_world_face2) / 2f;

        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(overlapCenter_world);

        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;

        // --- First click: Should pick the front mesh (m_Mesh)
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);

        SceneSelection currentSelection = EditorSceneViewPicker.selection;
        Assert.AreEqual(m_Mesh, currentSelection.mesh, "First click: Front ProBuilder mesh should be picked.");
        Assert.AreEqual(1, currentSelection.faces.Count, "First click: Exactly one face should be selected.");
        Assert.AreEqual(face1, currentSelection.faces.First(), "First click: The front face should be picked.");

        // --- Second click: Should pick the back mesh (mesh2) due to deep cycling
        // Simulate releasing and pressing mouse again at the same spot
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);

        currentSelection = EditorSceneViewPicker.selection;
        Assert.AreEqual(mesh2, currentSelection.mesh, "Second click: Back ProBuilder mesh should be picked (cycled).");
        Assert.AreEqual(1, currentSelection.faces.Count, "Second click: Exactly one face should be selected.");
        Assert.AreEqual(face2, currentSelection.faces.First(), "Second click: The back face should be picked.");

        // --- Third click: Should cycle back to the front mesh (m_Mesh)
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);

        currentSelection = EditorSceneViewPicker.selection;
        Assert.AreEqual(m_Mesh, currentSelection.mesh, "Third click: Front ProBuilder mesh should be picked again (cycled).");
        Assert.AreEqual(1, currentSelection.faces.Count, "Third click: Exactly one face should be selected.");
        Assert.AreEqual(face1, currentSelection.faces.First(), "Third click: The front face should be picked again.");

        UObject.DestroyImmediate(mesh2.gameObject);
    }

    [Test]
    public void FacePicker_DoesNotPickFace_BehindCamera()
    {
        // Move the entire mesh behind the camera's near clip plane
        // m_Mesh at y=11
        m_Mesh.transform.position = new Vector3(0, 11, 0);
        m_Mesh.Refresh();

        m_Mesh.GetComponent<MeshCollider>().sharedMesh = m_Mesh.mesh;
        m_Mesh.GetComponent<MeshCollider>().enabled = true;

        Face faceToTest = m_Mesh.facesInternal[0];
        Vector3 centerOfFace_world = GetFaceCenterWorld(m_Mesh, faceToTest); // Use helper
        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(centerOfFace_world);

        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Face,
            m_PickerPreferences);

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNull(currentSelection.mesh, "No mesh should be selected.");
        Assert.IsEmpty(currentSelection.faces, "No faces should be selected.");
        Assert.IsNull(currentSelection.gameObject, "No GameObject should be selected.");
    }
}
