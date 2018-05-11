using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using System;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.Test;

namespace UnityEngine.ProBuilder.EditorTests.Editor
{
	static class TestSelectionRenderState
	{
		[Test]
		public static void TestSelectionRenderStateMatchesUnity()
		{
			Assert.AreEqual( (int) SelectionRenderState.None, (int) EditorSelectedRenderState.Hidden );
			Assert.AreEqual( (int) SelectionRenderState.Wireframe, (int) EditorSelectedRenderState.Wireframe );
			Assert.AreEqual( (int) SelectionRenderState.Outline, (int) EditorSelectedRenderState.Highlight );
		}
	}
}
