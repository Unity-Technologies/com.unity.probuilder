using UObject = UnityEngine.Object;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

[TestFixture]
public class UVEditorTests
{
    ProBuilderMesh m_cube;

    [SetUp]
    public void Setup()
    {
        m_cube = ShapeFactory.Instantiate<Cube>();
        EditorUtility.InitObject(m_cube);
	    // Unsure UV bounds origin is not at (0,0) lower left
        foreach (var face in m_cube.facesInternal)
            face.uv = new AutoUnwrapSettings(face.uv) { anchor = AutoUnwrapSettings.Anchor.UpperLeft, offset = new Vector2(-0.5f, -0.5f) };
        m_cube.RefreshUV(m_cube.faces);
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        ToolManager.SetActiveContext<PositionToolContext>();
        UVEditor.MenuOpenUVEditor();
    }

    [TearDown]
    public void Cleanup()
    {
        // Close the UV Editor window first
        if (UVEditor.instance != null)
        {
            UVEditor.instance.Close();
        }

        // Clear ProBuilder selections
        MeshSelection.ClearElementSelection();
        Selection.activeGameObject = null;

        // Reset tool context
        ToolManager.SetActiveContext<GameObjectToolContext>();

        // Destroy the cube
        if (m_cube != null && m_cube.gameObject != null)
        {
            UObject.DestroyImmediate(m_cube.gameObject);
        }

        m_cube = null;

        // Clear undo to prevent resurrection
        Undo.ClearAll();
    }

    [Test]
    public void Manual_BoxProjection()
    {
        //Select faces
        List<Face> selectedFaces = new List<Face>();
        selectedFaces.Add(m_cube.faces[2]);
        selectedFaces.Add(m_cube.faces[4]);
        selectedFaces.Add(m_cube.faces[5]);
        MeshSelection.SetSelection(m_cube.gameObject);
        m_cube.SetSelectedFaces(selectedFaces);
        MeshSelection.OnObjectSelectionChanged();

        foreach (Face f in selectedFaces)
        {
            Assert.That(f.manualUV, Is.EqualTo(false));
        }

        //Select faces
        UVEditor.instance.Menu_SetManualUV();

        foreach (Face f in selectedFaces)
        {
            Assert.That(f.manualUV, Is.EqualTo(true));
        }

        //Modify those faces
        Vector2 minimalUV = UVEditor.instance.UVSelectionMinimalUV();
        Assert.That(minimalUV, !Is.EqualTo(UVEditor.LowerLeft));

        UVEditor.instance.Menu_BoxProject();
        minimalUV = UVEditor.instance.UVSelectionMinimalUV();
        Assert.That(minimalUV, Is.EqualTo(UVEditor.LowerLeft));
    }

    [Test]
    public void Manual_PlanarProjection()
    {
        //Select faces
        List<Face> selectedFaces = new List<Face>();
        selectedFaces.Add(m_cube.faces[2]);
        selectedFaces.Add(m_cube.faces[4]);
        selectedFaces.Add(m_cube.faces[5]);
        MeshSelection.SetSelection(m_cube.gameObject);
        m_cube.SetSelectedFaces(selectedFaces);
        MeshSelection.OnObjectSelectionChanged();

        foreach (Face f in selectedFaces)
        {
            Assert.That(f.manualUV, Is.EqualTo(false));
        }

        //Select faces
        UVEditor.instance.Menu_SetManualUV();

        foreach (Face f in selectedFaces)
        {
            Assert.That(f.manualUV, Is.EqualTo(true));
        }

	//Confirm that UV bounds origin are not at the LowerLeft corner
        Vector2 minimalUV = UVEditor.instance.UVSelectionMinimalUV();
        Assert.That(minimalUV, !Is.EqualTo(UVEditor.LowerLeft));

        //Do UV projection - UVs should align with LowerLeft corner
        UVEditor.instance.Menu_PlanarProject();
        minimalUV = UVEditor.instance.UVSelectionMinimalUV();
        Assert.That(minimalUV, Is.EqualTo(UVEditor.LowerLeft));
    }

