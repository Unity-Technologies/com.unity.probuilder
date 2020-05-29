using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

class SelectFacesByColor
{
    ProBuilderMesh[] selectables;
    List<Color> faceColors;

    void Setup(bool withColor = false)
    {
        ProBuilderMesh shape1 = ShapeGenerator.CreateShape(ShapeType.Cube);
        shape1.transform.position = Vector3.zero - shape1.GetComponent<MeshRenderer>().bounds.center;

        ProBuilderMesh shape2 = ShapeGenerator.CreateShape(ShapeType.Cube);
        shape2.transform.position = Vector3.zero - shape2.GetComponent<MeshRenderer>().bounds.center;

        if (withColor)
        {
            //Cube has 6 faces
            faceColors = new List<Color>();
            faceColors.Add(Color.red);
            faceColors.Add(Color.green);
            faceColors.Add(Color.blue);
            faceColors.Add(Color.grey);
            faceColors.Add(Color.cyan);
            faceColors.Add(Color.black);

            for (var i = 0; i < 6; ++i)
            {
                shape1.SetFaceColor(shape1.facesInternal[i], faceColors[i]);
            }

            for (var i = 0; i < 6; ++i)
            {
                shape2.SetFaceColor(shape2.facesInternal[i], faceColors[i]);
            }
        }

        selectables = new ProBuilderMesh[]
        {
            shape1,
            shape2
        };
    }

    [TearDown]
    public void Cleanup()
    {
        for (int i = 0; i < selectables.Length; i++)
            UObject.DestroyImmediate(selectables[i].gameObject);
    }

    [Test]
    public void SelectFaces_WithoutColor()
    {
        Setup();

        //Make first faces selected
        ProBuilderMesh mesh = selectables[0];
        Assert.IsNotNull(mesh.faces);
        Face face = selectables[0].faces[0];
        List<Face> selectedFaces = new List<Face>();
        selectedFaces.Add(face);
        mesh.SetSelectedFaces(selectedFaces);
        Assert.AreEqual(mesh.selectedFaceCount, 1);
        MeshSelection.SetSelection(mesh.gameObject);
        MeshSelection.OnObjectSelectionChanged();

        foreach (var currObject in selectables)
        {
            //Validate that prior not all faces are selected
            Assert.AreNotEqual(currObject.selectedFacesInternal.Length, 6);
        }

        UnityEditor.ProBuilder.Actions.SelectVertexColor selectColorAction = new UnityEditor.ProBuilder.Actions.SelectVertexColor();
        selectColorAction.DoAction();

        foreach (var currObject in selectables)
        {
            //All selectable object should have all faces selected
            Assert.AreEqual(currObject.selectedFacesInternal.Length, 6);
        }
    }

    [Test]
    public void SelectFaces_WithColor()
    {
        Setup(true /*with color*/);

        //Make first faces selected
        ProBuilderMesh mesh = selectables[0];
        Assert.IsNotNull(mesh.faces);
        Face face = selectables[0].faces[0];
        List<Face> selectedFaces = new List<Face>();
        selectedFaces.Add(face);
        mesh.SetSelectedFaces(selectedFaces);
        Assert.AreEqual(mesh.selectedFaceCount, 1);
        MeshSelection.SetSelection(mesh.gameObject);
        MeshSelection.OnObjectSelectionChanged();

        //Validate that prior only a face on first cube is selected
        Assert.AreEqual(selectables[0].selectedFacesInternal.Length, 1);
        Assert.AreEqual(selectables[1].selectedFacesInternal.Length, 0);

        UnityEditor.ProBuilder.Actions.SelectVertexColor selectColorAction = new UnityEditor.ProBuilder.Actions.SelectVertexColor();
        selectColorAction.DoAction();

        //Validate that after a face is selected on both cube
        Color[] colors0 = selectables[0].colorsInternal;
        Color[] colors1 = selectables[1].colorsInternal;

        Assert.AreEqual(selectables[0].selectedFacesInternal.Length, 1);
        Assert.AreEqual(selectables[1].selectedFacesInternal.Length, 1);

        int[] tris0 = selectables[0].selectedFacesInternal[0].distinctIndexesInternal;
        int[] tris1 = selectables[1].selectedFacesInternal[0].distinctIndexesInternal;
        Assert.AreEqual(tris0.Length, tris1.Length);

        //Validate that the face match
        for (int n = 0; n < tris0.Length; n++)
        {
            Assert.AreEqual(colors0[tris0[n]], colors1[tris1[n]]);
            Assert.AreEqual(colors0[tris0[n]], faceColors[0]);
        }
    }
}
