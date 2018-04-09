using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using ProBuilder.Core;
using ProBuilder.Test;
using UnityEngine.TestTools;
using ProBuilder.EditorCore;
using UnityEditor;

namespace ProBuilder.EditorTests.Object
{
	public class CreateDestroy
	{
		[Test]
		public static void DestroyDeletesMesh()
		{
			var pb = pb_ShapeGenerator.CreateShape(pb_ShapeType.Cube);
			Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
			UObject.DestroyImmediate(pb.gameObject);
			// IsNull doesn't work due to c#/c++ goofiness
			Assert.IsTrue(mesh == null);
		}

		[Test]
		public static void DestroyWithNoDeleteFlagPreservesMesh()
		{
			var pb = pb_ShapeGenerator.CreateShape(pb_ShapeType.Cube);

			try
			{
				Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
				pb.dontDestroyMeshOnDelete = true;
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
			var pb = pb_ShapeGenerator.CreateShape(pb_ShapeType.Cube);
			string path = pb_TestUtility.SaveAssetTemporary<Mesh>(pb.msh);
			Mesh mesh = pb.GetComponent<MeshFilter>().sharedMesh;
			UObject.DestroyImmediate(pb.gameObject);
			Assert.IsFalse(mesh == null);
			AssetDatabase.DeleteAsset(path);
			LogAssert.NoUnexpectedReceived();
		}
	}
}
