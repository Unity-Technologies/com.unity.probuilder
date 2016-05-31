#if !UNITY_4_7 && !UNITY_5_0 && !PROTOTYPE
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Test
{
	public class Optimize
	{
		[MenuItem("Tools/Debug/ProBuilder/Test/Collapse Shared Vertices")]
		[Test]
		public static void CollapseSharedVertices()
		{
			pb_Object pb = pb_ShapeGenerator.CubeGenerator(Vector3.one * 4f);
			pb.Subdivide();
			pb.Subdivide();
			pb.Subdivide();

			pb.ToMesh();
			Assert.AreEqual(1536, pb.vertexCount);
			pb.Refresh();
			Assert.AreEqual(1536, pb.msh.vertexCount);
			pb.Optimize();
			Assert.AreEqual(1536, pb.vertexCount);
			Assert.AreEqual(486, pb.msh.vertexCount);

			pb.faces[0].material = null;
			pb.faces[1].material = null;
			pb.faces[2].material = null;
			
			pb.ToMesh();
			Assert.AreEqual(1536, pb.msh.vertexCount);
			pb.Refresh();
			pb.Optimize();
			Assert.AreEqual(493, pb.msh.vertexCount);
		}

	}
}

#endif
