using System;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Tests.Framework;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOperations
{
    static class DeleteElementsTests
    {
        static System.Random s_Random = new System.Random();

        static ShapeType[] shapeTypes
        {
            get { return (ShapeType[])typeof(ShapeType).GetEnumValues(); }
        }

        [Test]
        public static void DeleteFirstFace_CreatesValidMesh([ValueSource("shapeTypes")] ShapeType shape)
        {
            var mesh = ShapeGenerator.CreateShape(shape);

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
        public static void DeleteRandomFace_CreatesValidMesh([ValueSource("shapeTypes")] ShapeType shape)
        {
            var mesh = ShapeGenerator.CreateShape(shape);

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
        public static void DeleteAllFaces_CreatesValidMesh([ValueSource("shapeTypes")] ShapeType shape)
        {
            var mesh = ShapeGenerator.CreateShape(shape);

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
}
