using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Test;
using UnityEditor;
using UnityEngine.TestTools;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOps.Face
{
	static class Delete
	{
		static System.Random s_Random = new System.Random();

		[Test]
		public static void DeleteFirstFace()
		{
			using (var shapes = new TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<ProBuilderMesh>) shapes)
				{
					var face = pb.facesInternal.FirstOrDefault();
					pb.DeleteFace(face);
					pb.ToMesh();
					pb.Refresh();
#if PB_CREATE_TEST_MESH_TEMPLATES
					TestUtility.SaveAssetTemplate(pb.mesh, pb.name);
#endif
					TestUtility.AssertMeshAttributesValid(pb.mesh);
					var template = TestUtility.GetAssetTemplate<Mesh>(pb.name);
					Assert.IsNotNull(template);
					TestUtility.AssertAreEqual(template, pb.mesh);
				}
			}
		}

		[Test]
		public static void DeleteRandomFace()
		{
			using (var shapes = new TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<ProBuilderMesh>) shapes)
				{
					var face = pb.facesInternal[s_Random.Next(0, pb.faceCount)];
					int vertexCount = pb.vertexCount;
					int faceVertexCount = face.distinctIndices.Length;
					pb.DeleteFace(face);
					pb.ToMesh();
					pb.Refresh();

					TestUtility.AssertMeshAttributesValid(pb.mesh);
					Assert.AreEqual(pb.vertexCount, vertexCount - faceVertexCount);
				}
			}
		}

		[Test]
		public static void DeleteAllFaces()
		{
			using (var shapes = new TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<ProBuilderMesh>) shapes)
				{
					pb.DeleteFaces(pb.facesInternal);
					pb.ToMesh();
					pb.Refresh();
					TestUtility.AssertMeshAttributesValid(pb.mesh);
					Assert.AreEqual(pb.vertexCount, 0);
				}
			}
		}
	}
}
