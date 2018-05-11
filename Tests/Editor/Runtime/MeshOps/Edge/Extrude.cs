using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Test;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEditor;
using UnityEngine.TestTools;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOps.Edge
{
	static class Extrude
	{
		static System.Random m_Random = new System.Random();

		[Test]
		public static void ExtrudeSingleEdge()
		{
			var pb = ShapeGenerator.CreateShape(ShapeType.Cube);

			try
			{
				int vertexCount = pb.vertexCount;
				UnityEngine.ProBuilder.Edge[] edges = new UnityEngine.ProBuilder.Edge[1];
				UnityEngine.ProBuilder.Face face = pb.facesInternal[m_Random.Next(0, pb.faceCount)];
				edges[0] = face.edgesInternal[m_Random.Next(0, face.edgesInternal.Length)];
				// as group, enable manifold extrude
				Assert.IsFalse(pb.Extrude(edges, .5f, true, false) != null, "Do not allow manifold edge extrude");
				Assert.IsTrue(pb.Extrude(edges, .5f, true, true) != null, "Do allow manifold edge extrude");
				pb.ToMesh();
				pb.Refresh();
				TestUtility.AssertMeshAttributesValid(pb.mesh);
				Assert.AreEqual(vertexCount + edges.Length * 4, pb.vertexCount);
			}
			finally
			{
				UObject.DestroyImmediate(pb.gameObject);
			}
		}

		[Test]
		public static void ExtrudeMultipleEdges()
		{
			var pb = ShapeGenerator.CreateShape(ShapeType.Cube);

			try
			{
				int vertexCount = pb.vertexCount;
				UnityEngine.ProBuilder.Edge[] edges = new UnityEngine.ProBuilder.Edge[pb.faceCount];
				for (int i = 0; i < pb.faceCount; i++)
					edges[i] = pb.facesInternal[i].edgesInternal[m_Random.Next(0, pb.facesInternal[i].edgesInternal.Length)];
				// as group, enable manifold extrude
				Assert.IsFalse(pb.Extrude(edges, .5f, true, false) != null, "Do not allow manifold edge extrude");
				Assert.IsTrue(pb.Extrude(edges, .5f, true, true) != null, "Do allow manifold edge extrude");
				pb.ToMesh();
				pb.Refresh();
				TestUtility.AssertMeshAttributesValid(pb.mesh);
				Assert.AreEqual(vertexCount + edges.Length * 4, pb.vertexCount);
			}
			finally
			{
				UObject.DestroyImmediate(pb.gameObject);
			}
		}
	}
}
