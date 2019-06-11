using System.Linq;
using NUnit.Framework;
using UnityEngine.ProBuilder.MeshOperations;

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
            Assert.That(MeshValidation.IsSplitFace(shape, face), Is.False);
        }

        [Test]
        public void MeshValidation_IsSplitFace_DetectsDisconnectedTriangles()
        {
            var cube = ShapeGenerator.CreateShape(ShapeType.Cube);

            Assume.That(cube, Is.Not.Null);

            int index = 1;
            Face a = cube.faces[0], b = cube.faces[index++];
            Assume.That(a, Is.Not.Null);
            Assume.That(b, Is.Not.Null);

            while (FacesAreAdjacent(cube, a, b) && index < cube.faceCount)
                b = cube.faces[index++];

            Assume.That(FacesAreAdjacent(cube, a, b), Is.False);

            var res = MergeElements.Merge(cube, new Face[] { a, b });

            Assume.That(cube.faceCount, Is.EqualTo(5));

            Assert.That(MeshValidation.IsSplitFace(cube, res), Is.True);
        }

        static bool FacesAreAdjacent(ProBuilderMesh mesh, Face a, Face b)
        {
            for (int i = 0, c = a.edgesInternal.Length; i < c; i++)
            {
                var ea = mesh.GetSharedVertexHandleEdge(a.edgesInternal[i]);

                for (int n = 0; n < b.edgesInternal.Length; n++)
                {
                    var eb = mesh.GetSharedVertexHandleEdge(b.edgesInternal[n]);

                    if (ea == eb)
                        return true;
                }
            }

            return false;
        }
    }
}
