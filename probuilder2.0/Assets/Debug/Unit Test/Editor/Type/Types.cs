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

		[Test]
		public static void TestHashCollisions_IVEC3()
		{
			pb_IntVec3[] ivec3 = pbUtil.Fill<pb_IntVec3>(LEN, (i) =>
				{
					return (pb_IntVec3) new Vector3(UnityEngine.Random.Range(-10f, 10f),
													UnityEngine.Random.Range(-10f, 10f),
													UnityEngine.Random.Range(-10f, 10f)); });

			Assert.IsTrue( GetCollisionsCount(ivec3) < LEN * .05f );
		}

		[Test]
		public static void TestHashCollisions_EDGE()
		{
			System.Random r = new System.Random();
			pb_Edge[] edge = pbUtil.Fill<pb_Edge>(LEN, (i) => { return new pb_Edge(r.Next(0,512), r.Next(0,512)); });
			Assert.IsTrue( GetCollisionsCount(edge) < LEN * .05f );
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
