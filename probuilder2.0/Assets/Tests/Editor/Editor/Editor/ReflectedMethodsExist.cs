using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;
using System;
using ProBuilder.EditorCore;
using ProBuilder.Test;
using System.Reflection;

namespace ProBuilder.EditorTests.Editor
{
	public static class ReflectedMethodsExist
	{
		const BindingFlags k_BindingFlagsAll = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		[Test]
		public static void OnPreSceneGUIDelegate()
		{
			var fi = typeof(SceneView).GetField("onPreSceneGUIDelegate", k_BindingFlagsAll);
			Assert.IsNotNull(fi);
		}

		[Test]
		public static void ResetOnSceneGUIState()
		{
			// no longer necessary as of 2018.2
			var mi = typeof(SceneView).GetMethod("ResetOnSceneGUIState", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.IsNotNull(mi);
		}

		[Test]
		public static void ApplyWireMaterial()
		{
			var m_ApplyWireMaterial = typeof(HandleUtility).GetMethod(
				"ApplyWireMaterial",
				BindingFlags.Static | BindingFlags.NonPublic,
				null,
				new System.Type[] { typeof(UnityEngine.Rendering.CompareFunction) },
				null);
			Assert.IsNotNull(m_ApplyWireMaterial);
		}
	}
}
