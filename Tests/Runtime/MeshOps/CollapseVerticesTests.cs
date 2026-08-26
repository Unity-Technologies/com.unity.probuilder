using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.ProBuilder.Tests;
using UnityEngine.ProBuilder.Tests.Framework;
#if UNITY_6000_7_OR_NEWER
using UnityEngine.TestTools;
#endif

#if UNITY_6000_7_OR_NEWER
[UnityCoreClrExplicitDisabled("https://jira.unity3d.com/browse/UUM-148933", "ProBuilder test mesh templates resolve their asset path from StackTrace calling-method info, which fails on CoreCLR")]
#endif
static class CollapseVerticesTests
{
    [Test]
    public static void CollapseToFirst_MatchesTemplate()
    {
        var cube = ShapeFactory.Instantiate<Cube>();
        var res = cube.MergeVertices(new[] { 0, 1 }, true);

        Assert.AreEqual(3, res);

        cube.ToMesh();
        cube.Refresh();

#if UNITY_EDITOR && PB_CREATE_TEST_MESH_TEMPLATES
        TestUtility.SaveAssetTemplate(cube.mesh, cube.name);
#endif
        RuntimeUtility.AssertMeshAttributesValid(cube.mesh);
        var template = TestUtility.GetAssetTemplate<Mesh>(cube.name);
        Assert.IsNotNull(template);
        RuntimeUtility.AssertAreEqual(template, cube.mesh);

        Object.DestroyImmediate(cube);
    }

    [Test]
    public static void CollapseToCenter_MatchesTemplate()
    {
        var cube = ShapeFactory.Instantiate<Cube>();
        cube.MergeVertices(new[] { 0, 1 });

        cube.ToMesh();
        cube.Refresh();

#if UNITY_EDITOR && PB_CREATE_TEST_MESH_TEMPLATES
        TestUtility.SaveAssetTemplate(cube.mesh, cube.name);
#endif
        RuntimeUtility.AssertMeshAttributesValid(cube.mesh);
        var template = TestUtility.GetAssetTemplate<Mesh>(cube.name);
        Assert.IsNotNull(template);
        RuntimeUtility.AssertAreEqual(template, cube.mesh);

        UnityEngine.Object.DestroyImmediate(cube);
    }
}
