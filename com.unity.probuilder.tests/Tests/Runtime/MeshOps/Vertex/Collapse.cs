using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Test;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOps.Vertex
{
	static class Collapse
	{
		[Test]
		public static void CollapseToFirst()
		{
			var cube = ShapeGenerator.CreateShape(ShapeType.Cube);
			var res = cube.MergeVertices(new [] { 0, 1 }, true);

			Assert.AreEqual(3, res);

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
		public static void CollapseToCenter()
		{
			var cube = ShapeGenerator.CreateShape(ShapeType.Cube);
			var res = cube.MergeVertices(new [] { 0, 1 });

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