using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Test;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOps.Edge
{
	static class Bridge
	{
		[Test]
		public static void BridgeQuad()
		{
			var cube = ShapeGenerator.CreateShape(ShapeType.Cube);
			cube.DeleteFace(cube.faces[0]);
			var holes = ElementSelection.FindHoles(cube, cube.sharedVertexesInternal.Select(x => x[0]));
			Assert.AreEqual(1, holes.Count, "found exactly 1 hole");
			var bridgedFace = cube.Bridge(holes[0][0], holes[0][2]);
			Assert.IsNotNull(bridgedFace);
			Assert.AreEqual(4, bridgedFace.edgesInternal.Length);
			cube.ToMesh();
			cube.Refresh();

#if PB_CREATE_TEST_MESH_TEMPLATES
			TestUtility.SaveAssetTemplate(cube.mesh, cube.name);
#endif
			TestUtility.AssertMeshAttributesValid(cube.mesh);
			var template = TestUtility.GetAssetTemplate<Mesh>(cube.name);
			Assert.IsNotNull(template);
			TestUtility.AssertAreEqual(template, cube.mesh);

			UnityEngine.Object.DestroyImmediate(cube);
		}

		[Test]
		public static void BridgeTri()
		{
			var cube = ShapeGenerator.CreateShape(ShapeType.Cube);
			cube.DeleteFace(cube.faces[0]);
			var holes = ElementSelection.FindHoles(cube, cube.sharedVertexesInternal.Select(x => x[0]));
			Assert.AreEqual(1, holes.Count, "found exactly 1 hole");
			var bridgedFace = cube.Bridge(holes[0][0], holes[0][1]);
			Assert.IsNotNull(bridgedFace);
			Assert.AreEqual(3, bridgedFace.edgesInternal.Length);
			cube.ToMesh();
			cube.Refresh();

#if PB_CREATE_TEST_MESH_TEMPLATES
			TestUtility.SaveAssetTemplate(cube.mesh, cube.name);
#endif
			TestUtility.AssertMeshAttributesValid(cube.mesh);
			var template = TestUtility.GetAssetTemplate<Mesh>(cube.name);
			Assert.IsNotNull(template);
			TestUtility.AssertAreEqual(template, cube.mesh);

			UnityEngine.Object.DestroyImmediate(cube);
		}
	}
}