    /// <summary>
    /// Test that moving a single unconnected face doesn't cause distortion
    /// </summary>
    [Test]
    public void MoveSingleFace_PreservesRelativePositions()
    {
        // Setup: One face in manual UV mode, NOT in a texture group
        var face0 = m_cube.facesInternal[0];

        face0.manualUV = false;
        face0.textureGroup = -1; // No texture group (isolated face)

        m_cube.ToMesh();
        m_cube.Refresh();

        // Select the face
        MeshSelection.SetSelection(m_cube.gameObject);
        m_cube.SetSelectedFaces(new Face[] { face0 });
        MeshSelection.OnObjectSelectionChanged();

        // Capture initial UV positions
        var face0InitialUVs = UnityEngine.ProBuilder.ArrayUtility.ValuesWithIndexes(
            m_cube.texturesInternal, face0.distinctIndexesInternal);

        // Calculate initial relative offsets within the face
        Vector2[] face0InitialOffsets = new Vector2[face0InitialUVs.Length];
        for (int i = 0; i < face0InitialUVs.Length; i++)
            face0InitialOffsets[i] = face0InitialUVs[i] - face0InitialUVs[0];

        // Simulate a move operation
        Vector2 moveDelta = new Vector2(0.1f, 0.2f);
        UVEditor.instance.SceneMoveTool(moveDelta);

        // Get final UV positions
        var face0FinalUVs = UnityEngine.ProBuilder.ArrayUtility.ValuesWithIndexes(
            m_cube.texturesInternal, face0.distinctIndexesInternal);

        // TEST 1: Verify each vertex moved by the delta
        for (int i = 0; i < face0InitialUVs.Length; i++)
        {
            Assert.That(face0FinalUVs[i].x, Is.EqualTo(face0InitialUVs[i].x + moveDelta.x).Within(0.0001f),
                $"Vertex {i} X should move by delta");
            Assert.That(face0FinalUVs[i].y, Is.EqualTo(face0InitialUVs[i].y + moveDelta.y).Within(0.0001f),
                $"Vertex {i} Y should move by delta");
        }

        // TEST 2: Verify relative offsets within the face are preserved (no distortion)
        for (int i = 0; i < face0FinalUVs.Length; i++)
        {
            Vector2 finalOffset = face0FinalUVs[i] - face0FinalUVs[0];
            Assert.That(finalOffset.x, Is.EqualTo(face0InitialOffsets[i].x).Within(0.0001f),
                $"Vertex {i} relative X offset changed - face was distorted!");
            Assert.That(finalOffset.y, Is.EqualTo(face0InitialOffsets[i].y).Within(0.0001f),
                $"Vertex {i} relative Y offset changed - face was distorted!");
        }

        // Cleanup
        UVEditor.instance.OnFinishUVModification();
    }

