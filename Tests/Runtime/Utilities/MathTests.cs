using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Math = UnityEngine.ProBuilder.Math;

class MathTests
{
    static IEnumerable<TestCaseData> s_ReflectPointCases()
    {
        yield return new TestCaseData(new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, -1f));
        yield return new TestCaseData(new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(0f, -1f));
        yield return new TestCaseData(new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(1f, -1f));
        yield return new TestCaseData(new Vector2(3f, 4f), new Vector2(0f, 0f), new Vector2(0f, 10f), new Vector2(-3f, 4f));
        yield return new TestCaseData(new Vector2(-3f, 4f), new Vector2(0f, 0f), new Vector2(0f, 10f), new Vector2(3f, 4f));
        yield return new TestCaseData(new Vector2(2f, 0f), new Vector2(1f, 1f), new Vector2(4f, 4f), new Vector2(0f, 2f));
    }

    // UUM-157579
    [TestCaseSource(nameof(s_ReflectPointCases))]
    public void ReflectPoint_ReturnsMirrorImageAcrossLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, Vector2 expected)
    {
        var reflected = Math.ReflectPoint(point, lineStart, lineEnd);

        Assert.That(reflected.x, Is.EqualTo(expected.x).Within(0.0001f));
        Assert.That(reflected.y, Is.EqualTo(expected.y).Within(0.0001f));
    }

    static IEnumerable<TestCaseData> s_NearestPointRayRayCases()
    {
        // Orthogonal rays intersecting: ray A from origin along X, ray B from (2, -1, 0) along Y -> nearest point on A is (2, 0, 0)
        yield return new TestCaseData(Vector3.zero, Vector3.right, new Vector3(2f, -1f, 0f), Vector3.up, new Vector3(2f, 0f, 0f));
        // Skew rays in 3D: ray A from (0,0,0) along (1,0,0), ray B from (1, 1, -2) along (0, 0, 1) -> nearest point on A is (1, 0, 0)
        yield return new TestCaseData(Vector3.zero, Vector3.right, new Vector3(1f, 1f, -2f), Vector3.forward, new Vector3(1f, 0f, 0f));
        // Parallel rays: returns ray A origin without NaN or division by zero
        yield return new TestCaseData(new Vector3(1f, 2f, 3f), Vector3.up, new Vector3(4f, 5f, 6f), Vector3.up, new Vector3(1f, 2f, 3f));
        // Anti-parallel rays: returns ray A origin
        yield return new TestCaseData(new Vector3(1f, 2f, 3f), Vector3.up, new Vector3(4f, 5f, 6f), Vector3.down, new Vector3(1f, 2f, 3f));
        // Degenerate zero-direction ray: returns ray A origin
        yield return new TestCaseData(new Vector3(1f, 2f, 3f), Vector3.zero, new Vector3(4f, 5f, 6f), Vector3.up, new Vector3(1f, 2f, 3f));
    }

    [TestCaseSource(nameof(s_NearestPointRayRayCases))]
    public void GetNearestPointRayRay_ComputesCorrectNearestPoint(Vector3 ao, Vector3 ad, Vector3 bo, Vector3 bd, Vector3 expected)
    {
        var result = Math.GetNearestPointRayRay(ao, ad, bo, bd);

        Assert.That(result.x, Is.EqualTo(expected.x).Within(0.0001f));
        Assert.That(result.y, Is.EqualTo(expected.y).Within(0.0001f));
        Assert.That(result.z, Is.EqualTo(expected.z).Within(0.0001f));
    }
}
