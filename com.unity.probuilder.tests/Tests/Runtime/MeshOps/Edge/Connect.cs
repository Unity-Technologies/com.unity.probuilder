using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Tests.Framework;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOps.Edge
{
	static class Connect
	{
		[Test]
		public static void Connect2Edges()
		{
			using (var shapes = new TestUtility.BuiltInPrimitives())
			{
				foreach (var mesh in (IEnumerable<ProBuilderMesh>)shapes)
				{
					var face = mesh.facesInternal[0];
					mesh.Connect(new ProBuilder.Edge[] { face.edgesInternal[0], face.edgesInternal[1] });
					mesh.ToMesh();
					mesh.Refresh();

#if PB_CREATE_TEST_MESH_TEMPLATES
					TestUtility.SaveAssetTemplate(mesh.mesh, mesh.name);
#endif
					TestUtility.AssertMeshAttributesValid(mesh.mesh);
					var template = TestUtility.GetAssetTemplate<Mesh>(mesh.name);
					Assert.IsNotNull(template);
					TestUtility.AssertMeshesAreEqual(template, mesh.mesh);
				}
			}
		}

		[Test]
		public static void ConnectRetainsMaterial()
		{
			var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);
			mesh.facesInternal[0].material = TestUtility.redMaterial;
			for (int i = 1; i < mesh.faceCount; i++)
				mesh.facesInternal[i].material = TestUtility.blueMaterial;
			var res = mesh.Connect(new ProBuilder.Edge[] { mesh.facesInternal[0].edgesInternal[0], mesh.facesInternal[0].edgesInternal[1] });
			mesh.ToMesh();
			Assert.AreEqual(2, mesh.mesh.subMeshCount);
			foreach(var face in res.item1)
				Assert.AreEqual(TestUtility.redMaterial, face.material);
			foreach (var face in mesh.facesInternal)
			{
				if(!res.item1.Contains(face))
					Assert.AreEqual(TestUtility.blueMaterial, face.material);
			}
		}
	}
}