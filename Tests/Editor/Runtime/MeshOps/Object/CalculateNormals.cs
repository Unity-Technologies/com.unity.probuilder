using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.Test;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOps.Object
{
	static class CalculateNormals
	{
		const float k_NormalCompareEpsilon = .0001f;

		[Test]
		public static void HardNormalsAreNormalized()
		{
			using (var shapes = new TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<ProBuilderMesh>)shapes)
				{
					foreach (var face in pb.faces)
						face.smoothingGroup = Smoothing.smoothingGroupNone;

					pb.Refresh(RefreshMask.Normals);

					Vector3[] normals = pb.GetNormals();

					foreach (var nrm in normals)
					{
						Assert.AreEqual(1f, nrm.magnitude, k_NormalCompareEpsilon, pb.name);
					}
				}
			}
		}

		[Test]
		public static void SoftNormalsAreNormalized()
		{
			using (var shapes = new TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<ProBuilderMesh>)shapes)
				{
					foreach (var face in pb.faces)
						face.smoothingGroup = 1;

					pb.Refresh(RefreshMask.Normals);

					Vector3[] normals = pb.GetNormals();

					foreach (var nrm in normals)
					{
						Assert.AreEqual(1f, nrm.magnitude, k_NormalCompareEpsilon, pb.name);
					}
				}
			}
		}

		[Test]
		public static void SoftNormalsAreSoft()
		{
			using (var shapes = new TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<ProBuilderMesh>)shapes)
				{
					foreach (var face in pb.faces)
						face.smoothingGroup = 1;

					pb.ToMesh();
					pb.Refresh();

					Vector3[] normals = pb.GetNormals();

					foreach (var common in pb.sharedIndexes)
					{
						int[] arr = common;
						Vector3 nrm = normals[arr[0]];

						for(int i = 1, c = arr.Length; i < c; i++)
							Assert.AreEqual(nrm, normals[arr[i]]);
					}
				}
			}
		}
	}
}