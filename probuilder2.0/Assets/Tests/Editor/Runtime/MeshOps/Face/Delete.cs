using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using ProBuilder.Core;
using ProBuilder.MeshOperations;
using ProBuilder.Test;
using UnityEditor;
using UnityEngine.TestTools;

namespace ProBuilder.RuntimeTests.MeshOps.Face
{
	public class Delete
	{
		static void DeleteFirstFace(pb_Object pb)
		{
			var face = pb.faces.FirstOrDefault();
			pb.DeleteFace(face);
			pb.ToMesh();
			pb.Refresh();
#if PB_CREATE_TEST_MESH_TEMPLATES
			pb_TestUtility.SaveAssetTemplate(pb.msh, pb.name);
#endif
			pb_TestUtility.AssertMeshAttributesValid(pb.msh);
			var template = pb_TestUtility.GetAssetTemplate<Mesh>(pb.name);
			Assert.IsNotNull(template);
			pb_TestUtility.AssertAreEqual(pb.msh, template);
		}

		[Test]
		public static void DeleteRandomFace()
		{
			using (var shapes = new pb_TestUtility.BuiltInPrimitives())
			{
				foreach(var pb in (IEnumerable<pb_Object>) shapes)
					DeleteFirstFace(pb);
			}
		}
	}
}
