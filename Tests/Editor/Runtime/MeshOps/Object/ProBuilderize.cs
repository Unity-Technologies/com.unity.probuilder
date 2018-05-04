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
		public static void CubeSurvivesRoundTrip()
		{
			var pb = ShapeGenerator.CubeGenerator(Vector3.one);
			try
			{
				var dup = new GameObject().AddComponent<ProBuilderMesh>();
				var importer = new MeshImporter(dup);
				importer.Import(pb.gameObject);
				dup.ToMesh();
				dup.Refresh();
				TestUtility.AssertAreEqual(pb.mesh, dup.mesh, pb.name);
			}
			catch
			{
				UnityEngine.Object.DestroyImmediate(pb.gameObject);
			}
		}
	}
}