using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace UnityEngine.ProBuilder.EditorTests.Editor
{
    [ExecuteInEditMode]
    class InternalMethodLogger : ScriptableObject
    {
        public static List<string> invokedMethods = new List<string>();

        void OnEnableINTERNAL()
        {
            invokedMethods.Add("OnEnableINTERNAL");
        }

        void OnDisableINTERNAL()
        {
            invokedMethods.Add("OnDisableINTERNAL");
        }
    }

    public class InternalMethodsAreInvoked
    {
        [Test]
        public void OnEnableINTERNAL_IsInvoked()
        {
            InternalMethodLogger.invokedMethods.Clear();
            Assume.That(InternalMethodLogger.invokedMethods, Is.Empty);
            InternalMethodLogger logger = ScriptableObject.CreateInstance<InternalMethodLogger>();
            UnityEngine.Object.DestroyImmediate(logger);
            Assert.That(InternalMethodLogger.invokedMethods.FirstOrDefault(), Is.EqualTo("OnEnableINTERNAL"));
        }

        [Test]
        public void OnDisableINTERNAL_IsInvoked()
        {
            InternalMethodLogger.invokedMethods.Clear();
            Assume.That(InternalMethodLogger.invokedMethods, Is.Empty);
            InternalMethodLogger logger = ScriptableObject.CreateInstance<InternalMethodLogger>();
            UnityEngine.Object.DestroyImmediate(logger);
            Assert.That(InternalMethodLogger.invokedMethods.LastOrDefault(), Is.EqualTo("OnDisableINTERNAL"));
        }
    }
}
