using System;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Tests.Framework;
using System.Collections.Generic;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.TestTools;

static class DeleteElementsTests
{
    static System.Random s_Random = new System.Random();

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

    [Test, Ignore("Mesh template tests are unstable")]
    public static void DeleteFirstFace_CreatesValidMesh([ValueSource("shapeTypes")] Type shape)
    {
        var mesh = ShapeFactory.Instantiate(shape);

        try
        {
            var face = mesh.facesInternal.FirstOrDefault();
            mesh.DeleteFace(face);
            mesh.ToMesh();
            mesh.Refresh();

#if PB_CREATE_TEST_MESH_TEMPLATES
            TestUtility.SaveAssetTemplate(mesh.mesh, mesh.name);
#endif
            TestUtility.AssertMeshAttributesValid(mesh.mesh);
            var template = TestUtility.GetAssetTemplate<Mesh>(mesh.name);
            Assert.IsNotNull(template);
            TestUtility.AssertAreEqual(template, mesh.mesh);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(mesh.gameObject);
        }
    }

    [Test]
    public static void DeleteRandomFace_CreatesValidMesh([ValueSource("shapeTypes")] Type shape)
    {
        var mesh = ShapeFactory.Instantiate(shape);

        try
        {
            var face = mesh.facesInternal[s_Random.Next(0, mesh.faceCount)];
            int vertexCount = mesh.vertexCount;
            int faceVertexCount = face.distinctIndexes.Count;
            mesh.DeleteFace(face);
            mesh.ToMesh();
            mesh.Refresh();

            TestUtility.AssertMeshAttributesValid(mesh.mesh);
            Assert.AreEqual(mesh.vertexCount, vertexCount - faceVertexCount);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(mesh.gameObject);
        }
    }

    [Test]
    public static void DeleteFirstFaceAndValidateMaterials([ValueSource("shapeTypes")] Type shape)
    {
        var mesh = ShapeFactory.Instantiate(shape);

        var redMat = new Material(Shader.Find("Standard"));
        redMat.color = Color.red;
        var greenMat = new Material(Shader.Find("Standard"));
        greenMat.color = Color.green;

        var materialCount = 0;
        if (mesh.faceCount > 1)
        {
            mesh.renderer.sharedMaterials = new Material[] { redMat, greenMat };
            materialCount = mesh.renderer.sharedMaterials.Length;
            Assert.AreEqual(materialCount, 2);

            mesh.facesInternal.FirstOrDefault().submeshIndex = 1; // green
            for (int i = 1; i < mesh.facesInternal.Length; i++)
                mesh.facesInternal[i].submeshIndex = 0; // red
        }
        else
        {
            mesh.renderer.sharedMaterials = new Material[] { redMat };
            materialCount = mesh.renderer.sharedMaterials.Length;
            Assert.AreEqual(materialCount, 1);

            mesh.facesInternal.FirstOrDefault().submeshIndex = 1; // green
        }

        mesh.ToMesh();
        mesh.Refresh();

        Assert.AreEqual(materialCount, mesh.mesh.subMeshCount);

        try
        {
            var face = mesh.facesInternal.FirstOrDefault();
            int vertexCount = mesh.vertexCount;
            int faceVertexCount = face.distinctIndexes.Count;
            mesh.DeleteFace(face);
            mesh.ToMesh();
            mesh.Refresh();

            TestUtility.AssertMeshAttributesValid(mesh.mesh);
            Assert.AreEqual(mesh.vertexCount, vertexCount - faceVertexCount);
            Assert.AreEqual(materialCount - 1, mesh.renderer.sharedMaterials.Length);
            Assert.AreEqual(materialCount - 1, mesh.mesh.subMeshCount);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(mesh.gameObject);
        }
    }

    [Test]
    public static void DeleteAllFaces_CreatesValidMesh([ValueSource("shapeTypes")] Type shape)
    {
        var mesh = ShapeFactory.Instantiate(shape);

        try
        {
            mesh.DeleteFaces(mesh.facesInternal);
            mesh.ToMesh();
            mesh.Refresh();
            TestUtility.AssertMeshAttributesValid(mesh.mesh);
            Assert.AreEqual(mesh.vertexCount, 0);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(mesh.gameObject);
        }
    }
}
