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
        var vertices = m_Mesh.mesh.GetVertices();

        var isVertexAtExpectedPosition = false;

        // If a knot is added at x = 6f, and the mesh is extruded, then we expect to have at least one vertex at that position
        foreach (var vertex in vertices)
        {
            if (Mathf.Approximately(vertex.position.x, 6f))
            {
                isVertexAtExpectedPosition = true;
                break;
            }
        }

        Assert.That(m_Mesh.splineContainer.Splines[0].Knots.Count(), Is.EqualTo(3));
        Assert.That(isVertexAtExpectedPosition, Is.True);
    }

    [Test]
    public void BezierMesh_ChangingRadius_ExtrudesMeshCorrectly()
    {
        Assert.That(m_Mesh, Is.Not.Null);

        var radius = m_Mesh.radius;
        var startOfSplinePos = (Vector3)m_Mesh.splineContainer.Splines[0].EvaluatePosition(0f);
        var positionOfFirstVertex = m_Mesh.mesh.GetVertices()[0].position;

        Assert.That(radius, Is.EqualTo((startOfSplinePos - positionOfFirstVertex).magnitude));

        var newRadius = m_Mesh.radius = 10f;
        m_Mesh.ExtrudeMesh();

        positionOfFirstVertex = m_Mesh.mesh.GetVertices()[0].position;
        Assert.That(newRadius, Is.EqualTo((startOfSplinePos - positionOfFirstVertex).magnitude));
    }

    [Test]
    public void BezierMesh_ChangingSegmentCount_ExtrudesMeshCorrectly()
    {
        Assert.That(m_Mesh, Is.Not.Null);

        var segmentsPerUnit = m_Mesh.segmentsPerUnit;
        var expectedVertexCount = (int)m_Mesh.splineContainer.Splines[0].GetLength() * segmentsPerUnit *
            m_Mesh.faceCountPerSegment + m_Mesh.faceCountPerSegment;

        Assert.That(m_Mesh.mesh.vertexCount, Is.EqualTo(expectedVertexCount));

        var newSegmentsPerUnit = m_Mesh.segmentsPerUnit = 2;
        m_Mesh.ExtrudeMesh();
        var newExpectedVertexCount = (int)m_Mesh.splineContainer.Splines[0].GetLength() * newSegmentsPerUnit *
            m_Mesh.faceCountPerSegment + m_Mesh.faceCountPerSegment;

        Assert.That(m_Mesh.mesh.vertexCount, Is.EqualTo(newExpectedVertexCount));
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
        var newTotalSegmentCount = (int)m_Mesh.splineContainer.Splines[0].GetLength() * m_Mesh.segmentsPerUnit;
        var newFaceCount = m_Mesh.faceCountPerSegment * totalSegmentCount;
        var newExpectedFaceCount = m_Mesh.faceCountPerSegment * newTotalSegmentCount;

        Assert.That(newFaceCount, Is.EqualTo(newExpectedFaceCount));
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
        var isVertexFoundAtP1 = false;
        var isVertexFoundAtP2 = false;
        var isVertexFoundAtP3 = false;
        var isVertexFoundAtP4 = false;

        foreach (var vertex in vertices)
        {
            var pos = vertex.position;

            if (Mathf.Approximately(pos.x, p1.x))
                isVertexFoundAtP1 = true;
            if (Mathf.Approximately(pos.x, p2.x))
                isVertexFoundAtP2 = true;
            if (Mathf.Approximately(pos.x, p3.x))
                isVertexFoundAtP3 = true;
            if (Mathf.Approximately(pos.x, p4.x))
                isVertexFoundAtP4 = true;
        }

        Assert.That(isVertexFoundAtP1, Is.True);
        Assert.That(isVertexFoundAtP2, Is.True);
        Assert.That(isVertexFoundAtP3, Is.True);
        Assert.That(isVertexFoundAtP4, Is.True);
    }
}
#endif
