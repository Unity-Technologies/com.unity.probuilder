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
}
