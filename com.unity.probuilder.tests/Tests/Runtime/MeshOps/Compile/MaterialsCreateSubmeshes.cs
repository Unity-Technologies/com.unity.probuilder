using NUnit.Framework;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder.Tests.Framework;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOps.Edge
{
    static class MaterialsCreateSubmeshes
    {
        [Test]
        public static void MeshWith2MaterialsCreates2Submeshes()
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);
            Assert.AreEqual(1, mesh.mesh.subMeshCount);

            mesh.faces[0].material = TestUtility.redMaterial;

            mesh.ToMesh();

            Assert.AreEqual(2, mesh.mesh.subMeshCount);

            UObject.Destroy(mesh.gameObject);
        }

        [Test]
        public static void AllNullMaterialsCreate1Submesh()
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);
            foreach (var face in mesh.facesInternal)
                face.material = null;
            mesh.ToMesh();
            Assert.AreEqual(1, mesh.mesh.subMeshCount);
            UObject.Destroy(mesh.gameObject);
        }

        [Test]
        public static void MaterialsAreCollapsed()
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);

            mesh.facesInternal[0].material = TestUtility.redMaterial;
            mesh.facesInternal[1].material = TestUtility.redMaterial;

            mesh.facesInternal[2].material = TestUtility.blueMaterial;
            mesh.facesInternal[3].material = TestUtility.blueMaterial;

            mesh.facesInternal[4].material = TestUtility.greenMaterial;
            mesh.facesInternal[5].material = TestUtility.greenMaterial;

            mesh.ToMesh();

            Assert.AreEqual(3, mesh.mesh.subMeshCount);

            UObject.Destroy(mesh.gameObject);
        }
    }
}