using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.ProBuilder.Tests;
using UnityEngine.ProBuilder.Tests.Framework;
#if UNITY_6000_7_OR_NEWER
using UnityEngine.TestTools;
#endif

#if UNITY_6000_7_OR_NEWER
[UnityCoreClrExplicitDisabled("https://jira.unity3d.com/browse/UUM-148933", "ProBuilder test mesh templates resolve their asset path from StackTrace calling-method info, which fails on CoreCLR")]
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
        var compare = TestUtility.GetAssetTemplate<Mesh>(dup.name);
        Assert.IsNotNull(compare);
        RuntimeUtility.AssertAreEqual(compare, dup.mesh);

        UObject.DestroyImmediate(cube);
        UObject.DestroyImmediate(dup);
    }
}
