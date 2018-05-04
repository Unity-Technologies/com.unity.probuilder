using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Test;
using UnityEditor;
using UnityEngine.TestTools;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOps.Face
{
	static class SubdivideTest
	{
		[Test]
		public static void SubdivideFirstFace()
		{
			using (var shapes = new TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<ProBuilderMesh>) shapes)
				{
					var face = pb.facesInternal.FirstOrDefault();
					Subdivision.Subdivide(pb, new ProBuilder.Face[] { face });
					pb.ToMesh();
					pb.Refresh();
//#if PB_CREATE_TEST_MESH_TEMPLATES
//					TestUtility.SaveAssetTemplate(pb.mesh, pb.name);
//#endif
//					TestUtility.AssertMeshAttributesValid(pb.mesh);
//					var template = TestUtility.GetAssetTemplate<Mesh>(pb.name);
//					Assert.IsNotNull(template);
//					TestUtility.AssertAreEqual(template, pb.mesh);
				}
			}
		}
	}
}
