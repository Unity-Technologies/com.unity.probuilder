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
    public void MergeObjects_WithPolyShapeCompPresent_ResultNoLongerHasPolyShapeComp()
    {
        var meshes = new List<ProBuilderMesh>();
        meshes.Add(m_mesh1);
        meshes.Add(m_mesh2);
        meshes.Add(m_mesh3);

        foreach (var mesh in meshes)
            mesh.gameObject.AddComponent<PolyShape>();

        MeshSelection.SetSelection(new List<GameObject>( new[]{ m_mesh1.gameObject, m_mesh2.gameObject, m_mesh3.gameObject }));
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        var mergeObjectsAction = new MergeObjects();
        var newMeshes = mergeObjectsAction.DoMergeObjectsAction();
        
        var polyShapeFound = newMeshes[0].gameObject.GetComponent<PolyShape>() != null;
        Assert.That(polyShapeFound, Is.Not.True, "There should be no PolyShape component on ProBuilder Meshes after mesh combine.");
    }
}
