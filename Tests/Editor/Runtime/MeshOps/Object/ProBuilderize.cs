using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.Test;
using UnityEditor;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOps.Object
{
	static class ProBuilderize
	{
		[Test]
		[MenuItem("Tools/Do Probuilderize testt")]
		public static void BuiltInShapesRoundTrip()
		{
			using (var shapes = new TestUtility.BuiltInPrimitives())
			{
				foreach (var pb in (IEnumerable<ProBuilderMesh>)shapes)
				{
					pb.ToMesh();
					pb.Refresh();
					// no optimization for mesh
					var dup = new GameObject().AddComponent<ProBuilderMesh>();
					var importer = new MeshImporter(dup);
					importer.Import(pb.gameObject);
					dup.ToMesh();
					dup.Refresh();
					TestUtility.AssertAreEqual(pb.mesh, dup.mesh, pb.name);
				}
			}
		}
	}
}