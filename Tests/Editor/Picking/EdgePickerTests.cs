using System;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object; // Use UnityEngine.Object for clarity
using NUnit.Framework;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.TestTools;

[TestFixture]
public class EdgePickerTests
{
    private ProBuilderMesh m_Mesh;
    private Camera m_Camera;
    private ScenePickerPreferences m_PickerPreferences;

    [SetUp]
    public void Setup()
    {
        var window = EditorWindow.GetWindow<SceneView>();
        window.Show(false);
        window.Repaint();
        window.Focus();

        m_Camera = new GameObject("TestCamera", typeof(Camera)).GetComponent<Camera>();
        m_Camera.transform.position = new Vector3(0, 0, -10);
        m_Camera.transform.LookAt(Vector3.zero);
        m_Camera.orthographic = false;
        m_Camera.cullingMask = ~0;

        m_Mesh = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Plane>();
        m_Mesh.name = "TestPlane";
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
    public void EdgePicker_PicksVisibleEdge()
    {
        Edge edgeToPick = m_Mesh.facesInternal[0].edgesInternal[0];

        Vector3 pA_world = m_Mesh.transform.TransformPoint(m_Mesh.positionsInternal[edgeToPick.a]);
        Vector3 pB_world = m_Mesh.transform.TransformPoint(m_Mesh.positionsInternal[edgeToPick.b]);

        Vector3 centerOfEdge_world = (pA_world + pB_world) / 2f;
        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(centerOfEdge_world);

        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Edge,
            m_PickerPreferences);

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNotNull(currentSelection.mesh, "A mesh should be selected.");
        Assert.AreEqual(m_Mesh, currentSelection.mesh, "The correct mesh should be selected.");
        Assert.IsNotNull(currentSelection.edges, "Edges collection should not be null.");
        Assert.AreEqual(1, currentSelection.edges.Count, "Exactly one edge should be selected.");
        Assert.AreEqual(edgeToPick, currentSelection.edges.First(), "The expected edge should be picked.");
    }

