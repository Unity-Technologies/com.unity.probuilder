// ***********************************************************************
// Copyright (c) 2017 Unity Technologies. All rights reserved.
//
// Licensed under the ##LICENSENAME##.
// See LICENSE.md file in the project root for full license information.
// ***********************************************************************

using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;

namespace FbxExporters.UnitTests
{
    public class ModelExporterTest
    {
        // add any GameObject that gets created to this list
        // so that it gets deleted in the TearDown
        private List<GameObject> m_createdObjects;

        [SetUp]
        public void Init()
        {
            m_createdObjects = new List<GameObject> ();
        }

        [TearDown]
        public void Term()
        {
            foreach (var obj in m_createdObjects) {
                GameObject.DestroyImmediate (obj);
            }
        }

        /// <summary>
        /// Creates a GameObject.
        /// Adds the object to the list of objects to be deleted
        /// on TearDown.
        /// </summary>
        /// <returns>The game object.</returns>
        /// <param name="name">Name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="type">Primitive Type.</param>
        private GameObject CreateGameObject (string name, Transform parent = null, PrimitiveType type = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive (type);
            go.name = name;
            go.transform.SetParent (parent);
            m_createdObjects.Add (go);
            return go;
        }

        [Test]
        public void TestFindCenter ()
        {
            // Create 3 objects
            var cube = CreateGameObject ("cube");
            var cube1 = CreateGameObject ("cube1");
            var cube2 = CreateGameObject ("cube2");

            // Set their transforms
            cube.transform.localPosition = new Vector3 (23, -5, 10);
            cube1.transform.localPosition = new Vector3 (23, -5, 4);
            cube1.transform.localScale = new Vector3 (1, 1, 2);
            cube2.transform.localPosition = new Vector3 (28, 0, 10);
            cube2.transform.localScale = new Vector3 (3, 1, 1);

            // Find the center
            var center = FbxExporters.Editor.ModelExporter.FindCenter(new GameObject[]{cube,cube1,cube2});

            // Check that it is what we expect
            Assert.AreEqual(center, new Vector3(26, -2.5f, 6.75f));
        }

        [Test]
        public void TestRemoveRedundantObjects ()
        {
            var root = CreateGameObject ("root");
            var child1 = CreateGameObject ("child1", root.transform);
            var child2 = CreateGameObject ("child2", root.transform);
            var root2 = CreateGameObject ("root2");

            // test set: root
            // expected result: root
            var result = FbxExporters.Editor.ModelExporter.RemoveRedundantObjects(new Object[]{root});
            Assert.AreEqual (1, result.Count);
            Assert.IsTrue (result.Contains (root));

            // test set: root, child1
            // expected result: root
            result = FbxExporters.Editor.ModelExporter.RemoveRedundantObjects(new Object[]{root, child1});
            Assert.AreEqual (1, result.Count);
            Assert.IsTrue (result.Contains (root));

            // test set: root, child1, child2, root2
            // expected result: root, root2
            result = FbxExporters.Editor.ModelExporter.RemoveRedundantObjects(new Object[]{root, root2, child2, child1});
            Assert.AreEqual (2, result.Count);
            Assert.IsTrue (result.Contains (root));
            Assert.IsTrue (result.Contains (root2));

            // test set: child1, child2
            // expected result: child1, child2
            result = FbxExporters.Editor.ModelExporter.RemoveRedundantObjects(new Object[]{child2, child1});
            Assert.AreEqual (2, result.Count);
            Assert.IsTrue (result.Contains (child1));
            Assert.IsTrue (result.Contains (child2));
        }

        [Test]
        public void TestConvertToValidFilename()
        {
            // test already valid filenames
            var filename = "foobar.fbx";
            var result = FbxExporters.Editor.ModelExporter.ConvertToValidFilename (filename);
            Assert.AreEqual (filename, result);

            filename = "foo_bar 1.fbx";
            result = FbxExporters.Editor.ModelExporter.ConvertToValidFilename (filename);
            Assert.AreEqual (filename, result);

            // test invalid filenames
            filename = "?foo**bar///.fbx";
            result = FbxExporters.Editor.ModelExporter.ConvertToValidFilename (filename);
#if UNITY_EDITOR_WIN
            Assert.AreEqual ("_foo__bar___.fbx", result);
#else
            Assert.AreEqual ("?foo**bar___.fbx", result);
#endif

            filename = "foo$?ba%r 2.fbx";
            result = FbxExporters.Editor.ModelExporter.ConvertToValidFilename (filename);
#if UNITY_EDITOR_WIN
            Assert.AreEqual ("foo$_ba%r 2.fbx", result);
#else
            Assert.AreEqual ("foo$?ba%r 2.fbx", result);
#endif
        }
    }
}