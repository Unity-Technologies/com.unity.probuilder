using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using ProBuilder.Core;
using ProBuilder.Test;

namespace ProBuilder.RuntimeTests.Type
{
	public static class TestEdge
	{
		const int TestIterationCount = 512;

		static System.Random m_Random = new System.Random();

		static pb_Edge RandEdge()
		{
			return new pb_Edge(m_Random.Next(0, 1024), m_Random.Next(0, 1024));
		}

		[Test]
		public static void TestHashCollisions_EDGE()
		{
			pb_Edge[] edge = pb_Util.Fill<pb_Edge>(TestIterationCount, (i) => { return RandEdge(); });
			Assert.IsTrue(TestHashUtility.GetCollisionsCount(edge) < TestIterationCount * .05f);
		}

		[Test]
		public static void TestComparison_EDGE()
		{
			pb_Edge a = (pb_Edge) RandEdge();
			pb_Edge b = (pb_Edge) (a + 20);
			pb_Edge c = (pb_Edge) new pb_Edge(a.x + 10, a.x);
			pb_Edge d = (pb_Edge) new pb_Edge(a.x, a.y);

			pb_Edge[] arr = pb_Util.Fill<pb_Edge>(24, (i) => { return i % 2 == 0 ? a : (pb_Edge) RandEdge(); });

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
