using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Tests
{
    public class UVGeneration
    {
        ProBuilderMesh m_PBMesh = null;

        static int[] s_ConeSubDivAxes = new int[] { 6, 3, 5, 20 };

        [TearDown]
        public void Term()
        {
            if (m_PBMesh)
            {
                UObject.DestroyImmediate(m_PBMesh.gameObject);
            }
        }

        [Test, TestCaseSource(typeof(UVGeneration), "s_ConeSubDivAxes")]
        public void NewShape_CreateCone_FaceUVsAreConsistent(int subDivAxis)
        {
            m_PBMesh = ShapeGenerator.GenerateCone(PivotLocation.Center, 0.5f, 1f, subDivAxis);

            var faces = m_PBMesh.facesInternal;
            var uvs = m_PBMesh.texturesInternal;

            Assert.That(faces.Length, Is.EqualTo(subDivAxis*2));

            var firstFace = faces[0];
            var firstFaceIndices = firstFace.distinctIndexesInternal;
            // every other face in the array is a side face and should have the same UVs
            for (int i = 2; i < faces.Length; i += 2)
            {
                var faceIndices = faces[i].distinctIndexesInternal;
                Assert.That(faceIndices.Length, Is.EqualTo(firstFaceIndices.Length));
                for(int j = 0; j < faceIndices.Length; j++)
                {
                    Assert.That(uvs[faceIndices[j]], Is.EqualTo(uvs[firstFaceIndices[j]]));
                }
            }
        }
    }
}
