#if USING_SPLINES && UNITY_2021_3_OR_NEWER

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Splines;
using Object = UnityEngine.Object;
using Spline = UnityEngine.Splines.Spline;

class BezierMeshTests
{
    BezierMesh m_Mesh;

    [SetUp]
    public void SetUp()
    {
        GameObject go = new GameObject();
        m_Mesh = go.AddComponent<BezierMesh>();
        float3 tan = new float3(0f, 0f, 2f);
        float3 p1 = new float3(3f, 0f, 0f);

        var spline = new List<Spline> { new Spline() };
        spline[0].Add(new BezierKnot(float3.zero, -tan, tan, Quaternion.identity));
        spline[0].Add(new BezierKnot(p1, p1 + tan, p1 + -tan, Quaternion.identity));

        Assume.That(m_Mesh.splineContainer, Is.Not.Null);

        m_Mesh.splineContainer.Splines = spline;
        m_Mesh.ExtrudeMesh();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_Mesh);
    }

    [Test]
    public void BezierMesh_AddingAKnot_ExtrudesMesh()
    {
        Assert.That(m_Mesh, Is.Not.Null);

        var newKnot = new BezierKnot(new float3(6f, 0f, 0f));
        m_Mesh.splineContainer.Splines[0].Add(newKnot);

        Assert.That(m_Mesh.splineContainer.Splines[0].Knots.Count(), Is.EqualTo(3));

        var vertices = m_Mesh.mesh.GetVertices();
        var vertexCountAtPosition = 0;

        // If a knot is added at x = 6f and the mesh is extruded, then we expect to have n number of vertices at
        // x = 6f, where n is the number of faces per segment on the mesh
        foreach (var vertex in vertices)
        {
            if (Mathf.Approximately(vertex.position.x, 6f))
                vertexCountAtPosition++;
        }

        Assert.That(m_Mesh.faceCountPerSegment, Is.EqualTo(vertexCountAtPosition));
    }

    [Test]
    public void BezierMesh_ChangingRadius_ExtrudesMeshCorrectly()
    {
        Assert.That(m_Mesh, Is.Not.Null);

        var startOfSplinePos = (Vector3)m_Mesh.splineContainer.Splines[0].EvaluatePosition(0f);
        var positionOfFirstVertex = m_Mesh.mesh.GetVertices()[0].position;

        Assert.That(m_Mesh.radius, Is.EqualTo((startOfSplinePos - positionOfFirstVertex).magnitude));

        m_Mesh.radius = 10f;
        m_Mesh.ExtrudeMesh();

        positionOfFirstVertex = m_Mesh.mesh.GetVertices()[0].position;
        Assert.That(m_Mesh.radius, Is.EqualTo((startOfSplinePos - positionOfFirstVertex).magnitude));
    }

    [Test]
    public void BezierMesh_ChangingSegmentCount_ExtrudesMeshCorrectly()
    {
        Assert.That(m_Mesh, Is.Not.Null);

        var expectedVertexCount = (int)m_Mesh.splineContainer.Splines[0].GetLength() * m_Mesh.segmentsPerUnit *
            m_Mesh.faceCountPerSegment + m_Mesh.faceCountPerSegment;

        Assert.That(m_Mesh.mesh.vertexCount, Is.EqualTo(expectedVertexCount));

        m_Mesh.segmentsPerUnit = 2;
        m_Mesh.ExtrudeMesh();
        expectedVertexCount = (int)m_Mesh.splineContainer.Splines[0].GetLength() * m_Mesh.segmentsPerUnit *
            m_Mesh.faceCountPerSegment + m_Mesh.faceCountPerSegment;

        Assert.That(m_Mesh.mesh.vertexCount, Is.EqualTo(expectedVertexCount));
    }

    [Test]
    public void BezierMesh_ChangingFaceCount_ExtrudesMeshCorrectly()
    {
        Assert.That(m_Mesh, Is.Not.Null);

        var totalSegmentCount = (int)m_Mesh.splineContainer.Splines[0].GetLength() * m_Mesh.segmentsPerUnit;
        var faceCount = m_Mesh.faceCountPerSegment * totalSegmentCount;
        var expectedFaceCount = m_Mesh.faceCountPerSegment * totalSegmentCount;

        Assert.That(faceCount, Is.EqualTo(expectedFaceCount));

        m_Mesh.faceCountPerSegment = 10;
        m_Mesh.ExtrudeMesh();
        totalSegmentCount = (int)m_Mesh.splineContainer.Splines[0].GetLength() * m_Mesh.segmentsPerUnit;
        faceCount = m_Mesh.faceCountPerSegment * totalSegmentCount;
        expectedFaceCount = m_Mesh.faceCountPerSegment * totalSegmentCount;

        Assert.That(faceCount, Is.EqualTo(expectedFaceCount));
    }

    [Test]
    public void BezierMesh_SplineWithLessThanTwoKnots_DoesNotExtrudeMesh()
    {
        Assert.That(m_Mesh, Is.Not.Null);

        m_Mesh.splineContainer.Splines[0].Knots = new List<BezierKnot> { new BezierKnot(float3.zero) };
        m_Mesh.ExtrudeMesh();

        Assert.That(m_Mesh.mesh.vertexCount, Is.EqualTo(0));
    }

    [Test]
    public void BezierMesh_HavingMultipleSplinesInTheContainer_ExtrudesMeshOnAllSplines()
    {
        Assert.That(m_Mesh, Is.Not.Null);

        var splines = new List<Spline>();

        var p1 = float3.zero;
        var p2 = new float3(5f, 0f, 0f);
        splines.Add(new Spline(new List<BezierKnot>()
        {
            new BezierKnot(p1),
            new BezierKnot(p2)
        }));

        var p3 = new float3(10f, 0f, 0f);
        var p4 = new float3(15f, 0f, 0f);
        splines.Add(new Spline(new List<BezierKnot>()
        {
            new BezierKnot(p3),
            new BezierKnot(p4)
        }));

        m_Mesh.splineContainer.Splines = splines;
        Assert.That(m_Mesh.splineContainer.Splines.Count(), Is.EqualTo(2));

        m_Mesh.ExtrudeMesh();

        var vertices = m_Mesh.mesh.GetVertices();

        var spline1VertexCount = (int) splines[0].GetLength() * m_Mesh.segmentsPerUnit * m_Mesh.faceCountPerSegment + m_Mesh.faceCountPerSegment;
        var spline2VertexCount = (int) splines[1].GetLength() * m_Mesh.segmentsPerUnit * m_Mesh.faceCountPerSegment + m_Mesh.faceCountPerSegment;

        Assume.That(spline1VertexCount + spline2VertexCount, Is.EqualTo(m_Mesh.mesh.GetVertices().Length));

        Assert.That(Mathf.Approximately(vertices[0].position.x, p1.x), Is.True);
        Assert.That(Mathf.Approximately(vertices[spline1VertexCount - 1].position.x, p2.x), Is.True);

        Assert.That(Mathf.Approximately(vertices[spline1VertexCount - 1 + m_Mesh.faceCountPerSegment].position.x, p3.x), Is.True);
        Assert.That(Mathf.Approximately(vertices[spline1VertexCount - 1 + spline2VertexCount].position.x, p4.x), Is.True);
    }
}
#endif
