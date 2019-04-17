using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Tests.Framework;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOperations
{
    static class ConnectElementsTests
    {
        public static ShapeType[] shapeTypes
        {
            get { return (ShapeType[])typeof(ShapeType).GetEnumValues(); }
        }

        [Test]
        public static void ConnectEdges_MatchesTemplate([ValueSource("shapeTypes")] ShapeType shapeType)
        {
            var mesh = ShapeGenerator.CreateShape(shapeType);

            Assume.That(mesh, Is.Not.Null);

            try
            {
                var face = mesh.facesInternal[0];

                mesh.Connect(new ProBuilder.Edge[] { face.edgesInternal[0], face.edgesInternal[1] });
                mesh.ToMesh();
                mesh.Refresh();

#if PB_CREATE_TEST_MESH_TEMPLATES
                TestUtility.SaveAssetTemplate(mesh.mesh, mesh.name);
#endif
                TestUtility.AssertMeshAttributesValid(mesh.mesh);
                var template = TestUtility.GetAssetTemplate<Mesh>(mesh.name);
                Assert.IsNotNull(template);
                TestUtility.AssertMeshesAreEqual(template, mesh.mesh);
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
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);

            mesh.renderer.sharedMaterials = new[]
            {
                TestUtility.redMaterial,
                TestUtility.greenMaterial
            };

            mesh.facesInternal[0].submeshIndex = 1;

            var res = mesh.Connect(new ProBuilder.Edge[] { mesh.facesInternal[0].edgesInternal[0], mesh.facesInternal[0].edgesInternal[1] });
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
}
