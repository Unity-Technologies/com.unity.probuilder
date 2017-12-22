using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using ProBuilder.Core;
using UnityEngine.TestTools;
using ProBuilder.EditorCore;

namespace ProBuilder.Test
{
	public class CopyPaste : IPrebuildSetup, IPostBuildCleanup
	{
		static pb_Object[] m_Meshes = null;

		public void Setup()
		{
			pb_ShapeType[] primitives = Enum.GetValues(typeof(pb_ShapeType)) as pb_ShapeType[];
			m_Meshes = new pb_Object[primitives.Length];
			for (int i = 0, c = primitives.Length; i < c; i++)
			{
				m_Meshes[i] = pb_ShapeGenerator.CreateShape(primitives[i]);
				m_Meshes[i].Optimize();
			}
		}

		public void Cleanup()
		{
			for (int i = 0, c = m_Meshes.Length; i < c; i++)
				UObject.DestroyImmediate(m_Meshes[i]);
		}

		[Test]
		public static void CopyWithVerifyIsUnique()
		{
			var original = m_Meshes[0];
			var copy = UObject.Instantiate(original);
			pb_EditorUtility.VerifyMesh(copy);
			Assert.AreNotEqual(copy, original, "GameObject references are equal");
			Assert.IsFalse(ReferenceEquals(copy.msh, original.msh), "Mesh references are equal");
			pb_TestUtility.AssertAreEqual(copy.msh, original.msh);
		}

		[Test]
		public static void CopyReferencesOriginalMesh()
		{
			var original = m_Meshes[0];
			var copy = UObject.Instantiate(original);
			Assert.AreNotEqual(copy, original, "GameObject references are equal");
			Assert.IsTrue(ReferenceEquals(copy.msh, original.msh), "Mesh references are equal");
		}
	}
}