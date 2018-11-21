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
    static class SubdivideTest
    {
        [Test]
        public static void SubdivideFirstFace()
        {
            using (var shapes = new TestUtility.BuiltInPrimitives())
            {
                foreach (var pb in (IEnumerable<ProBuilderMesh>)shapes)
                {
                    var face = pb.facesInternal.FirstOrDefault();
                    Subdivision.Subdivide(pb, new ProBuilder.Face[] { face });
                    pb.ToMesh();
                    pb.Refresh();
#if PB_CREATE_TEST_MESH_TEMPLATES
                    TestUtility.SaveAssetTemplate(pb.mesh, pb.name);
#endif
                    TestUtility.AssertMeshAttributesValid(pb.mesh);
                    var template = TestUtility.GetAssetTemplate<Mesh>(pb.name);
                    Assert.IsNotNull(template);
                    TestUtility.AssertMeshesAreEqual(template, pb.mesh);
                }
            }
        }

        [Test]
        public static void SubdivideRetainsMaterial()
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);

            mesh.facesInternal[0].submeshIndex = 1;

            var res = Subdivision.Subdivide(mesh, new[] { mesh.facesInternal[0] });

            foreach (var face in res)
                Assert.AreEqual(1, face.submeshIndex);

            foreach (var face in mesh.facesInternal.Where(x => !res.Contains(x)))
                Assert.AreEqual(0, face.submeshIndex);
        }
    }
}
