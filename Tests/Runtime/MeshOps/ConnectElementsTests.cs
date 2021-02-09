using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.ProBuilder.Tests.Framework;

static class ConnectElementsTests
{
    public static List<Type> shapeTypes {
        get {
            var list = new List<Type>();
            var types = typeof(ShapePrimitive).Assembly.GetTypes();
            foreach (var type in types)
            {
                if (typeof(ShapePrimitive).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    list.Add(type);
                }
            }
            return list;
        }
    }

    [Test]
    public static void ConnectEdges_CreatesValidGeometry([ValueSource("shapeTypes")] Type shapeType)
    {
        var mesh = ShapeFactory.Instantiate(shapeType);

        Assume.That(mesh, Is.Not.Null);
        Assume.That(mesh.faceCount, Is.GreaterThan(0));

        try
        {
            var face = mesh.facesInternal[0];
            var previousEdgeCount = mesh.edgeCount;
            Assume.That(previousEdgeCount, Is.GreaterThan(0));

            mesh.Connect(new Edge[] { face.edgesInternal[0], face.edgesInternal[1] });
            mesh.ToMesh();
            mesh.Refresh();

            TestUtility.AssertMeshIsValid(mesh);
            Assert.That(previousEdgeCount, Is.LessThan(mesh.edgeCount));
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
    public static void ConnectEdges_RetainsMaterial()
    {
        var mesh = ShapeFactory.Instantiate<Cube>();

        mesh.renderer.sharedMaterials = new[]
        {
            TestUtility.redMaterial,
            TestUtility.greenMaterial
        };

        mesh.facesInternal[0].submeshIndex = 1;

        var res = mesh.Connect(new Edge[] { mesh.facesInternal[0].edgesInternal[0], mesh.facesInternal[0].edgesInternal[1] });
        mesh.ToMesh();
        Assert.AreEqual(2, mesh.mesh.subMeshCount, "submesh count");

        foreach (var face in res.item1)
            Assert.AreEqual(1, face.submeshIndex);

        foreach (var face in mesh.facesInternal)
        {
            if (!res.item1.Contains(face))
                Assert.AreEqual(0, face.submeshIndex);
        }
    }
}
