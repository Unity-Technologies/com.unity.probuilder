using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using ProBuilder.Core;
using ProBuilder.MeshOperations;
using UnityEditor;
using UnityEngine.TestTools;

namespace ProBuilder.Test
{
	public class Extrude
	{
		[Test]
		public static void ExtrudeAllFaces_FaceNormal()
		{
			LogAssert.NoUnexpectedReceived();

			using(var shapes = new pb_TestUtility.BasicShapes())
			{
				foreach (var pb in (IEnumerable<pb_Object>) shapes)
				{
					pb.Extrude(pb.faces, ExtrudeMethod.FaceNormal, .5f);
					pb.ToMesh();
					pb.Refresh();
					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
					pb_TestUtility.SaveAssetTemplate(pb.msh);
					// AssetDatabase.CreateAsset(pb.msh, pb_TestUtility.MeshTemplateDirectory + "/MeshOps/Extrude/AllFaces/FaceNormal_" + pb.name + ".asset");
				}
			}
		}

		[Test]
		public static void ExtrudeAllFaces_IndividualFaces()
		{
			LogAssert.NoUnexpectedReceived();

			using(var shapes = new pb_TestUtility.BasicShapes())
			{
				foreach (var pb in (IEnumerable<pb_Object>) shapes)
				{
					pb.Extrude(pb.faces, ExtrudeMethod.IndividualFaces, .5f);
					pb.ToMesh();
					pb.Refresh();
					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
					pb_TestUtility.SaveAssetTemplate(pb.msh);
					// AssetDatabase.CreateAsset(pb.msh, pb_TestUtility.MeshTemplateDirectory + "/MeshOps/Extrude/AllFaces/IndividualFaces_" + pb.name + ".asset");
				}
			}
		}

		[Test]
		public static void ExtrudeAllFaces_VertexNormal()
		{
			LogAssert.NoUnexpectedReceived();

			using(var shapes = new pb_TestUtility.BasicShapes())
			{
				foreach (var pb in (IEnumerable<pb_Object>) shapes)
				{
					pb.Extrude(pb.faces, ExtrudeMethod.VertexNormal, .5f);
					pb.ToMesh();
					pb.Refresh();
					pb_TestUtility.AssertMeshAttributesValid(pb.msh);
					pb_TestUtility.SaveAssetTemplate(pb.msh);
					// AssetDatabase.CreateAsset(pb.msh, pb_TestUtility.MeshTemplateDirectory + "/MeshOps/Extrude/AllFaces/VertexNormal_" + pb.name + ".asset");
				}
			}
		}

	}
}