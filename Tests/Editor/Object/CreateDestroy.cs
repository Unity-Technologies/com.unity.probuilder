using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEngine.TestTools;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.ProBuilder.MeshOperations;

static class CreateDestroy
{
    // Unstable on Ubuntu 16.04, tested locally on 20.04
    [Test, Platform(Exclude = "Linux")]
    public static void DestroyDeletesMesh()
    {
        var pb = ShapeFactory.Instantiate<Cube>();
        Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
        UObject.DestroyImmediate(pb.gameObject);

        // IsNull doesn't work due to c#/c++ goofiness
        Assert.IsTrue(mesh == null);
    }

    [Test]
    public static void DestroyWithNoDeleteFlagPreservesMesh()
    {
        var pb = ShapeFactory.Instantiate<Cube>();

        try
        {
            Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
            pb.preserveMeshAssetOnDestroy = true;
            UObject.DestroyImmediate(pb.gameObject);
            Assert.IsFalse(mesh == null);
        }
        finally
        {
            if (pb != null)
                UObject.DestroyImmediate(pb.gameObject);
        }
    }

    [Test]
    public static void DestroyDoesNotDeleteMeshBackByAsset()
    {
        var pb = ShapeFactory.Instantiate<Cube>();
        string path = TestUtility.SaveAssetTemporary<Mesh>(pb.mesh);
        Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
        UObject.DestroyImmediate(pb.gameObject);
        Assert.IsFalse(mesh == null);
        AssetDatabase.DeleteAsset(path);
        LogAssert.NoUnexpectedReceived();
    }

    [Test, Ignore("Requires ENABLE_DRIVEN_PROPERTIES feature")]
    public static void CreatePrimitive_SetsMeshFilterHideFlags_DontSave()
    {
        var mesh = ShapeFactory.Instantiate<Cube>();
        Assume.That(mesh, Is.Not.Null);
        Assume.That(mesh.filter, Is.Not.Null);
        Assert.That(mesh.filter.hideFlags & HideFlags.DontSave, Is.EqualTo(HideFlags.DontSave));
        UObject.DestroyImmediate(mesh.gameObject);
    }
    
    [Test]
    public static void CreateEmptyPolyShape_SubmeshCountIsZero_MaterialCountIsZero()
    {
        GameObject go = new GameObject("PolyShape");
        var polyShape = go.AddComponent<PolyShape>();
        ProBuilderMesh pb = go.AddComponent<ProBuilderMesh>();

        Assume.That(polyShape, Is.Not.Null, "PolyShape should not be null at this point.");
        Assume.That(pb, Is.Not.Null, "ProBuilderMesh should not be null at this point.");
        
        pb.CreateShapeFromPolygon(polyShape.m_Points, polyShape.extrude, polyShape.flipNormals);
        UnityEditor.ProBuilder.EditorUtility.InitObject(pb);
        
        var zeroSubmeshAndMaterials = (pb.renderer.sharedMaterial == null && pb.mesh.subMeshCount == 0);
        
        Assert.That(zeroSubmeshAndMaterials, Is.True, "Creating empty PolyShape should result in mesh with zero sub meshes and zero materials.");
    }
}