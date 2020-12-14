using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class UVGeneration
{
    ProBuilderMesh m_PBMesh = null;

    static int[] s_ConeSubDivAxes = new int[] { 6, 3, 5, 20 };

    [TearDown]
    public void TearDown()
    {
        if (m_PBMesh)
            UObject.DestroyImmediate(m_PBMesh.gameObject);
    }

    static ProBuilderMesh GenerateCone(PivotLocation pivotType, float radius, float height, int subdivAxis)
    {
        ProBuilderMesh pb = ShapeFactory.Instantiate(typeof(Cone), pivotType);
        Cone cone = pb.GetComponent<ShapeComponent>().shape as Cone;
        cone.m_NumberOfSides = subdivAxis;
        cone.RebuildMesh(pb, new Vector3(radius, height, radius), PivotLocation.FirstVertex);
        pb.RefreshUV(pb.faces);
        return pb;
    }

    [Test, TestCaseSource(typeof(UVGeneration), "s_ConeSubDivAxes")]
    public void NewShape_CreateCone_FaceUVsAreConsistent(int subDivAxis)
    {
        m_PBMesh = GenerateCone(PivotLocation.Center, 0.5f, 1f, subDivAxis);

        var faces = m_PBMesh.facesInternal;
        var uvs = m_PBMesh.texturesInternal;

        Assert.That(faces.Length, Is.EqualTo(subDivAxis * 2));

        var firstFace = faces[0];
        var firstFaceIndices = firstFace.distinctIndexesInternal;

        // every other face in the array is a side face and should have the same UVs
        for (int i = 2; i < faces.Length; i += 2)
        {
            var faceIndices = faces[i].distinctIndexesInternal;
            Assert.That(faceIndices.Length, Is.EqualTo(firstFaceIndices.Length));
            for (int j = 0; j < faceIndices.Length; j++)
            {
                Assert.That(uvs[faceIndices[j]], Is.EqualTo(uvs[firstFaceIndices[j]]));
            }
        }
    }
}
