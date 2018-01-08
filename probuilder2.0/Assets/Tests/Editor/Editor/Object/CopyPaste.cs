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

namespace ProBuilder.EditorTests.Object
{
	public class CopyPaste
	{
		[Test]
		public static void CopyWithVerifyIsUnique()
		{
			var original = pb_ShapeGenerator.CreateShape(pb_ShapeType.Cube);
			original.Optimize();

			var copy = UObject.Instantiate(original);

			try
			{
				pb_EditorUtility.VerifyMesh(copy);
				Assert.AreNotEqual(copy, original, "GameObject references are equal");
				Assert.IsFalse(ReferenceEquals(copy.msh, original.msh), "Mesh references are equal");
				pb_TestUtility.AssertAreEqual(copy.msh, original.msh);
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
			var original = pb_ShapeGenerator.CreateShape(pb_ShapeType.Cube);
			var copy = UObject.Instantiate(original);

			try
			{
				Assert.AreNotEqual(copy, original, "GameObject references are equal");
				Assert.IsTrue(ReferenceEquals(copy.msh, original.msh), "Mesh references are equal");
			}
			finally
			{
				UObject.DestroyImmediate(original.gameObject);
				UObject.DestroyImmediate(copy.gameObject);
			}
		}
	}
}
