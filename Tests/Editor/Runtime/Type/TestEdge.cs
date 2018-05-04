using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;

namespace UnityEngine.ProBuilder.RuntimeTests.Type
{
	static class TestEdge
	{
		const int TestIterationCount = 512;

		static System.Random m_Random = new System.Random();

		static Edge RandEdge()
		{
			return new Edge(m_Random.Next(0, 1024), m_Random.Next(0, 1024));
		}

		[Test]
		public static void TestHashCollisions_EDGE()
		{
			Edge[] edge = ArrayUtility.Fill<Edge>(TestIterationCount, (i) => { return RandEdge(); });
			Assert.IsTrue(TestHashUtility.GetCollisionsCount(edge) < TestIterationCount * .05f);
		}

		[Test]
		public static void TestComparison_EDGE()
		{
			Edge a = (Edge) RandEdge();
			Edge b = (Edge) (a + 20);
			Edge c = (Edge) new Edge(a.x + 10, a.x);
			Edge d = (Edge) new Edge(a.x, a.y);

			Edge[] arr = ArrayUtility.Fill<Edge>(24, (i) => { return i % 2 == 0 ? a : (Edge) RandEdge(); });

			Assert.IsFalse(a == b, "a == b");
			Assert.IsFalse(a == c, "a == c");
			Assert.IsFalse(a.GetHashCode() == b.GetHashCode(), "a.GetHashCode() == b.GetHashCode()");
			Assert.IsFalse(a.GetHashCode() == c.GetHashCode(), "a.GetHashCode() == c.GetHashCode()");
			Assert.IsTrue(a.GetHashCode() == d.GetHashCode(), "a.GetHashCode() == d.GetHashCode()");
			Assert.AreEqual(a, d, "Assert.AreEqual(a, d);");
			Assert.IsTrue(a == d, "Assert.IsTrue(a != d);");
			Assert.AreEqual(13, arr.Distinct().Count(), "Assert.AreEqual(13, arr.Distinct().Count());");
		}
	}
}
