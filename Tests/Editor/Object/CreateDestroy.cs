using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEngine.TestTools;
using UnityEditor;
using UnityEngine.ProBuilder;

static class CreateDestroy
{
    [Test]
    public static void DestroyDeletesMesh()
    {
        var pb = ShapeGenerator.CreateShape(ShapeType.Cube);
        Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
        UObject.DestroyImmediate(pb.gameObject);

        // IsNull doesn't work due to c#/c++ goofiness
        Assert.IsTrue(mesh == null);
    }

    [Test]
    public static void DestroyWithNoDeleteFlagPreservesMesh()
    {
        var pb = ShapeGenerator.CreateShape(ShapeType.Cube);

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
        var pb = ShapeGenerator.CreateShape(ShapeType.Cube);
        string path = TestUtility.SaveAssetTemporary<Mesh>(pb.mesh);
        Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
        UObject.DestroyImmediate(pb.gameObject);
        Assert.IsFalse(mesh == null);
        AssetDatabase.DeleteAsset(path);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public static void CreatePrimitive_SetsMeshFilterHideFlags_DontSave()
    {
        var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);
        Assume.That(mesh, Is.Not.Null);
        Assume.That(mesh.filter, Is.Not.Null);
        Assert.That(mesh.filter.hideFlags & HideFlags.DontSave, Is.EqualTo(HideFlags.DontSave));
        UObject.DestroyImmediate(mesh.gameObject);
    }
}
