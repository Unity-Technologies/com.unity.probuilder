using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using ProBuilder.Core;

namespace ProBuilder.RuntimeTests.Type
{
	public static class TestHashUtility
	{
		public static int GetCollisionsCount<T>(IEnumerable<T> list)
		{
			IEnumerable<IGrouping<int, T>> hashes = list.GroupBy(x => x.GetHashCode(), x => x);
			int collisions = 0;

			foreach (var group in hashes)
			{
				IEnumerable<T> dist = group.Distinct();

				if (dist.Count() > 1)
					collisions += dist.Count() - 1;
			}

			return collisions;
		}
	}

	public class TestVector
	{
		const int TestIterationCount = 512;

		static Vector3 RandVec3()
		{
			return new Vector3(
				UnityEngine.Random.Range(-10f, 10f),
				UnityEngine.Random.Range(-10f, 10f),
				UnityEngine.Random.Range(-10f, 10f));
		}

		static float RandFlt()
		{
			return UnityEngine.Random.Range(0f, 100f) * .001f;
		}

		static pb_Vertex RandVertex()
		{
			pb_Vertex v = new pb_Vertex(true);
			v.position = RandVec3();
			v.color = new Color(RandFlt(), RandFlt(), RandFlt(), RandFlt());
			v.normal = RandVec3();
			v.tangent = (Vector4) RandVec3();
			v.uv0 = (Vector2) RandVec3();
			v.uv2 = (Vector2) RandVec3();
			v.uv3 = (Vector4) RandVec3();
			v.uv4 = (Vector4) RandVec3();
			return v;
		}

		[Test]
		public static void TestHashCollisions_IVEC3()
		{
			pb_IntVec3[] ivec3 = pb_Util.Fill<pb_IntVec3>(TestIterationCount, (i) => { return (pb_IntVec3) RandVec3(); });
			Assert.IsTrue( TestHashUtility.GetCollisionsCount(ivec3) < TestIterationCount * .05f );
		}

		[Test]
		public static void TestVectorHashOverflow()
		{
			Vector3 over = new Vector3(((float) int.MaxValue) + 10f, 0f, 0f);
			Vector3 under = new Vector3(((float) -int.MaxValue) - 10f, 0f, 0f);
			Vector3 inf = new Vector3(Mathf.Infinity, 0f, 0f);
			Vector3 nan = new Vector3(float.NaN, 0f, 0f);

			// mostly checking that GetHashCode doesn't throw an error when converting bad float values
			Assert.AreEqual(pb_Vector.GetHashCode(over), 1499503, "Over");
			Assert.AreEqual(pb_Vector.GetHashCode(under), 2147303674, "Under");
			Assert.AreNotEqual(pb_Vector.GetHashCode(inf), 0, "Inf");
			Assert.AreNotEqual(pb_Vector.GetHashCode(nan), 0, "NaN");
		}

		[Test]
		public static void TestComparison_IVEC3()
		{
			pb_IntVec3 a = (pb_IntVec3) RandVec3();
			pb_IntVec3 b = (pb_IntVec3) (a.vec * 2.3f);
			pb_IntVec3 c = (pb_IntVec3) new Vector3(a.x, a.y + .001f, a.z);
			pb_IntVec3 d = (pb_IntVec3) new Vector3(a.x, a.y, a.z);

			pb_IntVec3[] arr = pb_Util.Fill<pb_IntVec3>(24, (i) => { return i % 2 == 0 ? a : (pb_IntVec3) RandVec3(); });

			Assert.IsFalse(a == b);
			Assert.IsFalse(a == c);
			Assert.IsTrue(a == d);
			Assert.IsFalse(a.GetHashCode() == b.GetHashCode());
			Assert.IsFalse(a.GetHashCode() == c.GetHashCode());
			Assert.IsTrue(a.GetHashCode() == d.GetHashCode());
			Assert.AreEqual(13, arr.Distinct().Count());
		}

		[Test]
		public static void TestComparison_VERTEX()
		{
			pb_Vertex a = RandVertex();
			pb_Vertex b = RandVertex();
			pb_Vertex c = RandVertex();
			pb_Vertex d = new pb_Vertex(a);

			// reference
			Assert.IsFalse(a == b);
			Assert.IsFalse(a == c);
			Assert.IsFalse(a == d);

			// hash
			Assert.IsFalse(a.GetHashCode() == b.GetHashCode());
			Assert.IsFalse(a.GetHashCode() == c.GetHashCode());
			Assert.True(a.GetHashCode() == d.GetHashCode());

			// value
			Assert.AreNotEqual(a, b);
			Assert.AreNotEqual(a, c);
			Assert.AreEqual(a, d);

			d.normal *= 3f;
			Assert.AreNotEqual(a, d);
		}
	}
}
