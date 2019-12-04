using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

class ExtrudeTests
{
    public static ShapeType[] shapeTypes
    {
        get { return (ShapeType[])typeof(ShapeType).GetEnumValues(); }
    }

    static System.Random m_Random = new System.Random();

    [Test]
    public static void Extrude_OneEdge_CreatesValidGeometry()
    {
        var pb = ShapeGenerator.CreateShape(ShapeType.Cube);

        try
        {
            int vertexCount = pb.vertexCount;
            Edge[] edges = new UnityEngine.ProBuilder.Edge[1];
            Face face = pb.facesInternal[m_Random.Next(0, pb.faceCount)];
            edges[0] = face.edgesInternal[m_Random.Next(0, face.edgesInternal.Length)];
            // as group, enable manifold extrude
            Assert.IsFalse(pb.Extrude(edges, .5f, true, false) != null, "Do not allow manifold edge extrude");
            Assert.IsTrue(pb.Extrude(edges, .5f, true, true) != null, "Do allow manifold edge extrude");
            pb.ToMesh();
            pb.Refresh();
            TestUtility.AssertMeshAttributesValid(pb.mesh);
            Assert.AreEqual(vertexCount + edges.Length * 4, pb.vertexCount);
        }
        finally
        {
            UObject.DestroyImmediate(pb.gameObject);
        }
    }

    [Test]
    public static void Extrude_MultipleEdges_CreatesValidGeometry()
    {
        var pb = ShapeGenerator.CreateShape(ShapeType.Cube);

        try
        {
            int vertexCount = pb.vertexCount;
            UnityEngine.ProBuilder.Edge[] edges = new UnityEngine.ProBuilder.Edge[pb.faceCount];
            for (int i = 0; i < pb.faceCount; i++)
                edges[i] = pb.facesInternal[i].edgesInternal[m_Random.Next(0, pb.facesInternal[i].edgesInternal.Length)];
            // as group, enable manifold extrude
            Assert.IsFalse(pb.Extrude(edges, .5f, true, false) != null, "Do not allow manifold edge extrude");
            Assert.IsTrue(pb.Extrude(edges, .5f, true, true) != null, "Do allow manifold edge extrude");
            pb.ToMesh();
            pb.Refresh();
            TestUtility.AssertMeshAttributesValid(pb.mesh);
            Assert.AreEqual(vertexCount + edges.Length * 4, pb.vertexCount);
        }
        finally
        {
            UObject.DestroyImmediate(pb.gameObject);
        }
    }

