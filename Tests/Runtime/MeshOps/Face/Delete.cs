using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Tests.Framework;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOps.Face
{
    static class Delete
    {
        static System.Random s_Random = new System.Random();

        [Test]
        public static void DeleteFirstFace()
        {
            using (var shapes = new TestUtility.BuiltInPrimitives())
            {
                foreach (var mesh in (IEnumerable<ProBuilderMesh>)shapes)
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
            }
        }

        [Test]
        public static void DeleteRandomFace()
        {
            using (var shapes = new TestUtility.BuiltInPrimitives())
            {
                foreach (var pb in (IEnumerable<ProBuilderMesh>)shapes)
                {
                    var face = pb.facesInternal[s_Random.Next(0, pb.faceCount)];
                    int vertexCount = pb.vertexCount;
                    int faceVertexCount = face.distinctIndexes.Count;
                    pb.DeleteFace(face);
                    pb.ToMesh();
                    pb.Refresh();

                    TestUtility.AssertMeshAttributesValid(pb.mesh);
                    Assert.AreEqual(pb.vertexCount, vertexCount - faceVertexCount);
                }
            }
        }

        [Test]
        public static void DeleteAllFaces()
        {
            using (var shapes = new TestUtility.BuiltInPrimitives())
            {
                foreach (var pb in (IEnumerable<ProBuilderMesh>)shapes)
                {
                    pb.DeleteFaces(pb.facesInternal);
                    pb.ToMesh();
                    pb.Refresh();
                    TestUtility.AssertMeshAttributesValid(pb.mesh);
                    Assert.AreEqual(pb.vertexCount, 0);
                }
            }
        }
    }
}
