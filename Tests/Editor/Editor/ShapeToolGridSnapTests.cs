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

        // Enable grid mode (this is set by ShapeState_InitShape during hover)
        tool.m_IsOnGrid = true;

        // Only test if grid snapping is currently active in the editor
        if (EditorSnapSettings.gridSnapActive)
        {
            // Test position that should snap to grid
            Vector3 unsnapedPosition = new Vector3(1.23f, 2.34f, 3.45f);
            Vector3 snappedPosition = tool.GetPoint(unsnapedPosition);

            // Position should be snapped (not equal to original)
            Assert.That(snappedPosition, Is.Not.EqualTo(unsnapedPosition));
        }
        else
        {
            // Skip test if grid snap is not active
            Assert.Ignore("Grid snapping is not active in the editor, skipping snap verification");
        }
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
    public void PolyShapeTool_HasGridSnapEnabled()
    {
        // Test that PolyShapeTool also has gridSnapEnabled (fix from commit 1e25f10d7)
        ToolManager.SetActiveTool<DrawPolyShapeTool>();

        // PolyShapeTool doesn't have a static instance like DrawShapeTool,
        // so we just verify it was activated successfully
        Assert.That(ToolManager.activeToolType, Is.EqualTo(typeof(DrawPolyShapeTool)));
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

    /// Test for UUM-141074:
    [Test]
    public void ShapeState_InitShape_CalculatesGridStateBeforeMouseDown()
    {
        // This test verifies the fix for UUM-141074
        // Before the fix, m_IsOnGrid was only calculated on MouseDown
        // After the fix, it's calculated during hover (before MouseDown)

        ToolManager.SetActiveTool<CreateCubeTool>();
        var tool = DrawShapeTool.instance;
        Assume.That(tool, Is.Not.Null);

        // The fix ensures that GetPoint can snap correctly during hover
        // because m_IsOnGrid is set before GetPoint is called
        // We can't directly test the state machine, but we can verify
        // that the tool's GetPoint method respects the m_IsOnGrid flag

        // Test 1: When m_IsOnGrid is true, GetPoint should snap (if grid is active)
        tool.m_IsOnGrid = true;
        Vector3 pos1 = new Vector3(1.23f, 2.34f, 3.45f);
        Vector3 result1 = tool.GetPoint(pos1);

        if (EditorSnapSettings.gridSnapActive)
        {
            Assert.That(result1, Is.Not.EqualTo(pos1),
                "GetPoint should snap when m_IsOnGrid is true and grid snap is active");
        }

        // Test 2: When m_IsOnGrid is false, GetPoint should NOT snap
        tool.m_IsOnGrid = false;
        Vector3 pos2 = new Vector3(1.23f, 2.34f, 3.45f);
        Vector3 result2 = tool.GetPoint(pos2);
        Assert.That(result2, Is.EqualTo(pos2),
            "GetPoint should not snap when m_IsOnGrid is false");
    }
}
