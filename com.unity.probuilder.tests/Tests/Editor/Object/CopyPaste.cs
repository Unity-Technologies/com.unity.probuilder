using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.Test;
using UnityEditor.ProBuilder;

namespace UnityEngine.ProBuilder.EditorTests.Object
{
	static class CopyPaste
	{
		[Test]
		public static void CopyWithVerifyIsUnique()
		{
			var original = ShapeGenerator.CreateShape(ShapeType.Cube);
			var copy = UObject.Instantiate(original);

			try
			{
				// optimize after instantiate because Instantiate runs mesh through serialization, introducing tiny rounding
				// errors in some fields. by comparing the results post-serialization we get a more accurate diff
				original.Optimize(true);
				EditorUtility.SynchronizeWithMeshFilter(copy);
				Assert.AreNotEqual(copy, original, "GameObject references are equal");
				Assert.IsFalse(ReferenceEquals(copy.mesh, original.mesh), "Mesh references are equal");
				TestUtility.AssertAreEqual(original.mesh, copy.mesh);
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
