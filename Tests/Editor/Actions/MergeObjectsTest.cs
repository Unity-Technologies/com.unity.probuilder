using System.Collections.Generic;
using UObject = UnityEngine.Object;
using UnityEngine;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.Actions;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

public class MergeObjectsTest 
{
    ProBuilderMesh m_mesh1;
    ProBuilderMesh m_mesh2;
    ProBuilderMesh m_mesh3;
    
    [SetUp]
    public void SetUp()
    {
        m_mesh1 = ShapeFactory.Instantiate<Cube>();
        m_mesh2 = ShapeFactory.Instantiate<Cone>();
        m_mesh3 = ShapeFactory.Instantiate<Cylinder>();
        m_mesh1.gameObject.AddComponent<BoxCollider>();
    }

    [TearDown]
    public void Cleanup()
    {
        UObject.DestroyImmediate(m_mesh1);
        UObject.DestroyImmediate(m_mesh2);
        UObject.DestroyImmediate(m_mesh3);
    }
    
    [Test]
    [TestCase(typeof(PolyShape))]
    [TestCase(typeof(ProBuilderShape))]
    public void MergeObjects_WithShapeCompPresent_ResultNoLongerHasShapeComp(System.Type shapeCompType)
    {
        var meshes = new List<ProBuilderMesh>();
        meshes.Add(m_mesh1);
        meshes.Add(m_mesh2);
        meshes.Add(m_mesh3);

        foreach (var mesh in meshes)
        {
            if (shapeCompType == typeof(PolyShape)) 
                mesh.gameObject.AddComponent<PolyShape>();
            else
                mesh.gameObject.AddComponent<ProBuilderShape>();
        }

        MeshSelection.SetSelection(new List<GameObject>( new[]{ m_mesh1.gameObject, m_mesh2.gameObject, m_mesh3.gameObject }));
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        var mergeObjectsAction = new MergeObjects();
        var newMeshes = mergeObjectsAction.DoMergeObjectsAction();

        var shapeCompFound = false; 
        if (shapeCompType == typeof(PolyShape)) 
            shapeCompFound = newMeshes[0].gameObject.GetComponent<PolyShape>() != null;
        else 
            shapeCompFound = newMeshes[0].gameObject.GetComponent<ProBuilderShape>() != null;
        
        Assert.That(shapeCompFound, Is.Not.True, $"There should be no {shapeCompType.Name} component on ProBuilder Meshes after mesh combine.");
    }
}
