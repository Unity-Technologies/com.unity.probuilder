using System;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Tests.Framework;
using System.Collections.Generic;
using UnityEngine.ProBuilder.Shapes;

static class DeleteElementsTests
{
    static System.Random s_Random = new System.Random();

    static int s_SubmeshCount = 0;

    public static List<Type> shapeTypes
    {
        get
        {
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

    public static Material[] materials
    {
        get
        {
            var array = new Material[6];

            var redMat = new Material(Shader.Find("Standard"));
            redMat.SetColor("_Color", Color.red);

            var greenMat = new Material(Shader.Find("Standard"));
            greenMat.SetColor("_Color", Color.green);

            var yellowMat = new Material(Shader.Find("Standard"));
            yellowMat.SetColor("_Color", Color.yellow);

            var blueMat = new Material(Shader.Find("Standard"));
            blueMat.SetColor("_Color", Color.blue);

            var whiteMat = new Material(Shader.Find("Standard"));
            whiteMat.SetColor("_Color", Color.white);

            var blackMat = new Material(Shader.Find("Standard"));
            blackMat.SetColor("_Color", Color.black);

            array[0] = redMat;
            array[1] = greenMat;
            array[2] = yellowMat;
            array[3] = blueMat;
            array[4] = whiteMat;
            array[5] = blackMat;

            return array;
        }
    }

    public static object[] faceIndices =
    {
        new object[] { 0, -1, -1 }, // Remove the first face.
        new object[] { 5, -1, -1 }, // Remove the last face.
        new object[] { 0,  1, -1 }, // Remove the first two faces.
        new object[] { 4,  5, -1 }, // Remove the last two faces
        new object[] { 0,  5, -1 }, // Remove the first and last faces.
        new object[] { 2,  4, -1 }, // Remove two middle faces.
        new object[] { 0,  3, -1 }, // Remove the first face and a middle face.
        new object[] { 3,  5, -1 }, // Remove a middle face and the last face.
        new object[] { 0,  3,  5 }  // Remove the first face, a middle face and the last face.
    };

    [TearDown]
    public static void Cleanup()
    {
        s_SubmeshCount = 0;
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

            var faceList = new List<Face>();
            faceList.Add(face);

            DeleteFacesAndValidateMesh(mesh, faceList);
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
            DeleteFacesAndValidateMesh(mesh, mesh.facesInternal);
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

    [TestCaseSource(nameof(faceIndices))]
    public static void CreateCube_DeleteFaces_ValidateMeshAndMaterials(int firstIndex, int secondIndex, int thirdIndex)
    {
        var mesh = ShapeFactory.Instantiate<Cube>();
        mesh = AddMaterialsTo(mesh, mesh.facesInternal, materials);

        var faceIndices = new List<int>();
        if (firstIndex != -1)
            faceIndices.Add(firstIndex);
        if (secondIndex != -1)
            faceIndices.Add(secondIndex);
        if (thirdIndex != -1)
            faceIndices.Add(thirdIndex);

        var faces = new List<Face>();
        foreach (var index in faceIndices)
            faces.Add(mesh.facesInternal[index]);

        try
        {
            DeleteFacesAndValidateAll(mesh, faces, faceIndices);
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

    static ProBuilderMesh AddMaterialsTo(ProBuilderMesh mesh, IEnumerable<Face> faces, Material[] materials)
    {
        if (materials == null)
            return mesh;

        mesh.renderer.sharedMaterials = materials;

        var currentSubmeshIndex = 0;
        var facesList = faces.ToList();
        for (int i = 0; i < facesList.Count; i++)
        {
            mesh.facesInternal[i].submeshIndex = currentSubmeshIndex;

            if (currentSubmeshIndex < materials.Length - 1 && currentSubmeshIndex < facesList.Count - 1)
                currentSubmeshIndex++;
        }

        mesh.ToMesh();
        mesh.Refresh();

        s_SubmeshCount = currentSubmeshIndex + 1;
        Assert.AreEqual(s_SubmeshCount, mesh.mesh.subMeshCount);

        return mesh;
    }

    static void DeleteFacesAndValidateMesh(ProBuilderMesh mesh, IEnumerable<Face> faces)
    {
        var meshVertexCount = mesh.vertexCount;
        var totalFaceVertexCount = 0;
        foreach (var face in faces)
            totalFaceVertexCount += face.distinctIndexes.Count;

        mesh.DeleteFaces(faces);
        mesh.ToMesh();
        mesh.Refresh();

        TestUtility.AssertMeshAttributesValid(mesh.mesh);
        Assert.AreEqual(mesh.vertexCount, meshVertexCount - totalFaceVertexCount);
    }

    static void DeleteFacesAndValidateAll(ProBuilderMesh mesh, IEnumerable<Face> faces, List<int> removedMaterialIndices)
    {
        DeleteFacesAndValidateMesh(mesh, faces);

        mesh.ToMesh();
        mesh.Refresh();

        Assert.AreEqual(s_SubmeshCount - removedMaterialIndices.Count, mesh.renderer.sharedMaterials.Length);
        Assert.AreEqual(s_SubmeshCount - removedMaterialIndices.Count, mesh.mesh.subMeshCount);

        var newMaterials = materials.ToList();
        foreach (var removedMatIndex in removedMaterialIndices)
        {
            var removedMat = materials[removedMatIndex];
            for (int i = 0; i < newMaterials.Count; i++)
            {
                if (newMaterials[i].color == removedMat.color)
                {
                    newMaterials.RemoveAt(i);
                    break;
                }
            }
        }

        for (int i = 0; i < mesh.facesInternal.Length; i++)
        {
            var submeshIndex = mesh.facesInternal[i].submeshIndex;
            var currentMaterial = mesh.renderer.sharedMaterials[submeshIndex];

            Assert.AreEqual(newMaterials[i].color, currentMaterial.color);
        }
    }
}
