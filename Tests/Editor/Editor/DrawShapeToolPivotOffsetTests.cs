using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using ToolManager = UnityEditor.EditorTools.ToolManager;

public class DrawShapeToolPivotOffsetTests
{
    Pref<PivotLocation> m_PivotPref = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Cube", PivotLocation.Center);
    PivotLocation m_PreviousPivot;

    [SetUp]
    public void SetUp()
    {
        ToolManager.SetActiveContext<GameObjectToolContext>();
        ToolManager.SetActiveTool<CreateCubeTool>();

        m_PreviousPivot = m_PivotPref.value;
        m_PivotPref.SetValue(PivotLocation.FirstVertex);
    }

    [TearDown]
    public void TearDown()
    {
        m_PivotPref.SetValue(m_PreviousPivot);
        ToolManager.RestorePreviousPersistentTool();
    }

    // For a First Vertex pivot, the pivot always sits at bounds.center - size/2: the sign of `size`
    // itself already encodes which side the shape extends toward, so the previous drag's recorded
    // corner-to-center vector should have no bearing on where a same-or-different-sized duplicate's
    // pivot lands. Both cases below place a duplicate of the same currentSize/expectedOffset, only
    // the stale drag data differs, to prove that stale data can't leak into the result.
    static IEnumerable<TestCaseData> PivotOffsetCases()
    {
        // Drawn previously as a 4x4x4 cube, now duplicating at 1x1x1 (Shape Settings edited down).
        yield return new TestCaseData(new Vector3(-2f, -2f, -2f), Vector3.one, new Vector3(-0.5f, -0.5f, -0.5f))
            .SetName("PreviewPivotPosition_AllPositiveOldSize_ScalesToCurrentSize");

        // Drawn previously as an all-negative-size shape (e.g. dragged backwards on every axis), now
        // duplicating at 1x1x-1 (matching a Stairs-style shape with a negative Z size).
        yield return new TestCaseData(new Vector3(2f, 2f, 2f), new Vector3(1f, 1f, -1f), new Vector3(-0.5f, -0.5f, 0.5f))
            .SetName("PreviewPivotPosition_NegativeAxisInOldSize_DoesNotFlipCurrentAxis");

        // Drawn toward negative X (size.x negative), then duplicated unchanged: this gives a
        // *positive* m_LastNonDuplicateCenterToOrigin.x alongside a *negative* current size.x - the
        // exact combination that reverses the pivot onto the wrong corner if that stale vector's sign
        // is multiplied against the current (still negative) size instead of driving off size alone.
        yield return new TestCaseData(new Vector3(1.5f, -0.5f, -0.5f), new Vector3(-3f, 1f, 1f), new Vector3(1.5f, -0.5f, -0.5f))
            .SetName("PreviewPivotPosition_NegativeDragDirection_DoesNotReversePivotCorner");
    }

    [TestCaseSource(nameof(PivotOffsetCases))]
    public void PreviewPivotPosition_ScalesWithCurrentBoundsSize_NotStaleDragSize(Vector3 lastCenterToOrigin, Vector3 currentSize, Vector3 expectedOffset)
    {
        var tool = DrawShapeTool.instance;
        Assume.That(tool, Is.Not.Null);

        tool.m_PlaneRotation = Quaternion.identity;
        tool.m_LastNonDuplicateCenterToOrigin = lastCenterToOrigin;
        tool.m_Bounds = new Bounds(new Vector3(5f, 0.5f, 5f), currentSize);

        var pivot = tool.previewPivotPosition;
        var offset = pivot - tool.m_Bounds.center;

        Assert.That(offset.x, Is.EqualTo(expectedOffset.x).Within(0.0001f));
        Assert.That(offset.y, Is.EqualTo(expectedOffset.y).Within(0.0001f));
        Assert.That(offset.z, Is.EqualTo(expectedOffset.z).Within(0.0001f));
    }
}
