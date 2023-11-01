using UObject = UnityEngine.Object;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

public class UVEditorWindow
{
    ProBuilderMesh m_cube;

    [SetUp]
    public void Setup()
    {
        ToolManager.SetActiveContext<PositionToolContext>();
        UVEditor.MenuOpenUVEditor();

        m_cube = ShapeFactory.Instantiate<Cube>();
        EditorUtility.InitObject(m_cube);
    }

    [TearDown]
    public void Cleanup()
    {
        UObject.DestroyImmediate(m_cube.gameObject);
        ToolManager.SetActiveContext<GameObjectToolContext>();
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

        //Modify those faces
        Vector2 minimalUV = UVEditor.instance.UVSelectionMinimalUV();
        Assert.That(minimalUV, !Is.EqualTo(UVEditor.LowerLeft));

        UVEditor.instance.Menu_PlanarProject();
        minimalUV = UVEditor.instance.UVSelectionMinimalUV();
        Assert.That(minimalUV, Is.EqualTo(UVEditor.LowerLeft));
    }
}
