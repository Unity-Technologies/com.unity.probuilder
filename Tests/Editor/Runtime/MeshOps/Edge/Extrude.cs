using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using ProBuilder.Core;
using ProBuilder.Test;
using ProBuilder.MeshOperations;
using UnityEditor;
using UnityEngine.TestTools;

namespace ProBuilder.RuntimeTests.MeshOps.Edge
{
	public class Extrude
	{
		static System.Random m_Random = new System.Random();

		[Test]
		public static void ExtrudeSingleEdge()
		{
			var pb = pb_ShapeGenerator.CreateShape(pb_ShapeType.Cube);

			try
			{
				int vertexCount = pb.vertexCount;
				pb_Edge[] edges = new pb_Edge[1];
				pb_Edge[] extruded;
				pb_Face face = pb.faces[m_Random.Next(0, pb.faceCount)];
				edges[0] = face.edges[m_Random.Next(0, face.edges.Length)];
				// as group, enable manifold extrude
				Assert.IsFalse(pb.Extrude(edges, .5f, true, false, out extruded), "Do not allow manifold edge extrude");
				Assert.IsTrue(pb.Extrude(edges, .5f, true, true, out extruded), "Do allow manifold edge extrude");
				pb.ToMesh();
				pb.Refresh();
				pb_TestUtility.AssertMeshAttributesValid(pb.msh);
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
			var pb = pb_ShapeGenerator.CreateShape(pb_ShapeType.Cube);

			try
			{
				int vertexCount = pb.vertexCount;
				pb_Edge[] edges = new pb_Edge[pb.faceCount];
				pb_Edge[] extruded;
				for (int i = 0; i < pb.faceCount; i++)
					edges[i] = pb.faces[i].edges[m_Random.Next(0, pb.faces[i].edges.Length)];
				// as group, enable manifold extrude
				Assert.IsFalse(pb.Extrude(edges, .5f, true, false, out extruded), "Do not allow manifold edge extrude");
				Assert.IsTrue(pb.Extrude(edges, .5f, true, true, out extruded), "Do allow manifold edge extrude");
				pb.ToMesh();
				pb.Refresh();
				pb_TestUtility.AssertMeshAttributesValid(pb.msh);
				Assert.AreEqual(vertexCount + edges.Length * 4, pb.vertexCount);
			}
			finally
			{
				UObject.DestroyImmediate(pb.gameObject);
			}
		}
	}
}
