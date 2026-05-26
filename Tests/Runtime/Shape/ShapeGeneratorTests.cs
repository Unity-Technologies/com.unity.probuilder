using UnityEngine;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Tests;
using System.Collections.Generic;
using System;
using UnityEngine.ProBuilder.Shapes;
#if UNITY_EDITOR && PB_CREATE_TEST_MESH_TEMPLATES
using UnityEngine.ProBuilder.Tests.Framework;
#endif

class ShapeGeneratorTests
{
    public static List<Type> shapeTypes {
        get {
            var list = new List<Type>();
            var types = typeof(Shape).Assembly.GetTypes();
            foreach (var type in types)
            {
                if (typeof(Shape).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    list.Add(type);
                }
            }
            return list;
        }
    }

    [Test, Ignore("Mesh template comparison tests are unstable")]
    public void ShapeGenerator_MatchesTemplate([ValueSource("shapeTypes")] Type type)
    {
        ProBuilderMesh pb = ShapeFactory.Instantiate(type);

        Assume.That(pb, Is.Not.Null);

#if UNITY_EDITOR && PB_CREATE_TEST_MESH_TEMPLATES
        // save a template mesh. the mesh is saved in a the Templates folder with the path extracted from:
        // Templates/{Asset Type}/{CallingFilePathRelativeToTests}/{MethodName}/{AssetName}.asset
        // note - pb_DestroyListener will not let pb_Object destroy meshes backed by an asset, so there's no need
        // to set `dontDestroyOnDelete` in the editor.
        TestUtility.SaveAssetTemplate(pb.GetComponent<MeshFilter>().sharedMesh, type.ToString());
#else
        try
        {

            RuntimeUtility.AssertMeshAttributesValid(pb.mesh);
            var template = Resources.Load<Mesh>(RuntimeUtility.GetResourcesPath<Mesh>(type.ToString()));
            Assert.IsNotNull(template);
            Assert.IsTrue(RuntimeUtility.AssertAreEqual(template, pb.mesh), type.ToString() + " value-wise mesh comparison");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(pb.gameObject);
        }
#endif
    }

    [Test]
    public static void MeshAttributes_AreValid([ValueSource("shapeTypes")] Type shape)
    {
        var mesh = ShapeFactory.Instantiate(shape);

        try
        {
            Assume.That(mesh, Is.Not.Null);

            Assert.NotNull(mesh.positionsInternal, mesh.name);
            Assert.NotNull(mesh.facesInternal, mesh.name);
            Assert.NotNull(mesh.texturesInternal, mesh.name);
            Assert.NotNull(mesh.sharedVerticesInternal, mesh.name);
            Assert.NotNull(mesh.sharedTextures, mesh.name);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(mesh);
        }
    }
}
