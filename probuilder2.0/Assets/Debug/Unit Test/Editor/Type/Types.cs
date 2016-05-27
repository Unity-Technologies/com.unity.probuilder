using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using System;

namespace ProBuilder2.Test
{
	public class TestTypes
	{
		const int LEN = 512;

		static Vector3 RandVec3()
		{
			return new Vector3(
				UnityEngine.Random.Range(-10f, 10f),
				UnityEngine.Random.Range(-10f, 10f),
				UnityEngine.Random.Range(-10f, 10f));
		}

		static System.Random _random = new System.Random();

		static pb_Edge RandEdge()
		{
			return new pb_Edge(_random.Next(0, 1024), _random.Next(0, 1024));
		}


		[Test]
		public static void TestHashCollisions_IVEC3()
		{
			pb_IntVec3[] ivec3 = pbUtil.Fill<pb_IntVec3>(LEN, (i) => { return (pb_IntVec3) RandVec3(); });
			Assert.IsTrue( GetCollisionsCount(ivec3) < LEN * .05f );
		}

		[Test]
		public static void TestHashCollisions_EDGE()
		{
			pb_Edge[] edge = pbUtil.Fill<pb_Edge>(LEN, (i) => { return RandEdge(); });
			Assert.IsTrue( GetCollisionsCount(edge) < LEN * .05f );
		}

		[Test]
		public static void TestHashComparison_IVEC3()
		{
			pb_IntVec3 a = (pb_IntVec3) RandVec3();
			pb_IntVec3 b = (pb_IntVec3) (a.vec * 2.3f);
			pb_IntVec3 c = (pb_IntVec3) new Vector3(a.x, a.y + .001f, a.z);
			pb_IntVec3 d = (pb_IntVec3) new Vector3(a.x, a.y, a.z);

			pb_IntVec3[] arr = pbUtil.Fill<pb_IntVec3>(24, (i) => { return i % 2 == 0 ? a : (pb_IntVec3) RandVec3(); });

			Assert.IsFalse(a == b);
			Assert.IsFalse(a == c);
			Assert.IsTrue(a == d);
			Assert.AreEqual(13, arr.Distinct().Count());
		}

		[Test]
		public static void TestHashComparison_EDGE()
		{
			pb_Edge a = (pb_Edge) RandEdge();
			pb_Edge b = (pb_Edge) (a + 20);
			pb_Edge c = (pb_Edge) new pb_Edge(a.x + 10, a.x);
			pb_Edge d = (pb_Edge) new pb_Edge(a.x, a.y);

			pb_Edge[] arr = pbUtil.Fill<pb_Edge>(24, (i) => { return i % 2 == 0 ? a : (pb_Edge) RandEdge(); });

			Assert.IsFalse(a == b);
			Assert.IsFalse(a == c);
			Assert.AreEqual(a, d);
			Assert.IsTrue(a != d);
			Assert.AreEqual(13, arr.Distinct().Count());
		}

		static int GetCollisionsCount<T>(IEnumerable<T> list)
		{
			IEnumerable<IGrouping<int, T>> hashes = list.GroupBy(x => x.GetHashCode(), x => x);
			int collisions = 0;

			foreach(var group in hashes)
			{
				IEnumerable<T> dist = group.Distinct();

				if(dist.Count() > 1)
					collisions += dist.Count() - 1;
			}

			return collisions;
		}
	}
}
