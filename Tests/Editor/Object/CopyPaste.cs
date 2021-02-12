using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

static class CopyPaste
{
    [Test]
    public static void CopyWithVerify_CreatesNewMeshAsset()
    {
        var original = ShapeFactory.Instantiate<Cube>();
        var copy = UObject.Instantiate(original);

        try
        {
            // optimize after instantiate because Instantiate runs mesh through serialization, introducing tiny rounding
            // errors in some fields. by comparing the results post-serialization we get a more accurate diff
            original.Optimize();
            EditorUtility.SynchronizeWithMeshFilter(copy);
            Assert.AreNotEqual(copy, original, "GameObject references are equal");
            Assert.IsFalse(ReferenceEquals(copy.mesh, original.mesh), "Mesh references are equal");
            TestUtility.AssertAreEqual(original.mesh, copy.mesh);
        }
        finally
        {
            UObject.DestroyImmediate(original.gameObject);
            UObject.DestroyImmediate(copy.gameObject);
        }
    }

    [Test]
    public static void Copy_ReferencesOriginalMesh()
    {
        var original = ShapeFactory.Instantiate<Cube>();
        var copy = UObject.Instantiate(original);

        try
        {
            Assert.AreNotEqual(copy, original, "GameObject references are equal");
            Assert.IsTrue(ReferenceEquals(copy.mesh, original.mesh), "Mesh references are equal");
        }
        finally
        {
            UObject.DestroyImmediate(original.gameObject);
            UObject.DestroyImmediate(copy.gameObject);
        }
    }
}
