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

public class UVEditorWindow
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
        ToolManager.SetActiveContext<GameObjectToolContext>();
        UObject.DestroyImmediate(m_cube.gameObject);
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
}
