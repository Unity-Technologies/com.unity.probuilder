using System;
using System.Collections;
using System.Collections.Generic;
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
	public class Extrude
	{
		static System.Random m_Random = new System.Random();

		[Test]
		public static void ExtrudeAllFaces_FaceNormal()
		{
			using(var shapes = new pb_TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<pb_Object>) shapes)
				{
					pb.Extrude(pb.faces, ExtrudeMethod.FaceNormal, .5f);
					pb.ToMesh();
					pb.Refresh();
					LogAssert.NoUnexpectedReceived();
					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
#if PB_CREATE_TEST_MESH_TEMPLATES
					pb_TestUtility.SaveAssetTemplate(pb.msh, pb.name);
#endif
					Mesh template = pb_TestUtility.GetAssetTemplate<Mesh>(pb.name);
					pb_TestUtility.AssertAreEqual(pb.msh, template);
				}
			}
		}

		[Test]
		public static void ExtrudeAllFaces_IndividualFaces()
		{
			using(var shapes = new pb_TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<pb_Object>) shapes)
				{
					int vertexCountBeforeExtrude = pb.vertexCount;
					pb.Extrude(pb.faces, ExtrudeMethod.IndividualFaces, .5f);
					pb.ToMesh();
					pb.Refresh();
					LogAssert.NoUnexpectedReceived();
					Assert.AreNotEqual(vertexCountBeforeExtrude, pb.vertexCount);
					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
#if PB_CREATE_TEST_MESH_TEMPLATES
					pb_TestUtility.SaveAssetTemplate(pb.msh, pb.name);
#endif
					Mesh template = pb_TestUtility.GetAssetTemplate<Mesh>(pb.name);
					pb_TestUtility.AssertAreEqual(pb.msh, template);
				}
			}
		}

		[Test]
		public static void ExtrudeAllFaces_VertexNormal()
		{
			using(var shapes = new pb_TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<pb_Object>) shapes)
				{
					pb.Extrude(pb.faces, ExtrudeMethod.VertexNormal, .5f);
					pb.ToMesh();
					pb.Refresh();
					LogAssert.NoUnexpectedReceived();
					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
#if PB_CREATE_TEST_MESH_TEMPLATES
					pb_TestUtility.SaveAssetTemplate(pb.msh, pb.name);
#endif
					Mesh template = pb_TestUtility.GetAssetTemplate<Mesh>(pb.name);
					pb_TestUtility.AssertAreEqual(pb.msh, template);
				}
			}
		}

		static void ExtrudeSingleFace(ExtrudeMethod method, float distance = 1f)
		{
			using(var shapes = new pb_TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<pb_Object>) shapes)
				{
					int initialVertexCount = pb.vertexCount;
					pb_Face face = pb.faces[m_Random.Next(0, pb.faces.Length)];
					pb.Extrude(new pb_Face[] {face}, method, 1f);
					pb.ToMesh();
					pb.Refresh();
					LogAssert.NoUnexpectedReceived();
					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
#if PB_CREATE_TEST_MESH_TEMPLATES
					pb_TestUtility.SaveAssetTemplate(pb.msh, pb.name);
#endif
					Assert.AreEqual(initialVertexCount + face.edges.Length * 4, pb.vertexCount);
				}
			}
		}

		[Test]
		public static void ExtrudeSingleFace_FaceNormal()
		{
			ExtrudeSingleFace(ExtrudeMethod.FaceNormal);
		}

		[Test]
		public static void ExtrudeSingleFace_VertexNormal()
		{
			ExtrudeSingleFace(ExtrudeMethod.VertexNormal);
		}

		[Test]
		public static void ExtrudeSingleFace_IndividualFaces()
		{
			ExtrudeSingleFace(ExtrudeMethod.IndividualFaces);
		}

		[Test]
		public static void ExtrudeSingleFace_NegativeDirection_FaceNormal()
		{
			ExtrudeSingleFace(ExtrudeMethod.FaceNormal, -.5f);
		}

		[Test]
		public static void ExtrudeSingleFace_NegativeDirection_VertexNormal()
		{
			ExtrudeSingleFace(ExtrudeMethod.VertexNormal, -.5f);
		}

		[Test]
		public static void ExtrudeSingleFace_NegativeDirection_IndividualFaces()
		{
			ExtrudeSingleFace(ExtrudeMethod.IndividualFaces, -.5f);
		}

		[Test]
		public static void ExtrudeFaceMultipleTimes()
		{
			using (var shapes = new pb_TestUtility.BuiltInPrimitives())
			{
				foreach (pb_Object pb in shapes)
				{
					int initialVertexCount = pb.vertexCount;
					pb_Face face = pb.faces[m_Random.Next(0, pb.faces.Length)];
					pb.Extrude(new pb_Face[] {face}, ExtrudeMethod.FaceNormal, 1f);
					pb.ToMesh();
					pb.Refresh();
					LogAssert.NoUnexpectedReceived();
					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
					Assert.AreEqual(initialVertexCount + face.edges.Length * 4, pb.vertexCount);

					initialVertexCount = pb.vertexCount;
					pb.Extrude(new pb_Face[] {face}, ExtrudeMethod.VertexNormal, 1f);
					pb.ToMesh();
					pb.Refresh();
					LogAssert.NoUnexpectedReceived();
					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
					Assert.AreEqual(initialVertexCount + face.edges.Length * 4, pb.vertexCount);

					initialVertexCount = pb.vertexCount;
					pb.Extrude(new pb_Face[] {face}, ExtrudeMethod.IndividualFaces, 1f);
					pb.ToMesh();
					pb.Refresh();
					LogAssert.NoUnexpectedReceived();
					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
					Assert.AreEqual(initialVertexCount + face.edges.Length * 4, pb.vertexCount);
				}
			}
		}
	}
}
