using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.ProBuilder.Tests;
#if UNITY_EDITOR && PB_CREATE_TEST_MESH_TEMPLATES
using UnityEngine.ProBuilder.Tests.Framework;
#endif

static class VertexColorTests
{
    [Test]
    public static void DuplicateMesh_ApplyColor_MatchesTemplate()
    {
        var cube = ShapeFactory.Instantiate<Cube>();
        var dup = UObject.Instantiate(cube.gameObject).GetComponent<ProBuilderMesh>();

        dup.SetFaceColor(dup.faces[0], Color.blue);

        dup.ToMesh();
        dup.Refresh();

#if UNITY_EDITOR && PB_CREATE_TEST_MESH_TEMPLATES
        TestUtility.SaveAssetTemplate(dup.mesh, dup.name);
#endif
        RuntimeUtility.AssertMeshAttributesValid(dup.mesh);
        var compare = Resources.Load<Mesh>(RuntimeUtility.GetResourcesPath<Mesh>(dup.name));
        Assert.IsNotNull(compare);
        RuntimeUtility.AssertAreEqual(compare, dup.mesh);

        UObject.DestroyImmediate(cube);
        UObject.DestroyImmediate(dup);
    }
}
