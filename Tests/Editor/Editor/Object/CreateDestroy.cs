using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Test;
using UnityEngine.TestTools;
using UnityEditor.ProBuilder;
using UnityEditor;

namespace UnityEngine.ProBuilder.EditorTests.Object
{
	static class CreateDestroy
	{
		[Test]
		public static void DestroyDeletesMesh()
		{
			var pb = ShapeGenerator.CreateShape(ShapeType.Cube);
			Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
			UObject.DestroyImmediate(pb.gameObject);
			// IsNull doesn't work due to c#/c++ goofiness
			Assert.IsTrue(mesh == null);
		}

		[Test]
		public static void DestroyWithNoDeleteFlagPreservesMesh()
		{
			var pb = ShapeGenerator.CreateShape(ShapeType.Cube);

			try
			{
				Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
				pb.preserveMeshAssetOnDestroy = true;
				UObject.DestroyImmediate(pb.gameObject);
				Assert.IsFalse(mesh == null);
			}
			finally
			{
				if(pb != null)
					UObject.DestroyImmediate(pb.gameObject);
			}
		}

		[Test]
		public static void DestroyDoesNotDeleteMeshBackByAsset()
		{
			var pb = ShapeGenerator.CreateShape(ShapeType.Cube);
			string path = TestUtility.SaveAssetTemporary<Mesh>(pb.mesh);
			Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
			UObject.DestroyImmediate(pb.gameObject);
			Assert.IsFalse(mesh == null);
			AssetDatabase.DeleteAsset(path);
			LogAssert.NoUnexpectedReceived();
		}
	}
}
