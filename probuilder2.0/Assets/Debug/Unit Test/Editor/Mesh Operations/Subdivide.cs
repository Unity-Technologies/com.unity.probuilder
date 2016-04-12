#if !UNITY_4_7 && !UNITY_5_0 && !PROTOTYPE
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.Test
{
	public class Subdivide
	{
		class VertexCountTestData
		{
			public pb_Object pb;
			// How many times to subdivide this object.
			public int subdivisions;
			// How many vertices should result.
			public int expectedVertexCount;

			public VertexCountTestData(pb_Object pb, int subdivisions, int expectedVertexCount)
			{
				this.pb = pb;
				this.subdivisions = subdivisions;
				this.expectedVertexCount = expectedVertexCount;
			}
		}

		[MenuItem("Tools/ProBuilder/Test/Subdivide/Vertex Count Test")]
		[Test]
		public static void VertexCountTest()
		{
			List<VertexCountTestData> vertexCountTestData = new List<VertexCountTestData>()
			{
				new VertexCountTestData(pb_ShapeGenerator.IcosahedronGenerator(2f, 1), 1, 960),
				new VertexCountTestData(pb_ShapeGenerator.IcosahedronGenerator(2f, 2), 1, 3840)
			};

			foreach(VertexCountTestData data in vertexCountTestData)
			{
				for(int i = 0; i < data.subdivisions; i++)
					data.pb.Subdivide();

				Assert.AreEqual(data.expectedVertexCount, data.pb.vertexCount);
				GameObject.DestroyImmediate(data.pb.gameObject);
			}
		}

		[MenuItem("Tools/ProBuilder/Test/Subdivide/Empty Channel Test")]
		[Test]
		public static void EmptyChannelTest()
		{
			pb_Object pb = pb_ShapeGenerator.CubeGenerator(Vector3.zero);
			pb.Subdivide();

			GameObject.DestroyImmediate(pb.gameObject);
		}
	}
}

#endif
