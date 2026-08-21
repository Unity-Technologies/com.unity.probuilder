using NUnit.Framework;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder;
using UnityEngine;
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

    // Reproduces: create a shape with Pivot = First Vertex at one size (e.g. drag out a 4x4x4 cube),
    // then place a duplicate (shift-click) at a different, smaller size (e.g. 1x1x1 after editing
    // Shape Settings). The duplicate's pivot offset from its own bounds center must scale with the
    // size of the duplicate being placed, not the size of the shape it was copied from.
    [Test]
    public void PreviewPivotPosition_ScalesWithCurrentBoundsSize_NotStaleDragSize()
    {
        var tool = DrawShapeTool.instance;
        Assume.That(tool, Is.Not.Null);

        tool.m_PlaneRotation = Quaternion.identity;

        // Corner-to-center offset captured from a previously drawn 4x4x4 shape.
        tool.m_LastNonDuplicateCenterToOrigin = new Vector3(-2f, -2f, -2f);

        // Bounds for the shape currently being previewed/duplicated: size has since been changed to 1x1x1.
        tool.m_Bounds = new Bounds(new Vector3(5f, 0.5f, 5f), Vector3.one);

        var pivot = tool.previewPivotPosition;
        var offset = pivot - tool.m_Bounds.center;

        Assert.That(offset.x, Is.EqualTo(-0.5f).Within(0.0001f));
        Assert.That(offset.y, Is.EqualTo(-0.5f).Within(0.0001f));
        Assert.That(offset.z, Is.EqualTo(-0.5f).Within(0.0001f));
    }
}
