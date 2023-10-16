using NUnit.Framework;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using ToolManager = UnityEditor.EditorTools.ToolManager;

public class EditorToolTests
{
    [OneTimeSetUp]
    public void SetUp()
    {
        ToolManager.SetActiveContext<PositionToolContext>();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        ToolManager.SetActiveContext<GameObjectToolContext>();
    }

    [Test]
    public void SetToolKeepsContext()
    {
        ProBuilderEditor.selectMode = SelectMode.Face;
        Assume.That(ProBuilderEditor.selectMode, Is.EqualTo(SelectMode.Face));
        Assume.That(ToolManager.activeToolType, Is.EqualTo(typeof(ProbuilderMoveTool)));
        Tools.current = Tool.Rotate;
        Assert.That(ToolManager.activeToolType, Is.EqualTo(typeof(ProbuilderRotateTool)));
    }

    [Test]
    public void SetSelectModeSetsTool()
    {
        var editor = EditorWindow.GetWindow<ProBuilderEditor>();
        Assume.That(editor, Is.Not.Null);
        ProBuilderEditor.selectMode = SelectMode.None;
        Assume.That(ProBuilderEditor.selectMode, Is.EqualTo(SelectMode.None));
        Tools.current = Tool.Move;

        ProBuilderEditor.selectMode = SelectMode.Face;
        Assert.That(ToolManager.activeToolType, Is.EqualTo(typeof(ProbuilderMoveTool)));
    }
}
