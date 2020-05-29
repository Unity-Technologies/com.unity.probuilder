using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

class SelectMaterial
{
    ProBuilderMesh[] selectables;

    [SetUp]
    public void Setup()
    {
        ProBuilderMesh shape1 = ShapeGenerator.CreateShape(ShapeType.Cube);
        shape1.transform.position = Vector3.zero - shape1.GetComponent<MeshRenderer>().bounds.center;

        ProBuilderMesh shape2 = ShapeGenerator.CreateShape(ShapeType.Cube);
        shape2.transform.position = Vector3.zero - shape2.GetComponent<MeshRenderer>().bounds.center;

        shape1.AddToFaceSelection(0);
        shape1.AddToFaceSelection(1);
        shape1.AddToFaceSelection(2);
        shape1.SetMaterial(shape1.GetSelectedFaces(), BuiltinMaterials.facePickerMaterial);
        shape1.ClearSelection();
        shape1.AddToFaceSelection(3);
        shape1.AddToFaceSelection(4);
        shape1.AddToFaceSelection(5);
        shape1.SetMaterial(shape1.GetSelectedFaces(), null);
        shape1.ClearSelection();

        shape2.AddToFaceSelection(0);
        shape2.AddToFaceSelection(1);
        shape2.AddToFaceSelection(2);
        shape2.SetMaterial(shape2.GetSelectedFaces(), BuiltinMaterials.colliderMaterial);
        shape2.ClearSelection();
        shape2.AddToFaceSelection(3);
        shape2.AddToFaceSelection(4);
        shape2.AddToFaceSelection(5);
        shape2.SetMaterial(shape2.GetSelectedFaces(), BuiltinMaterials.facePickerMaterial);
        shape2.ClearSelection();

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
        {
            UObject.DestroyImmediate(selectables[i].gameObject);
        }
    }

    [Test]
    public void SelectMaterial_WithMaterial()
    {
        //Make first faces selected
        ProBuilderMesh mesh = selectables[0];
        MeshSelection.SetSelection(mesh.gameObject);
        mesh.AddToFaceSelection(0);
        MeshSelection.OnObjectSelectionChanged();

        UnityEditor.ProBuilder.Actions.SelectMaterial selectMaterial = new UnityEditor.ProBuilder.Actions.SelectMaterial();
        var oldValue = selectMaterial.m_RestrictToSelectedObjects.value;
        selectMaterial.m_RestrictToSelectedObjects.value = false;
        selectMaterial.DoAction();
        selectMaterial.m_RestrictToSelectedObjects.value = oldValue;

        //We need to force the object selection change here to ensure that MeshSelection reflect the result
        //of the action, which typically would notify the MeshSelection asynchronously.
        MeshSelection.OnObjectSelectionChanged();

        Assert.That(MeshSelection.selectedObjectCount, Is.EqualTo(2));
        Assert.That(mesh.selectedFaceCount, Is.EqualTo(3));
        Assert.That(mesh.selectedFaceIndexes.IndexOf(0), !Is.EqualTo(-1));
        Assert.That(mesh.selectedFaceIndexes.IndexOf(1), !Is.EqualTo(-1));
        Assert.That(mesh.selectedFaceIndexes.IndexOf(2), !Is.EqualTo(-1));
        Assert.That(selectables[1].selectedFaceCount, Is.EqualTo(3));
        Assert.That(selectables[1].selectedFaceIndexes.IndexOf(3), !Is.EqualTo(-1));
        Assert.That(selectables[1].selectedFaceIndexes.IndexOf(4), !Is.EqualTo(-1));
        Assert.That(selectables[1].selectedFaceIndexes.IndexOf(5), !Is.EqualTo(-1));
    }

    [Test]
    public void SelectMaterial_WithNullMaterial()
    {
        //Make first faces selected
        ProBuilderMesh mesh = selectables[0];
        MeshSelection.SetSelection(mesh.gameObject);
        mesh.AddToFaceSelection(3);
        MeshSelection.OnObjectSelectionChanged();

        UnityEditor.ProBuilder.Actions.SelectMaterial selectMaterial = new UnityEditor.ProBuilder.Actions.SelectMaterial();
        var oldValue = selectMaterial.m_RestrictToSelectedObjects.value;
        selectMaterial.m_RestrictToSelectedObjects.value = false;
        selectMaterial.DoAction();
        selectMaterial.m_RestrictToSelectedObjects.value = oldValue;

        //We need to force the object selection change here to ensure that MeshSelection reflect the result
        //of the action, which typically would notify the MeshSelection asynchronously.
        MeshSelection.OnObjectSelectionChanged();

        Assert.That(MeshSelection.selectedObjectCount, Is.EqualTo(1));
        Assert.That(mesh.selectedFaceCount, Is.EqualTo(3));
        Assert.That(mesh.selectedFaceIndexes.IndexOf(3), !Is.EqualTo(-1));
        Assert.That(mesh.selectedFaceIndexes.IndexOf(4), !Is.EqualTo(-1));
        Assert.That(mesh.selectedFaceIndexes.IndexOf(5), !Is.EqualTo(-1));
        Assert.That(selectables[1].selectedFaceCount, Is.EqualTo(0));
    }
}
