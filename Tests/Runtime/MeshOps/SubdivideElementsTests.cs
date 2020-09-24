using System.Collections.Generic;
using System;
using System.Linq;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Tests.Framework;

static class SubdivideElementsTests
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

    [Test]
    public static void SubdivideFirstFace_CreatesValidMesh([ValueSource("shapeTypes")] Type shape)
    {
        var pb = ShapeGenerator.CreateShape(shape);

        try
        {
            var faceCount = pb.faceCount;
            var face = pb.facesInternal.FirstOrDefault();
            Subdivision.Subdivide(pb, new Face[] { face });
            pb.ToMesh();
            pb.Refresh();

            TestUtility.AssertMeshIsValid(pb);
            TestUtility.AssertMeshAttributesValid(pb.mesh);

            Assert.That(faceCount, Is.LessThan(pb.faceCount));
        }
        finally
        {
            UObject.DestroyImmediate(pb.gameObject);
        }
    }

    [Test]
    public static void SubdivideObject_RetainsMaterial()
    {
        var mesh = ShapeGenerator.CreateShape<Cube>();

        mesh.facesInternal[0].submeshIndex = 1;

        var res = Subdivision.Subdivide(mesh, new[] { mesh.facesInternal[0] });

        foreach (var face in res)
            Assert.AreEqual(1, face.submeshIndex);

        foreach (var face in mesh.facesInternal.Where(x => !res.Contains(x)))
            Assert.AreEqual(0, face.submeshIndex);
    }

    [Test]
    public static void SubdivideSplitFaces_SeparatesAndSubdivides()
    {
        var cube = TestUtility.CreateCubeWithNonContiguousMergedFace();

        try
        {
            var res = Subdivision.Subdivide(cube.item1, new List<Face>() { cube.item2 });
            // Expected result is that the invalid merged face will be split into two faces, then each face
            // subdivided.
            Assert.That(res, Has.Length.EqualTo(8));
        }
        finally
        {
            if(cube.item1 != null)
                UObject.DestroyImmediate(cube.item1.gameObject);
        }
    }
}
