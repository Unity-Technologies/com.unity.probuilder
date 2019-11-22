using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Tests
{
    static class TestSelectionRenderState
    {
        [Test]
        public static void TestSelectionRenderStateMatchesUnity()
        {
            Assert.AreEqual((int)SelectionRenderState.None, (int)EditorSelectedRenderState.Hidden);
            Assert.AreEqual((int)SelectionRenderState.Wireframe, (int)EditorSelectedRenderState.Wireframe);
            Assert.AreEqual((int)SelectionRenderState.Outline, (int)EditorSelectedRenderState.Highlight);
        }
    }
}
