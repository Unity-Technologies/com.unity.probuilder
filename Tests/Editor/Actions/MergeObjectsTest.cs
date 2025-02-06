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
    
    [SetUp]
    public void SetUp()
    {
        m_mesh1 = ShapeFactory.Instantiate<Cube>();
        m_mesh2 = ShapeFactory.Instantiate<Cone>();
    }

    [TearDown]
    public void Cleanup()
    {
        UObject.DestroyImmediate(m_mesh1);
        UObject.DestroyImmediate(m_mesh2);
    }
    
    [Test]
    [TestCase(typeof(PolyShape),typeof(PolyShape))] 
    [TestCase(typeof(ProBuilderShape),typeof(ProBuilderShape))] 
    [TestCase(typeof(ProBuilderShape),typeof(PolyShape))]
    public void MergeObjects_WithShapeCompPresent_ResultNoLongerHasShapeComp(System.Type shapeCompTypeA, System.Type shapeCompTypeB)
    {
        void AttachComponentOfType(ProBuilderMesh mesh, System.Type shapeCompType)
        {
            if (shapeCompType == typeof(PolyShape)) 
                mesh.gameObject.AddComponent<PolyShape>();
            else
                mesh.gameObject.AddComponent<ProBuilderShape>();
        }
        
        var meshes = new List<ProBuilderMesh>();
        meshes.Add(m_mesh1);
        meshes.Add(m_mesh2);
        
        Assume.That(meshes.Count, Is.EqualTo(2));

        AttachComponentOfType(meshes[0], shapeCompTypeA);
        AttachComponentOfType(meshes[1], shapeCompTypeB);
        
        MeshSelection.SetSelection(new List<GameObject>( new[]{ m_mesh1.gameObject, m_mesh2.gameObject }));
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        var mergeObjectsAction = new MergeObjects();
        var newMeshes = mergeObjectsAction.DoMergeObjectsAction();
        
        Assume.That(newMeshes.Count, Is.EqualTo(1), "There should only be one mesh after merging objects.");

        var shapeCompFound = false; 
        shapeCompFound |= newMeshes[0].gameObject.GetComponent<PolyShape>() != null;
        shapeCompFound |= newMeshes[0].gameObject.GetComponent<ProBuilderShape>() != null;
        
        Assert.That(shapeCompFound, Is.Not.True, $"There should be no {shapeCompTypeA.Name} or {shapeCompTypeB.Name} component on ProBuilder Meshes after mesh combine.");
    }
}