    [Test]
    public static void ExtrudeAllFaces_FaceNormal([ValueSource("shapeTypes")] ShapeType shape)
    {
        var mesh = ShapeGenerator.CreateShape(shape);

        try
        {
            mesh.Extrude(mesh.facesInternal, ExtrudeMethod.FaceNormal, .5f);
            mesh.ToMesh();
            mesh.Refresh();
            LogAssert.NoUnexpectedReceived();
            TestUtility.AssertMeshAttributesValid(mesh.mesh);
#if PB_CREATE_TEST_MESH_TEMPLATES
            TestUtility.SaveAssetTemplate(mesh.mesh, mesh.name);
#endif
            Mesh template = TestUtility.GetAssetTemplate<Mesh>(mesh.name);
            TestUtility.AssertAreEqual(template, mesh.mesh, message: mesh.name);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
        finally
        {
            if(mesh != null)
                Object.DestroyImmediate(mesh.gameObject);
        }
    }

    [Test]
    public static void ExtrudeAllFaces_IndividualFaces([ValueSource("shapeTypes")] ShapeType shape)
    {
        var mesh = ShapeGenerator.CreateShape(shape);

        try
        {
            int vertexCountBeforeExtrude = mesh.vertexCount;
            mesh.Extrude(mesh.facesInternal, ExtrudeMethod.IndividualFaces, .5f);
            mesh.ToMesh();
            mesh.Refresh();
            LogAssert.NoUnexpectedReceived();
            Assert.AreNotEqual(vertexCountBeforeExtrude, mesh.vertexCount);
            TestUtility.AssertMeshAttributesValid(mesh.mesh);
#if PB_CREATE_TEST_MESH_TEMPLATES
            TestUtility.SaveAssetTemplate(mesh.mesh, mesh.name);
#endif
            Mesh template = TestUtility.GetAssetTemplate<Mesh>(mesh.name);
            TestUtility.AssertAreEqual(template, mesh.mesh, message: mesh.name);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
        finally
        {
            if (mesh != null)
                Object.DestroyImmediate(mesh.gameObject);
        }
    }

    [Test]
    public static void ExtrudeAllFaces_VertexNormal([ValueSource("shapeTypes")] ShapeType shape)
    {
        var mesh = ShapeGenerator.CreateShape(shape);

        try
        {
            mesh.Extrude(mesh.facesInternal, ExtrudeMethod.VertexNormal, .5f);
            mesh.ToMesh();
            mesh.Refresh();
            LogAssert.NoUnexpectedReceived();
            TestUtility.AssertMeshAttributesValid(mesh.mesh);
#if PB_CREATE_TEST_MESH_TEMPLATES
            TestUtility.SaveAssetTemplate(mesh.mesh, mesh.name);
#endif
            Mesh template = TestUtility.GetAssetTemplate<Mesh>(mesh.name);
            TestUtility.AssertAreEqual(template, mesh.mesh, message: mesh.name);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
        finally
        {
            if (mesh != null)
                Object.DestroyImmediate(mesh.gameObject);
        }
    }

    static ExtrudeMethod[] extrudeMethods
    {
        get { return (ExtrudeMethod[])typeof(ExtrudeMethod).GetEnumValues(); }
    }

    static float[] extrudeDistance = new float[] { .5f, -.5f };

    [Test]
    public void Extrude_Face_CreatesValidGeometry(
        [ValueSource("shapeTypes")] ShapeType shape,
        [ValueSource("extrudeMethods")] ExtrudeMethod extrudeMethod,
        [ValueSource("extrudeDistance")] float distance)
    {
        var mesh = ShapeGenerator.CreateShape(shape);

        try
        {
            int initialVertexCount = mesh.vertexCount;
            Face face = mesh.facesInternal[m_Random.Next(0, mesh.facesInternal.Length)];
            mesh.Extrude(new Face[] { face }, extrudeMethod, distance);
            mesh.ToMesh();
            mesh.Refresh();
            LogAssert.NoUnexpectedReceived();
            TestUtility.AssertMeshAttributesValid(mesh.mesh);
#if PB_CREATE_TEST_MESH_TEMPLATES
            TestUtility.SaveAssetTemplate(mesh.mesh, mesh.name);
#endif
            Assert.AreEqual(initialVertexCount + face.edgesInternal.Length * 4, mesh.vertexCount);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
        finally
        {
            if (mesh != null)
                Object.DestroyImmediate(mesh.gameObject);
        }
    }

    [Test]
    public static void Extrude_Face_MultipleTimes_CreatesValidGeometry([ValueSource("shapeTypes")] ShapeType shape)
    {
        var mesh = ShapeGenerator.CreateShape(shape);

        try
        {
            int initialVertexCount = mesh.vertexCount;
            UnityEngine.ProBuilder.Face face = mesh.facesInternal[m_Random.Next(0, mesh.facesInternal.Length)];
            mesh.Extrude(new UnityEngine.ProBuilder.Face[] {face}, ExtrudeMethod.FaceNormal, 1f);
            mesh.ToMesh();
            mesh.Refresh();
            LogAssert.NoUnexpectedReceived();
            TestUtility.AssertMeshAttributesValid(mesh.mesh);
            Assert.AreEqual(initialVertexCount + face.edgesInternal.Length * 4, mesh.vertexCount);

            initialVertexCount = mesh.vertexCount;
            mesh.Extrude(new UnityEngine.ProBuilder.Face[] {face}, ExtrudeMethod.VertexNormal, 1f);
            mesh.ToMesh();
            mesh.Refresh();
            LogAssert.NoUnexpectedReceived();
            TestUtility.AssertMeshAttributesValid(mesh.mesh);
            Assert.AreEqual(initialVertexCount + face.edgesInternal.Length * 4, mesh.vertexCount);

            initialVertexCount = mesh.vertexCount;
            mesh.Extrude(new UnityEngine.ProBuilder.Face[] {face}, ExtrudeMethod.IndividualFaces, 1f);
            mesh.ToMesh();
            mesh.Refresh();
            LogAssert.NoUnexpectedReceived();
            TestUtility.AssertMeshAttributesValid(mesh.mesh);
            Assert.AreEqual(initialVertexCount + face.edgesInternal.Length * 4, mesh.vertexCount);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
        finally
        {
            if (mesh != null)
                Object.DestroyImmediate(mesh.gameObject);
        }
    }
}
