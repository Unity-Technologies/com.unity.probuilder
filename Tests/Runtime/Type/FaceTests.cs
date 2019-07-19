using System.Linq;
using NUnit.Framework;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Tests.Framework;

namespace UnityEngine.ProBuilder.RuntimeTests.Type
{
    public class FaceTests
    {
        static int[] k_IndexCounts = new int[] { 3, 4, 5 };

        [Test]
        public void MeshValidation_IsSplitFace_ConfirmsValidFaces([ValueSource("k_IndexCounts")] int indexCount)
        {
            var points = new Vector3[indexCount];
            var indices = new int[(indexCount - 1) * 3];

            for (int i = 0; i < indexCount; i++)
            {
                float travel = ((i + 1) / (float)indexCount) * Mathf.PI * 2f;
                points[i] = new Vector3(Mathf.Cos(travel), 0f, Mathf.Sin(travel));
            }

            for (int i = 1; i < indexCount - 1; i++)
            {
                indices[(i-1) * 3 + 0] = 0;
                indices[(i-1) * 3 + 1] = i;
                indices[(i-1) * 3 + 2] = (i + 1) % indexCount;
            }

            var shape = ProBuilderMesh.Create(points, new Face[] { new Face(indices) });

            Assume.That(shape, Is.Not.Null);
            var face = shape.faces.First();
            Assume.That(face, Is.Not.Null);
            Assume.That(face.edgesInternal, Has.Length.EqualTo(indexCount));
            Assert.That(MeshValidation.ContainsNonContiguousTriangles(shape, face), Is.False);
        }

        [Test]
        public void MeshValidation_IsSplitFace_DetectsDisconnectedTriangles()
        {
            var cube = TestUtility.CreateCubeWithNonContiguousMergedFace();
            Assume.That(cube.item1.faceCount, Is.EqualTo(5));
            Assert.That(MeshValidation.ContainsNonContiguousTriangles(cube.item1, cube.item2), Is.True);
        }

    }
}
