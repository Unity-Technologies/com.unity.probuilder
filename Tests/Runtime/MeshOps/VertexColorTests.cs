using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.ProBuilder.Tests.Framework;

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

#if PB_CREATE_TEST_MESH_TEMPLATES
        TestUtility.SaveAssetTemplate(dup.mesh, dup.name);
#endif
        var compare = TestUtility.GetAssetTemplate<Mesh>(dup.name);

        TestUtility.AssertAreEqual(compare, dup.mesh);

        UObject.DestroyImmediate(cube);
        UObject.DestroyImmediate(dup);
    }
}
