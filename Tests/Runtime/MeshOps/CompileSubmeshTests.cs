using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder.Tests.Framework;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOperations
{
    static class CompileSubmeshTests
    {
        [Test]
        public static void MeshWithTwoMaterials_CreatesTwoSubmeshes()
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);
            mesh.renderer.sharedMaterials = new Material[2];

            Assert.AreEqual(1, mesh.mesh.subMeshCount);
            mesh.faces[0].submeshIndex = 1;
            mesh.ToMesh();

            Assert.AreEqual(2, mesh.mesh.subMeshCount);

            UObject.Destroy(mesh.gameObject);
        }

        [Test]
        public static void GetSubmeshes_DoesNot_ExceedMaterialCount()
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);

            mesh.facesInternal[0].submeshIndex = 1;
            mesh.facesInternal[1].submeshIndex = 2;
            mesh.facesInternal[3].submeshIndex = 3;

            mesh.renderer.sharedMaterials = new Material[1];
            mesh.ToMesh();
            Assert.AreEqual(1, mesh.mesh.subMeshCount);

            mesh.renderer.sharedMaterials = new Material[2];
            mesh.ToMesh();
            Assert.AreEqual(2, mesh.mesh.subMeshCount);

            mesh.renderer.sharedMaterials = new Material[3];
            mesh.ToMesh();
            Assert.AreEqual(3, mesh.mesh.subMeshCount);

            UObject.DestroyImmediate(mesh.gameObject);
        }

        [Test]
        public static void InvalidSubmeshIndex_CreatesValidMesh()
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);

            mesh.renderer.sharedMaterials = new Material[2]
            {
                TestUtility.redMaterial,
                TestUtility.blueMaterial
            };

            // Should map to submesh 0
            mesh.facesInternal[0].submeshIndex = -1;

            // Should map to submesh 1
            mesh.facesInternal[1].submeshIndex = 10;

            mesh.ToMesh();
            Assert.AreEqual(2, mesh.mesh.subMeshCount);

            UObject.Destroy(mesh.gameObject);
        }

        [Test]
        public static void SubmeshIndexes_AreMappedToCorrectMaterial()
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);

            mesh.renderer.sharedMaterials = new Material[2]
            {
                TestUtility.redMaterial,
                TestUtility.blueMaterial
            };

            mesh.facesInternal[0].submeshIndex = 1;

            mesh.ToMesh();

            var submesh0 = mesh.facesInternal.Where((x, i) => i > 0).SelectMany(y => y.indexes).ToArray();
            var submesh1 = mesh.facesInternal[0].indexes;

            var compiled0 = mesh.mesh.GetTriangles(0);
            var compiled1 = mesh.mesh.GetTriangles(1);

            TestUtility.AssertSequenceEqual(submesh0, compiled0);
            TestUtility.AssertSequenceEqual(submesh1, compiled1);

            UObject.DestroyImmediate(mesh);
        }

        [Test]
        public static void SubmeshIndexes_AreClamped()
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);

            mesh.renderer.sharedMaterials = new Material[3]
            {
                TestUtility.redMaterial,
                TestUtility.blueMaterial,
                TestUtility.greenMaterial
            };

            // Should map to 0
            mesh.facesInternal[0].submeshIndex = -1;
            // Should map to 1
            mesh.facesInternal[1].submeshIndex = 1;
            // Should map to 2
            mesh.facesInternal[2].submeshIndex = 10;

            mesh.ToMesh();

            Assert.AreEqual(3, mesh.mesh.subMeshCount);

            var submesh0 = mesh.facesInternal.Where((x, i) => i != 1 && i != 2).SelectMany(y => y.indexes).ToArray();
            var submesh1 = mesh.facesInternal[1].indexes;
            var submesh2 = mesh.facesInternal[2].indexes;

            var compiled0 = mesh.mesh.GetTriangles(0);
            var compiled1 = mesh.mesh.GetTriangles(1);
            var compiled2 = mesh.mesh.GetTriangles(2);

            TestUtility.AssertSequenceEqual(submesh0, compiled0);
            TestUtility.AssertSequenceEqual(submesh1, compiled1);
            TestUtility.AssertSequenceEqual(submesh2, compiled2);

            UObject.DestroyImmediate(mesh);
        }

        [Test]
        public static void Materials_AreCondensedToSmallestRepresentation()
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);
            mesh.renderer.sharedMaterials = new Material[3];

            mesh.facesInternal[0].submeshIndex = 0;
            mesh.facesInternal[1].submeshIndex = 0;

            mesh.facesInternal[2].submeshIndex = 1;
            mesh.facesInternal[3].submeshIndex = 1;

            mesh.facesInternal[4].submeshIndex = 2;
            mesh.facesInternal[5].submeshIndex = 2;

            mesh.ToMesh();

            Assert.AreEqual(3, mesh.mesh.subMeshCount);

            UObject.Destroy(mesh.gameObject);
        }

        [Test]
        public static void FaceMaterialProperty_UpgradesCorrectly()
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);
            var meshFormat = typeof(ProBuilderMesh).GetField("m_MeshFormatVersion", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(meshFormat);
            meshFormat.SetValue(mesh, -1);

            mesh.renderer.sharedMaterials = new Material[]
            {
                TestUtility.redMaterial,
                TestUtility.greenMaterial,
                TestUtility.blueMaterial
            };

            foreach (var face in mesh.facesInternal)
                face.submeshIndex = -1;

#pragma warning disable 618
            mesh.facesInternal[0].material = TestUtility.greenMaterial;
            mesh.facesInternal[1].material = TestUtility.blueMaterial;
#pragma warning restore 618

            mesh.ToMesh();

            Assert.AreEqual(3, mesh.mesh.subMeshCount);

#pragma warning disable 618
            foreach (var face in mesh.facesInternal)
                Assert.IsTrue(face.material == null);
#pragma warning restore 618

            Assert.AreEqual(1, mesh.facesInternal[0].submeshIndex);
            Assert.AreEqual(2, mesh.facesInternal[1].submeshIndex);

            UObject.DestroyImmediate(mesh.gameObject);
        }
    }
}
