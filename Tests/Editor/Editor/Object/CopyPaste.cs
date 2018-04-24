using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using ProBuilder.Test;
using UnityEngine.TestTools;
using UnityEditor.ProBuilder;

namespace ProBuilder.EditorTests.Object
{
	public class CopyPaste
	{
		[Test]
		public static void CopyWithVerifyIsUnique()
		{
			var original = ShapeGenerator.CreateShape(ShapeType.Cube);
			original.Optimize();

			var copy = UObject.Instantiate(original);

			try
			{
				EditorUtility.VerifyMesh(copy);
				Assert.AreNotEqual(copy, original, "GameObject references are equal");
				Assert.IsFalse(ReferenceEquals(copy.mesh, original.mesh), "Mesh references are equal");
				pb_TestUtility.AssertAreEqual(copy.mesh, original.mesh);
			}
			finally
			{
				UObject.DestroyImmediate(original.gameObject);
				UObject.DestroyImmediate(copy.gameObject);
			}
		}

		[Test]
		public static void CopyReferencesOriginalMesh()
		{
			var original = ShapeGenerator.CreateShape(ShapeType.Cube);
			var copy = UObject.Instantiate(original);

			try
			{
				Assert.AreNotEqual(copy, original, "GameObject references are equal");
				Assert.IsTrue(ReferenceEquals(copy.mesh, original.mesh), "Mesh references are equal");
			}
			finally
			{
				UObject.DestroyImmediate(original.gameObject);
				UObject.DestroyImmediate(copy.gameObject);
			}
		}
	}
}