    [Test]
    public void EdgePicker_DoesNotPickWhenNotHovering()
    {
        Vector2 mousePos = new Vector2(Screen.width / 2f, Screen.height / 2f + 500f);

        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Edge,
            m_PickerPreferences);

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNull(currentSelection.mesh, "No mesh should be selected.");
        Assert.IsEmpty(currentSelection.edges, "No edges should be selected.");
    }

    [Test]
    public void EdgePicker_PicksCorrectEdgeWithMultipleOverlapping()
    {
        m_Camera.transform.position = new Vector3(0.5f, 0.5f, -10);
        m_Camera.transform.LookAt(Vector3.zero);

        ProBuilderMesh mesh2 = ShapeFactory.Instantiate<UnityEngine.ProBuilder.Shapes.Plane>();
        mesh2.name = "TestPlane2";
        mesh2.transform.position = new Vector3(0.1f, 0.1f, 0.5f);
        mesh2.Refresh();

        var meshCollider2 = mesh2.gameObject.GetComponent<MeshCollider>();
        if (meshCollider2 == null) meshCollider2 = mesh2.gameObject.AddComponent<MeshCollider>();
        meshCollider2.sharedMesh = mesh2.mesh;
        meshCollider2.enabled = true;

        MeshSelection.AddToSelection(mesh2.gameObject);

        Edge edge1 = m_Mesh.facesInternal[0].edgesInternal[0];
        Vector3 pA1_world = m_Mesh.transform.TransformPoint(m_Mesh.positionsInternal[edge1.a]);
        Vector3 pB1_world = m_Mesh.transform.TransformPoint(m_Mesh.positionsInternal[edge1.b]);
        Vector3 center1_world = (pA1_world + pB1_world) / 2f;

        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(center1_world);

        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Edge,
            m_PickerPreferences);

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNotNull(currentSelection.mesh, "An edge should be picked.");
        Assert.AreEqual(m_Mesh, currentSelection.mesh, "The closer mesh (m_Mesh) should be picked.");
        Assert.IsNotNull(currentSelection.edges, "Edges collection should not be null.");
        Assert.AreEqual(1, currentSelection.edges.Count, "Exactly one edge should be selected.");
        Assert.AreEqual(edge1, currentSelection.edges.First(), "The expected edge (from m_Mesh) should be picked.");

        UObject.DestroyImmediate(mesh2.gameObject);
    }

    [Test]
    public void EdgePicker_PicksClippedEdge_OnePointBehindCamera()
    {

        m_Camera.nearClipPlane = 0.1f;
        m_Camera.transform.position = new Vector3(0, 0, -0.5f);
        m_Camera.transform.LookAt(Vector3.zero);

        Edge targetEdge = m_Mesh.facesInternal[0].edgesInternal[0];

        Vector3[] currentLocalPositions = m_Mesh.positionsInternal;
        currentLocalPositions[targetEdge.a] = new Vector3(currentLocalPositions[targetEdge.a].x, currentLocalPositions[targetEdge.a].y, -0.6f);

        m_Mesh.Rebuild();

        m_Mesh.GetComponent<MeshCollider>().sharedMesh = m_Mesh.mesh;
        m_Mesh.GetComponent<MeshCollider>().enabled = true;

        Vector3 testPointOnClippedEdge_world = m_Mesh.transform.TransformPoint(-0.5f, 0, -0.3f);
        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(testPointOnClippedEdge_world);

        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Edge,
            m_PickerPreferences);

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNotNull(currentSelection.mesh, "Clipped edge should be picked.");
        Assert.AreEqual(m_Mesh, currentSelection.mesh, "The correct mesh should be selected.");
        Assert.IsNotNull(currentSelection.edges, "Edges collection should not be null.");
        Assert.AreEqual(1, currentSelection.edges.Count, "Exactly one edge should be selected.");
        Assert.AreEqual(targetEdge, currentSelection.edges.First(), "The expected clipped edge should be picked.");
    }

    [Test]
    public void EdgePicker_DoesNotPickEdge_BothPointsBehindCamera()
    {
        m_Camera.nearClipPlane = 0.1f;
        m_Camera.transform.position = new Vector3(0, 0, -0.5f);
        m_Camera.transform.LookAt(Vector3.zero);

        m_Mesh.transform.position = new Vector3(0, 0, -1.0f);
        m_Mesh.Refresh();

        m_Mesh.GetComponent<MeshCollider>().sharedMesh = m_Mesh.mesh;
        m_Mesh.GetComponent<MeshCollider>().enabled = true;

        Edge edgeToTest = m_Mesh.facesInternal[0].edgesInternal[0];
        Vector3 pA_world = m_Mesh.transform.TransformPoint(m_Mesh.positionsInternal[edgeToTest.a]);
        Vector3 pB_world = m_Mesh.transform.TransformPoint(m_Mesh.positionsInternal[edgeToTest.b]);

        Vector3 centerOfEdge_world = (pA_world + pB_world) / 2f;
        Vector2 mousePos = UnityEditor.HandleUtility.WorldToGUIPoint(centerOfEdge_world);

        UnityEngine.TestTools.LogAssert.Expect("Handles.GetClosestPickingID called outside an editor OnGUI");
        UnityEngine.TestTools.LogAssert.Expect("Assertion failed on expression: 'device.IsInsideFrame()'");
        EditorSceneViewPicker.DoMouseClick(
            CreateMouseEvent(mousePos, EventType.MouseDown, EventModifiers.None),
            SelectMode.Edge,
            m_PickerPreferences);

        SceneSelection currentSelection = EditorSceneViewPicker.selection;

        Assert.IsNull(currentSelection.mesh, "No mesh should be selected.");
        Assert.IsEmpty(currentSelection.edges, "No edges should be selected.");
    }
}
