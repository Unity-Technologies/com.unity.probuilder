using NUnit.Framework;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using ToolManager = UnityEditor.EditorTools.ToolManager;

public class ShapeToolGridSnapTests
{
    [SetUp]
    public void SetUp()
    {
        ToolManager.SetActiveContext<GameObjectToolContext>();
    }

    [TearDown]
    public void TearDown()
    {
        ToolManager.RestorePreviousPersistentTool();
    }

    [Test]
    public void CreateCubeTool_HasGridSnapEnabled()
    {
        ToolManager.SetActiveTool<CreateCubeTool>();
        var toolType = ToolManager.activeToolType;

        Assert.That(toolType, Is.EqualTo(typeof(CreateCubeTool)));

        // Verify the tool has gridSnapEnabled property set to true
        var tool = DrawShapeTool.instance;
        Assert.That(tool, Is.Not.Null);
        Assert.That(tool.gridSnapEnabled, Is.True);
    }

    [Test]
    public void DrawShapeTool_GetPoint_SnapsWhenOnGridAndGridSnapActive()
    {
        ToolManager.SetActiveTool<CreateCubeTool>();
        var tool = DrawShapeTool.instance;
        Assume.That(tool, Is.Not.Null);

        EditorSnapSettings.gridSnapEnabled = true;
        EditorSnapSettings.snapEnabled = true;

        var previousPivotRotation = Tools.pivotRotation;
        Tools.pivotRotation = PivotRotation.Global;

        // Enable grid mode (this is set by ShapeState_InitShape during hover)
        tool.m_IsOnGrid = true;

        // Only test if grid snapping is currently active in the editor
        if (EditorSnapSettings.gridSnapActive)
        {
            // Test position that should snap to grid
            Vector3 unsnappedPosition = new Vector3(1.23f, 2.34f, 3.45f);
            Vector3 snappedPosition = tool.GetPoint(unsnappedPosition);

            // Position should be snapped (not equal to original)
            Assert.That(snappedPosition, Is.Not.EqualTo(unsnappedPosition));
        }
        else
        {
            // Skip test if grid snap is not active
            Assert.Ignore("Grid snapping is not active in the editor, skipping snap verification");
        }

        Tools.pivotRotation = previousPivotRotation;
    }

    [Test]
    public void DrawShapeTool_GetPoint_DoesNotSnapWhenOffGrid()
    {
        ToolManager.SetActiveTool<CreateCubeTool>();
        var tool = DrawShapeTool.instance;
        Assume.That(tool, Is.Not.Null);

        // Disable grid mode
        tool.m_IsOnGrid = false;

        // Test position should NOT snap regardless of editor snap settings
        Vector3 position = new Vector3(1.23f, 2.34f, 3.45f);
        Vector3 result = tool.GetPoint(position);

        // Position should remain unchanged when m_IsOnGrid is false
        Assert.That(result, Is.EqualTo(position));
    }

    [Test]
    public void DrawShapeTool_GetPoint_RespectsIncrementalSnap()
    {
        ToolManager.SetActiveTool<CreateCubeTool>();
        var tool = DrawShapeTool.instance;
        Assume.That(tool, Is.Not.Null);

        // Get current incremental snap value
        Vector3 snapValue = EditorSnapping.incrementalSnapMoveValue;

        // Test with incremental snap enabled
        Vector3 unsnappedPosition = new Vector3(1.23f, 2.34f, 3.45f);
        Vector3 snappedPosition = tool.GetPoint(unsnappedPosition, useIncrementSnap: true);

        // Position should be snapped to incremental snap value
        Assert.That(snappedPosition, Is.Not.EqualTo(unsnappedPosition));

        // Verify each component is a multiple of snap value (within tolerance)
        Assert.That(snappedPosition.x % snapValue.x, Is.EqualTo(0).Within(0.001f));
        Assert.That(snappedPosition.y % snapValue.y, Is.EqualTo(0).Within(0.001f));
        Assert.That(snappedPosition.z % snapValue.z, Is.EqualTo(0).Within(0.001f));
    }
}