    [Test]
    public void MoveConnectedFaces_PreservesRelativePositions()
    {
        // Setup: Two faces in the same texture group (Auto UV mode)
        var face0 = m_cube.facesInternal[0];
        var face1 = m_cube.facesInternal[1];

        face0.manualUV = false;
        face1.manualUV = false;
        face0.textureGroup = 1;
        face1.textureGroup = 1;

        m_cube.ToMesh();
        m_cube.Refresh();

        // Select ONLY face0
        MeshSelection.SetSelection(m_cube.gameObject);
        m_cube.SetSelectedFaces(new Face[] { face0 });
        MeshSelection.OnObjectSelectionChanged();

        // Capture initial UV positions of BOTH faces
        var face0InitialUVs = UnityEngine.ProBuilder.ArrayUtility.ValuesWithIndexes(
            m_cube.texturesInternal, face0.distinctIndexesInternal);
        var face1InitialUVs = UnityEngine.ProBuilder.ArrayUtility.ValuesWithIndexes(
            m_cube.texturesInternal, face1.distinctIndexesInternal);

        // Calculate initial relative offsets within each face
        Vector2[] face0InitialOffsets = new Vector2[face0InitialUVs.Length];
        for (int i = 0; i < face0InitialUVs.Length; i++)
            face0InitialOffsets[i] = face0InitialUVs[i] - face0InitialUVs[0];

        Vector2[] face1InitialOffsets = new Vector2[face1InitialUVs.Length];
        for (int i = 0; i < face1InitialUVs.Length; i++)
            face1InitialOffsets[i] = face1InitialUVs[i] - face1InitialUVs[0];

        // Calculate initial distance between the two faces
        Vector2 face0InitialCenter = Bounds2D.Center(face0InitialUVs);
        Vector2 face1InitialCenter = Bounds2D.Center(face1InitialUVs);
        Vector2 initialCenterDistance = face1InitialCenter - face0InitialCenter;

        // Simulate a move operation
        Vector2 moveDelta = new Vector2(0.1f, 0.2f);
        UVEditor.instance.SceneMoveTool(moveDelta);

        // Get final UV positions
        var face0FinalUVs = UnityEngine.ProBuilder.ArrayUtility.ValuesWithIndexes(
            m_cube.texturesInternal, face0.distinctIndexesInternal);
        var face1FinalUVs = UnityEngine.ProBuilder.ArrayUtility.ValuesWithIndexes(
            m_cube.texturesInternal, face1.distinctIndexesInternal);

        // TEST 1: Verify each vertex in face0 moved by the delta
        for (int i = 0; i < face0InitialUVs.Length; i++)
        {
            Assert.That(face0FinalUVs[i].x, Is.EqualTo(face0InitialUVs[i].x + moveDelta.x).Within(0.0001f),
                $"Face0 vertex {i} X should move by delta");
            Assert.That(face0FinalUVs[i].y, Is.EqualTo(face0InitialUVs[i].y + moveDelta.y).Within(0.0001f),
                $"Face0 vertex {i} Y should move by delta");
        }

        // TEST 2: Verify each vertex in face1 moved by the delta (auto-selected)
        for (int i = 0; i < face1InitialUVs.Length; i++)
        {
            Assert.That(face1FinalUVs[i].x, Is.EqualTo(face1InitialUVs[i].x + moveDelta.x).Within(0.0001f),
                $"Face1 vertex {i} X should move by delta");
            Assert.That(face1FinalUVs[i].y, Is.EqualTo(face1InitialUVs[i].y + moveDelta.y).Within(0.0001f),
                $"Face1 vertex {i} Y should move by delta");
        }

        // TEST 3: Verify relative offsets within face0 are preserved (no distortion)
        for (int i = 0; i < face0FinalUVs.Length; i++)
        {
            Vector2 finalOffset = face0FinalUVs[i] - face0FinalUVs[0];
            Assert.That(finalOffset.x, Is.EqualTo(face0InitialOffsets[i].x).Within(0.0001f),
                $"Face0 vertex {i} relative X offset changed - face was distorted!");
            Assert.That(finalOffset.y, Is.EqualTo(face0InitialOffsets[i].y).Within(0.0001f),
                $"Face0 vertex {i} relative Y offset changed - face was distorted!");
        }

        // TEST 4: Verify relative offsets within face1 are preserved (no distortion)
        for (int i = 0; i < face1FinalUVs.Length; i++)
        {
            Vector2 finalOffset = face1FinalUVs[i] - face1FinalUVs[0];
            Assert.That(finalOffset.x, Is.EqualTo(face1InitialOffsets[i].x).Within(0.0001f),
                $"Face1 vertex {i} relative X offset changed - face was distorted!");
            Assert.That(finalOffset.y, Is.EqualTo(face1InitialOffsets[i].y).Within(0.0001f),
                $"Face1 vertex {i} relative Y offset changed - face was distorted!");
        }

        // TEST 5: Verify the distance between face centers is preserved
        Vector2 face0FinalCenter = Bounds2D.Center(face0FinalUVs);
        Vector2 face1FinalCenter = Bounds2D.Center(face1FinalUVs);
        Vector2 finalCenterDistance = face1FinalCenter - face0FinalCenter;

        Assert.That(finalCenterDistance.x, Is.EqualTo(initialCenterDistance.x).Within(0.0001f),
            "Distance between face centers X changed - faces were recentered!");
        Assert.That(finalCenterDistance.y, Is.EqualTo(initialCenterDistance.y).Within(0.0001f),
            "Distance between face centers Y changed - faces were recentered!");

        // Cleanup
        UVEditor.instance.OnFinishUVModification();
    }
}
