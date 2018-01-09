using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using ProBuilder.Core;
using ProBuilder.MeshOperations;
using ProBuilder.Test;
using UnityEditor;
using UnityEngine.TestTools;

namespace ProBuilder.RuntimeTests.MeshOps.Face
{
	public class Delete
	{
		static System.Random s_Random = new System.Random();

		[Test]
		public static void DeleteFirstFace()
		{
			using (var shapes = new pb_TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<pb_Object>) shapes)
				{
					var face = pb.faces.FirstOrDefault();
					pb.DeleteFace(face);
					pb.ToMesh();
					pb.Refresh();
#if PB_CREATE_TEST_MESH_TEMPLATES
					pb_TestUtility.SaveAssetTemplate(pb.msh, pb.name);
#endif
					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
					var template = pb_TestUtility.GetAssetTemplate<Mesh>(pb.name);
					Assert.IsNotNull(template);
					pb_TestUtility.AssertAreEqual(pb.msh, template);
				}
			}
		}

		[Test]
		public static void DeleteRandomFace()
		{
			using (var shapes = new pb_TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<pb_Object>) shapes)
				{
					var face = pb.faces[s_Random.Next(0, pb.faceCount)];
					int vertexCount = pb.vertexCount;
					int faceVertexCount = face.distinctIndices.Length;
					pb.DeleteFace(face);
					pb.ToMesh();
					pb.Refresh();

					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
					Assert.AreEqual(pb.vertexCount, vertexCount - faceVertexCount);
				}
			}
		}

		[Test]
		public static void DeleteAllFaces()
		{
			using (var shapes = new pb_TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<pb_Object>) shapes)
				{
					pb.DeleteFaces(pb.faces);
					pb.ToMesh();
					pb.Refresh();
					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
					Assert.AreEqual(pb.vertexCount, 0);
				}
			}
		}
	}
}
