using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System;
using System.Reflection;

namespace UnityEngine.ProBuilder.EditorTests.Editor
{
    static class ReflectedMethodsExist
    {
        const BindingFlags k_BindingFlagsAll = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

#if !UNITY_2019_1_OR_NEWER
        [Test]
        public static void SceneView_OnPreSceneGUIDelegate()
        {
            var fi = typeof(SceneView).GetField("onPreSceneGUIDelegate", k_BindingFlagsAll);
            Assert.IsNotNull(fi);
        }

        [Test]
        public static void EditorWindow_ShowWindowPopupWithMode()
        {
            var mi = typeof(EditorWindow).GetMethod(
                "ShowPopupWithMode",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(mi);
        }

#endif

        [Test]
        public static void HandleUtility_ApplyWireMaterial()
        {
            var m_ApplyWireMaterial = typeof(UnityEditor.HandleUtility).GetMethod(
                    "ApplyWireMaterial",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new System.Type[] { typeof(UnityEngine.Rendering.CompareFunction) },
                    null);
            Assert.IsNotNull(m_ApplyWireMaterial);
        }

        [Test]
        public static void GetDefaultMaterial()
        {
            var mi = typeof(Material).GetMethod("GetDefaultMaterial", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(mi);
        }

        [Test]
        public static void DrivenPropertyManager_RegisterProperty()
        {
            Assert.That(SerializationUtility.registerProperty, Is.Not.Null);
        }

        [Test]
        public static void DrivenPropertyManager_UnregisterProperty()
        {
            Assert.That(SerializationUtility.unregisterProperty, Is.Not.Null);
        }

        [Test]
        public static void AnnotationUtility_SetIconEnabled()
        {
            Assert.DoesNotThrow(() =>
            {
                UnityEditor.ProBuilder.EditorUtility.SetGizmoIconEnabled(typeof(ProBuilderMesh), false);
            });
        }
    }
}
