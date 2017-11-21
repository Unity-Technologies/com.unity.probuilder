using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using ProBuilder.Core;
using ProBuilder.MeshOperations;

namespace ProBuilder.Test
{
	[TestFixture]
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
				Object.DestroyImmediate(data.pb.gameObject);
			}
		}
	}
}
